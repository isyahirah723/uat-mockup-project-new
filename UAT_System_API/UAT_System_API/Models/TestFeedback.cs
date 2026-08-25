using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization; 

namespace UAT_System_API.Models
{
    [Table("test_feedbacks")]
    public class TestFeedbacks
    {
        [Key]
        public int id { get; set; }

        [ForeignKey("TestCase")]
        public int test_case_id { get; set; }

        public string? user_name { get; set; }
        public int? rating { get; set; }
        public string? comment { get; set; }
        public DateTime created_at { get; set; } = DateTime.Now;

        
        [JsonIgnore]
        public virtual TestCases? TestCase { get; set; }
    }
}