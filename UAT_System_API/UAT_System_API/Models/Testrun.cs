using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UAT_System_API.Models
{
    [Table("test_runs")]
    public class TestRuns
    {
        [Key]
        public int id { get; set; }
        public int test_case_id { get; set; }
        public int? test_assignment_id { get; set; }
        public int executed_by { get; set; }
        public string? run_status { get; set; } = "Not Started";
        public DateTime? started_at { get; set; }
        public DateTime? completed_at { get; set; }
        public DateTime created_at { get; set; } = DateTime.Now;
    }
}