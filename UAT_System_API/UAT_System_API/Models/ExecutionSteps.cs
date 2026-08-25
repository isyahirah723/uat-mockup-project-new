using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UAT_System_API.Models
{
    [Table("execution_steps")]
    public class ExecutionSteps
    {
        [Key]
        public int id { get; set; }
        public int test_case_id { get; set; }
        public string? run_id { get; set; }
        public string? step_name { get; set; }
        public string? required_role { get; set; }
        public string? execution_status { get; set; }
        public int sequence_order { get; set; }
        public bool has_defect { get; set; }
        public string? severity { get; set; }
        public string? ticket_id { get; set; }
        public string? actual_result { get; set; }
        public string? comments { get; set; }
        public string? crt_user_id { get; set; }
        public DateTime dt_created { get; set; } = DateTime.Now;
    }
}