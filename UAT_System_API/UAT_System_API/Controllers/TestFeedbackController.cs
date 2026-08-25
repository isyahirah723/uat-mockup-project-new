using Microsoft.AspNetCore.Mvc;
using UAT_System_API.Data;
using UAT_System_API.Models;
using Microsoft.EntityFrameworkCore;

namespace UAT_System_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestFeedbacksController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TestFeedbacksController(AppDbContext context)
        {
            _context = context;
        }

        
        [HttpGet]
        public async Task<ActionResult> GetTestFeedbacks()
        {
            var query = from f in _context.TestFeedbacks
                        join tc in _context.TestCases on f.test_case_id equals tc.id into tcJoin
                        from testCase in tcJoin.DefaultIfEmpty()
                        select new
                        {
                            f.id,
                            f.test_case_id,
                            test_case_code = testCase != null ? testCase.test_case_code : null,
                            test_case_title = testCase != null ? testCase.title : null,
                            f.user_name,
                            f.rating,
                            f.comment,
                            f.created_at
                        };

            return Ok(await query.OrderByDescending(f => f.created_at).ToListAsync());
        }

       
        [HttpGet("by-test-case/{testCaseId}")]
        public async Task<ActionResult> GetFeedbacksByTestCase(int testCaseId)
        {
            var query = from f in _context.TestFeedbacks
                        where f.test_case_id == testCaseId
                        join tc in _context.TestCases on f.test_case_id equals tc.id into tcJoin
                        from testCase in tcJoin.DefaultIfEmpty()
                        select new
                        {
                            f.id,
                            f.test_case_id,
                            test_case_code = testCase != null ? testCase.test_case_code : null,
                            f.user_name,
                            f.rating,
                            f.comment,
                            f.created_at
                        };

            return Ok(await query.OrderByDescending(f => f.created_at).ToListAsync());
        }

       
        [HttpPost]
        public async Task<ActionResult<TestFeedbacks>> PostTestFeedbacks(TestFeedbacks feedback)
        {
            _context.TestFeedbacks.Add(feedback);
            await _context.SaveChangesAsync();

            var testCase = await _context.TestCases.FindAsync(feedback.test_case_id);

            _context.AuditLogs.Add(new AuditLog
            {
                RunId = testCase?.test_case_code ?? feedback.test_case_id.ToString(),
                Action = "CREATE",
                StatusOld = null,
                StatusNew = feedback.rating?.ToString(),
                Title = testCase?.title ?? "Feedback",
                Details = string.IsNullOrWhiteSpace(feedback.comment) ? "Feedback submitted" : feedback.comment,
                CrtUserId = feedback.user_name ?? "System",
                DtCreated = DateTime.Now
            });
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetTestFeedbacks", new { id = feedback.id }, feedback);
        }

       
        [HttpPost("SaveBatch/{testCaseId}")]
        public async Task<IActionResult> SaveBatch(int testCaseId, [FromBody] List<TestFeedbacks> feedbacks)
        {
            if (feedbacks == null) return BadRequest("Feedbacks list is required.");

            
            var existingFeedbacks = _context.TestFeedbacks.Where(f => f.test_case_id == testCaseId);
            _context.TestFeedbacks.RemoveRange(existingFeedbacks);

            foreach (var fb in feedbacks)
            {
                fb.test_case_id = testCaseId;
                _context.TestFeedbacks.Add(fb);
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "Feedbacks saved successfully." });
        }
    }
}