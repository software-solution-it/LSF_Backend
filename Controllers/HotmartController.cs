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

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LSF.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class HotmartController : ControllerBase
    {
        private readonly APIDbContext _dbContext;
        private readonly Random _random = new Random();
        private static readonly RandomNumberGenerator _rng = RandomNumberGenerator.Create();

        public HotmartController(APIDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpPost("/hotmart/purchase")]
        public async Task<IActionResult> PostPurchase([FromBody] PurchaseObject purchase)
        {
            if (purchase.Data.Buyer == null) return BadRequest("Purchase não encontrado");

            var t = await _dbContext.Users.FirstOrDefaultAsync(user => user.Email == purchase.Data.Buyer.Email);

            if (t != null) return BadRequest("Email ja cadastrado!");

            var newBuyer = purchase.Data.Buyer;

            var randomCharts = GenerateRandomChars();
            var randomPassword = GeneratePassword();

            var newUser = new User
            {
                Name = newBuyer.Name,
                UserName = $"{newBuyer.Name}_{randomCharts}",
                Email = newBuyer.Email,
                Phone = newBuyer.CheckoutPhone ?? "",
                Password = randomPassword

            };

            var userRole = new UserRole
            {
                UserId = newUser.Id,
                RoleId = 2
            };


            var emailContent = $"Seu login para acessar o App é : {newUser.Email}";
            var emailSubject = $"Sua senha é: {randomPassword}";

            var result = await SendEmailAsync(newUser.Email, emailSubject, emailContent);

            if (!result) return BadRequest("Falha ao enviar email.");

            await _dbContext.UserRoles.AddAsync(userRole);
            await _dbContext.SaveChangesAsync();
            await _dbContext.Users.AddAsync(newUser);
            await _dbContext.SaveChangesAsync();

            return Ok(purchase);
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
            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#";
            char[] password = new char[length];

            // Adicionar pelo menos uma letra maiúscula
            password[0] = chars[RandomNumber(chars.Length)];

            // Adicionar pelo menos um dígito
            password[1] = chars[RandomNumber(chars.Length - 10) + 52];

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

    }
}
