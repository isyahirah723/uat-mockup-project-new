using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UAT_System_API.Models
{
    [Table("execution_attachments")]
    public class ExecutionAttachments
    {
        [Key]
        public int id { get; set; }
        public int execution_step_id { get; set; }
        public string file_name { get; set; } = string.Empty;
        public string file_path { get; set; } = string.Empty;
        public DateTime uploaded_at { get; set; }
    }
}