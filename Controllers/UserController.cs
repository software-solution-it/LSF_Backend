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
using System;
using System.Security.Cryptography;


namespace LSF.Controllers
{
    [Route("user/")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly APIDbContext _dbContext;
        private readonly IConfiguration _config;
        private readonly Random _random = new Random();
        private static readonly RandomNumberGenerator _rng = RandomNumberGenerator.Create();

        public UserController(APIDbContext dbContext, IConfiguration config)
        {
            _dbContext = dbContext;
            _config = config;
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login(string email, string password)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            

            var user = await _dbContext.Users
                .Where(u => u.Email == email)
                .Join(_dbContext.UserRoles, u => u.Id, ur => ur.UserId, (u, ur) => new { User = u, UserRole = ur })
                .Join(_dbContext.Roles, ur => ur.UserRole.RoleId, r => r.Id, (ur, r) => new { User = ur.User, Role = r })
                .FirstOrDefaultAsync();

            if (user == null) return Unauthorized("Usuário não encontrado");

            if (!VerifyPassword(password, user.User.Password)) return Unauthorized("Senha incorreta");


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

        static bool VerifyPassword(string password, string storedHash)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                string hashedPassword = HashPassword(password);
                return hashedPassword == storedHash;
            }
        }

        static string HashPassword(string password)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // Calcula o hash da senha
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));

                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
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
        public async Task<IActionResult> Register(UserModel model)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);

                if (!IsPasswordValidate(model.Password!)) return BadRequest(ModelState);

                var hashedPassword = HashPassword(model.Password);

                var newUser = new User
                {
                    Name = model.Name,
                    UserName = model.UserName,
                    Phone = model.Phone,
                    Email = model.Email,
                    Password = hashedPassword,
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

        public static string GeneratePassword(int length = 12)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#";
            char[] password = new char[length];

            password[0] = chars[RandomNumber(26)]; 

            password[1] = chars[RandomNumber(10) + 52];

            password[2] = chars[RandomNumber(31)];

            for (int i = 3; i < length; i++)
            {
                password[i] = chars[RandomNumber(chars.Length)];
            }

            Shuffle(password);

            return new string(password);
        }

        private static int RandomNumber(int maxValue)
        {
            byte[] randomNumber = new byte[1];
            _rng.GetBytes(randomNumber);
            double asciiValueOfRandomCharacter = Convert.ToDouble(randomNumber[0]);
            double multiplier = Math.Max(0, (asciiValueOfRandomCharacter / 255d) - 0.00000000001d);
            int range = maxValue - 1;
            double randomValueInRange = Math.Floor(multiplier * range + 0.5d);
            return Convert.ToInt32(randomValueInRange);
        }

        private static void Shuffle<T>(T[] array)
        {
            int n = array.Length;
            while (n > 1)
            {
                int k = RandomNumber(n--);
                T temp = array[n];
                array[n] = array[k];
                array[k] = temp;
            }
        }

        [HttpPost("Customer")]
        [Authorize]
        public async Task<IActionResult> Customer()
        {
            try
            {
                var userId = Int32.Parse(User.Claims.FirstOrDefault(c => c.Type == "userId")?.Value);

                var userWithDetails = await (from u in _dbContext.Users
                                             where u.Id == userId
                                             join ug in _dbContext.User_Geolocation on u.Id equals ug.UserId into ugGroup
                                             from ug in ugGroup.DefaultIfEmpty()
                                             join up in _dbContext.User_Point on u.Id equals up.UserId into upGroup
                                             from up in upGroup.DefaultIfEmpty()
                                             join us in _dbContext.User_Supplier on u.Id equals us.UserId into usGroup
                                             from us in usGroup.DefaultIfEmpty()
                                             join ut in _dbContext.User_Technician on u.Id equals ut.UserId into utGroup
                                             from ut in utGroup.DefaultIfEmpty()
                                             join g in _dbContext.Geolocation on ug.GeolocationId equals g.Id into gGroup
                                             from g in gGroup.DefaultIfEmpty()
                                             join p in _dbContext.Point on up.PointId equals p.Id into pGroup
                                             from p in pGroup.DefaultIfEmpty()
                                             join s in _dbContext.Supplier on us.SupplierId equals s.Id into sGroup
                                             from s in sGroup.DefaultIfEmpty()
                                             join t in _dbContext.Technician on ut.TechnicianId equals t.Id into tGroup
                                             from t in tGroup.DefaultIfEmpty()
                                             select new
                                             {
                                                 User = new
                                                 {
                                                     u.Id,
                                                     u.Name,
                                                     u.UserName,
                                                     u.Phone,
                                                     u.Email,
                                                     u.UserImage,
                                                     u.ReceiptConfirmed
                                                 },
                                                 Geolocation = g,
                                                 Point = p,
                                                 Supplier = s,
                                                 Technician = t
                                             }).FirstOrDefaultAsync();





                await _dbContext.SaveChangesAsync();

                return Ok(userWithDetails);
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

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            var users = await _dbContext.Users
                                        .Select(u => new
                                        {
                                            u.Id,
                                            u.Name,
                                            u.UserName,
                                            u.Phone,
                                            u.Email,
                                            u.Password,
                                            u.Receipt,
                                            u.UserImage,
                                            u.RecoveryCode
                                        })
                                        .ToListAsync();

            return Ok(users);
        }

        [HttpGet("GetById/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var user = await _dbContext.Users
                                       .Where(u => u.Id == id)
                                       .Select(u => new
                                       {
                                           u.Id,
                                           u.Name,
                                           u.UserName,
                                           u.Phone,
                                           u.Email,
                                           u.Password,
                                           u.Receipt,
                                           u.UserImage,
                                           u.RecoveryCode
                                       })
                                       .FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound("Usuário não encontrado");
            }

            return Ok(user);
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
            existingUser.Receipt = updatedUser.Receipt ?? existingUser.Receipt;
            existingUser.Name = updatedUser.Name ?? existingUser.Name;
            existingUser.UserName = updatedUser.UserName ?? existingUser.UserName;
            existingUser.Phone = updatedUser.Phone ?? existingUser.Phone;
            existingUser.ReceiptConfirmed = updatedUser.ReceiptConfirmed ?? existingUser.ReceiptConfirmed;

            await _dbContext.SaveChangesAsync();

            var updatedUserData = new
            {
                existingUser.Id,
                existingUser.Name,
                existingUser.UserName,
                existingUser.Phone,
                existingUser.Email,
                existingUser.Password,
                existingUser.Receipt,
                existingUser.UserImage,
                existingUser.ReceiptConfirmed
            };

            return Ok(updatedUserData);
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
                    user.Receipt = memoryStream.ToArray();
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