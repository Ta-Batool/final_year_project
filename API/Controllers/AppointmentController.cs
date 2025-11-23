using Microsoft.AspNetCore.Mvc;
using API.Services;
using Model;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AppointmentsController : ControllerBase
    {
        private readonly IAppointmentService _appointmentService;

        public AppointmentsController(IAppointmentService appointmentService)
        {
            _appointmentService = appointmentService;
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

        // ✅ Create new appointment
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

            updatedAppointment.UserId = existing.UserId; // ensure linked data remains correct
            updatedAppointment.DoctorId = existing.DoctorId;

            await _appointmentService.UpdateAsync(id, updatedAppointment);
            return NoContent();
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
    }
}
