using UAT_System_API.Data; 
using MailKit.Security;
using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.EntityFrameworkCore;

public class EmailService
{
    private readonly AppDbContext _db;
    public EmailService(AppDbContext db) => _db = db;

    public async Task SendEmailAsync(string toEmail, string subject, string htmlBody, string? attachmentPath = null)
    {
        var settings = await _db.SystemSettings.FirstOrDefaultAsync()
            ?? throw new Exception("System settings tak dijumpai dalam DB. Sila save settings dulu.");

        if (!settings.EmailEnabled)
            throw new Exception("Email notification disabled dalam Settings.");

        if (string.IsNullOrWhiteSpace(settings.SmtpHost) || string.IsNullOrWhiteSpace(settings.SmtpUser))
            throw new Exception("SMTP Host / Username belum diisi dalam Settings.");

        var message = new MimeMessage();
        message.From.Add(MailboxAddress.Parse(settings.FromEmail ?? settings.SmtpUser));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = subject;

        var builder = new BodyBuilder { HtmlBody = htmlBody };
        if (attachmentPath != null && File.Exists(attachmentPath))
            builder.Attachments.Add(attachmentPath);
        message.Body = builder.ToMessageBody();

        using var client = new SmtpClient();
        var socketOption = settings.SmtpPort == 465
            ? SecureSocketOptions.SslOnConnect
            : SecureSocketOptions.StartTls;

        await client.ConnectAsync(settings.SmtpHost, settings.SmtpPort, socketOption);
        await client.AuthenticateAsync(settings.SmtpUser, settings.SmtpPassword);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}
