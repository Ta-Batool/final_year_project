using Microsoft.AspNetCore.Mvc;
using API.Services;
using Model;
using API.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AppointmentsController : ControllerBase
    {
        private readonly IAppointmentService _appointmentService;
        private readonly DoctorService _doctorService;

        public AppointmentsController(IAppointmentService appointmentService, DoctorService doctorService)
        {
            _appointmentService = appointmentService;
            _doctorService = doctorService;
        }

        // ✅ Get all appointments
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Appointment>>> GetAllAppointments()
        {
            var appointments = await _appointmentService.GetAllAsync();
            return Ok(appointments);
        }

        // ✅ Get appointment by Id
        [HttpGet("{id}")]
        public async Task<ActionResult<Appointment>> GetAppointment(string id)
        {
            var appointment = await _appointmentService.GetByIdAsync(id);
            if (appointment == null) return NotFound();
            return Ok(appointment);
        }

        // ✅ Get appointments for a specific user
        [HttpGet("by-user/{userId}")]
        public async Task<ActionResult<IEnumerable<Appointment>>> GetAppointmentsByUser(string userId)
        {
            var appointments = await _appointmentService.GetByUserIdAsync(userId);
            return Ok(appointments);
        }

        // ✅ Get appointments for a specific doctor
        [HttpGet("by-doctor/{doctorId}")]
        public async Task<ActionResult<IEnumerable<Appointment>>> GetAppointmentsByDoctor(string doctorId)
        {
            var appointments = await _appointmentService.GetByDoctorIdAsync(doctorId);
            return Ok(appointments);
        }

        // ✅ Create new appointment (raw CRUD)
        [HttpPost]
        public async Task<IActionResult> CreateAppointment([FromBody] Appointment appointment)
        {
            if (appointment == null)
                return BadRequest("Appointment data is required.");

            await _appointmentService.CreateAsync(appointment);
            return Ok(appointment);
        }

        // ✅ Update appointment by Id
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAppointment(string id, [FromBody] Appointment updatedAppointment)
        {
            var existing = await _appointmentService.GetByIdAsync(id);
            if (existing == null) return NotFound();

            updatedAppointment.UserId = existing.UserId;
            updatedAppointment.DoctorId = existing.DoctorId;

            await _appointmentService.UpdateAsync(id, updatedAppointment);
            return NoContent();
        }

        // ✅ NEW: Update appointment status
        // PUT api/appointments/{id}/status
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateAppointmentStatus(string id, [FromBody] string status)
        {
            if (string.IsNullOrWhiteSpace(status))
                return BadRequest("Status is required.");

            var existing = await _appointmentService.GetByIdAsync(id);
            if (existing == null) return NotFound("Appointment not found.");

            // Optional: normalize common values
            // "Cancel" -> "Cancelled", "Complete" -> "Completed"
            if (string.Equals(status, "Cancel", StringComparison.OrdinalIgnoreCase))
                status = "Cancelled";
            if (string.Equals(status, "Complete", StringComparison.OrdinalIgnoreCase))
                status = "Completed";

            existing.Status = status;

            await _appointmentService.UpdateAsync(id, existing);

            // return updated appointment for UI
            return Ok(existing);
        }

        // ✅ Delete appointment
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAppointment(string id)
        {
            var existing = await _appointmentService.GetByIdAsync(id);
            if (existing == null) return NotFound();

            await _appointmentService.DeleteAsync(id);
            return NoContent();
        }

        // ------------------------------------------------------------------
        // ✅ Doctor Availability
        // GET api/appointments/doctor/{doctorId}/availability?date=2025-12-18&slotMinutes=30
        // Supports:
        //  - Date-specific slots (TimeSlots.Date) preferred
        //  - Weekly slots fallback (TimeSlots.Day)
        // ------------------------------------------------------------------
        [HttpGet("doctor/{doctorId}/availability")]
        public async Task<ActionResult<AvailabilityResponseDto>> GetDoctorAvailability(
            string doctorId,
            [FromQuery] DateTime date,
            [FromQuery] int slotMinutes = 30
        )
        {
            if (slotMinutes <= 0) return BadRequest("slotMinutes must be > 0");

            var doctor = await _doctorService.GetByIdAsync(doctorId);
            if (doctor == null) return NotFound("Doctor not found");

            var dayName = date.DayOfWeek.ToString(); // e.g. "Monday"

            // ✅ Prefer DATE-specific slots if present for that date
            var dateSlots = doctor.Slots?
                .Where(s => s.Date.HasValue &&
                            s.Date.Value.Date == date.Date &&
                            s.StartTime != null &&
                            s.EndTime != null)
                .ToList() ?? new List<TimeSlots>();

            List<TimeSlots> activeSlots;

            if (dateSlots.Any())
            {
                activeSlots = dateSlots;
            }
            else
            {
                // fallback to weekly day-based
                activeSlots = doctor.Slots?
                    .Where(s => !string.IsNullOrWhiteSpace(s.Day) &&
                                string.Equals(s.Day, dayName, StringComparison.OrdinalIgnoreCase) &&
                                s.StartTime != null &&
                                s.EndTime != null)
                    .ToList() ?? new List<TimeSlots>();
            }

            if (!activeSlots.Any())
                return Ok(new AvailabilityResponseDto(doctorId, date.Date, slotMinutes, new()));

            var booked = await _appointmentService.GetByDoctorAndDateAsync(doctorId, date.Date);

            var available = new List<TimeSlotDto>();

            foreach (var s in activeSlots)
            {
                if (s.StartTime == null || s.EndTime == null) continue;

                var start = date.Date.Add(s.StartTime.Value.ToTimeSpan());
                var end = date.Date.Add(s.EndTime.Value.ToTimeSpan());

                // safety
                if (end <= start) continue;

                for (var t = start; t.AddMinutes(slotMinutes) <= end; t = t.AddMinutes(slotMinutes))
                {
                    var slotStart = t;
                    var slotEnd = t.AddMinutes(slotMinutes);

                    var overlaps = booked.Any(a =>
                        a.Status == "Scheduled" &&
                        a.Start < slotEnd &&
                        a.End > slotStart
                    );

                    if (!overlaps)
                        available.Add(new TimeSlotDto(slotStart, slotEnd));
                }
            }

            return Ok(new AvailabilityResponseDto(doctorId, date.Date, slotMinutes, available));
        }

        // ------------------------------------------------------------------
        // ✅ Book appointment (validates inside available hours + no overlap)
        // POST api/appointments/book
        // Validates:
        //  - If date-specific slots exist for that date => must fit within them
        //  - Else => must fit within weekly slots for that weekday
        // ------------------------------------------------------------------
        [HttpPost("book")]
        public async Task<IActionResult> Book([FromBody] BookAppointmentRequest req)
        {
            if (req == null) return BadRequest("Request required");
            if (req.DurationMinutes <= 0) return BadRequest("Invalid duration");
            if (string.IsNullOrWhiteSpace(req.PatientUserId)) return BadRequest("PatientUserId required");
            if (string.IsNullOrWhiteSpace(req.DoctorId)) return BadRequest("DoctorId required");

            var doctor = await _doctorService.GetByIdAsync(req.DoctorId);
            if (doctor == null) return NotFound("Doctor not found");

            var end = req.Start.AddMinutes(req.DurationMinutes);
            var dayName = req.Start.DayOfWeek.ToString();

            // ✅ If date slots exist for this date, enforce date-based validation
            var hasDateSlots = doctor.Slots != null && doctor.Slots.Any(s =>
                s.Date.HasValue &&
                s.Date.Value.Date == req.Start.Date &&
                s.StartTime != null &&
                s.EndTime != null
            );

            bool within;

            if (hasDateSlots)
            {
                within = doctor.Slots!.Any(s =>
                    s.Date.HasValue &&
                    s.Date.Value.Date == req.Start.Date &&
                    s.StartTime != null && s.EndTime != null &&
                    req.Start.TimeOfDay >= s.StartTime.Value.ToTimeSpan() &&
                    end.TimeOfDay <= s.EndTime.Value.ToTimeSpan()
                );
            }
            else
            {
                within = doctor.Slots != null && doctor.Slots.Any(s =>
                    !string.IsNullOrWhiteSpace(s.Day) &&
                    string.Equals(s.Day, dayName, StringComparison.OrdinalIgnoreCase) &&
                    s.StartTime != null && s.EndTime != null &&
                    req.Start.TimeOfDay >= s.StartTime.Value.ToTimeSpan() &&
                    end.TimeOfDay <= s.EndTime.Value.ToTimeSpan()
                );
            }

            if (!within)
                return BadRequest("Selected time is outside doctor's available hours");

            // Conflict check (double booking protection)
            var overlap = await _appointmentService.HasOverlapAsync(req.DoctorId, req.Start, end);
            if (overlap)
                return Conflict("Time slot already booked");

            var appt = new Appointment
            {
                UserId = req.PatientUserId,
                DoctorId = req.DoctorId,
                Start = req.Start,
                End = end,
                Status = "Scheduled",
                Notes = req.Notes ?? ""
            };

            await _appointmentService.CreateAsync(appt);
            return Ok(appt);
        }
    }
}
