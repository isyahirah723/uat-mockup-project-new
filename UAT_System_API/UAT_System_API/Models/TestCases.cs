using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UAT_System_API.Models
{
    [Table("test_cases")]
    public class TestCases
    {
        [Key]
        public int id { get; set; }
        public string? test_case_code { get; set; }
        public string? title { get; set; }
        public string? test_department { get; set; }
        public string? version_tag { get; set; }
        public string? module { get; set; }
        public int? cycle_id { get; set; }
        public string? priority { get; set; }
        public string? status { get; set; }
        public string? assigned_to { get; set; }
        public string? test_number { get; set; }
        public DateTime? test_date { get; set; }
        public DateTime? execution_date { get; set; }
        public string? test_description { get; set; }
        public string? dependencies { get; set; }
        public string? conditions { get; set; }
        public string? control_notes { get; set; }
        public int? created_by { get; set; }
        public DateTime created_at { get; set; } = DateTime.Now;
        public DateTime updated_at { get; set; } = DateTime.Now;
        public virtual ICollection<TestSteps> steps { get; set; } = new List<TestSteps>();
        public virtual ICollection<TestFeedbacks> feedbacks { get; set; } = new List<TestFeedbacks>();
    }
}