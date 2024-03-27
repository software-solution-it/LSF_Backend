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

        [HttpPost("Login")]
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

        [HttpPost("Register")]
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

        [HttpGet("Hotmart")]
        public async Task<IActionResult> Teste()
        {
            var client = new HttpClient();

            // Faça a solicitação para obter o token de acesso
            var requestBody = "{}"; // Seu corpo da solicitação
            var requestToken = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri("https://api-sec-vlc.hotmart.com/security/oauth/token?grant_type=client_credentials&client_id=dc18e069-9d41-4d03-9106-54149c9701ad&client_secret=d360b6a5-2dc4-414f-acd5-944471fa8f31"),
                Content = new StringContent(requestBody, System.Text.Encoding.UTF8, "application/json")
            };
            requestToken.Headers.Add("Authorization", "Basic ZGMxOGUwNjktOWQ0MS00ZDAzLTkxMDYtNTQxNDljOTcwMWFkOmQzNjBiNmE1LTJkYzQtNDE0Zi1hY2Q1LTk0NDQ3MWZhOGYzMQ==");
            var responseToken = await client.SendAsync(requestToken);
            var responseBodyToken = await responseToken.Content.ReadAsStringAsync();
            var accessToken = ""; // Extrair o token de acesso do responseBodyToken, dependendo do formato da resposta

            // Faça a solicitação para obter informações sobre as assinaturas usando o token de acesso
            var requestSubscriptions = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri("https://sandbox.hotmart.com/payments/api/v1/subscriptions"),
            };
            requestSubscriptions.Headers.Add("Authorization", $"Bearer {accessToken}");

            var responseSubscriptions = await client.SendAsync(requestSubscriptions);
            var responseBodySubscriptions = await responseSubscriptions.Content.ReadAsStringAsync();

            return Ok(responseBodySubscriptions);
        }

        [HttpGet("GetAll")]
        public IEnumerable<User> Get()
        {
            return _dbContext.User.ToList();
        }

        // GET api/<UserController>/5
        [HttpGet("GetById{id}")]
        public User Get(int id)
        {
            return _dbContext.User.FirstOrDefault(t => t.Id == id);
        }

        // PUT api/<UserController>/5
        [HttpPut("Put/{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] User updatedUser)
        {
            var existingUser = await _dbContext.Users.FindAsync(id);
            if (existingUser == null)
            {
                return NotFound("Usuário não encontrado");
            }

            existingUser.UserName = updatedUser.UserName;
            existingUser.Email = updatedUser.Email;
            existingUser.PhoneNumber = updatedUser.PhoneNumber;
            existingUser.UserImage = updatedUser.UserImage;
            existingUser.Comprovante = updatedUser.Comprovante;

            await _dbContext.SaveChangesAsync();

            return Ok(existingUser);
        }

        // PUT api/<UserController>/5
        [HttpPost("UpdateUserAndAddRole")]
        public async Task<IActionResult> UpdateUserAndAddRole(int userId, string roleName)
        {
            // Busca o usuário pelo ID
            var user = await _userManager.FindByIdAsync(userId.ToString());
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

        [HttpPost("UploadPdf/{userId}")]
        public async Task<IActionResult> UploadPdf(string userId, IFormFile pdfFile)
        {
            // Verifica se o ID do usuário é válido
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("User ID is required.");
            }

            // Verifica se o arquivo PDF é válido
            if (pdfFile == null || pdfFile.Length == 0)
            {
                return BadRequest("PDF file is required.");
            }

            // Busca o usuário pelo ID
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound($"User with ID {userId} not found.");
            }

            try
            {
                // Lê o arquivo PDF em bytes
                using (var memoryStream = new MemoryStream())
                {
                    await pdfFile.CopyToAsync(memoryStream);
                    user.Comprovante = memoryStream.ToArray();
                }

                // Atualiza o usuário com o arquivo PDF
                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    return StatusCode(500, $"Failed to update user with ID {userId}.");
                }

                return Ok("PDF uploaded successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }


        [HttpPost("UploadImage/{userId}")]
        public async Task<IActionResult> UploadImage(string userId, IFormFile imageFile)
        {
            // Verifica se o ID do usuário é válido
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("User ID is required.");
            }

            // Verifica se o arquivo de imagem é válido
            if (imageFile == null || imageFile.Length == 0)
            {
                return BadRequest("Image file is required.");
            }

            // Busca o usuário pelo ID
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound($"User with ID {userId} not found.");
            }

            try
            {
                // Verifica se o arquivo é uma imagem
                if (!imageFile.ContentType.StartsWith("image"))
                {
                    return BadRequest("Only image files are allowed.");
                }

                // Lê a imagem em bytes
                using (var memoryStream = new MemoryStream())
                {
                    await imageFile.CopyToAsync(memoryStream);
                    user.UserImage = memoryStream.ToArray();
                }

                // Atualiza o usuário com a imagem
                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    return StatusCode(500, $"Failed to update user with ID {userId}.");
                }

                return Ok("Image uploaded successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        // DELETE api/<UserController>/5
        [HttpDelete("Delete{id}")]
        public IActionResult Delete(int id)
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