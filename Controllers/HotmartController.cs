using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LSF.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class HotmartController : ControllerBase
    {
        // GET: api/<Hotmart>
        [HttpGet("/hotmart/purchase")]
        public IActionResult Get(Purchase objeto)
        {
            return Ok(objeto);
        }

        // GET api/<Hotmart>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<Hotmart>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<Hotmart>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<Hotmart>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
