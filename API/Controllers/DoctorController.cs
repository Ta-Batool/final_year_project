using Microsoft.AspNetCore.Mvc;
using API.Services;
using Model;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DoctorsController : ControllerBase
    {
        private readonly DoctorService _doctorService;

        public DoctorsController(DoctorService doctorService)
        {
            _doctorService = doctorService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Doctor>>> GetAllDoctors()
        {
            var doctors = await _doctorService.GetAllAsync();
            return Ok(doctors);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Doctor>> GetDoctor(string id)
        {
            var doctor = await _doctorService.GetByIdAsync(id);

            if (doctor == null)
                return NotFound();

            return Ok(doctor);
        }

        [HttpPost]
        public async Task<IActionResult> CreateDoctor([FromBody] Doctor doctor)
        {
            if (doctor == null)
            {
                return BadRequest("Doctor data is required.");
            }

            await _doctorService.CreateAsync(doctor);
            return Ok(doctor);
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDoctor(string id, Doctor doctor)
        {
            var existingDoctor = await _doctorService.GetByIdAsync(id);
            if (existingDoctor == null)
                return NotFound();

            doctor.Id = id;
            await _doctorService.UpdateAsync(id, doctor);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDoctor(string id)
        {
            var existingDoctor = await _doctorService.GetByIdAsync(id);
            if (existingDoctor == null)
                return NotFound();

            await _doctorService.DeleteAsync(id);
            return NoContent();
        }

        [HttpGet("by-client/{clientId}")]
        public async Task<ActionResult<Doctor>> GetByClientId(string clientId)
        {
            var doctor = await _doctorService.GetDoctorByClientIdAsync(clientId);
            if (doctor == null)
                return NotFound();

            return Ok(doctor);
        }
        [HttpPut("by-client/{clientId}")]
            public async Task<IActionResult> UpdateDoctorByClientId(
            string clientId,
            [FromBody] Doctor updatedDoctor)  // must match the body JSON
            {
                if (updatedDoctor == null)
                    return BadRequest("updatedDoctor cannot be null");

                var doctor = await _doctorService.GetDoctorByClientIdAsync(clientId);
                if (doctor == null)
                    return NotFound();

                updatedDoctor.Id = doctor.Id;

                await _doctorService.UpdateDoctorAsync(doctor.Id, updatedDoctor);
                return NoContent();
            }
    }
}
