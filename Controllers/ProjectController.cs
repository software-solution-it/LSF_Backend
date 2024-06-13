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
    public class ProjectController : ControllerBase
    {
        private readonly APIDbContext _dbContext;

        public ProjectController(APIDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet("GetAll")]
        public IEnumerable<Project> GetAll()
        {
            return _dbContext.Project.ToList();
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var project = await (from p in _dbContext.Project
                                 where p.Id == id
                                 select new
                                 {
                                     p.Id,
                                     p.Name,
                                     Geolocation = (from pg in _dbContext.Project_Geolocation
                                                    join g in _dbContext.Geolocation on pg.GeolocationId equals g.Id
                                                    where pg.ProjectId == p.Id
                                                    select g).FirstOrDefault(),
                                     Point = (from pp in _dbContext.Project_Point
                                              join pt in _dbContext.Point on pp.PointId equals pt.Id
                                              where pp.ProjectId == p.Id
                                              select pt).FirstOrDefault(),
                                     Suppliers = (from ps in _dbContext.Project_Supplier
                                                  join s in _dbContext.Supplier on ps.SupplierId equals s.Id
                                                  where ps.ProjectId == p.Id
                                                  select s).ToList(),
                                     Technician = (from ptc in _dbContext.Project_Technician
                                                   join t in _dbContext.Technician on ptc.TechnicianId equals t.Id
                                                   where ptc.ProjectId == p.Id
                                                   select t).FirstOrDefault(),
                                     Electric = (from e in _dbContext.Project_Electric
                                                 where e.ProjectId == p.Id
                                                 select e).FirstOrDefault(),
                                     ProjectFiles = new
                                     {
                                         ConfirmedReceipt = (from pf in _dbContext.Project_File
                                                             where pf.ProjectId == p.Id
                                                             select pf.ConfirmedReceipt).FirstOrDefault(),
                                         ReceiptDeclinedReason = (from pf in _dbContext.Project_File
                                                                  where pf.ProjectId == p.Id
                                                                  select pf.ReceiptDeclinedReason).FirstOrDefault(),
                                         Recipe = (from pf in _dbContext.Project_File
                                                   join f in _dbContext.FileModel on pf.FileId equals f.Id
                                                   where pf.ProjectId == p.Id && f.FileType == "Recipe"
                                                   select f).FirstOrDefault(),
                                         HydraulicModel = (from pf in _dbContext.Project_File
                                                           join fh in _dbContext.FileModel on pf.FileId equals fh.Id
                                                           where pf.ProjectId == p.Id && fh.FileType == "HydraulicModel"
                                                           select fh).FirstOrDefault(),
                                         ElectricModel = (from pf in _dbContext.Project_File
                                                          join fe in _dbContext.FileModel on pf.FileId equals fe.Id
                                                          where pf.ProjectId == p.Id && fe.FileType == "ElectricModel"
                                                          select fe).FirstOrDefault(),
                                         SketchModel = (from pf in _dbContext.Project_File
                                                        join fs in _dbContext.FileModel on pf.FileId equals fs.Id
                                                        where pf.ProjectId == p.Id && fs.FileType == "SketchModel"
                                                        select fs).FirstOrDefault()
                                     }
                                 }).FirstOrDefaultAsync();

            if (project == null)
            {
                return NotFound();
            }

            return Ok(project);
        }

        [HttpPut("PutProject")]
        [Authorize]
        public async Task<IActionResult> PutProject(int projectId, string name)
        {
            if (projectId <= 0 || string.IsNullOrEmpty(name))
            {
                return BadRequest("ID do projeto ou nome do projeto inválido.");
            }

            try
            {
                // Obtém o ID do usuário autenticado dos claims
                var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "userId");
                if (userIdClaim == null)
                {
                    return BadRequest("Claim 'userId' não encontrada.");
                }

                if (!int.TryParse(userIdClaim.Value, out int userId))
                {
                    return BadRequest("O ID do usuário é inválido.");
                }

                // Obtém o projeto pelo ID e o usuário autenticado
                var project = await _dbContext.Project.FirstOrDefaultAsync(p => p.Id == projectId);
                if (project == null)
                {
                    return NotFound("Projeto não encontrado.");
                }

                var user = await _dbContext.Users.Include(u => u.Projects).FirstOrDefaultAsync(u => u.Id == userId);
                if (user == null)
                {
                    return NotFound("Usuário não encontrado.");
                }

                // Verifica se o usuário tem permissão para modificar o projeto
                if (!user.Projects.Any(p => p.Id == projectId))
                {
                    return Forbid("Você não tem permissão para modificar este projeto.");
                }

                // Atualiza o projeto existente
                project.Name = name;
                _dbContext.Project.Update(project);
                await _dbContext.SaveChangesAsync();

                return Ok(project);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno do servidor: {ex.Message}");
            }
        }


       [HttpPost("PostProject")]
        public async Task<IActionResult> PostProject([FromBody] Project newProject)
            {

            if (newProject == null || string.IsNullOrEmpty(newProject.userId.ToString()))
            {
                return BadRequest("Dados do usuário inválido.");
            }

            try
                {
                    await _dbContext.Project.AddAsync(newProject);
                    await _dbContext.SaveChangesAsync();
                    return Ok(newProject);
                }
                catch (Exception ex)
                {
                    return StatusCode(500, $"Erro interno do servidor: {ex.Message}");
                }
        }
    }
}