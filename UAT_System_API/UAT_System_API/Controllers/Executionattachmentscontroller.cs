using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UAT_System_API.Data;
using UAT_System_API.Models;

namespace UAT_System_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExecutionAttachmentsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ExecutionAttachmentsController(AppDbContext context)
        {
            _context = context;
        }

        // GET api/ExecutionAttachments
        // Used by FeedbackView.vue, TestRunReportPage.vue and the PDF report
        // to show the screenshots / files a tester uploaded with a defect.
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ExecutionAttachments>>> GetAttachments()
        {
            return Ok(await _context.ExecutionAttachments
                .OrderBy(a => a.id)
                .ToListAsync());
        }

        // POST api/ExecutionAttachments
        // POST api/ExecutionAttachments/upload   <- the URL TestExecutionPage.vue actually calls
        [HttpPost]
        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<ExecutionAttachments>> PostAttachment([FromForm] ExecutionAttachmentDto dto)
        {
            if (dto.file == null || dto.file.Length == 0)
                return BadRequest("No file uploaded.");

            var stepExists = await _context.ExecutionSteps.AnyAsync(s => s.id == dto.execution_step_id);
            if (!stepExists)
                return BadRequest($"Execution step {dto.execution_step_id} does not exist.");

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "execution");
            Directory.CreateDirectory(uploadsFolder);

            var safeFileName = $"{Guid.NewGuid()}_{Path.GetFileName(dto.file.FileName)}";
            var filePath = Path.Combine(uploadsFolder, safeFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await dto.file.CopyToAsync(stream);
            }

            var attachment = new ExecutionAttachments
            {
                execution_step_id = dto.execution_step_id,
                file_name = dto.file.FileName,
                file_path = $"/uploads/execution/{safeFileName}",
                uploaded_at = DateTime.Now
            };

            _context.ExecutionAttachments.Add(attachment);
            await _context.SaveChangesAsync();

            return Ok(attachment);
        }
    }
}