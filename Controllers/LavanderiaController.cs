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
    public class LavanderiaController : ControllerBase
    {
        private readonly APIDbContext _dbContext;

        public LavanderiaController(APIDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // GET: api/lavanderia
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Lavanderia>>> Get()
        {
            return await _dbContext.Lavanderia.ToListAsync();
        }

        // GET: api/lavanderia/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Lavanderia>> Get(int id)
        {
            var lavanderia = await _dbContext.Lavanderia.FindAsync(id);

            if (lavanderia == null)
            {
                return NotFound();
            }

            return lavanderia;
        }


        [HttpPost]
        public async Task<ActionResult<Lavanderia>> Post(Lavanderia lavanderia)
        {
            _dbContext.Lavanderia.Add(lavanderia);
            await _dbContext.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { id = lavanderia.Id }, lavanderia);
        }

        // PUT: api/lavanderia/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, Lavanderia lavanderia)
        {
            if (id != lavanderia.Id)
            {
                return BadRequest();
            }

            _dbContext.Entry(lavanderia).State = EntityState.Modified;

            try
            {
                await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LavanderiaExists(id))
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

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var lavanderia = await _dbContext.Lavanderia.FindAsync(id);
            if (lavanderia == null)
            {
                return NotFound();
            }

            _dbContext.Lavanderia.Remove(lavanderia);
            await _dbContext.SaveChangesAsync();

            return NoContent();
        }

        private bool LavanderiaExists(int id)
        {
            return _dbContext.Lavanderia.Any(e => e.Id == id);
        }
    }
}
