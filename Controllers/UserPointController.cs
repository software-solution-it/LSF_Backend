using LSF.Data;
using LSF.Models;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LSF.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class UserPointController : ControllerBase
    {
        private readonly APIDbContext _dbContext;

        public UserPointController(APIDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public IEnumerable<ProjectPoint> Get()
        {
            return _dbContext.Project_Point.ToList();
        }

        [HttpGet("{id}")]
        public ActionResult<ProjectPoint> Get(int id)
        {
            var userGeo = _dbContext.Project_Point.FirstOrDefault(t => t.Id == id);

            if (userGeo == null)
            {
                return BadRequest();
            }

            return userGeo;
        }

        [HttpPost]
        public async Task<IActionResult> CreateUserPoint(ProjectPointModel model, int projectId)
        {
            var user = await _dbContext.Users.FindAsync(model.ProjectId);
            if (user == null)
            {
                return NotFound("Usuário não encontrado.");
            }

            var geolocation = await _dbContext.Geolocation.FindAsync(model.PointId);
            if (geolocation == null)
            {
                return NotFound("Geolocalização não encontrada.");
            }

            var UserPoint = new ProjectPoint
            {
                ProjectId = model.ProjectId,
                PointId = model.PointId
            };

            _dbContext.Project_Point.Add(UserPoint);
            await _dbContext.SaveChangesAsync();

            return Ok("Relacionamento usuário-geolocalização criado com sucesso.");
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, ProjectPointModel model, int projectId)
        {
            var userGeo = await _dbContext.Project_Point.FindAsync(id);
            if (userGeo == null)
            {
                return NotFound("Relacionamento usuário-geolocalização não encontrado.");
            }

            userGeo.ProjectId = model.ProjectId;
            userGeo.PointId = model.PointId;

            _dbContext.Project_Point.Update(userGeo);
            await _dbContext.SaveChangesAsync();

            return Ok("Relacionamento usuário-geolocalização atualizado com sucesso.");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userGeo = await _dbContext.Project_Point.FindAsync(id);
            if (userGeo == null)
            {
                return NotFound("Relacionamento usuário-geolocalização não encontrado.");
            }

            _dbContext.Project_Point.Remove(userGeo);
            await _dbContext.SaveChangesAsync();

            return Ok("Relacionamento usuário-geolocalização excluído com sucesso.");
        }
    }
}
