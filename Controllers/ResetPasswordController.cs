using LSF.Data;
using LSF.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.Text;
using System.Threading.Tasks;

namespace LSF.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ResetPasswordController : ControllerBase
    {
        private readonly APIDbContext _dbContext;
        private readonly UserManager<User> _userManager;

        public ResetPasswordController(APIDbContext dbContext,UserManager<User> userManager)
        {
            this._dbContext = dbContext;
            this._userManager = userManager;
        }

        [HttpPost]
        public async Task<IActionResult> SendEmail(string email)
        {
            var user = await (
                from u in _dbContext.Users
                join ur in _dbContext.UserRoles on u.Id equals ur.UserId
                join r in _dbContext.Roles on ur.RoleId equals r.Id
                where u.Email == email
                select new { User = u, Role = r }
            ).FirstOrDefaultAsync();

            if (user != null)
            {
                var usuario = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
                
                var token = await _userManager.GeneratePasswordResetTokenAsync(usuario);
                var urlConfirmacao = $"https://web.faculdadedalavanderia.com.br/token?TokenReset={token}";
                var mensagem = new StringBuilder();
                mensagem.Append($"<p>Olá, {usuario.Name}.</p>");
                mensagem.Append("<p>Houve uma solicitação de redefinição de senha para seu usuário em nosso site. Se não foi você que fez a solicitação, ignore essa mensagem. Caso tenha sido você, clique no link abaixo para criar sua nova senha:</p>");
                mensagem.Append($"<p><a href='{urlConfirmacao}'>Redefinir Senha</a></p>");
                mensagem.Append("<p>Atenciosamente,<br>Equipe de Suporte</p>");
 
               //logica para enviar o email vem aki


                return new JsonResult(new { TokenReset = token });
            }
            else
            {
                return new JsonResult(new { message = $"Usuário/e-mail <b>'{email}'</b> não encontrado." });
            }
        }
    }
}
