using Microsoft.EntityFrameworkCore;
using RandevuTakipSistemi.Web.Entities;
using RandevuTakipSistemi.Web.Enums;
using RandevuTakipSistemi.Web.Repositories;
using System.Net;
using System.Net.Mail;

namespace RandevuTakipSistemi.Web.Services;

public interface INotificationService
{
    Task ProcessNotificationsAsync();
}

public class NotificationService(
    IConfiguration configuration,
    IRepository<Notification> notificationRepository) : INotificationService
{
    public async Task ProcessNotificationsAsync()
    {
        var notifications = await notificationRepository.Table
            .Include(x => x.Appointment).ThenInclude(x => x.User)
            .Where(x => x.Status == NotificationStatus.Waiting
                        || (x.Status == NotificationStatus.Failed && x.FailedCount < 3))
            .ToListAsync();

        var emailSettings = configuration.GetSection("EmailSettings");
        var filteredNotifications = notifications.Where(x => x.SentAt <= DateTime.Now).ToList();

        foreach (var notification in filteredNotifications)
        {
            notification.Status = NotificationStatus.Pending;
            await notificationRepository.SaveAllAsync();

            try
            {
                await SendEmailAsync(notification, notification.Appointment.User.Email, emailSettings);
                notification.Status = NotificationStatus.Completed;
            }
            catch (Exception ex)
            {
                notification.Status = NotificationStatus.Failed;
                notification.FailedCount++;
                notification.LastError = ex.Message;
            }

            await notificationRepository.SaveAllAsync();
        }
    }

    private static async Task SendEmailAsync(Notification notification, string recipient,IConfigurationSection emailSettings)
    {
        var smtpClient = new SmtpClient(emailSettings["Host"])
        {
            Port = int.Parse(emailSettings["Port"] ?? "1025"),
            EnableSsl = bool.Parse(emailSettings["EnableSsl"] ?? "false")
        };

        var userName = emailSettings["UserName"];
        var password = emailSettings["Password"];
        if (!string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(password))
            smtpClient.Credentials = new NetworkCredential(recipient, emailSettings["User"]);

        var mailMessage = new MailMessage
        {
            From = new MailAddress(emailSettings["FromEmail"] ?? "no-reply@halic-server.local", emailSettings["FromName"]),
            Subject = emailSettings["Subject"] ?? "BMY515 Randevu Bildirimi",
            Body = GetMailBody(notification),
            IsBodyHtml = true
        };

        mailMessage.To.Add(recipient);

        await smtpClient.SendMailAsync(mailMessage);
    }

    private static string GetMailBody(Notification notification)
    {
        const string body = """
                            <!DOCTYPE html>
                            <html>
                            <head>
                                <meta charset="UTF-8">
                                <meta name="viewport" content="width=device-width, initial-scale=1.0">
                                <title>Randevu Bildirimi</title>
                                <style>
                                    body {
                                        font-family: Arial, sans-serif;
                                        margin: 0;
                                        padding: 0;
                                        background-color: #f4f4f4;
                                    }
                                    .email-container {
                                        max-width: 600px;
                                        margin: 20px auto;
                                        background-color: #ffffff;
                                        border-radius: 8px;
                                        overflow: hidden;
                                        box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
                                    }
                                    .email-header {
                                        background-color: #007BFF;
                                        color: #ffffff;
                                        text-align: center;
                                        padding: 20px;
                                    }
                                    .email-header h1 {
                                        margin: 0;
                                        font-size: 24px;
                                    }
                                    .email-body {
                                        padding: 20px;
                                        color: #333333;
                                        line-height: 1.6;
                                    }
                                    .email-body p {
                                        margin: 10px 0;
                                    }
                                    .email-body .highlight {
                                        color: #007BFF;
                                        font-weight: bold;
                                    }
                                    .email-footer {
                                        background-color: #f4f4f4;
                                        text-align: center;
                                        padding: 15px;
                                        font-size: 12px;
                                        color: #666666;
                                    }
                                    .email-footer a {
                                        color: #007BFF;
                                        text-decoration: none;
                                    }
                                </style>
                            </head>
                            <body>
                                <div class="email-container">
                                    <div class="email-header">
                                        <h1>Randevu Bildirimi</h1>
                                    </div>
                                    <div class="email-body">
                                        <p>Merhaba,</p>
                                        <p>Bu e-posta, yaklaşan randevunuz hakkında size bilgi vermek için gönderilmiştir.</p>
                                        <p><strong>Randevu Detayları:</strong></p>
                                        <ul>
                                            <li><strong>Başlık:</strong> <span class="highlight">{{RandevuBaslik}}</span></li>
                                            <li><strong>Başlangıç:</strong> <span class="highlight">{{RandevuBaslangic}}</span></li>
                                            <li><strong>Bitiş:</strong> <span class="highlight">{{RandevuBitis}}</span></li>
                                            <li><strong>Mesaj:</strong> <span class="highlight">{{Mesaj}}</span></li>
                                        </ul>
                                        <p>Lütfen bu bilgileri gözden geçirin ve gerektiğinde bizimle iletişime geçin.</p>
                                        <p>Teşekkürler,</p>
                                        <p><strong>Randevu Takip Sistemi</strong></p>
                                    </div>
                                    <div class="email-footer">
                                        <p>Bu e-posta sistem tarafından otomatik olarak gönderilmiştir. Lütfen yanıtlamayın.</p>
                                        <p><a href="https://halic.edu.tr/tr/universitemiz/gizlilik-bildirimi">Gizlilik Politikası</a> | <a href="https://halic.edu.tr/tr/halicte-yasam/iletisim">İletişim</a></p>
                                    </div>
                                </div>
                            </body>
                            </html>
                            """;
        
        var emailBody = body
            .Replace("{{RandevuBaslik}}", notification.Appointment.Title)
            .Replace("{{RandevuBaslangic}}", notification.Appointment.StartDate.ToString("dd/MM/yyyy HH:mm"))
            .Replace("{{RandevuBitis}}", notification.Appointment.StartDate.ToString("dd/MM/yyyy HH:mm"))
            .Replace("{{Mesaj}}", notification.Message);
        
        return emailBody;
    }
}
