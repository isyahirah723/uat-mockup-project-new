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
public async Task<ActionResult<TestCases>> PostTestCases([FromBody] TestCases testCase)
{
    if (!ModelState.IsValid)
    {
        return BadRequest(ModelState);
    }

    // Automatik tetapkan atau semak 'created_by' untuk elak Foreign Key violation
    if (!testCase.created_by.HasValue || testCase.created_by.Value <= 0)
    {
        var defaultUser = await _context.Users.FirstOrDefaultAsync();
        if (defaultUser != null)
        {
            testCase.created_by = defaultUser.id; // Ambil ID user pertama yang wujud dalam DB
        }
        else
        {
            return BadRequest(new { message = "Tiada sebarang rekod User di dalam pangkalan data. Sila masukkan sekurang-kurangnya satu user." });
        }
    }
    else
    {
        var userExists = await _context.Users.AnyAsync(u => u.id == testCase.created_by.Value);
        if (!userExists)
        {
            return BadRequest(new { message = $"User dengan ID {testCase.created_by} tidak wujud dalam pangkalan data." });
        }
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

    if (string.IsNullOrWhiteSpace(testCase.test_number))
    {
        var datePart = DateTime.Now.ToString("yyyyMMdd");
        var prefix = $"TN-{datePart}-";

        var todayCount = await _context.TestCases
            .CountAsync(t => t.test_number != null && t.test_number.StartsWith(prefix));

        testCase.test_number = prefix + (todayCount + 1).ToString("000");

        while (await _context.TestCases.AnyAsync(t => t.test_number == testCase.test_number))
        {
            todayCount++;
            testCase.test_number = prefix + (todayCount + 1).ToString("000");
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
        public async Task<IActionResult> PutTestCases(int id, [FromBody] TestCases testCase)
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
            existingTestCase.cycle_id = (testCase.cycle_id <= 0) ? null : testCase.cycle_id;
            existingTestCase.priority = testCase.priority;
            existingTestCase.status = testCase.status;
            existingTestCase.assigned_to = testCase.assigned_to;
            existingTestCase.test_description = testCase.test_description;
            existingTestCase.dependencies = testCase.dependencies;
            existingTestCase.conditions = testCase.conditions;
            existingTestCase.control_notes = testCase.control_notes;

            var existingSteps = await _context.TestSteps
                .Where(s => s.test_case_id == id)
                .ToListAsync();

            var incoming = testCase.steps ?? new List<TestSteps>();
            var incomingIds = incoming.Where(s => s.id > 0).Select(s => s.id).ToHashSet();

            var stepsToRemove = existingSteps.Where(es => !incomingIds.Contains(es.id)).ToList();
            if (stepsToRemove.Count > 0)
                _context.TestSteps.RemoveRange(stepsToRemove);

            foreach (var incomingStep in incoming)
            {
                if (incomingStep.id > 0)
                {
                    var matching = existingSteps.FirstOrDefault(es => es.id == incomingStep.id);
                    if (matching != null)
                    {
                        matching.step_order = incomingStep.step_order;
                        matching.description = incomingStep.description;
                        matching.expected = incomingStep.expected;
                        matching.actual = incomingStep.actual;
                        matching.pass_fail = incomingStep.pass_fail;
                    }
                }
                else
                {
                    _context.TestSteps.Add(new TestSteps
                    {
                        test_case_id = id,
                        step_order = incomingStep.step_order,
                        description = incomingStep.description,
                        expected = incomingStep.expected,
                        actual = incomingStep.actual,
                        pass_fail = incomingStep.pass_fail
                    });
                }
            }

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

        [HttpPatch("{id}/approve")]
        public async Task<IActionResult> SetApproval(int id, [FromBody] ApprovalRequest request)
        {
            var testCase = await _context.TestCases.FindAsync(id);
            if (testCase == null)
            {
                return NotFound();
            }

            var oldApproval = testCase.approval_status;

            testCase.approval_status = request.approved ? "Approved" : "Not Approved";
            testCase.approved_by = request.approvedBy;
            testCase.approved_at = DateTime.Now;
            testCase.updated_at = DateTime.Now;

            await _context.SaveChangesAsync();

            _context.AuditLogs.Add(new AuditLog
            {
                RunId = testCase.test_case_code,
                Action = "UPDATE",
                StatusOld = oldApproval,
                StatusNew = testCase.approval_status,
                Title = testCase.title,
                Details = request.approved ? "Test case approved by admin" : "Test case rejected by admin",
                CrtUserId = await ResolveUserName(request.approvedBy),
                DtCreated = DateTime.Now
            });
            await _context.SaveChangesAsync();

            return Ok(new { testCase.id, testCase.approval_status, testCase.approved_by, testCase.approved_at });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTestCases(int id)
        {
            var testCase = await _context.TestCases.FindAsync(id);
            if (testCase == null)
            {
                return NotFound();
            }

            var assignmentIds = await _context.TestAssignments
                .Where(a => a.test_case_id == id)
                .Select(a => a.id)
                .ToListAsync();

            var runIds = assignmentIds.Any()
                ? await _context.TestRuns
                    .Where(r => r.test_assignment_id != null && assignmentIds.Contains(r.test_assignment_id.Value))
                    .Select(r => r.id)
                    .ToListAsync()
                : new List<int>();

            var relatedFeedbacks = await _context.TestFeedbacks
                .Where(f => f.test_case_id == id)
                .ToListAsync();
            _context.TestFeedbacks.RemoveRange(relatedFeedbacks);
            await _context.SaveChangesAsync();

            if (runIds.Any())
            {
                var relatedStepIds = await _context.ExecutionSteps
                    .Where(es => es.run_id_fk != null && runIds.Contains(es.run_id_fk.Value))
                    .Select(es => es.id)
                    .ToListAsync();

                if (relatedStepIds.Any())
                {
                    var relatedAttachments = await _context.ExecutionAttachments
                        .Where(ea => relatedStepIds.Contains(ea.execution_step_id))
                        .ToListAsync();
                    _context.ExecutionAttachments.RemoveRange(relatedAttachments);
                    await _context.SaveChangesAsync();
                }

                var relatedExecSteps = await _context.ExecutionSteps
                    .Where(es => es.run_id_fk != null && runIds.Contains(es.run_id_fk.Value))
                    .ToListAsync();
                _context.ExecutionSteps.RemoveRange(relatedExecSteps);
                await _context.SaveChangesAsync();

                var relatedRuns = await _context.TestRuns
                    .Where(r => r.test_assignment_id != null && assignmentIds.Contains(r.test_assignment_id.Value))
                    .ToListAsync();
                _context.TestRuns.RemoveRange(relatedRuns);
                await _context.SaveChangesAsync();
            }

            var relatedAssignments = await _context.TestAssignments
                .Where(a => a.test_case_id == id)
                .ToListAsync();
            _context.TestAssignments.RemoveRange(relatedAssignments);
            await _context.SaveChangesAsync();

            var relatedSteps = await _context.TestSteps
                .Where(s => s.test_case_id == id)
                .ToListAsync();
            _context.TestSteps.RemoveRange(relatedSteps);
            await _context.SaveChangesAsync();

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

        public class ApprovalRequest
        {
            public bool approved { get; set; }
            public int? approvedBy { get; set; }
        }
    }
}