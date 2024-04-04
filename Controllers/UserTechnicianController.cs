using LSF.Data;
using LSF.Models;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LSF.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserTechnicianController : ControllerBase
    {
        private readonly APIDbContext _dbContext;

        public UserTechnicianController(APIDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public IEnumerable<UserTechnician> Get()
        {
            return _dbContext.User_Technician.ToList();
        }

        [HttpGet("{id}")]
        public ActionResult<UserTechnician> Get(int id)
        {
            var userGeo = _dbContext.User_Technician.FirstOrDefault(t => t.Id == id);

            if (userGeo == null)
            {
                return BadRequest();
            }

            return userGeo;
        }

        [HttpPost]
        public async Task<IActionResult> CreateUserTechnician(UserTechnicianModel model)
        {
            var user = await _dbContext.Users.FindAsync(model.UserId);
            if (user == null)
            {
                return NotFound("Usuário não encontrado.");
            }

            var geolocation = await _dbContext.Geolocation.FindAsync(model.TechnicianId);
            if (geolocation == null)
            {
                return NotFound("Technician não encontrada.");
            }

            var UserTechnician = new UserTechnician
            {
                UserId = model.UserId,
                TechnicianId = model.TechnicianId
            };

            _dbContext.User_Technician.Add(UserTechnician);
            await _dbContext.SaveChangesAsync();

            return Ok("Relacionamento usuário-Technician criado com sucesso.");
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, UserTechnicianModel model)
        {
            var userGeo = await _dbContext.User_Technician.FindAsync(id);
            if (userGeo == null)
            {
                return NotFound("Relacionamento usuário-Technician não encontrado.");
            }

            userGeo.UserId = model.UserId;
            userGeo.TechnicianId = model.TechnicianId;

            _dbContext.User_Technician.Update(userGeo);
            await _dbContext.SaveChangesAsync();

            return Ok("Relacionamento usuário-Technician atualizado com sucesso.");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userGeo = await _dbContext.User_Technician.FindAsync(id);
            if (userGeo == null)
            {
                return NotFound("Relacionamento usuário-Technician não encontrado.");
            }

            _dbContext.User_Technician.Remove(userGeo);
            await _dbContext.SaveChangesAsync();

            return Ok("Relacionamento usuário-Technician excluído com sucesso.");
        }
    }
}
