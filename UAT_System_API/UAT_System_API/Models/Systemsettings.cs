using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("system_settings")]
public class SystemSettings
{
    [Key]
    public int Id { get; set; }

    [Column("system_name")]
    public string SystemName { get; set; } = "UAT Management System";

    [Column("language")]
    public string Language { get; set; } = "English";

    [Column("timezone")]
    public string Timezone { get; set; } = "Asia/Kuala_Lumpur (+08:00)";

    [Column("email_enabled")]
    public bool EmailEnabled { get; set; }

    [Column("smtp_host")]
    public string? SmtpHost { get; set; }

    [Column("smtp_port")]
    public int SmtpPort { get; set; } = 587;

    [Column("smtp_user")]
    public string? SmtpUser { get; set; }

    [Column("smtp_password")]
    public string? SmtpPassword { get; set; }

    [Column("smtp_secure")]
    public bool SmtpSecure { get; set; } = true;

    [Column("from_email")]
    public string? FromEmail { get; set; }

    [Column("to_email")]
    public string? ToEmail { get; set; }

    [Column("log_user_activity")]
    public bool LogUserActivity { get; set; } = true;

    [Column("retention_period")]
    public string RetentionPeriod { get; set; } = "90 Days";

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}