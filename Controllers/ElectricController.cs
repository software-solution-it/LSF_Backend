using LSF.Data;
using LSF.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace LSF.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ElectricController : ControllerBase
    {
        private readonly APIDbContext _dbContext;
        private readonly Random _random = new Random();
        private static readonly RandomNumberGenerator _rng = RandomNumberGenerator.Create();

        public ElectricController(APIDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpPost("/electric/post")]
        [Authorize]
        public async Task<IActionResult> PostElectric([FromBody] ProjectElectric electric)
        {
            try
            {

                var existingElectric = await _dbContext.Project_Electric.FirstOrDefaultAsync(m => m.ProjectId == electric.ProjectId);
                if (existingElectric != null)
                {
                    throw new Exception("Projeto Elétrico já existente");
                }
                else
                {
                    await _dbContext.Project_Electric.AddAsync(electric);
                }

                await _dbContext.SaveChangesAsync();

                return Ok(electric);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }
    }
}
