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
    public class MaquinaLavarController : ControllerBase
    {
        private readonly APIDbContext _dbContext;

        public MaquinaLavarController(APIDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // GET: api/maquinalavar
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MaquinaLavar>>> Get()
        {
            return await _dbContext.MaquinaLavar.ToListAsync();
        }

        // GET: api/maquinalavar/5
        [HttpGet("{id}")]
        public async Task<ActionResult<MaquinaLavar>> Get(int id)
        {
            var maquinaLavar = await _dbContext.MaquinaLavar.FindAsync(id);

            if (maquinaLavar == null)
            {
                return NotFound();
            }

            return maquinaLavar;
        }

        // POST: api/maquinalavar
        [HttpPost]
        public async Task<ActionResult<MaquinaLavar>> Post(MaquinaLavar maquinaLavar)
        {
            _dbContext.MaquinaLavar.Add(maquinaLavar);
            await _dbContext.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { id = maquinaLavar.Id }, maquinaLavar);
        }

        // PUT: api/maquinalavar/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, MaquinaLavar maquinaLavar)
        {
            if (id != maquinaLavar.Id)
            {
                return BadRequest();
            }

            _dbContext.Entry(maquinaLavar).State = EntityState.Modified;

            try
            {
                await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MaquinaLavarExists(id))
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

        // DELETE: api/maquinalavar/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var maquinaLavar = await _dbContext.MaquinaLavar.FindAsync(id);
            if (maquinaLavar == null)
            {
                return NotFound();
            }

            _dbContext.MaquinaLavar.Remove(maquinaLavar);
            await _dbContext.SaveChangesAsync();

            return NoContent();
        }

        private bool MaquinaLavarExists(int id)
        {
            return _dbContext.MaquinaLavar.Any(e => e.Id == id);
        }
    }
}
