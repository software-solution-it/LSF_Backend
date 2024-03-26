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
    public class EmpresaController : ControllerBase
    {
        private readonly APIDbContext _dbContext;

        public EmpresaController(APIDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // GET: api/empresa
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Empresa>>> Get()
        {
            return await _dbContext.Empresa.ToListAsync();
        }

        // GET: api/empresa/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Empresa>> Get(int id)
        {
            var empresa = await _dbContext.Empresa.FindAsync(id);

            if (empresa == null)
            {
                return NotFound();
            }

            return empresa;
        }

        // POST: api/empresa
        [HttpPost]
        public async Task<ActionResult<Empresa>> Post(Empresa empresa)
        {
            _dbContext.Empresa.Add(empresa);
            await _dbContext.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { id = empresa.Id }, empresa);
        }

        // PUT: api/empresa/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, Empresa empresa)
        {
            if (id != empresa.Id)
            {
                return BadRequest();
            }

            _dbContext.Entry(empresa).State = EntityState.Modified;

            try
            {
                await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EmpresaExists(id))
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

        // DELETE: api/empresa/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var empresa = await _dbContext.Empresa.FindAsync(id);
            if (empresa == null)
            {
                return NotFound();
            }

            _dbContext.Empresa.Remove(empresa);
            await _dbContext.SaveChangesAsync();

            return NoContent();
        }

        private bool EmpresaExists(int id)
        {
            return _dbContext.Empresa.Any(e => e.Id == id);
        }
    }
}
