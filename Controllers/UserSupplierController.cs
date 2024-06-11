using LSF.Data;
using LSF.Models;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LSF.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class UserSupplierController : ControllerBase
    {
        private readonly APIDbContext _dbContext;

        public UserSupplierController(APIDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // GET: api/<UserSupplierController>
        [HttpGet]
        public IEnumerable<ProjectSupplier> Get()
        {
            return _dbContext.Project_Supplier.ToList();
        }

        // GET api/<UserSupplierController>/5
        [HttpGet("{id}")]
        public ActionResult<ProjectSupplier> Get(int id)
        {
            var userGeo = _dbContext.Project_Supplier.FirstOrDefault(t => t.Id == id);

            if (userGeo == null)
            {
                return BadRequest();
            }

            return userGeo;
        }

        [HttpPost]
        public async Task<IActionResult> CreateUserSupplier(ProjectSupplierModel model)
        {
            var user = await _dbContext.Users.FindAsync(model.ProjectId);
            if (user == null)
            {
                return NotFound("Usuário não encontrado.");
            }

            var geolocation = await _dbContext.Geolocation.FindAsync(model.SupplierId);
            if (geolocation == null)
            {
                return NotFound("Supplier não encontrada.");
            }

            var UserSupplier = new ProjectSupplier
            {
                ProjectId = model.ProjectId,
                SupplierId = model.SupplierId
            };

            _dbContext.Project_Supplier.Add(UserSupplier);
            await _dbContext.SaveChangesAsync();

            return Ok("Relacionamento usuário-supplier criado com sucesso.");
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, ProjectSupplierModel model)
        {
            var userGeo = await _dbContext.Project_Supplier.FindAsync(id);
            if (userGeo == null)
            {
                return NotFound("Relacionamento usuário-supplier não encontrado.");
            }

            userGeo.ProjectId = model.ProjectId;
            userGeo.SupplierId = model.SupplierId;

            _dbContext.Project_Supplier.Update(userGeo);
            await _dbContext.SaveChangesAsync();

            return Ok("Relacionamento usuário-supplier atualizado com sucesso.");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userGeo = await _dbContext.Project_Supplier.FindAsync(id);
            if (userGeo == null)
            {
                return NotFound("Relacionamento usuário-supplier não encontrado.");
            }

            _dbContext.Project_Supplier.Remove(userGeo);
            await _dbContext.SaveChangesAsync();

            return Ok("Relacionamento usuário-supplier excluído com sucesso.");
        }
    }
}
