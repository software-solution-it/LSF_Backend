using LSF.Data;
using LSF.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PurchaseObjects;
using System;
using System.Net.Mail;
using System.Net;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Text;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LSF.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class HotmartController : ControllerBase
    {
        private readonly APIDbContext _dbContext;
        private readonly Random _random = new Random();
        private readonly ProjectController _projectController;
        private static readonly RandomNumberGenerator _rng = RandomNumberGenerator.Create();

        public HotmartController(APIDbContext dbContext, ProjectController projectController)
        {
            _dbContext = dbContext;
            _projectController = projectController;
        }

        [HttpPost("/hotmart/purchase")]
        public async Task<IActionResult> PostPurchase([FromBody] PurchaseObject purchase)
        {
            if (purchase.Data.Buyer == null)
            {
                return BadRequest("Purchase não encontrado");
            }

            var existingUser = await _dbContext.Users.FirstOrDefaultAsync(user => user.Email == purchase.Data.Buyer.Email);

            if (existingUser != null)
            {
                return BadRequest("Email já cadastrado!");
            }

            var newBuyer = purchase.Data.Buyer;
            var randomChars = GenerateRandomChars();
            var randomPassword = GeneratePassword();
            var hashedPassword = HashPassword(randomPassword);

            var newUser = new User
            {
                Name = newBuyer.Name,
                UserName = $"{newBuyer.Name}_{randomChars}",
                Email = newBuyer.Email,
                Phone = newBuyer.CheckoutPhone ?? "",
                Password = hashedPassword
            };

            var templatePath = Path.Combine("/var/app/current/", "Email.html");
            string emailHtml = await System.IO.File.ReadAllTextAsync(templatePath);

            emailHtml = emailHtml.Replace("{{UserName}}", newUser.Name)
                                 .Replace("{{Email}}", newUser.Email)
                                 .Replace("{{Password}}", randomPassword);

            var emailSubject = "Bem vindo ao Faculdade da Lavanderia";
            var emailResult = await SendEmailAsync(newUser.Email, emailSubject, emailHtml);

            if (!emailResult)
            {
                var fallbackTemplatePath = Path.Combine("/var/app/", "Email.html");
                string fallbackEmailHtml = await System.IO.File.ReadAllTextAsync(fallbackTemplatePath);

                fallbackEmailHtml = fallbackEmailHtml.Replace("{{UserName}}", newUser.Name)
                                                     .Replace("{{Email}}", newUser.Email)
                                                     .Replace("{{Password}}", randomPassword);

                var fallbackEmailResult = await SendEmailAsync(newUser.Email, emailSubject, fallbackEmailHtml);

                if (!fallbackEmailResult)
                {
                    return BadRequest("Falha ao enviar email.");
                }
            }
             
            await _dbContext.Users.AddAsync(newUser);
            await _dbContext.SaveChangesAsync();

            var userRole = new UserRole
            {
                UserId = newUser.Id,
                RoleId = 2
            };

            await _dbContext.UserRoles.AddAsync(userRole);
            await _dbContext.SaveChangesAsync();

            var project = new Project
            {
                userId = newUser.Id,
                Status = true
            };

            await _projectController.PostProject(project);

            return Ok(purchase);
        }



        private string HashPassword(string password)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // Calcula o hash da senha
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));

                // Converte o array de bytes para uma string hexadecimal
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        private async Task<bool> SendEmailAsync(string emailDestinatário, string emailSubject, string emailHtml)
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
                        mensagem.Body = emailHtml;
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

        public static bool IsPasswordValidate(string senha)
        {
            if (senha.Length < 8) return false;
            if (!senha.Any(char.IsUpper)) return false;
            if (!senha.Any(char.IsDigit)) return false;
            if (!Regex.IsMatch(senha, @"[!@#$%^&*()_+=\[{\]};:<>|./?,-]")) return false;

            return true;
        }

        private string GenerateRandomChars()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, 3)
              .Select(s => s[_random.Next(s.Length)]).ToArray());
        }

        public static string GeneratePassword(int length = 12)
        {
            int RandomNumber(int max)
            {
                Random random = new Random();
                return random.Next(0, max);
            }

            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#";
            char[] password = new char[length];

            // Adicionar pelo menos uma letra maiúscula
            password[0] = chars[RandomNumber(52)]; // 52 é o índice máximo para letras maiúsculas

            // Adicionar pelo menos um dígito
            password[1] = chars[RandomNumber(10) + 52]; // Entre 52 e 61 são os índices para dígitos

            // Adicionar um caractere especial
            password[2] = chars[RandomNumber(3) + 62]; // Entre 62 e 64 são os índices para caracteres especiais

            // Adicionar o restante dos caracteres aleatórios
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

    }
}
