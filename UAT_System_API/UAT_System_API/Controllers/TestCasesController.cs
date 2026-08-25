using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UAT_System_API.Data;
using UAT_System_API.Models;

namespace UAT_System_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestCasesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TestCasesController(AppDbContext context)
        {
            _context = context;
        }

        
        private async Task<string> ResolveUserName(int? userId)
        {
            if (!userId.HasValue) return "System";
            var user = await _context.Users.FindAsync(userId.Value);
            return user?.full_name ?? userId.Value.ToString();
        }

       
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TestCases>>> GetTestCases()
        {
            return await _context.TestCases
                .Include(tc => tc.steps)
                .Include(tc => tc.feedbacks)
                .ToListAsync();
        }

        
        [HttpGet("{id}")]
        public async Task<ActionResult<TestCases>> GetTestCase(int id)
        {
            var testCase = await _context.TestCases
                .Include(tc => tc.steps)
                .Include(tc => tc.feedbacks)
                .FirstOrDefaultAsync(tc => tc.id == id);

            if (testCase == null)
            {
                return NotFound();
            }

            return testCase;
        }

       
        [HttpPost]
        public async Task<ActionResult<TestCases>> PostTestCases(TestCases testCase)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }


            if (testCase.cycle_id <= 0)
            {
                testCase.cycle_id = null;
            }

            if (string.IsNullOrWhiteSpace(testCase.test_case_code))
            {
                var count = await _context.TestCases.CountAsync();
                testCase.test_case_code = "TC-" + (count + 1).ToString("000");

               
                while (await _context.TestCases.AnyAsync(t => t.test_case_code == testCase.test_case_code))
                {
                    count++;
                    testCase.test_case_code = "TC-" + (count + 1).ToString("000");
                }
            }

            _context.TestCases.Add(testCase);
            await _context.SaveChangesAsync();

            _context.AuditLogs.Add(new AuditLog
            {
                RunId = testCase.test_case_code,
                Action = "CREATE",
                StatusOld = null,
                StatusNew = testCase.status,
                Title = testCase.title,
                Details = "Test case created",
                CrtUserId = await ResolveUserName(testCase.created_by),
                DtCreated = DateTime.Now
            });
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTestCase), new { id = testCase.id }, testCase);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutTestCases(int id, TestCases testCase)
        {
            if (id != testCase.id)
            {
                return BadRequest("ID Mismatch");
            }


            var existingTestCase = await _context.TestCases.FindAsync(id);
            if (existingTestCase == null)
            {
                return NotFound();
            }

            var oldStatus = existingTestCase.status;

            existingTestCase.test_case_code = testCase.test_case_code;
            existingTestCase.title = testCase.title;
            existingTestCase.test_department = testCase.test_department;
            existingTestCase.version_tag = testCase.version_tag;
            existingTestCase.module = testCase.module;
            existingTestCase.cycle_id = testCase.cycle_id;
            existingTestCase.priority = testCase.priority;
            existingTestCase.status = testCase.status;
            existingTestCase.assigned_to = testCase.assigned_to;
            existingTestCase.test_description = testCase.test_description;
            existingTestCase.dependencies = testCase.dependencies;
            existingTestCase.conditions = testCase.conditions;
            existingTestCase.control_notes = testCase.control_notes;

            try
            {
                await _context.SaveChangesAsync();

                _context.AuditLogs.Add(new AuditLog
                {
                    RunId = existingTestCase.test_case_code,
                    Action = "UPDATE",
                    StatusOld = oldStatus,
                    StatusNew = existingTestCase.status,
                    Title = existingTestCase.title,
                    Details = "Test case updated",
                    CrtUserId = await ResolveUserName(existingTestCase.created_by),
                    DtCreated = DateTime.Now
                });
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.TestCases.Any(e => e.id == id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Ok(existingTestCase);
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTestCases(int id)
        {
            var testCase = await _context.TestCases.FindAsync(id);
            if (testCase == null)
            {
                return NotFound();
            }

            var relatedSteps = _context.TestSteps.Where(s => s.test_case_id == id);
            _context.TestSteps.RemoveRange(relatedSteps);

            var relatedFeedbacks = _context.TestFeedbacks.Where(f => f.test_case_id == id);
            _context.TestFeedbacks.RemoveRange(relatedFeedbacks);

            _context.TestCases.Remove(testCase);
            await _context.SaveChangesAsync();

            _context.AuditLogs.Add(new AuditLog
            {
                RunId = testCase.test_case_code,
                Action = "DELETE",
                StatusOld = testCase.status,
                StatusNew = null,
                Title = testCase.title,
                Details = "Test case deleted",
                CrtUserId = await ResolveUserName(testCase.created_by),
                DtCreated = DateTime.Now
            });
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}