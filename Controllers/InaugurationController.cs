using LSF.Data;
using LSF.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LSF.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class InaugurationController : ControllerBase
    {
        private readonly APIDbContext _dbContext;

        public InaugurationController(APIDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet("GetAll")]
        public IEnumerable<Inauguration> GetAll()
        {
            return _dbContext.Inauguration.ToList();
        }

        [HttpGet("{id}")]
        public ActionResult<Inauguration> Get(int id)
        {
            var Inauguration = _dbContext.Inauguration.FirstOrDefault(t => t.Id == id);

            if (Inauguration == null) return BadRequest();

            return Inauguration;
        }

        [HttpPost]
        public async Task<IActionResult> PostInauguration(string name)
        {
            if (name == null) return BadRequest("Dados do Inauguration inválidos");
    
            var userId = Int32.Parse(User.Claims.FirstOrDefault(c => c.Type == "userId")?.Value);

            var newInauguration = new Inauguration
            {
                Name = name,
            };

            try
            {
                var result = await _dbContext.Inauguration.AddAsync(newInauguration);
                await _dbContext.SaveChangesAsync();

                var userInauguration = new UserInauguration
                {
                    UserId = userId,
                    InaugurationId = newInauguration?.Id
                };

                await _dbContext.User_Inauguration.AddAsync(userInauguration);
                await _dbContext.SaveChangesAsync();

                return Ok(userInauguration);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno do servidor: {ex.Message}");
            }
        }

    }
}