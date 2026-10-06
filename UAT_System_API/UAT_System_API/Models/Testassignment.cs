using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UAT_System_API.Models
{
    [Table("test_assignments")]
    public class TestAssignments
    {
        [Key]
        public int id { get; set; }
        public int test_case_id { get; set; }
        public string? department { get; set; }
        public string? team { get; set; }
        public int user_id { get; set; }
        public int? assigned_by { get; set; }
        public DateTime assigned_at { get; set; } = DateTime.Now;
        public string? status { get; set; } = "Pending";
    }
}