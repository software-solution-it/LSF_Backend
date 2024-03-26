using LSF.Data;
using LSF.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using NuGet.Protocol.Plugins;
using System.Text;
using System.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using IdentityModel.Client;
using TokenResponse = LSF.Models.TokenResponse;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LSF.Controllers
{
    [Route("user/")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly APIDbContext _dbContext;
        private readonly UserManager<User> _userManager;
        private readonly IEmailSender<User> _emailSender;
        private readonly IConfiguration _config;

        public UserController(APIDbContext dbContext, UserManager<User> userManager, IEmailSender<User> sender, IConfiguration config)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _emailSender = sender;
            _config = config;
        }

        [HttpPost("/login")]
        public IActionResult Login(RegisterCustom model, [FromServices] IConfiguration _config)
        {
            if (model.Email != null && model.Password != null)
            {
                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
                var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

                var claims = new[]
                {
            new Claim("username", model.Email),
            new Claim("role", "User"),
        };

                var accessToken = new JwtSecurityToken(
                    issuer: _config["Jwt:Issuer"],
                    audience: _config["Jwt:Issuer"],
                    claims: claims,
                    expires: DateTime.Now.AddMinutes(120),
                    signingCredentials: credentials
                );

                var accessTokenString = new JwtSecurityTokenHandler().WriteToken(accessToken);

                var refreshToken = GenerateRefreshToken();

                var tokenResponse = new TokenResponse(accessTokenString, refreshToken);


                return Ok(tokenResponse);
            }
            else
            {
                return Unauthorized();
            }

        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterCustom model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var newUser = new User
            {
                UserName = model.Email,
                Email = model.Email
                // Pode adicionar mais propriedades conforme necessário
            };

            var result = await _userManager.CreateAsync(newUser, model.Password);

            if (result.Succeeded)
            {
                return Ok("Usuário registrado com sucesso.");
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return BadRequest(ModelState);
            }
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (userId == null || token == null)
            {
                return BadRequest("Error");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return BadRequest("Error");
            }

            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
            {
                return Ok("EmailConfirmed");
            }
            else
            {
                return BadRequest("Error");
            }
        }

            // GET: api/<UserController>
            [HttpGet("GetAll")]
        public IEnumerable<User> Get()
        {
            return _dbContext.User.ToList();
        }

        // GET api/<UserController>/5
        [HttpGet("GetById{id}")]
        public User Get(int id)
        {
            return _dbContext.User.FirstOrDefault(t => t.Id == id.ToString());
        }

        [HttpGet("TesteUnknow")]
        [AllowAnonymous]
        public IActionResult TesteUnknow()
        {
            return Ok("Operação concluída com sucesso!");
        }

        [HttpGet("TesteToken")]
        [Authorize]
        public IActionResult TesteToken()
        {
            return Ok("Operação concluída com sucesso! TesteAdmin");
        }

        // PUT api/<UserController>/5
        [HttpPut("Put{id}")]
        public async Task<IActionResult> Put(string id, [FromBody] User user)
        {
            var userExistente = await _dbContext.Users.FindAsync(id);
            if (userExistente == null)
            {
                return NotFound("Técnico não encontrado");
            }

            // Atualize apenas as propriedades que foram fornecidas
            _dbContext.Entry(userExistente).CurrentValues.SetValues(user);

            // Salve as alterações no banco de dados
            await _dbContext.SaveChangesAsync();

            // Retorna uma resposta de sucesso com o usuário atualizado
            return Ok(userExistente);
        }

        // PUT api/<UserController>/5
        [HttpPost("UpdateUserAndAddRole")]
        public async Task<IActionResult> UpdateUserAndAddRole(string userId, string roleName)
        {
            // Busca o usuário pelo ID
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound("Usuário não encontrado");
            }

            // Atualiza as propriedades do usuário, se necessário
            // Exemplo: user.UserName = "NovoNome";

            // Adiciona a role ao usuário
            var result = await _userManager.AddToRoleAsync(user, roleName);
            if (!result.Succeeded)
            {
                // Se ocorrer algum erro ao adicionar a role, retorna uma mensagem de erro
                return BadRequest("Erro ao adicionar a role ao usuário");
            }

            // Salva as alterações no banco de dados
            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                // Se ocorrer algum erro ao atualizar o usuário, retorna uma mensagem de erro
                return BadRequest("Erro ao atualizar o usuário");
            }

            // Retorna uma mensagem de sucesso
            return Ok("Usuário atualizado e role adicionada com sucesso");
        }

        // DELETE api/<UserController>/5
        [HttpDelete("Delete{id}")]
        public IActionResult Delete(string id)
        {
            // Verifica se o recurso com o ID fornecido existe
            var user = _dbContext.User.Find(id);
            if (user == null)
            {
                return NotFound("Técnico não encontrado");
            }

            try
            {
                // Remove o recurso do contexto do banco de dados
                _dbContext.User.Remove(user);

                // Salva as alterações no banco de dados
                _dbContext.SaveChanges();

                // Retorna uma resposta de sucesso
                return NoContent(); // Retorna um código 204 No Content
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno do servidor: {ex.Message}");
            }
        }

    }
}