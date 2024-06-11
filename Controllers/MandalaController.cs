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
using System.ComponentModel.DataAnnotations; // Adiciona este namespace

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LSF.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class MandalaController : ControllerBase
    {
        private readonly APIDbContext _dbContext;
        private readonly Random _random = new Random();
        private static readonly RandomNumberGenerator _rng = RandomNumberGenerator.Create();

        public MandalaController(APIDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpPut("/mandala/update")]
        [Authorize]
        public async Task<IActionResult> PutMandala([FromBody] Mandala mandala)
        {
            try
            {
                var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "userId");
                if (userIdClaim == null)
                {
                    return BadRequest("Claim 'userId' not found.");
                }

                if (!int.TryParse(userIdClaim.Value, out int userId))
                {
                    return BadRequest("Invalid user ID.");
                }

                mandala.userId = userId;

                var existingMandala = await _dbContext.Mandala.FirstOrDefaultAsync(m => m.userId == userId);
                if (existingMandala != null)
                {
                    // Update all properties of the existing mandala with the new values, excluding the key properties
                    existingMandala.CopyPropertiesFrom(mandala, excludeKeys: true);

                    _dbContext.Mandala.Update(existingMandala);
                }
                else
                {
                    mandala.userId = userId;
                    await _dbContext.Mandala.AddAsync(mandala);
                }

                await _dbContext.SaveChangesAsync();

                return Ok(mandala);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        [HttpGet("/mandala")]
        [Authorize]
        public async Task<IActionResult> GetMandala()
        {
            try
            {
                var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "userId");
                if (userIdClaim == null)
                {
                    return BadRequest("Claim 'userId' not found.");
                }

                if (!int.TryParse(userIdClaim.Value, out int userId))
                {
                    return BadRequest("Invalid user ID.");
                }

                var mandala = await _dbContext.Mandala.FirstOrDefaultAsync(m => m.userId == userId);
                if (mandala == null)
                {
                    return NotFound("Mandala not found for the user.");
                }

                return Ok(mandala);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }
    }

    public static class ObjectExtensions
    {
        public static void CopyPropertiesFrom<T>(this T target, T source, bool excludeKeys = false)
        {
            var properties = typeof(T).GetProperties();
            foreach (var property in properties)
            {
                if (property.CanWrite)
                {
                    if (excludeKeys && property.GetCustomAttributes(typeof(KeyAttribute), false).Any())
                    {
                        continue; // Skip key properties
                    }
                    property.SetValue(target, property.GetValue(source));
                }
            }
        }
    }
}
