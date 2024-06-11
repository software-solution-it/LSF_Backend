using LSF.Data;
using LSF.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LSF.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
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
        public async Task<IActionResult> PostPoint(PointModel point, int projectId)
        {
            if (point == null)
            {
                return BadRequest("Dados do Point inválidos");
            }

            var userId = Int32.Parse(User.Claims.FirstOrDefault(c => c.Type == "userId")?.Value);

            var newpoint = new Point
            {
                Width = point.Width,
                Length = point.Length,
            };

            try
            {

                var project = await _dbContext.Project
                              .FirstOrDefaultAsync(p => p.Id == projectId && p.userId == userId);

                if (project == null)
                {
                    return Unauthorized("O usuário não está associado a este projeto.");
                }

                var result = await _dbContext.Point.AddAsync(newpoint);
                await _dbContext.SaveChangesAsync();

                var userPoint = new ProjectPoint
                {
                    ProjectId = projectId,
                    PointId = newpoint?.Id
                };

                await _dbContext.Project_Point.AddAsync(userPoint);
                await _dbContext.SaveChangesAsync();

                return Ok(userPoint);
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