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
    public class MarcaEmpresaController : ControllerBase
    {
        private readonly APIDbContext _dbContext;

        public MarcaEmpresaController(APIDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // GET: api/marcaempresa
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MarcaEmpresa>>> Get()
        {
            return await _dbContext.MarcaEmpresa.ToListAsync();
        }

        // GET: api/marcaempresa/5
        [HttpGet("{id}")]
        public async Task<ActionResult<MarcaEmpresa>> Get(int id)
        {
            var marcaEmpresa = await _dbContext.MarcaEmpresa.FindAsync(id);

            if (marcaEmpresa == null)
            {
                return NotFound();
            }

            return marcaEmpresa;
        }

        // POST: api/marcaempresa
        [HttpPost]
        public async Task<ActionResult<MarcaEmpresa>> Post(MarcaEmpresa marcaEmpresa)
        {
            _dbContext.MarcaEmpresa.Add(marcaEmpresa);
            await _dbContext.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { id = marcaEmpresa.Id }, marcaEmpresa);
        }

        // PUT: api/marcaempresa/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, MarcaEmpresa marcaEmpresa)
        {
            if (id != marcaEmpresa.Id)
            {
                return BadRequest();
            }

            _dbContext.Entry(marcaEmpresa).State = EntityState.Modified;

            try
            {
                await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MarcaEmpresaExists(id))
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

        // DELETE: api/marcaempresa/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var marcaEmpresa = await _dbContext.MarcaEmpresa.FindAsync(id);
            if (marcaEmpresa == null)
            {
                return NotFound();
            }

            _dbContext.MarcaEmpresa.Remove(marcaEmpresa);
            await _dbContext.SaveChangesAsync();

            return NoContent();
        }

        private bool MarcaEmpresaExists(int id)
        {
            return _dbContext.MarcaEmpresa.Any(e => e.Id == id);
        }
    }
}
