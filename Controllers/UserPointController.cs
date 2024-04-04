using LSF.Data;
using LSF.Models;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LSF.Controllers
{
    [Route("api/[controller]")]
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
        public IEnumerable<UserPoint> Get()
        {
            return _dbContext.User_Point.ToList();
        }

        [HttpGet("{id}")]
        public ActionResult<UserPoint> Get(int id)
        {
            var userGeo = _dbContext.User_Point.FirstOrDefault(t => t.Id == id);

            if (userGeo == null)
            {
                return BadRequest();
            }

            return userGeo;
        }

        [HttpPost]
        public async Task<IActionResult> CreateUserPoint(UserPointModel model)
        {
            var user = await _dbContext.Users.FindAsync(model.UserId);
            if (user == null)
            {
                return NotFound("Usuário não encontrado.");
            }

            var geolocation = await _dbContext.Geolocation.FindAsync(model.PointId);
            if (geolocation == null)
            {
                return NotFound("Geolocalização não encontrada.");
            }

            var UserPoint = new UserPoint
            {
                UserId = model.UserId,
                PointId = model.PointId
            };

            _dbContext.User_Point.Add(UserPoint);
            await _dbContext.SaveChangesAsync();

            return Ok("Relacionamento usuário-geolocalização criado com sucesso.");
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, UserPointModel model)
        {
            var userGeo = await _dbContext.User_Point.FindAsync(id);
            if (userGeo == null)
            {
                return NotFound("Relacionamento usuário-geolocalização não encontrado.");
            }

            userGeo.UserId = model.UserId;
            userGeo.PointId = model.PointId;

            _dbContext.User_Point.Update(userGeo);
            await _dbContext.SaveChangesAsync();

            return Ok("Relacionamento usuário-geolocalização atualizado com sucesso.");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userGeo = await _dbContext.User_Point.FindAsync(id);
            if (userGeo == null)
            {
                return NotFound("Relacionamento usuário-geolocalização não encontrado.");
            }

            _dbContext.User_Point.Remove(userGeo);
            await _dbContext.SaveChangesAsync();

            return Ok("Relacionamento usuário-geolocalização excluído com sucesso.");
        }
    }
}
