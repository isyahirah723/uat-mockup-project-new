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
        private readonly IWebHostEnvironment _env;

        public ExecutionAttachmentsController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        
        [HttpGet("by-step/{executionStepId}")]
        public async Task<ActionResult<IEnumerable<ExecutionAttachments>>> GetByStep(int executionStepId)
        {
            return await _context.ExecutionAttachments
                .Where(a => a.execution_step_id == executionStepId)
                .OrderByDescending(a => a.uploaded_at)
                .ToListAsync();
        }

        
        [HttpPost("Upload/{executionStepId}")]
        [RequestSizeLimit(20_000_000)]
        public async Task<ActionResult<ExecutionAttachments>> Upload(int executionStepId, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("File is required.");

            var stepExists = await _context.ExecutionSteps.AnyAsync(s => s.id == executionStepId);
            if (!stepExists)
                return BadRequest($"Execution step {executionStepId} does not exist.");

            var uploadsRoot = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "uploads", "execution-steps", executionStepId.ToString());
            Directory.CreateDirectory(uploadsRoot);

            var safeFileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
            var fullPath = Path.Combine(uploadsRoot, safeFileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var relativePath = $"/uploads/execution-steps/{executionStepId}/{safeFileName}";

            var attachment = new ExecutionAttachments
            {
                execution_step_id = executionStepId,
                file_name = file.FileName,
                file_path = relativePath,
                uploaded_at = DateTime.Now
            };

            _context.ExecutionAttachments.Add(attachment);
            await _context.SaveChangesAsync();

            return Ok(attachment);
        }

       
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var attachment = await _context.ExecutionAttachments.FindAsync(id);
            if (attachment == null) return NotFound();

            if (!string.IsNullOrEmpty(attachment.file_path))
            {
                var fullPath = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), attachment.file_path.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));
                if (System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);
                }
            }

            _context.ExecutionAttachments.Remove(attachment);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}