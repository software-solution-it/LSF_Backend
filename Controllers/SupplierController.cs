using LSF.Data;
using LSF.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LSF.Controllers
{
    [Route("api/[controller]")]
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

        [HttpPost]
        public async Task<IActionResult> PostSupplier(SupplierModel supp)
        {
            var userId = Int32.Parse(User.Claims.FirstOrDefault(c => c.Type == "userId")?.Value);

            var newsupp = new Supplier
            {
                City = supp.City,
                SupplierName = supp.SupplierName,
                SupplierResponsible = supp.SupplierResponsible,
                Phone = supp.Phone
            };

            try
            {

                var result = await _dbContext.Supplier.AddAsync(newsupp);
                await _dbContext.SaveChangesAsync();

                var userSupplier = new UserSupplier
                {
                    UserId = userId,
                    SupplierId = newsupp?.Id
                };

                await _dbContext.User_Supplier.AddAsync(userSupplier);
                await _dbContext.SaveChangesAsync();

                return Ok(userSupplier);
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
            suppExist.SupplierResponsible = supp.SupplierResponsible ?? suppExist.SupplierResponsible;
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