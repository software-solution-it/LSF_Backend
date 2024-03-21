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
    public class MaquinaController : ControllerBase
    {
        private readonly APIDbContext _dbContext;

        public MaquinaController(APIDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // GET: api/maquina
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Maquina>>> Get()
        {
            return await _dbContext.Maquina.ToListAsync();
        }

        // GET: api/maquina/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Maquina>> Get(int id)
        {
            var maquina = await _dbContext.Maquina.FindAsync(id);

            if (maquina == null)
            {
                return NotFound();
            }

            return maquina;
        }

        // POST: api/maquina
        [HttpPost]
        public async Task<ActionResult<Maquina>> Post(Maquina maquina)
        {
            _dbContext.Maquina.Add(maquina);
            await _dbContext.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { id = maquina.Id }, maquina);
        }

        // PUT: api/maquina/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, Maquina maquina)
        {
            if (id != maquina.Id)
            {
                return BadRequest();
            }

            _dbContext.Entry(maquina).State = EntityState.Modified;

            try
            {
                await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MaquinaExists(id))
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

        // DELETE: api/maquina/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var maquina = await _dbContext.Maquina.FindAsync(id);
            if (maquina == null)
            {
                return NotFound();
            }

            _dbContext.Maquina.Remove(maquina);
            await _dbContext.SaveChangesAsync();

            return NoContent();
        }

        private bool MaquinaExists(int id)
        {
            return _dbContext.Maquina.Any(e => e.Id == id);
        }
    }
}
