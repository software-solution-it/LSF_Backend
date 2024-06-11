using LSF.Data;
using LSF.Models;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LSF.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class UserTechnicianController : ControllerBase
    {
        private readonly APIDbContext _dbContext;

        public UserTechnicianController(APIDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public IEnumerable<ProjectTechnician> Get()
        {
            return _dbContext.Project_Technician.ToList();
        }

        [HttpGet("{id}")]
        public ActionResult<ProjectTechnician> Get(int id)
        {
            var userGeo = _dbContext.Project_Technician.FirstOrDefault(t => t.Id == id);

            if (userGeo == null)
            {
                return BadRequest();
            }

            return userGeo;
        }

        [HttpPost]
        public async Task<IActionResult> CreateUserTechnician(ProjectTechnicianModel model)
        {
            var user = await _dbContext.Users.FindAsync(model.ProjectId);
            if (user == null)
            {
                return NotFound("Usuário não encontrado.");
            }

            var geolocation = await _dbContext.Geolocation.FindAsync(model.TechnicianId);
            if (geolocation == null)
            {
                return NotFound("Technician não encontrada.");
            }

            var UserTechnician = new ProjectTechnician
            {
                ProjectId = model.ProjectId,
                TechnicianId = model.TechnicianId
            };

            _dbContext.Project_Technician.Add(UserTechnician);
            await _dbContext.SaveChangesAsync();

            return Ok("Relacionamento usuário-Technician criado com sucesso.");
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, ProjectTechnicianModel model)
        {
            var userGeo = await _dbContext.Project_Technician.FindAsync(id);
            if (userGeo == null)
            {
                return NotFound("Relacionamento usuário-Technician não encontrado.");
            }

            userGeo.ProjectId = model.ProjectId;
            userGeo.TechnicianId = model.TechnicianId;

            _dbContext.Project_Technician.Update(userGeo);
            await _dbContext.SaveChangesAsync();

            return Ok("Relacionamento usuário-Technician atualizado com sucesso.");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userGeo = await _dbContext.Project_Technician.FindAsync(id);
            if (userGeo == null)
            {
                return NotFound("Relacionamento usuário-Technician não encontrado.");
            }

            _dbContext.Project_Technician.Remove(userGeo);
            await _dbContext.SaveChangesAsync();

            return Ok("Relacionamento usuário-Technician excluído com sucesso.");
        }
    }
}
