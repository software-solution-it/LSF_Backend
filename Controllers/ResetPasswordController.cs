using LSF.Data;
using LSF.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using TokenResponse = LSF.Models.TokenResponse;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;

public class EmailRequest
{
    public string Email { get; set; }
    public EmailRequest(string email)
    {
      Email=email;
     
    }

}

public class ConfirmationRequest
{
    public string Token { get; set; }
    public int Id { get; set; }
    public ConfirmationRequest(string token,int id)
    {
      Token=token;
      Id=id;
    }

}

namespace LSF.Controllers
{
    [Route("ResetPassword/")]
    [ApiController]
    public class ResetPasswordController : ControllerBase
    {
        private readonly APIDbContext _dbContext;
        private readonly IConfiguration _config;

        public ResetPasswordController(APIDbContext dbContext, IConfiguration config)
        {
            this._dbContext = dbContext;
            _config = config;
        }

        [HttpPost("email")]
        public async Task<IActionResult> SendEmail(EmailRequest emailRequest )
        {
            var email = emailRequest.Email ?? string.Empty;
            try
            {
            var user = await (
                from u in _dbContext.Users
                join ur in _dbContext.UserRoles on u.Id equals ur.UserId
                join r in _dbContext.Roles on ur.RoleId equals r.Id
                where u.Email == email
                select new { User = u, Role = r }
            ).FirstOrDefaultAsync();

            System.Console.WriteLine(user);
            if (user != null)
            {
                var usuario = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
                
                var token = HashKey(usuario.Email);

                var urlConfirmacao = $"https://web.faculdadedalavanderia.com.br/confirmation?token={token}&Id={usuario.Id}";
                var mensagemBody = new StringBuilder();
                mensagemBody.Append($"<p>Olá, {usuario.Name}.</p>");
                mensagemBody.Append("<p>Houve uma solicitação de redefinição de senha para seu usuário em nosso site. Se não foi você que fez a solicitação, ignore essa mensagem. Caso tenha sido você, clique no link abaixo para criar sua nova senha:</p>");
                mensagemBody.Append($"<p><a href='{urlConfirmacao}'>Redefinir Senha</a></p>");
                mensagemBody.Append("<p>Atenciosamente,<br>Equipe de Suporte</p>");

                string mensagem = mensagemBody.ToString();
                
                
                var tokenCarry = new UserToken
                {
                     User_Id= usuario.Id,
                     CreatedAt = DateTime.UtcNow,
                     ResetToken = token
             
                };
                await _dbContext.User_Token.AddAsync(tokenCarry);
                var resultEmail=await _dbContext.SaveChangesAsync();
              
                if(resultEmail>0){
                  //O e-mail foi encontrado e seu será enviado
                  await SendEmailAsync(usuario.Email,"Redefinir senha da Faculdade da Lavanderia",mensagem);
                  return new JsonResult(new { status = "ok",message = "O email foi enviado, por favor verifique sua caixa de entrada!" });
                }
                else{
                  return new JsonResult(new { status = "error",message = "Erro ou adicionar valores" });
                }
              
              
            }
            else{
                return new JsonResult(new { status = "error",message = $"Usuário/e-mail {email} não encontrado." });
            }
            }
            catch
            {
              return new JsonResult(new { status = "error",message = $"Usuário/e-mail {email} não encontrado." });
            }
        }    
        private async Task<bool> SendEmailAsync(string emailDestinatário, string emailSubject, string message)
        {
            string remetenteEmail = "suporte@faculdadedalavanderia.com.br";
            string remetenteSenha = "Lsf#2024";
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

                    MailAddress remetente = new MailAddress(remetenteEmail, "LSF", Encoding.UTF8);

                    using (MailMessage mensagem = new MailMessage(remetente, new MailAddress(destinatarioEmail)))
                    {
                        mensagem.Subject = emailSubject;
                        mensagem.Body = message;
                        mensagem.IsBodyHtml = true;
                    
 
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
        
        [HttpPost("Confirmation")]
        public async Task<IActionResult> ConfirmationToken(ConfirmationRequest confirmationRequest )
        {
            string token = confirmationRequest.Token ?? string.Empty;
            int Id = confirmationRequest?.Id ?? 0;
            if(Id == 0 && !string.IsNullOrEmpty(token)){

                return new JsonResult(new { message = "o id nao pode ser nulo ou o token esta vazio" }); 
            }

            var tableToken = _dbContext.User_Token
                             .Where(ut => ut.User_Id == Id)
                             .FirstOrDefault();
            
            if (tableToken?.User_Id  == null)
            {
             return new JsonResult(new { status = "error",message = " primeiro Token não encontrado" });
            } 

            // resgata email e senha para criar um token de acesso no navegador 
            var userData = await (
            from u in _dbContext.Users
            join ut in _dbContext.User_Token on u.Id equals ut.User_Id
            select new 
            { 
             Email = u.Email, 
             Password = u.Password, 
            }
            ).FirstOrDefaultAsync();

            string Useremail=userData?.Email??String.Empty;
            string Userpassword=userData?.Password??String.Empty;

            // retorna o token de acesso
            var user = await (
                from u in _dbContext.Users
                join ur in _dbContext.UserRoles on u.Id equals ur.UserId
                join r in _dbContext.Roles on ur.RoleId equals r.Id
                where u.Email == Useremail
                select new { User = u, Role = r }
            ).FirstOrDefaultAsync();

            if (user == null) return new JsonResult(new { status = "error",message = " segundo Token não encontrado" });;

            if (!VerifyPassword(Userpassword, user.User.Password)) return Unauthorized("Senha incorreta");


            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim("userId", user.User.Id.ToString()),
                new Claim("role", user.Role.Id.ToString()),
                new Claim("userName", user.User.UserName),
                new Claim("fa", user.User.FirstAccess.ToString()),
                new Claim("active", user.User.isActive.ToString())
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


            try
            {
                // Remove o recurso do contexto do banco de dados
                _dbContext.User_Token.Remove(tableToken);

                // Salva as alterações no banco de dados
                _dbContext.SaveChanges();

            
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno do servidor na deletar token no ressetar senha: {ex.Message}");
            }


            return Ok(tokenResponse);
            

        }
        private string HashKey(string key)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // Calcula o hash 
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(key));

                // Converte o array de bytes para uma string hexadecimal
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
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
        static bool VerifyPassword(string password, string storedHash)
        {

                return password == storedHash;
            
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

    }
}
