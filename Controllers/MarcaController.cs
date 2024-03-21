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
    public class MarcaController : ControllerBase
    {
        private readonly APIDbContext _dbContext;

        public MarcaController(APIDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // GET: api/marca
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Marca>>> Get()
        {
            return await _dbContext.Marca.ToListAsync();
        }

        // GET: api/marca/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Marca>> Get(int id)
        {
            var marca = await _dbContext.Marca.FindAsync(id);

            if (marca == null)
            {
                return NotFound();
            }

            return marca;
        }

        // POST: api/marca
        [HttpPost]
        public async Task<ActionResult<Marca>> Post(Marca marca)
        {
            _dbContext.Marca.Add(marca);
            await _dbContext.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { id = marca.Id }, marca);
        }

        // PUT: api/marca/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, Marca marca)
        {
            if (id != marca.Id)
            {
                return BadRequest();
            }

            _dbContext.Entry(marca).State = EntityState.Modified;

            try
            {
                await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MarcaExists(id))
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

        // DELETE: api/marca/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var marca = await _dbContext.Marca.FindAsync(id);
            if (marca == null)
            {
                return NotFound();
            }

            _dbContext.Marca.Remove(marca);
            await _dbContext.SaveChangesAsync();

            return NoContent();
        }

        private bool MarcaExists(int id)
        {
            return _dbContext.Marca.Any(e => e.Id == id);
        }
    }
}
