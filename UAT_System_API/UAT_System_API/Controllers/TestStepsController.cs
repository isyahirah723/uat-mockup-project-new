using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UAT_System_API.Data;
using UAT_System_API.Models;

namespace UAT_System_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestStepsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TestStepsController(AppDbContext context)
        {
            _context = context;
        }

        
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TestSteps>>> GetTestSteps()
        {
            return await _context.TestSteps.ToListAsync();
        }

        
        [HttpGet("{id}")]
        public async Task<ActionResult<TestSteps>> GetTestStep(int id)
        {
            var testStep = await _context.TestSteps.FindAsync(id);

            if (testStep == null)
            {
                return NotFound();
            }

            return testStep;
        }

        
        [HttpPost]
        public async Task<ActionResult<TestSteps>> PostTestStep(TestSteps testStep)
        {
            _context.TestSteps.Add(testStep);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTestStep), new { id = testStep.id }, testStep);
        }

        
        [HttpPost("SaveBatch/{testCaseId}")]
        public async Task<IActionResult> SaveBatch(int testCaseId, [FromBody] List<TestSteps> steps)
        {
            if (steps == null) return BadRequest("Steps list is required.");

           
            var existingSteps = _context.TestSteps.Where(s => s.test_case_id == testCaseId);
            _context.TestSteps.RemoveRange(existingSteps);

            foreach (var step in steps)
            {
                step.test_case_id = testCaseId;
                _context.TestSteps.Add(step);
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Steps saved successfully." });
        }
    }
}