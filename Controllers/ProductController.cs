using LSF.Data;
using LSF.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LSF.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly APIDbContext _dbContext;

        public ProductController(APIDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet("GetAll")]
        public IEnumerable<ProductDomain> GetAll()
        {
            return _dbContext.Product_Domain.ToList();
        }

    }
}