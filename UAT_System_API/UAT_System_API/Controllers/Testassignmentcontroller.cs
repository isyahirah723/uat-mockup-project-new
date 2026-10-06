using Microsoft.AspNetCore.Mvc;
using UAT_System_API.Data;
using UAT_System_API.Models;
using Microsoft.EntityFrameworkCore;

namespace UAT_System_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestAssignmentsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TestAssignmentsController(AppDbContext context)
        {
            _context = context;
        }

        
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetAssignments()
        {
            var data = await (from a in _context.TestAssignments
                               join u in _context.Users on a.user_id equals u.id into uu
                               from u in uu.DefaultIfEmpty()
                               join tc in _context.TestCases on a.test_case_id equals tc.id into tcs
                               from tc in tcs.DefaultIfEmpty()
                               select new
                               {
                                   a.id,
                                   a.test_case_id,
                                   test_case_code = tc != null ? tc.test_case_code : null,
                                   title = tc != null ? tc.title : null,
                                   a.department,
                                   a.team,
                                   a.user_id,
                                   user_name = u != null ? u.full_name : null,
                                   a.status,
                                   a.assigned_at
                               })
                               .OrderByDescending(a => a.assigned_at)
                               .ToListAsync();

            return Ok(data);
        }

       
        [HttpGet("my")]
        public async Task<ActionResult<IEnumerable<object>>> GetMyAssignments([FromQuery] int userId)
        {
            var data = await (from a in _context.TestAssignments
                               where a.user_id == userId
                               join tc in _context.TestCases on a.test_case_id equals tc.id
                               join cy in _context.TestCycles on tc.cycle_id equals cy.id into cys
                               from cy in cys.DefaultIfEmpty()
                               select new
                               {
                                   assignment_id = a.id,
                                   test_case_id = tc.id,
                                   test_case_code = tc.test_case_code,
                                   title = tc.title,
                                   cycle_name = cy != null ? cy.name : null,
                                   priority = tc.priority,
                                   status = tc.status
                               })
                               .ToListAsync();

            return Ok(data);
        }

       
        [HttpGet("{id}")]
        public async Task<ActionResult<TestAssignments>> GetAssignment(int id)
        {
            var assignment = await _context.TestAssignments.FindAsync(id);
            if (assignment == null) return NotFound();
            return assignment;
        }

       [HttpPost]
        public async Task<ActionResult<TestAssignments>> PostAssignment(TestAssignments assignment)
        {
            // 1. Validate that the referenced test_case_id exists
            var testCaseExists = await _context.TestCases.AnyAsync(tc => tc.id == assignment.test_case_id);
            if (!testCaseExists)
            {
                return BadRequest(new { message = $"Test case with ID {assignment.test_case_id} does not exist in the database." });
            }

            // 2. Validate that the referenced user_id exists (if provided)
            if (assignment.user_id != 0)
            {
                var userExists = await _context.Users.AnyAsync(u => u.id == assignment.user_id);
                if (!userExists)
                {
                    return BadRequest(new { message = $"User with ID {assignment.user_id} does not exist in the database." });
                }
            }

            assignment.assigned_at = DateTime.Now;
            if (string.IsNullOrEmpty(assignment.status))
                assignment.status = "Pending";

            try
            {
                _context.TestAssignments.Add(assignment);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                var innerError = ex.InnerException?.Message ?? ex.Message;
                return BadRequest(new { message = "Failed to save assignment due to a database constraint.", error = innerError });
            }

            return CreatedAtAction(nameof(GetAssignment), new { id = assignment.id }, assignment);
        }
        
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAssignment(int id, TestAssignments updated)
        {
            var assignment = await _context.TestAssignments.FindAsync(id);
            if (assignment == null) return NotFound();

            if (updated.user_id != 0)
                assignment.user_id = updated.user_id;
            if (updated.test_case_id != 0)
                assignment.test_case_id = updated.test_case_id;
            if (!string.IsNullOrEmpty(updated.department))
                assignment.department = updated.department;
            if (!string.IsNullOrEmpty(updated.team))
                assignment.team = updated.team;
            if (!string.IsNullOrEmpty(updated.status))
                assignment.status = updated.status;
            if (updated.assigned_by != null)
                assignment.assigned_by = updated.assigned_by;

            await _context.SaveChangesAsync();
            return NoContent();
        }

       
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAssignment(int id)
        {
            var assignment = await _context.TestAssignments.FindAsync(id);
            if (assignment == null) return NotFound();

            _context.TestAssignments.Remove(assignment);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}