using LSF.Data;
using LSF.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using TokenResponse = LSF.Models.TokenResponse;
using System.Net.Mail;
using System.Net;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using Microsoft.AspNetCore.Authorization;


namespace LSF.Controllers
{
    [Route("user/")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly APIDbContext _dbContext;
        private readonly IConfiguration _config;

        public UserController(APIDbContext dbContext, IConfiguration config)
        {
            _dbContext = dbContext;
            _config = config;
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login(UserModelRegister model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _dbContext.Users
                .Where(u => u.Email == model.Email)
                .Join(_dbContext.UserRoles, u => u.Id, ur => ur.UserId, (u, ur) => new { User = u, UserRole = ur })
                .Join(_dbContext.Roles, ur => ur.UserRole.RoleId, r => r.Id, (ur, r) => new { User = ur.User, Role = r })
                .FirstOrDefaultAsync();
            if (user == null)
            {
                return Unauthorized();
            }

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim("userId", user.User.Id.ToString()),
                new Claim("role", user.Role.Id.ToString()),
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

        private bool VerifyPassword(User user, string password)
        {
            // Aqui você deve implementar a lógica para verificar se a senha fornecida corresponde à senha armazenada no banco de dados.
            // Por exemplo, você pode comparar o hash da senha fornecida com o hash de senha armazenado no banco de dados.
            // Não esqueça de considerar as técnicas de segurança adequadas, como o uso de funções de hash seguras (por exemplo, BCrypt, PBKDF2).
            // Este método é apenas um esboço e você precisa implementá-lo de acordo com suas necessidades.
            // Aqui está um exemplo simplificado:

            return user.Password == HashPassword(password);
        }

        private string HashPassword(string password)
        {
            // Aqui você deve implementar a lógica para criar um hash seguro da senha fornecida.
            // Por razões de segurança, é altamente recomendável usar uma biblioteca de hash de senha robusta, como BCrypt ou PBKDF2.
            // Este método é apenas um esboço e você precisa implementá-lo de acordo com suas necessidades.
            // Aqui está um exemplo simplificado:

            return password; // Exemplo: esta implementação simplesmente retorna a senha não hashada, o que não é seguro em produção.
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
        public async Task<IActionResult> Register(UserModelRegister model)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);

                if (!IsPasswordValidate(model.Password!)) return BadRequest(ModelState);

                var newUser = new User
                {
                    Email = model.Email,
                    Password = model.Password,
                };

                var result = await _dbContext.Users.AddAsync(newUser);
                await _dbContext.SaveChangesAsync();

                var userRole = new UserRole
                {
                    UserId = newUser.Id,
                    RoleId = 2
                };

                await _dbContext.UserRoles.AddAsync(userRole);
                await _dbContext.SaveChangesAsync();

                return Ok("Usuário registrado com sucesso.");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Ocorreu um erro durante o processamento da solicitação. {ex}");
            }
        }



        [HttpPost("Customer")]
        public async Task<IActionResult> Customer()
        {
            try
            {
                var userId = Int32.Parse(User.Claims.FirstOrDefault(c => c.Type == "userId")?.Value);

               // var result = await _dbContext.Users.AddAsync(newUser);
                await _dbContext.SaveChangesAsync();

                return Ok("Usuário registrado com sucesso.");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Ocorreu um erro durante o processamento da solicitação. {ex}");
            }
        }

        public static bool IsPasswordValidate(string senha)
        {
            if (senha.Length < 8) return false;
            if (!senha.Any(char.IsUpper)) return false;
            if (!senha.Any(char.IsDigit)) return false;
            if (!Regex.IsMatch(senha, @"[!@#$%^&*()_+=\[{\]};:<>|./?,-]")) return false;

            return true;
        }

        [HttpPost("ResetPassword")]
        public async Task<ActionResult> ResetPassword(string Email)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == Email);

            if (user == null)
            {
                return NotFound("Usuário não encontrado.");
            }

            var verificationCode = GerarCodigoVerificacao();

            user.RecoveryCode = verificationCode;
            await _dbContext.SaveChangesAsync();

            var timer = new Timer(async (state) =>
            {
                await RemoverCodigoDoBancoAsync(user);
            }, null, TimeSpan.FromHours(1), TimeSpan.Zero);

            var emailContent = $"Seu código de verificação é: {verificationCode}";
            var emailSubject = "Confirmação de Email";

            var result = await SendEmailAsync(user.Email, emailSubject, emailContent);

            if (!result)
            {
                return BadRequest("Falha ao enviar email de confirmação.");
            }

