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

        // GET: api/doctors
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Doctor>>> GetAllDoctors()
        {
            var doctors = await _doctorService.GetAllAsync();
            return Ok(doctors);
        }

        // GET: api/doctors/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Doctor>> GetDoctor(string id)
        {
            var doctor = await _doctorService.GetByIdAsync(id);

            if (doctor == null)
                return NotFound();

            return Ok(doctor);
        }

        // POST: api/doctors
        [HttpPost]
        public async Task<IActionResult> CreateDoctor([FromBody] Doctor doctor)
        {
            if (doctor == null)
                return BadRequest("Doctor data is required.");

            await _doctorService.CreateAsync(doctor);
            return Ok(doctor);
        }

        // PUT: api/doctors/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDoctor(string id, [FromBody] Doctor doctor)
        {
            var existingDoctor = await _doctorService.GetByIdAsync(id);
            if (existingDoctor == null)
                return NotFound();

            doctor.Id = id;
            await _doctorService.UpdateAsync(id, doctor);
            return NoContent();
        }

        // DELETE: api/doctors/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDoctor(string id)
        {
            var existingDoctor = await _doctorService.GetByIdAsync(id);
            if (existingDoctor == null)
                return NotFound();

            await _doctorService.DeleteAsync(id);
            return NoContent();
        }

        // GET: api/doctors/by-client/{clientId}
        [HttpGet("by-client/{clientId}")]
        public async Task<ActionResult<Doctor>> GetByClientId(string clientId)
        {
            var doctor = await _doctorService.GetDoctorByClientIdAsync(clientId);
            if (doctor == null)
                return NotFound();

            return Ok(doctor);
        }

        // PUT: api/doctors/by-client/{clientId}
        [HttpPut("by-client/{clientId}")]
        public async Task<IActionResult> UpdateDoctorByClientId(
            string clientId,
            [FromBody] Doctor updatedDoctor)
        {
            if (updatedDoctor == null)
                return BadRequest("updatedDoctor cannot be null");

            var doctor = await _doctorService.GetDoctorByClientIdAsync(clientId);
            if (doctor == null)
                return NotFound();

            // keep Mongo Id
            updatedDoctor.Id = doctor.Id;

            await _doctorService.UpdateAsync(doctor.Id, updatedDoctor);
            return NoContent();
        }
    }
}
