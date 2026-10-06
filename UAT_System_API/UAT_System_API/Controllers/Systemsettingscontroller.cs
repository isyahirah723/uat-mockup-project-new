using UAT_System_API.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class SystemSettingsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly EmailService _emailService;

    public SystemSettingsController(AppDbContext db, EmailService emailService)
    {
        _db = db;
        _emailService = emailService;
    }

    // GET api/SystemSettings
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var settings = await _db.SystemSettings.FirstOrDefaultAsync();
        if (settings == null)
        {
            settings = new SystemSettings();
            _db.SystemSettings.Add(settings);
            await _db.SaveChangesAsync();
        }
        
        return Ok(new
        {
            settings.SystemName,
            settings.Language,
            settings.Timezone,
            settings.EmailEnabled,
            settings.SmtpHost,
            settings.SmtpPort,
            settings.SmtpUser,
            settings.SmtpSecure,
            settings.FromEmail,
            settings.ToEmail,
            settings.LogUserActivity,
            settings.RetentionPeriod
        });
    }

    
    [HttpPut]
    public async Task<IActionResult> Update([FromBody] SystemSettingsDto dto)
    {
        var settings = await _db.SystemSettings.FirstOrDefaultAsync();
        if (settings == null)
        {
            settings = new SystemSettings();
            _db.SystemSettings.Add(settings);
        }

        settings.SystemName = dto.SystemName;
        settings.Language = dto.Language;
        settings.Timezone = dto.Timezone;
        settings.EmailEnabled = dto.EmailEnabled;
        settings.SmtpHost = dto.SmtpHost;
        settings.SmtpPort = dto.SmtpPort;
        settings.SmtpUser = dto.SmtpUser;
        settings.SmtpSecure = dto.SmtpSecure;
        settings.FromEmail = dto.FromEmail;
        settings.ToEmail = dto.ToEmail;
        settings.LogUserActivity = dto.LogUserActivity;
        settings.RetentionPeriod = dto.RetentionPeriod;

        
        if (!string.IsNullOrWhiteSpace(dto.SmtpPassword))
            settings.SmtpPassword = dto.SmtpPassword;

        settings.UpdatedAt = DateTime.Now;
        await _db.SaveChangesAsync();

        return Ok(new { message = "Settings updated." });
    }

   
    [HttpPost("test-email")]
    public async Task<IActionResult> TestEmail()
    {
        var settings = await _db.SystemSettings.FirstOrDefaultAsync();
        if (settings == null) return BadRequest(new { error = "Settings tak dijumpai. Sila save settings dulu." });

        try
        {
            await _emailService.SendEmailAsync(
                settings.ToEmail ?? settings.SmtpUser!,
                "✅ UAT System - Test Email",
                "<p>This is a test email from the UAT Management System. If you receive this email, your SMTP settings are correct.</p>"
            );
            return Ok(new { message = "Test email sent." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }
}

public class SystemSettingsDto
{
    public string SystemName { get; set; } = "";
    public string Language { get; set; } = "";
    public string Timezone { get; set; } = "";
    public bool EmailEnabled { get; set; }
    public string? SmtpHost { get; set; }
    public int SmtpPort { get; set; }
    public string? SmtpUser { get; set; }
    public string? SmtpPassword { get; set; }
    public bool SmtpSecure { get; set; }
    public string? FromEmail { get; set; }
    public string? ToEmail { get; set; }
    public bool LogUserActivity { get; set; }
    public string RetentionPeriod { get; set; } = "90 Days";
}