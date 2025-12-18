using Microsoft.AspNetCore.Mvc;
using API.Services;
using Model;
using System;
using System.Threading.Tasks;

namespace API.Controllers
{
    [ApiController]
    [Route("api/doctors/{doctorId}/patients")]
    public class DoctorPatientsController : ControllerBase
    {
        private readonly DoctorPatientService _dp;
        private readonly IMedicationService _meds;

        public DoctorPatientsController(DoctorPatientService dp, IMedicationService meds)
        {
            _dp = dp;
            _meds = meds;
        }

        [HttpPost("{patientUserId}/link")]
        public async Task<IActionResult> Link(string doctorId, string patientUserId)
        {
            await _dp.LinkAsync(doctorId, patientUserId);
            return Ok(new { message = "Linked successfully" });
        }

        [HttpGet]
        public async Task<IActionResult> List(string doctorId)
        {
            var list = await _dp.GetDoctorPatientsAsync(doctorId);
            return Ok(list);
        }

        [HttpPost("{patientUserId}/medications")]
        public async Task<IActionResult> PrescribeMedication(string doctorId, string patientUserId, [FromBody] MedicationPlan plan)
        {
            if (plan == null) return BadRequest("Plan is required");

            var linked = await _dp.IsLinkedAsync(doctorId, patientUserId);
            if (!linked) return Forbid("Doctor is not linked to this patient");

            plan.UserId = patientUserId;
            plan.PrescribedByDoctorId = doctorId;
            plan.PrescribedAt = DateTime.UtcNow;

            await _meds.AddPlanAsync(plan);
            return Ok(plan);
        }
    }
}
