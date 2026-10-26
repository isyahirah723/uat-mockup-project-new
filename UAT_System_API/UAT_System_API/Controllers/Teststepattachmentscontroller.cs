using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UAT_System_API.Data;
using UAT_System_API.Models;

namespace UAT_System_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestStepAttachmentsController : ControllerBase
    {
        private const long MaxBytes = 10 * 1024 * 1024; // 10 MB
        private static readonly string[] AllowedExt =
            { ".png", ".jpg", ".jpeg", ".gif", ".webp", ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".txt", ".csv" };
        private static readonly string[] ImageExt = { ".png", ".jpg", ".jpeg", ".gif", ".webp" };

        private readonly AppDbContext _context;

        public TestStepAttachmentsController(AppDbContext context)
        {
            _context = context;
        }

        // Absolute URL so the Vue app (a different origin) can open the file
        private object Shape(TestStepAttachments a) => new
        {
            a.id,
            a.test_step_id,
            a.file_name,
            a.file_path,
            url = $"{Request.Scheme}://{Request.Host}{a.file_path}",
            is_image = ImageExt.Contains(Path.GetExtension(a.file_name).ToLowerInvariant()),
            a.uploaded_at
        };

        // GET api/TestStepAttachments/by-step/5
        // Always 200 with a list (empty when the step has no files).
        [HttpGet("by-step/{stepId:int}")]
        public async Task<ActionResult> GetByStep(int stepId)
        {
            var rows = await _context.TestStepAttachments
                .Where(a => a.test_step_id == stepId)
                .OrderBy(a => a.id)
                .ToListAsync();
            return Ok(rows.Select(Shape));
        }

        // POST api/TestStepAttachments/upload   (multipart: test_step_id, file)
        [HttpPost]
        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult> Upload([FromForm] TestStepAttachmentDto dto)
        {
            if (dto.file == null || dto.file.Length == 0)
                return BadRequest("No file uploaded.");
            if (dto.file.Length > MaxBytes)
                return BadRequest("File is too large (maximum 10 MB).");

            var ext = Path.GetExtension(dto.file.FileName).ToLowerInvariant();
            if (!AllowedExt.Contains(ext))
                return BadRequest($"File type '{ext}' is not allowed.");

            if (!await _context.TestSteps.AnyAsync(s => s.id == dto.test_step_id))
                return BadRequest($"Test step {dto.test_step_id} does not exist.");

            var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "steps");
            Directory.CreateDirectory(folder);

            var storedName = $"{Guid.NewGuid()}{ext}";
            using (var stream = new FileStream(Path.Combine(folder, storedName), FileMode.Create))
            {
                await dto.file.CopyToAsync(stream);
            }

            var attachment = new TestStepAttachments
            {
                test_step_id = dto.test_step_id,
                file_name = Path.GetFileName(dto.file.FileName),
                file_path = $"/uploads/steps/{storedName}",
                uploaded_at = DateTime.Now
            };
            _context.TestStepAttachments.Add(attachment);
            await _context.SaveChangesAsync();

            return Ok(Shape(attachment));
        }

        // DELETE api/TestStepAttachments/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var a = await _context.TestStepAttachments.FindAsync(id);
            if (a == null) return NotFound();

            var physical = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", a.file_path.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            if (System.IO.File.Exists(physical)) System.IO.File.Delete(physical);

            _context.TestStepAttachments.Remove(a);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}