using LSF.Data;
using LSF.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

namespace LSF.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class SupplierController : ControllerBase
    {
        private readonly APIDbContext _dbContext;

        public SupplierController(APIDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public IEnumerable<Supplier> Get()
        {
            return _dbContext.Supplier.ToList();
        }

        [HttpGet("{id}")]
        public ActionResult<Supplier> Get(int id)
        {
            var supp = _dbContext.Supplier.FirstOrDefault(t => t.Id == id);

            if (supp == null)
            {
                return BadRequest();
            }

            return supp;
        }

        [HttpGet("SupplierType")]
        public ActionResult<IEnumerable<Supplier>> SupplierDomain(int supplierType)
        {
            var results = _dbContext.Supplier
                .Include(s => s.SupplierDomain)
                .Where(s => s.SupplierType == supplierType)
                .ToList();

            if (results.Count == 0) return BadRequest();

            return Ok(results);
        }

        [HttpGet("SupplierProducts")]
        public ActionResult<IEnumerable<Supplier>> SupplierProducts(int supplierType)
        {
            try
            {
                var results = _dbContext.Product_Domain
                    .Include(s => s.SupplierDomain)
                    .Where(s => s.SupplierType == supplierType)
                    .ToList();

                if (results.Count == 0) return BadRequest();

                return Ok(results);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno do servidor: {ex.Message}");
            }
        }


        [HttpPost]
        public async Task<IActionResult> PostSupplier(SupplierModel supp, int projectId)
        {
            var userId = Int32.Parse(User.Claims.FirstOrDefault(c => c.Type == "userId")?.Value);

            var newsupp = new Supplier
            {
                City = supp.City,
                SupplierName = supp.SupplierName,
                Phone = supp.Phone,
                SupplierType = supp.SupplierType
            };

            try
            {

                //var result = await _dbContext.Supplier.AddAsync(newsupp);
                //await _dbContext.SaveChangesAsync();

                var projectSupplier = new ProjectSupplier
                {
                    SupplierId = newsupp?.Id
                };

                await _dbContext.Project_Supplier.AddAsync(projectSupplier);
                await _dbContext.SaveChangesAsync();

                return Ok(projectSupplier);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno do servidor: {ex.Message}");
            }
        }

        [HttpPost("UserSupplier")]
        public async Task<IActionResult> UserSupplier(int supplierId, int projectId)
        {
            var userId = Int32.Parse(User.Claims.FirstOrDefault(c => c.Type == "userId")?.Value);

            try
            {

                var project = await _dbContext.Project
                              .FirstOrDefaultAsync(p => p.Id == projectId && p.userId == userId);

                if (project == null)
                {
                    return Unauthorized("O usuário não está associado a este projeto.");
                }


                var projectSupplier = new ProjectSupplier
                {
                    ProjectId = projectId,
                    SupplierId = supplierId
                };

                await _dbContext.Project_Supplier.AddAsync(projectSupplier);
                await _dbContext.SaveChangesAsync();

                return Ok(projectSupplier);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno do servidor: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, Supplier supp)
        {
            var suppExist = await _dbContext.Supplier.FindAsync(id);
            if (suppExist == null)
            {
                return NotFound("Supplier não encontrado");
            }

            suppExist.City = supp.City ?? suppExist.City;
            suppExist.SupplierName = supp.SupplierName ?? suppExist.SupplierName;
            suppExist.Phone = supp.Phone ?? suppExist.Phone;

            _dbContext.Supplier.Update(suppExist);
            await _dbContext.SaveChangesAsync();
            return Ok(suppExist);
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var supp = _dbContext.Supplier.Find(id);
            if (supp == null)
            {
                return NotFound("Supplier não encontrado");
            }

            try
            {
                _dbContext.Supplier.Remove(supp);
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