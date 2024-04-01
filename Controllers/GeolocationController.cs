using LSF.Data;
using LSF.Models;
using Microsoft.AspNetCore.Mvc;

namespace LSF.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GeolocationController : ControllerBase
    {
        private readonly APIDbContext _dbContext;

        public GeolocationController(APIDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public IEnumerable<Geolocation> Get()
        {
            return _dbContext.Geolocation.ToList();
        }

        [HttpGet("{id}")]
        public ActionResult<Geolocation> Get(int id)
        {
            var geo = _dbContext.Geolocation.FirstOrDefault(t => t.Id == id);

            if (geo == null)
            {
                return BadRequest();
            }

            return geo;
        }

        [HttpPost]
        public async Task<IActionResult> PostTecnico(GeolocationModel geo)
        {
            if (geo == null)
            {
                return BadRequest("Dados do Geolocation inválidos");
            }

            var newgeo = new Geolocation
            {
                Latitude = geo.Latitude,
                Longitude = geo.Longitude,
                Address = geo.Address,
            };

            try
            {
                _dbContext.Geolocation.Add(newgeo);
                await _dbContext.SaveChangesAsync();

                return Ok(newgeo);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno do servidor: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, Geolocation geo)
        {
            var geoExist = await _dbContext.Geolocation.FindAsync(id);
            if (geoExist == null)
            {
                return NotFound("Geolocation não encontrado");
            }

            geoExist.Latitude = geo.Latitude ?? geoExist.Latitude;
            geoExist.Longitude = geo.Longitude ?? geoExist.Longitude;
            geoExist.Address = geo.Address ?? geoExist.Address;

            _dbContext.Geolocation.Update(geoExist);
            await _dbContext.SaveChangesAsync();
            return Ok(geoExist);
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var geo = _dbContext.Geolocation.Find(id);
            if (geo == null)
            {
                return NotFound("Geolocation não encontrado");
            }

            try
            {
                _dbContext.Geolocation.Remove(geo);
                _dbContext.SaveChanges();
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno do servidor: {ex.Message}");
            }
        }

    }
}