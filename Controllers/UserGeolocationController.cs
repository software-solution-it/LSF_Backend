using LSF.Data;
using LSF.Models;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LSF.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserGeolocationController : ControllerBase
    {
        private readonly APIDbContext _dbContext;

        public UserGeolocationController(APIDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // GET: api/<UserGeolocationController>
        [HttpGet]
        public IEnumerable<UserGeolocation> Get()
        {
            return _dbContext.User_Geolocation.ToList();
        }

        // GET api/<UserGeolocationController>/5
        [HttpGet("{id}")]
        public ActionResult<UserGeolocation> Get(int id)
        {
            var userGeo = _dbContext.User_Geolocation.FirstOrDefault(t => t.Id == id);

            if (userGeo == null)
            {
                return BadRequest();
            }

            return userGeo;
        }

        [HttpPost]
        public async Task<IActionResult> CreateUserGeolocation(UserGeolocationModel model)
        {
            var user = await _dbContext.Users.FindAsync(model.UserId);
            if (user == null)
            {
                return NotFound("Usuário não encontrado.");
            }

            var geolocation = await _dbContext.Geolocation.FindAsync(model.GeolocationId);
            if (geolocation == null)
            {
                return NotFound("Geolocalização não encontrada.");
            }

            var userGeolocation = new UserGeolocation
            {
                UserId = model.UserId,
                GeolocationId = model.GeolocationId
            };

            _dbContext.User_Geolocation.Add(userGeolocation);
            await _dbContext.SaveChangesAsync();

            return Ok("Relacionamento usuário-geolocalização criado com sucesso.");
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, UserGeolocationModel model)
        {
            var userGeo = await _dbContext.User_Geolocation.FindAsync(id);
            if (userGeo == null)
            {
                return NotFound("Relacionamento usuário-geolocalização não encontrado.");
            }

            userGeo.UserId = model.UserId;
            userGeo.GeolocationId = model.GeolocationId;

            _dbContext.User_Geolocation.Update(userGeo);
            await _dbContext.SaveChangesAsync();

            return Ok("Relacionamento usuário-geolocalização atualizado com sucesso.");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userGeo = await _dbContext.User_Geolocation.FindAsync(id);
            if (userGeo == null)
            {
                return NotFound("Relacionamento usuário-geolocalização não encontrado.");
            }

            _dbContext.User_Geolocation.Remove(userGeo);
            await _dbContext.SaveChangesAsync();

            return Ok("Relacionamento usuário-geolocalização excluído com sucesso.");
        }
    }
}
