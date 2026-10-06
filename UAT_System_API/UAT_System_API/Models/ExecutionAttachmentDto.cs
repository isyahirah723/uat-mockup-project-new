namespace UAT_System_API.Models
{
    public class ExecutionAttachmentDto
    {
        public int execution_step_id { get; set; }
        public IFormFile file { get; set; } = null!;
    }
}