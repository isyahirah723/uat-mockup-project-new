using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UAT_System_API.Models
{
    // Reference file (screenshot, document) attached to a test step when the test case is written.
    // Not the same as ExecutionAttachments, which are evidence uploaded by a tester during a run.
    [Table("test_step_attachments")]
    public class TestStepAttachments
    {
        [Key]
        public int id { get; set; }
        public int test_step_id { get; set; }
        public string file_name { get; set; } = string.Empty;
        public string file_path { get; set; } = string.Empty;
        public DateTime uploaded_at { get; set; } = DateTime.Now;
    }

    public class TestStepAttachmentDto
    {
        public int test_step_id { get; set; }
        public IFormFile file { get; set; } = null!;
    }
}