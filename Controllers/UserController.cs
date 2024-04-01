using LSF.Data;
using LSF.Models;
using Microsoft.AspNetCore.Authorization;
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
using System.Net.Mail;
using System.Net;
using System.Text.Encodings.Web;
using Microsoft.EntityFrameworkCore;


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

            var user = await _dbContext.AspNetUsers.FirstOrDefaultAsync(u => u.Email == model.Email);
            if (user == null)
            {
                return Unauthorized();
            }

            if (!VerifyPassword(user, model.Password))
            {
                // Senha incorreta, retornar Unauthorized
                return Unauthorized();
            }

            // Crie e configure a chave de segurança
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            // Crie as reivindicações (claims) para o token JWT
            var claims = new[]
            {
                new Claim("username", user.Email),
                new Claim("role", "User"),
            };

            var accessToken = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Issuer"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(120),
                signingCredentials: credentials
            );

            // Escreva o token JWT como uma string
            var accessTokenString = new JwtSecurityTokenHandler().WriteToken(accessToken);

            // Gere um token de atualização (refresh token)
            var refreshToken = GenerateRefreshToken();

            // Crie a resposta contendo o token de acesso e o token de atualização
            var tokenResponse = new TokenResponse(accessTokenString, refreshToken);

            // Retorne um Ok com a resposta contendo os tokens
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
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var newUser = new User
                {
                    Email = model.Email,
                    Password = model.Password,
                };

                var result = await _dbContext.AspNetUsers.AddAsync(newUser);

                await _dbContext.SaveChangesAsync();

                return Ok("Usuário registrado com sucesso.");

                //if (result)
                //{
                //    var userId = await _dbContext.GetUserIdAsync(newUser);
                //    var code = await _dbContext.GenerateEmailConfirmationTokenAsync(newUser);
                //    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                //    var callbackUrl = Url.Action(action: "ConfirmEmail", controller: "User", values: new { userId = userId, code = code }, protocol: Request.Scheme);

                //    var resultEmail = await SendEmailAsync(newUser.Email, "Confirm Email", $"PleaseConfirm <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'> clickHere </a>");

                //    if (!resultEmail)
                //    {
                //        await _dbContext.DeleteAsync(newUser);
                //        return BadRequest("Falha ao enviar e-mail de confirmação. O usuário foi excluído.");
                //    }

                //    return Ok("Usuário registrado com sucesso.");
                //}
                //else
                //{
                //    foreach (var error in result.Errors)
                //    {
                //        ModelState.AddModelError("", error.Description);
                //    }
                //    return BadRequest(ModelState);
                //}
            }
            catch (Exception ex)
            {
                // Tratar a exceção aqui
                return StatusCode(StatusCodes.Status500InternalServerError, $"Ocorreu um erro durante o processamento da solicitação. {ex}");
            }
        }

        private async Task<bool> SendEmailAsync(string email, string subject, string confirmLink)
        {
            try
            {
                var mail = "contato@lavanderiasemfranquia.com";
                var pw = "App#LSF2024";

                MailMessage message = new MailMessage();
                SmtpClient smtpClient = new SmtpClient();

                message.From = new MailAddress(mail);
                message.To.Add(email);
                message.Subject = subject;
                message.IsBodyHtml = true;
                message.Body = confirmLink;

                smtpClient.Port = 465; // Porta SMTP padrão para o Gmail
                smtpClient.Host = "smtp.lavanderiasemfranquia.com"; // Host SMTP do Gmail

                smtpClient.EnableSsl = true;
                smtpClient.UseDefaultCredentials = false;
                smtpClient.Credentials = new NetworkCredential(mail, pw); // Substitua com suas credenciais reais

                smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;

                smtpClient.Send(message);

                return true;
            }
            catch (Exception ex)
            {
                BadRequest($"Ocorreu um erro ao enviar o e-mail: {ex.Message}");
                return false;
            }
        }

        //[HttpGet("ConfirmEmail")]
        //public async Task<IActionResult> ConfirmEmail(string userId, string code)
        //{
        //    if (userId == null || code == null)
        //    {
        //        // Se userId ou token forem nulos, retorne uma resposta de erro
        //        return BadRequest("UserId and token must be provided.");
        //    }

        //    var user = await _dbContext.FirstOr(userId);
        //    if (user == null)
        //    {
        //        // Se o usuário não for encontrado, retorne uma resposta de erro
        //        return NotFound($"User with ID {userId} not found.");
        //    }

        //    // Decode o token
        //    var decodedToken = WebEncoders.Base64UrlDecode(code);
        //    var decodedTokenString = Encoding.UTF8.GetString(decodedToken);

        //    // Confirme o e-mail do usuário
        //    var result = await _dbContext.ConfirmEmailAsync(user, decodedTokenString);
        //    if (!result.Succeeded)
        //    {
        //        // Se a confirmação falhar, retorne uma resposta de erro
        //        return BadRequest("Email confirmation failed.");
        //    }

        //    // Se a confirmação for bem-sucedida, retorne uma resposta de sucesso
        //    return Ok("Email confirmed successfully.");
        //}

        [HttpGet("Hotmart")]
        public async Task<IActionResult> Hotmart()
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
            return _dbContext.AspNetUsers.ToList();
        }

        // GET api/<UserController>/5
        [HttpGet("GetById{id}")]
        public User Get(int id)
        {
            return _dbContext.AspNetUsers.FirstOrDefault(t => t.Id == id);
        }

        // PUT api/<UserController>/5
        [HttpPut("Put/{id}")]
        public async Task<IActionResult> Put(int id, UserModel updatedUser)
        {
            var existingUser = await _dbContext.AspNetUsers.FindAsync(id);
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

        // PUT api/<UserController>/5
        [HttpPut("UpdateUserAndAddRole")]
        public async Task<IActionResult> UpdateUserAndAddRole(int userId, int roleName)
        {
            // Busca o usuário pelo ID
            var user = await _dbContext.AspNetUsers.FirstOrDefaultAsync(uid => uid.Id == userId);
            if (user == null)
            {
                return NotFound("Usuário não encontrado");
            }

            user.Role = roleName;
            
            var result = await _dbContext.SaveChangesAsync();

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
            var user = await _dbContext.AspNetUsers.FirstOrDefaultAsync(uid => uid.Id == userId);
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
            var user = await _dbContext.AspNetUsers.FirstOrDefaultAsync(uid => uid.Id == userId);
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
            var user = _dbContext.AspNetUsers.Find(id);
            if (user == null)
            {
                return NotFound("Técnico não encontrado");
            }

            try
            {
                // Remove o recurso do contexto do banco de dados
                _dbContext.AspNetUsers.Remove(user);

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