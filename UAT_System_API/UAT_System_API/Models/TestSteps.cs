using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace UAT_System_API.Models
{
    [Table("test_steps")]
    public class TestSteps
    {
        [Key]
        public int id { get; set; }

        [ForeignKey("TestCase")]
        public int test_case_id { get; set; }

        public int step_order { get; set; }
        public string? description { get; set; }
        public string? expected { get; set; }
        public string? actual { get; set; }
        public string? pass_fail { get; set; }

        [JsonIgnore]
        public virtual TestCases? TestCase { get; set; }
    }
}