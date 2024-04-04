using LSF.Data;
using LSF.Models;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LSF.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserSupplierController : ControllerBase
    {
        private readonly APIDbContext _dbContext;

        public UserSupplierController(APIDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // GET: api/<UserSupplierController>
        [HttpGet]
        public IEnumerable<UserSupplier> Get()
        {
            return _dbContext.User_Supplier.ToList();
        }

        // GET api/<UserSupplierController>/5
        [HttpGet("{id}")]
        public ActionResult<UserSupplier> Get(int id)
        {
            var userGeo = _dbContext.User_Supplier.FirstOrDefault(t => t.Id == id);

            if (userGeo == null)
            {
                return BadRequest();
            }

            return userGeo;
        }

        [HttpPost]
        public async Task<IActionResult> CreateUserSupplier(UserSupplierModel model)
        {
            var user = await _dbContext.Users.FindAsync(model.UserId);
            if (user == null)
            {
                return NotFound("Usuário não encontrado.");
            }

            var geolocation = await _dbContext.Geolocation.FindAsync(model.SupplierId);
            if (geolocation == null)
            {
                return NotFound("Supplier não encontrada.");
            }

            var UserSupplier = new UserSupplier
            {
                UserId = model.UserId,
                SupplierId = model.SupplierId
            };

            _dbContext.User_Supplier.Add(UserSupplier);
            await _dbContext.SaveChangesAsync();

            return Ok("Relacionamento usuário-supplier criado com sucesso.");
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, UserSupplierModel model)
        {
            var userGeo = await _dbContext.User_Supplier.FindAsync(id);
            if (userGeo == null)
            {
                return NotFound("Relacionamento usuário-supplier não encontrado.");
            }

            userGeo.UserId = model.UserId;
            userGeo.SupplierId = model.SupplierId;

            _dbContext.User_Supplier.Update(userGeo);
            await _dbContext.SaveChangesAsync();

            return Ok("Relacionamento usuário-supplier atualizado com sucesso.");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userGeo = await _dbContext.User_Supplier.FindAsync(id);
            if (userGeo == null)
            {
                return NotFound("Relacionamento usuário-supplier não encontrado.");
            }

            _dbContext.User_Supplier.Remove(userGeo);
            await _dbContext.SaveChangesAsync();

            return Ok("Relacionamento usuário-supplier excluído com sucesso.");
        }
    }
}
