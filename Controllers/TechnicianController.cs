using LSF.Data;
using LSF.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LSF.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class TechnicianController : ControllerBase
    {
        private readonly APIDbContext _dbContext;

        public TechnicianController(APIDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public IEnumerable<Technician> Get()
        {
            return _dbContext.Technician.ToList();
        }

        [HttpGet("{id}")]
        public ActionResult<Technician> Get(int id)
        {
            var tech = _dbContext.Technician.FirstOrDefault(t => t.Id == id);

            if(tech == null)
            {
                return BadRequest();
            }

            return tech;
        }

        [HttpGet("Country")]
        public async Task<ActionResult<IEnumerable<Technician>>> GetTechniciansByCountry(string country)
        {
            var technicians = await _dbContext.Technician
                .Where(t => t.Country == country)
                .ToListAsync();

            if (technicians == null || !technicians.Any())
            {
                return NotFound();
            }

            return technicians;
        }

        [HttpPost]
        public async Task<IActionResult> PostTechnician(int techId, int projectId)
        {
            var userId = Int32.Parse(User.Claims.FirstOrDefault(c => c.Type == "userId")?.Value);

            //var newTech = new Technician
            //{
            //    Name = tech.Name,
            //    Email = tech.Email,
            //    Phone = tech.Phone,
            //    City = tech.City,
            //    Country = tech.Country,
            //    Active = (bool) tech.Active
            //};

            try
            {

                //var result = await _dbContext.Technician.AddAsync(newTech);
                //await _dbContext.SaveChangesAsync();

                var project = await _dbContext.Project
                              .FirstOrDefaultAsync(p => p.Id == projectId && p.userId == userId);

                if (project == null)
                {
                    return Unauthorized("O usuário não está associado a este projeto.");
                }

                var userTechnician = new ProjectTechnician
                {
                    ProjectId = projectId,
                    TechnicianId = techId
                };

                await _dbContext.Project_Technician.AddAsync(userTechnician);
                await _dbContext.SaveChangesAsync();

                return Ok(userTechnician);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno do servidor: {ex.Message}");
            }
        }

        [HttpPost("UserTechnician")]
        public async Task<IActionResult> UserTechnician(int technicianId, int projectId)
        {
            var userId = Int32.Parse(User.Claims.FirstOrDefault(c => c.Type == "userId")?.Value);

            try
            {

                var userSupplier = new ProjectTechnician
                {
                    ProjectId = projectId,
                    TechnicianId = technicianId
                };

                await _dbContext.Project_Technician.AddAsync(userSupplier);
                await _dbContext.SaveChangesAsync();

                return Ok(userSupplier);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno do servidor: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, Technician tech)
        {
            var techExist = await _dbContext.Technician.FindAsync(id);
            if (techExist == null)
            {
                return NotFound("Técnico não encontrado");
            }

            techExist.Name = tech.Name ?? techExist.Name;
            techExist.Email = tech.Email ?? techExist.Email;
            techExist.Phone = tech.Phone ?? techExist.Phone;
            techExist.City = tech.City ?? techExist.City;
            techExist.Country = tech.Country ?? techExist.Country;
            techExist.Active = tech.Active ?? techExist.Active;

            _dbContext.Technician.Update(techExist);
            await _dbContext.SaveChangesAsync();
            return Ok(techExist);
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var tecnico = _dbContext.Technician.Find(id);
            if (tecnico == null)
            {
                return NotFound("Técnico não encontrado");
            }

            try
            {
                _dbContext.Technician.Remove(tecnico);
                _dbContext.SaveChanges();
                return NoContent(); 
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno do servidor: {ex.Message}");
            }
        }

    }
}