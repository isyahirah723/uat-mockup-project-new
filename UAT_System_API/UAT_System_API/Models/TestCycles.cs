using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UAT_System_API.Models
{
    [Table("test_cycles")]
    public class TestCycles
    {
        [Key]
        public int id { get; set; }
        public string? cycle_code { get; set; }

        public string? name { get; set; }
        public string? department { get; set; }
        public DateTime? dt_start { get; set; }
        public DateTime? dt_end { get; set; }
        public string? status { get; set; }
        public DateTime? created_date { get; set; }
        public string? auto_assign_rule { get; set; }
        public string? module { get; set; }
        public string? priority { get; set; }
        public string? match_department { get; set; }
        public DateTime created_at { get; set; } = DateTime.Now;
        public DateTime updated_at { get; set; } = DateTime.Now;
        public int? created_by { get; set; }
        public int? assigned_to { get; set; }
    }
}