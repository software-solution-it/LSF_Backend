using LSF.Data;
using LSF.Models;
using Microsoft.AspNetCore.Mvc;

namespace LSF.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PointController : ControllerBase
    {
        private readonly APIDbContext _dbContext;

        public PointController(APIDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public IEnumerable<Point> Get()
        {
            return _dbContext.Point.ToList();
        }

        [HttpGet("{id}")]
        public ActionResult<Point> Get(int id)
        {
            var point = _dbContext.Point.FirstOrDefault(t => t.Id == id);

            if (point == null)
            {
                return BadRequest();
            }

            return point;
        }

        [HttpPost]
        public async Task<IActionResult> PostTecnico(PointModel point)
        {
            if (point == null)
            {
                return BadRequest("Dados do Point inválidos");
            }

            var newpoint = new Point
            {
                Width = point.Width,
                Length = point.Length,
            };

            try
            {
                _dbContext.Point.Add(newpoint);
                await _dbContext.SaveChangesAsync();

                return Ok(newpoint);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno do servidor: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, Point point)
        {
            var pointExist = await _dbContext.Point.FindAsync(id);
            if (pointExist == null)
            {
                return NotFound("Point não encontrado");
            }

            pointExist.Width = point.Width ?? pointExist.Width;
            pointExist.Length = point.Length ?? pointExist.Length;

            _dbContext.Point.Update(pointExist);
            await _dbContext.SaveChangesAsync();
            return Ok(pointExist);
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var point = _dbContext.Point.Find(id);
            if (point == null)
            {
                return NotFound("Point não encontrado");
            }

            try
            {
                _dbContext.Point.Remove(point);
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