using Microsoft.AspNetCore.Mvc;
using UAT_System_API.Data;
using UAT_System_API.Models;
using Microsoft.EntityFrameworkCore;

namespace UAT_System_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestRunsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TestRunsController(AppDbContext context)
        {
            _context = context;
        }

       
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetRuns()
        {
            var data = await (from r in _context.TestRuns
                               join tc in _context.TestCases on r.test_case_id equals tc.id into tcs
                               from tc in tcs.DefaultIfEmpty()
                               join u in _context.Users on r.executed_by equals u.id into uu
                               from u in uu.DefaultIfEmpty()
                               select new
                               {
                                   r.id,
                                   r.test_case_id,
                                   test_case_code = tc != null ? tc.test_case_code : null,
                                   title = tc != null ? tc.title : null,
                                   r.test_assignment_id,
                                   r.executed_by,
                                   user_name = u != null ? u.full_name : null,
                                   r.run_status,
                                   r.started_at,
                                   r.completed_at,
                                   r.created_at
                               })
                               .OrderByDescending(r => r.created_at)
                               .ToListAsync();

            return Ok(data);
        }

        
        [HttpGet("by-testcase/{testCaseId}")]
        public async Task<ActionResult<IEnumerable<object>>> GetRunsByTestCase(int testCaseId)
        {
            var data = await (from r in _context.TestRuns
                               where r.test_case_id == testCaseId
                               join u in _context.Users on r.executed_by equals u.id into uu
                               from u in uu.DefaultIfEmpty()
                               select new
                               {
                                   r.id,
                                   r.executed_by,
                                   user_name = u != null ? u.full_name : null,
                                   r.run_status,
                                   r.started_at,
                                   r.completed_at
                               })
                               .OrderByDescending(r => r.started_at)
                               .ToListAsync();

            return Ok(data);
        }

       
        [HttpGet("my")]
        public async Task<ActionResult<IEnumerable<object>>> GetMyRuns([FromQuery] int userId)
        {
            var data = await (from r in _context.TestRuns
                               where r.executed_by == userId
                               join tc in _context.TestCases on r.test_case_id equals tc.id into tcs
                               from tc in tcs.DefaultIfEmpty()
                               select new
                               {
                                   r.id,
                                   r.test_case_id,
                                   title = tc != null ? tc.title : null,
                                   r.run_status,
                                   r.started_at,
                                   r.completed_at
                               })
                               .OrderByDescending(r => r.started_at)
                               .ToListAsync();

            return Ok(data);
        }

        
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetRun(int id)
        {
            var run = await (from r in _context.TestRuns
                              where r.id == id
                              join tc in _context.TestCases on r.test_case_id equals tc.id into tcs
                              from tc in tcs.DefaultIfEmpty()
                              join u in _context.Users on r.executed_by equals u.id into uu
                              from u in uu.DefaultIfEmpty()
                              select new
                              {
                                  r.id,
                                  r.test_case_id,
                                  test_case_code = tc != null ? tc.test_case_code : null,
                                  title = tc != null ? tc.title : null,
                                  r.test_assignment_id,
                                  r.executed_by,
                                  user_name = u != null ? u.full_name : null,
                                  r.run_status,
                                  r.started_at,
                                  r.completed_at,
                                  r.created_at
                              })
                              .FirstOrDefaultAsync();

            if (run == null) return NotFound();
            return Ok(run);
        }

       
        [HttpPost]
        public async Task<ActionResult<TestRuns>> PostRun(TestRuns run)
        {
            
            if (run.test_assignment_id != null)
            {
                var assignmentExists = await _context.TestAssignments.AnyAsync(a =>
                    a.id == run.test_assignment_id &&
                    a.user_id == run.executed_by &&
                    a.test_case_id == run.test_case_id);

                if (!assignmentExists)
                    return BadRequest("This user is not assigned to this test case.");
            }

            run.created_at = DateTime.Now;
            if (run.started_at == null) run.started_at = DateTime.Now;
            if (string.IsNullOrEmpty(run.run_status)) run.run_status = "In Progress";

            _context.TestRuns.Add(run);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetRun), new { id = run.id }, run);
        }

        
        [HttpPut("{id}")]
        public async Task<IActionResult> PutRun(int id, TestRuns updated)
        {
            var run = await _context.TestRuns.FindAsync(id);
            if (run == null) return NotFound();

            if (!string.IsNullOrEmpty(updated.run_status))
                run.run_status = updated.run_status;

            if (updated.started_at != null)
                run.started_at = updated.started_at;

            if (updated.completed_at != null)
                run.completed_at = updated.completed_at;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        
     [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRun(int id)
        {
            
            var relatedSteps = _context.ExecutionSteps.Where(e => e.run_id_fk == id);
            _context.ExecutionSteps.RemoveRange(relatedSteps);

            
            var run = await _context.TestRuns.FindAsync(id);
            if (run == null) return NotFound();

            
            _context.TestRuns.Remove(run);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}