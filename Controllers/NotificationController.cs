using LSF.Data;
using LSF.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace LSF.Controllers
{
    [Route("notifications")]
    [ApiController]
    public class NotificationsController : ControllerBase
    {
        private readonly APIDbContext _dbContext;

        public NotificationsController(APIDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateNotification(Notification notificationModel)
        {
            var notification = new Notification
            {
                UserId = notificationModel.UserId,
                Title = notificationModel.Title,
                Message = notificationModel.Message,
                Url = notificationModel.Url,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            await _dbContext.Notifications.AddAsync(notification);
            await _dbContext.SaveChangesAsync();

            return Ok("Notificação criada com sucesso.");
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserNotifications(int userId)
        {
            try
            {
                var notifications = await _dbContext.Notifications
                    .Where(n => n.UserId == userId)
                    .OrderByDescending(n => n.CreatedAt)
                    .ToListAsync();

                return Ok(notifications);
            }catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("mark-as-read/{id}")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var notification = await _dbContext.Notifications.FindAsync(id);
            if (notification == null)
            {
                return NotFound("Notificação não encontrada.");
            }

            notification.IsRead = true;
            await _dbContext.SaveChangesAsync();

            return Ok("Notificação marcada como lida.");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNotification(int id)
        {
            var notification = await _dbContext.Notifications.FindAsync(id);
            if (notification == null)
            {
                return NotFound("Notificação não encontrada.");
            }

            _dbContext.Notifications.Remove(notification);
            await _dbContext.SaveChangesAsync();

            return Ok("Notificação removida com sucesso.");
        }
    }
}
