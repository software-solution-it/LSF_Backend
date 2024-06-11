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
        private Random _random = new Random();
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


            var user = await (
                from u in _dbContext.Users
                join ur in _dbContext.UserRoles on u.Id equals ur.UserId
                join r in _dbContext.Roles on ur.RoleId equals r.Id
                where u.Email == email
                select new { User = u, Role = r }
            ).FirstOrDefaultAsync();

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
                var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "userId");
                if (userIdClaim == null)
                {
                    return BadRequest("Claim 'userId' não encontrada.");
                }

                if (!int.TryParse(userIdClaim.Value, out int userId))
                {
                    return BadRequest("O ID do usuário é inválido.");
                }

                // Consulta para obter o usuário
                var user = await _dbContext.Users
                    .Where(u => u.Id == userId)
                    .Select(u => new
                    {
                        u.Id,
                        u.Name,
                        u.Email,
                        // outros campos do usuário que você queira incluir
                    })
                    .FirstOrDefaultAsync();

                if (user == null)
                {
                    return NotFound("User not found.");
                }
                // Consulta para obter os projetos do usuário
var projects = await (from p in _dbContext.Project
                      where p.userId == userId
                      select new
                      {
                          p.Id,
                          p.Name,
                          Geolocation = (from pg in _dbContext.Project_Geolocation
                                         join g in _dbContext.Geolocation on pg.GeolocationId equals g.Id
                                         where pg.ProjectId == p.Id
                                         select g).FirstOrDefault(),
                          Point = (from pp in _dbContext.Project_Point
                                   join pt in _dbContext.Point on pp.PointId equals pt.Id
                                   where pp.ProjectId == p.Id
                                   select pt).FirstOrDefault(),
                          Suppliers = (from ps in _dbContext.Project_Supplier
                                       join s in _dbContext.Supplier on ps.SupplierId equals s.Id
                                       where ps.ProjectId == p.Id
                                       select s).ToList(),
                          Technician = (from ptc in _dbContext.Project_Technician
                                        join t in _dbContext.Technician on ptc.TechnicianId equals t.Id
                                        where ptc.ProjectId == p.Id
                                        select t).FirstOrDefault(),
                          Electric = (from e in _dbContext.Project_Electric
                                        where e.ProjectId == p.Id
                                        select e).FirstOrDefault(),
                          ProjectFiles = new
                          {
                              ConfirmedReceipt = (from pf in _dbContext.Project_File
                                                  where pf.ProjectId == p.Id
                                                  select pf.ConfirmedReceipt).FirstOrDefault(),
                              ReceiptDeclinedReason = (from pf in _dbContext.Project_File
                                                       where pf.ProjectId == p.Id
                                                       select pf.ReceiptDeclinedReason).FirstOrDefault(),
                              Recipe = (from pf in _dbContext.Project_File
                                        join f in _dbContext.FileModel on pf.FileId equals f.Id
                                        where pf.ProjectId == p.Id && f.FileType == "Recipe"
                                        select f).FirstOrDefault(),
                              HydraulicModel = (from pf in _dbContext.Project_File
                                                join fh in _dbContext.FileModel on pf.FileId equals fh.Id
                                                where pf.ProjectId == p.Id && fh.FileType == "HydraulicModel"
                                                select fh).FirstOrDefault(),
                              ElectricModel = (from pf in _dbContext.Project_File
                                               join fe in _dbContext.FileModel on pf.FileId equals fe.Id
                                               where pf.ProjectId == p.Id && fe.FileType == "ElectricModel"
                                               select fe).FirstOrDefault(),
                              SketchModel = (from pf in _dbContext.Project_File
                                             join fs in _dbContext.FileModel on pf.FileId equals fs.Id
                                             where pf.ProjectId == p.Id && fs.FileType == "SketchModel"
                                             select fs).FirstOrDefault()
                          }
                      }).ToListAsync();



                var result = new
                {
                    User = new
                    {
                        user.Id,
                        user.Name,
                        user.Email,
                        Projects = projects
                    }
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Ocorreu um erro durante o processamento da solicitação. {ex.Message}");
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
            string remetenteEmail = "suporte@faculdadedalavanderia.com.br";
            string remetenteSenha = "Lavanderiaprojeto#1";
            string destinatarioEmail = emailDestinatário;
            string smtpServidor = "smtp.hostinger.com";
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

        [HttpPost("UserProduct")]
        public async Task<bool> UserProduct(int supplierType, int productId, int quantity, int projectId)
        {
            var userId = Int32.Parse(User.Claims.FirstOrDefault(c => c.Type == "userId")?.Value);

            var result = new ProjectProduct
            {
                Quantity = quantity,
                ProjectId = projectId,
                ProductId = productId,
                SupplierType = supplierType
            };

            _dbContext.Project_Product.Add(result);

            await _dbContext.SaveChangesAsync();

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
                                            u.UserImage,
                                            u.RecoveryCode,
                                        })
                                        .ToListAsync();

            return Ok(users);
        }



        [HttpPut("Put/{id}")]
        public async Task<IActionResult> Put(int id, UserModel updatedUser)
        {
            var existingUser = await _dbContext.Users.FindAsync(id);

            if (existingUser == null) return NotFound("Usuário não encontrado");
            
            existingUser.Email = updatedUser.Email ?? existingUser.Email;
            existingUser.Password = updatedUser.Password ?? existingUser.Password;
            existingUser.UserImage = updatedUser.UserImage ?? existingUser.UserImage;
            existingUser.Name = updatedUser.Name ?? existingUser.Name;
            existingUser.UserName = updatedUser.UserName ?? existingUser.UserName;
            existingUser.Phone = updatedUser.Phone ?? existingUser.Phone;

            await _dbContext.SaveChangesAsync();

            var updatedUserData = new
            {
                existingUser.Id,
                existingUser.Name,
                existingUser.UserName,
                existingUser.Phone,
                existingUser.Email,
                existingUser.Password,
                existingUser.UserImage,
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