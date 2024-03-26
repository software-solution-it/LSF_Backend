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
    public class GeolocalizacaoController : ControllerBase
    {
        private readonly APIDbContext _dbContext;

        public GeolocalizacaoController(APIDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // GET: api/geolocalizacao
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Geolocalizacao>>> Get()
        {
            return await _dbContext.Geolocalizacao.ToListAsync();
        }

        // GET: api/geolocalizacao/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Geolocalizacao>> Get(int id)
        {
            var geolocalizacao = await _dbContext.Geolocalizacao.FindAsync(id);

            if (geolocalizacao == null)
            {
                return NotFound();
            }

            return geolocalizacao;
        }

        // POST: api/geolocalizacao
        [HttpPost]
        public async Task<ActionResult<Geolocalizacao>> Post(string altitude, string longitude)
        {

            var newGeo = new Geolocalizacao()
            {
                Altitude = altitude,
                Longitude = longitude
            };

            _dbContext.Geolocalizacao.Add(newGeo);
            await _dbContext.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { id = newGeo.Id }, newGeo);
        }

        // PUT: api/geolocalizacao/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, Geolocalizacao geolocalizacao)
        {
            if (id != geolocalizacao.Id)
            {
                return BadRequest();
            }

            _dbContext.Entry(geolocalizacao).State = EntityState.Modified;

            try
            {
                await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!GeolocalizacaoExists(id))
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

        // DELETE: api/geolocalizacao/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var geolocalizacao = await _dbContext.Geolocalizacao.FindAsync(id);
            if (geolocalizacao == null)
            {
                return NotFound();
            }

            _dbContext.Geolocalizacao.Remove(geolocalizacao);
            await _dbContext.SaveChangesAsync();

            return NoContent();
        }

        private bool GeolocalizacaoExists(int id)
        {
            return _dbContext.Geolocalizacao.Any(g => g.Id == id);
        }
    }
}
