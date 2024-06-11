using LSF.Data;
using LSF.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LSF.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class BotErrorController : ControllerBase
    {
        private readonly APIDbContext _dbContext;

        public BotErrorController(APIDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet("GetAll")]
        public IEnumerable<BotError> GetAll()
        {
            return _dbContext.BotError.ToList();
        }

        [HttpGet("{error}")]
        public ActionResult<BotError> Get(string error)
        {
            var BotError = _dbContext.BotError.FirstOrDefault(t => t.Visor == error);

            if (BotError == null) return BadRequest();

            return BotError;
        }

    }
}