using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UAT_System_API.Models
{
    [Table("audit_logs")]
    public class AuditLog
    {
        [Key]
        public int Id { get; set; }

        [Column("run_id")]
        public string? RunId { get; set; }

        [Column("action")]
        public string Action { get; set; } = string.Empty;

        [Column("status_old")]
        public string? StatusOld { get; set; }

        [Column("status_new")]
        public string? StatusNew { get; set; }

        [Column("title")]
        public string? Title { get; set; }

        [Column("details")]
        public string? Details { get; set; }

        [Column("crt_user_id")]
        public string? CrtUserId { get; set; }

        [Column("dt_created")]
        public DateTime? DtCreated { get; set; } = DateTime.Now;
    }
}