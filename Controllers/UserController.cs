using LSF.Data;
using LSF.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using NuGet.Protocol.Plugins;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LSF.Controllers
{
    [Route("user/")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly APIDbContext _dbContext;
        private readonly UserManager<User> _userManager;

        public UserController(APIDbContext dbContext, UserManager<User> userManager)
        {
            _dbContext = dbContext;
            _userManager = userManager;
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginRequest login)
        {
            // Crie uma instância do HttpClient
            using var client = new HttpClient();

            try
            {
                // Defina a URL do outro endpoint que deseja chamar
                var otherEndpointUrl = "https://api.faculdadedalavanderia.com.br/user/Login";

                // Serializar o objeto LoginRequest em uma string JSON
                var jsonContent = JsonSerializer.Serialize(login);

                // Crie uma instância de StringContent com a string JSON
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                // Envie uma requisição POST para o outro endpoint
                var response = await client.PostAsync(otherEndpointUrl, content);

                // Verifique se a resposta foi bem-sucedida
                if (response.IsSuccessStatusCode)
                {
                    // Leia o conteúdo da resposta
                    var responseBody = await response.Content.ReadAsStringAsync();

                    // Retorne a resposta recebida do outro endpoint
                    return Ok(responseBody);
                }
                else
                {
                    // Se a resposta não foi bem-sucedida, retorne um problema com o status da resposta
                    return Problem($"Failed to call other endpoint. Status: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                // Se ocorrer uma exceção durante a chamada, retorne um problema com a mensagem de erro
                return Problem($"An error occurred: {ex.Message}");
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
                var userId = await _userManager.GetUserIdAsync(newUser);
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(newUser);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                var callbackUrl = Url.Action(action: "ConfirmEmail", controller: "User", values: new { userId = userId, code = code }, protocol: Request.Scheme);

                var resultEmail = await SendEmailAsync(newUser.Email, "Confirm Email", $"PleaseConfirm <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'> clickHere </a>");

                if (!resultEmail)
                {
                    await _userManager.DeleteAsync(newUser);
                    return BadRequest("Falha ao enviar e-mail de confirmação. O usuário foi excluído.");
                }

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

        private async Task<bool> SendEmailAsync(string email, string subject, string confirmLink)
        {
            try
            {
                var mail = "gabrielsantos.new@gmail.com";
                var pw = "wihzypxkrodiupnv";

                MailMessage message = new MailMessage();
                SmtpClient smtpClient = new SmtpClient();

                message.From = new MailAddress(mail);
                message.To.Add(email);
                message.Subject = subject;
                message.IsBodyHtml = true;
                message.Body = confirmLink;

                smtpClient.Port = 587; // Porta SMTP padrão para o Gmail
                smtpClient.Host = "smtp.gmail.com"; // Host SMTP do Gmail

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

        [HttpGet("ConfirmEmail")]
        public async Task<IActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                // Se userId ou token forem nulos, retorne uma resposta de erro
                return BadRequest("UserId and token must be provided.");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                // Se o usuário não for encontrado, retorne uma resposta de erro
                return NotFound($"User with ID {userId} not found.");
            }

            // Decode o token
            var decodedToken = WebEncoders.Base64UrlDecode(code);
            var decodedTokenString = Encoding.UTF8.GetString(decodedToken);

            // Confirme o e-mail do usuário
            var result = await _userManager.ConfirmEmailAsync(user, decodedTokenString);
            if (!result.Succeeded)
            {
                // Se a confirmação falhar, retorne uma resposta de erro
                return BadRequest("Email confirmation failed.");
            }

            // Se a confirmação for bem-sucedida, retorne uma resposta de sucesso
            return Ok("Email confirmed successfully.");
        }

        // GET: api/<UserController>
        [HttpGet("GetAll")]
        public IEnumerable<User> Get()
        {
            return _dbContext.User.ToList();
        }

        // GET api/<UserController>/5
        [HttpGet("GetById{id}")]
        public User Get(string id)
        {
            return _dbContext.User.FirstOrDefault(t => t.Id == id.ToString());
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