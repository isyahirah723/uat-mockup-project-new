using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UAT_System_API.Data;
using UAT_System_API.Models;

namespace UAT_System_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExecutionStepsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ExecutionStepsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ExecutionSteps>>> GetExecutionSteps()
        {
            return await _context.ExecutionSteps.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ExecutionSteps>> GetExecutionStep(int id)
        {
            var step = await _context.ExecutionSteps.FindAsync(id);
            if (step == null) return NotFound();
            return step;
        }

        // GET: api/ExecutionSteps/by-test-case/5
        [HttpGet("by-test-case/{testCaseId}")]
        public async Task<ActionResult<IEnumerable<ExecutionSteps>>> GetByTestCase(int testCaseId)
        {
            return await _context.ExecutionSteps
                .Where(s => s.test_case_id == testCaseId)
                .OrderBy(s => s.sequence_order)
                .ToListAsync();
        }

        [HttpPost]
        public async Task<ActionResult<ExecutionSteps>> PostExecutionStep(ExecutionSteps step)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            _context.ExecutionSteps.Add(step);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetExecutionStep), new { id = step.id }, step);
        }

       
        [HttpPost("SaveBatch/{testCaseId}")]
        public async Task<IActionResult> SaveBatch(int testCaseId, [FromBody] List<ExecutionSteps> steps)
        {
            if (steps == null) return BadRequest("Steps list is required.");

            var existingSteps = _context.ExecutionSteps.Where(s => s.test_case_id == testCaseId);
            _context.ExecutionSteps.RemoveRange(existingSteps);

            foreach (var step in steps)
            {
                step.test_case_id = testCaseId;
                step.dt_created = DateTime.Now;
                _context.ExecutionSteps.Add(step);
            }

            await _context.SaveChangesAsync();

            var testCase = await _context.TestCases.FindAsync(testCaseId);
            var lastStep = steps.LastOrDefault();
            var defectCount = steps.Count(s => s.has_defect);

            _context.AuditLogs.Add(new AuditLog
            {
                RunId = testCase?.test_case_code ?? testCaseId.ToString(),
                Action = "UPDATE",
                StatusOld = null,
                StatusNew = lastStep?.execution_status,
                Title = testCase?.title ?? "Execution Steps",
                Details = $"Execution steps saved ({steps.Count} step(s), {defectCount} with defect)",
                CrtUserId = lastStep?.crt_user_id ?? "System",
                DtCreated = DateTime.Now
            });
            await _context.SaveChangesAsync();

            
            var savedSteps = await _context.ExecutionSteps
                .Where(s => s.test_case_id == testCaseId)
                .OrderBy(s => s.sequence_order)
                .ToListAsync();

            return Ok(new { message = "Execution steps saved successfully.", steps = savedSteps });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutExecutionStep(int id, ExecutionSteps step)
        {
            if (id != step.id) return BadRequest();
            if (!ModelState.IsValid) return BadRequest(ModelState);

            _context.Entry(step).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.ExecutionSteps.Any(e => e.id == id)) return NotFound();
                else throw;
            }
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteExecutionStep(int id)
        {
            var step = await _context.ExecutionSteps.FindAsync(id);
            if (step == null) return NotFound();

            _context.ExecutionSteps.Remove(step);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}