            return Ok("Reset password enviado com sucesso.");
        }

        private int GerarCodigoVerificacao()
        {
            var random = new Random();
            return random.Next(100000, 999999);
        }

        private async Task RemoverCodigoDoBancoAsync(User user)
        {
            if (user != null)
            {
                user.RecoveryCode = null;
                await _dbContext.SaveChangesAsync();
            }
        }

        private async Task<bool> SendEmailAsync(string emailDestinatário, string emailSubject, string emailContent)
        {
            string remetenteEmail = "contato@lavanderiasemfranquia.com";
            string remetenteSenha = "App#LSF2024";
            string destinatarioEmail = emailDestinatário;
            string smtpServidor = "smtp.titan.email";
            int porta = 587;

            try
            {
                using (SmtpClient smtp = new SmtpClient(smtpServidor, porta))
                {
                    smtp.EnableSsl = true;
                    smtp.UseDefaultCredentials = false;
                    smtp.Credentials = new NetworkCredential(remetenteEmail, remetenteSenha);
                    using (MailMessage mensagem = new MailMessage(new MailAddress(remetenteEmail, "Contato"), new MailAddress(destinatarioEmail)))
                    {
                        mensagem.Subject = emailSubject;
                        mensagem.Body = emailContent;

                        await smtp.SendMailAsync(mensagem);
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        [HttpPost("ConfirmCode")]
        public async Task<bool> ConfirmCode(string Email, int code)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == Email);

            if (user == null) return false;

            if(user.RecoveryCode != code)
            {
                return false;
            }

            return true;
        }

        [HttpPut("NewPassword")]
        public async Task<ActionResult> NewPassword(UserModelRegister model)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == model.Email);

            if (user == null) return BadRequest("Usuário não encontrado");

            if (!IsPasswordValidate(model.Password!)) return BadRequest("Senha inválida");

            user.Password = model.Password;

            await _dbContext.SaveChangesAsync();

            return Ok();
        }

        [HttpGet("Hotmart")]
        public async Task<IActionResult> Hotmart()
        {
            var client = new HttpClient();

            var requestBody = "{}"; 
            var requestToken = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri("https://api-sec-vlc.hotmart.com/security/oauth/token?grant_type=client_credentials&client_id=dc18e069-9d41-4d03-9106-54149c9701ad&client_secret=d360b6a5-2dc4-414f-acd5-944471fa8f31"),
                Content = new StringContent(requestBody, System.Text.Encoding.UTF8, "application/json")
            };
            requestToken.Headers.Add("Authorization", "Basic ZGMxOGUwNjktOWQ0MS00ZDAzLTkxMDYtNTQxNDljOTcwMWFkOmQzNjBiNmE1LTJkYzQtNDE0Zi1hY2Q1LTk0NDQ3MWZhOGYzMQ==");
            var responseToken = await client.SendAsync(requestToken);
            var responseBodyToken = await responseToken.Content.ReadAsStringAsync();
            var accessToken = ""; 

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
        [Authorize(Roles = "Admin")]
        public IEnumerable<User> Get()
        {
            return _dbContext.Users.ToList();
        }

        [HttpGet("GetById{id}")]
        [Authorize(Roles  = "Manager")]
        public User Get(int id)
        {
            return _dbContext.Users.FirstOrDefault(t => t.Id == id);
        }

        [HttpPut("Put/{id}")]
        public async Task<IActionResult> Put(int id, UserModel updatedUser)
        {
            var existingUser = await _dbContext.Users.FindAsync(id);
            if (existingUser == null)
            {
                return NotFound("Usuário não encontrado");
            }

            existingUser.Email = updatedUser.Email ?? existingUser.Email;
            existingUser.Password = updatedUser.Password ?? existingUser.Password;
            existingUser.UserImage = updatedUser.UserImage ?? existingUser.UserImage;
            existingUser.Comprovante = updatedUser.Comprovante ?? existingUser.Comprovante;

            await _dbContext.SaveChangesAsync();

            return Ok(existingUser); 
        }
        [HttpPut("UpdateUserAndAddRole")]
        [Authorize]
        public async Task<IActionResult> UpdateUserAndAddRole(int roleId)
        {
            var userId = Int32.Parse(User.Claims.FirstOrDefault(c => c.Type == "userId")?.Value);
            var user = await _dbContext.Users.FirstOrDefaultAsync(uid => uid.Id == userId);
            if (user == null)
            {
                return NotFound("Usuário não encontrado");
            }

            var role = await _dbContext.Roles.FirstOrDefaultAsync(r => r.Id == roleId);
            if (role == null)
            {
                return NotFound("Role não encontrada");
            }

            var rolesToDelete = await _dbContext.UserRoles
            .Where(ur => ur.UserId == userId && ur.RoleId != roleId)
            .ToListAsync();

            _dbContext.UserRoles.RemoveRange(rolesToDelete);
            await _dbContext.SaveChangesAsync();


            var existingUserRole = await _dbContext.UserRoles.FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId);
            if (existingUserRole != null)
            {
                return BadRequest("O usuário já possui esta role");
            }

            var userRole = new UserRole
            {
                UserId = userId,
                RoleId = role.Id
            };

            user.UserRoles.Add(userRole);

            await _dbContext.SaveChangesAsync();

            return Ok("Usuário atualizado e role adicionada com sucesso");
        }

        [HttpPut("UploadPdf/{userId}")]
        public async Task<IActionResult> UploadPdf(int userId, IFormFile pdfFile)
        {
            // Verifica se o arquivo PDF é válido
            if (pdfFile == null || pdfFile.Length == 0)
            {
                return BadRequest("PDF file is required.");
            }

            // Busca o usuário pelo ID
            var user = await _dbContext.Users.FirstOrDefaultAsync(uid => uid.Id == userId);
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
                    // Atualiza apenas o PDF do usuário
                    user.Comprovante = memoryStream.ToArray();
                }

                // Atualiza o usuário com o novo PDF
                var result = await _dbContext.SaveChangesAsync();

                return Ok("PDF uploaded successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }


        [HttpPut("UploadImage/{userId}")]
        public async Task<IActionResult> UploadImage(int userId, IFormFile imageFile)
        {
            // Verifica se o arquivo de imagem é válido
            if (imageFile == null || imageFile.Length == 0)
            {
                return BadRequest("Image file is required.");
            }

            // Busca o usuário pelo ID
            var user = await _dbContext.Users.FirstOrDefaultAsync(uid => uid.Id == userId);
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
                var result = await _dbContext.SaveChangesAsync();

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
            var user = _dbContext.Users.Find(id);
            if (user == null)
            {
                return NotFound("Técnico não encontrado");
            }

            try
            {
                // Remove o recurso do contexto do banco de dados
                _dbContext.Users.Remove(user);

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