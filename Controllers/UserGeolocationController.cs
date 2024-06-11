using LSF.Data;
using LSF.Models;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LSF.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class UserGeolocationController : ControllerBase
    {
        private readonly APIDbContext _dbContext;

        public UserGeolocationController(APIDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public IEnumerable<ProjectGeolocation> Get()
        {
            return _dbContext.Project_Geolocation.ToList();
        }

        [HttpGet("{id}")]
        public ActionResult<ProjectGeolocation> Get(int id)
        {
            var userGeo = _dbContext.Project_Geolocation.FirstOrDefault(t => t.Id == id);

            if (userGeo == null)
            {
                return BadRequest();
            }

            return userGeo;
        }

        [HttpPost]
        public async Task<IActionResult> CreateUserGeolocation(ProjectGeolocationModel model)
        {
            var user = await _dbContext.Users.FindAsync(model.ProjectId);
            if (user == null)
            {
                return NotFound("Usuário não encontrado.");
            }

            var geolocation = await _dbContext.Geolocation.FindAsync(model.GeolocationId);
            if (geolocation == null)
            {
                return NotFound("Geolocalização não encontrada.");
            }

            var userGeolocation = new ProjectGeolocation
            {
                ProjectId = model.ProjectId,
                GeolocationId = model.GeolocationId
            };

            _dbContext.Project_Geolocation.Add(userGeolocation);
            await _dbContext.SaveChangesAsync();

            return Ok("Relacionamento usuário-geolocalização criado com sucesso.");
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, ProjectGeolocationModel model, int projectId)
        {
            var userGeo = await _dbContext.Project_Geolocation.FindAsync(id);
            if (userGeo == null)
            {
                return NotFound("Relacionamento usuário-geolocalização não encontrado.");
            }

            userGeo.ProjectId = model.ProjectId;
            userGeo.GeolocationId = model.GeolocationId;

            _dbContext.Project_Geolocation.Update(userGeo);
            await _dbContext.SaveChangesAsync();

            return Ok("Relacionamento usuário-geolocalização atualizado com sucesso.");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userGeo = await _dbContext.Project_Geolocation.FindAsync(id);
            if (userGeo == null)
            {
                return NotFound("Relacionamento usuário-geolocalização não encontrado.");
            }

            _dbContext.Project_Geolocation.Remove(userGeo);
            await _dbContext.SaveChangesAsync();

            return Ok("Relacionamento usuário-geolocalização excluído com sucesso.");
        }
    }
}
