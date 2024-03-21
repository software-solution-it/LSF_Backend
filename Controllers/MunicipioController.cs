using LSF.Data;
using LSF.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace YourNamespace.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MunicipioController : ControllerBase
    {
        private readonly APIDbContext _dbContext;

        public MunicipioController(APIDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // GET: api/municipio
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Municipio>>> Get()
        {
            return await _dbContext.Municipio.ToListAsync();
        }

        // GET: api/municipio/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Municipio>> Get(int id)
        {
            var municipio = await _dbContext.Municipio.FindAsync(id);

            if (municipio == null)
            {
                return NotFound();
            }

            return municipio;
        }

        // POST: api/municipio
        [HttpPost]
        public async Task<ActionResult<Municipio>> Post(Municipio municipio)
        {
            _dbContext.Municipio.Add(municipio);
            await _dbContext.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { id = municipio.Id }, municipio);
        }

        // PUT: api/municipio/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, Municipio municipio)
        {
            if (id != municipio.Id)
            {
                return BadRequest();
            }

            _dbContext.Entry(municipio).State = EntityState.Modified;

            try
            {
                await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MunicipioExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/municipio/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var municipio = await _dbContext.Municipio.FindAsync(id);
            if (municipio == null)
            {
                return NotFound();
            }

            _dbContext.Municipio.Remove(municipio);
            await _dbContext.SaveChangesAsync();

            return NoContent();
        }

        private bool MunicipioExists(int id)
        {
            return _dbContext.Municipio.Any(e => e.Id == id);
        }
    }
}
