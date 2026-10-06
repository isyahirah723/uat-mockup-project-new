using Microsoft.AspNetCore.Mvc;
using UAT_System_API.Data;
using UAT_System_API.Models;
using Microsoft.EntityFrameworkCore;

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

        // Next free ticket number: DEF-001, DEF-002 ...
        private async Task<string> GenerateTicketId()
        {
            var existing = await _context.ExecutionSteps
                .Where(e => e.ticket_id != null && e.ticket_id.StartsWith("DEF-"))
                .Select(e => e.ticket_id!)
                .ToListAsync();

            var max = 0;
            foreach (var t in existing)
            {
                if (int.TryParse(t.Substring(4), out var n) && n > max) max = n;
            }
            return "DEF-" + (max + 1).ToString("000");
        }

        // GET api/ExecutionSteps/defects
        // Used by FeedbackView.vue (Defects tab).
        // Literal route "defects" takes priority over "{id}", so no clash.
        [HttpGet("defects")]
        public async Task<ActionResult> GetDefects()
        {
            var defects = await (
                from es in _context.ExecutionSteps
                join tc in _context.TestCases on es.test_case_id equals tc.id
                join run in _context.TestRuns on es.run_id_fk equals (int?)run.id into runs
                from run in runs.DefaultIfEmpty()
                join u in _context.Users on (int?)run.executed_by equals (int?)u.id into users
                from u in users.DefaultIfEmpty()
                where es.has_defect
                orderby es.dt_created descending
                select new
                {
                    es.id,
                    test_case_code = tc.test_case_code,
                    title = tc.title,
                    es.step_name,
                    es.severity,
                    es.ticket_id,
                    reported_by = u != null ? u.full_name : null,
                    es.dt_created,
                    es.actual_result,
                    es.comments
                }
            ).ToListAsync();

            return Ok(defects);
        }

        // GET api/ExecutionSteps/by-run/5
        // Used by TestRunsPage.vue and TestCasepage.vue.
        // Always returns 200 with a list (empty if the run has no steps yet).
        [HttpGet("by-run/{runId:int}")]
        public async Task<ActionResult<IEnumerable<ExecutionSteps>>> GetByRun(int runId)
        {
            var steps = await _context.ExecutionSteps
                .Where(es => es.run_id_fk == runId)
                .OrderBy(es => es.sequence_order)
                .ToListAsync();

            return Ok(steps);
        }

        // GET api/ExecutionSteps/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ExecutionSteps>> GetExecutionStep(int id)
        {
            var step = await _context.ExecutionSteps.FindAsync(id);
            if (step == null) return NotFound();
            return step;
        }

        // POST api/ExecutionSteps
        // Called the first time a step in a run is saved.
        [HttpPost]
        public async Task<ActionResult<ExecutionSteps>> PostExecutionStep(ExecutionSteps step)
        {
            step.dt_created = DateTime.Now;

            if (string.IsNullOrWhiteSpace(step.ticket_id) &&
                (step.execution_status == "Failed" || step.has_defect))
                step.ticket_id = await GenerateTicketId();

            _context.ExecutionSteps.Add(step);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetExecutionStep), new { id = step.id }, step);
        }

        // PUT api/ExecutionSteps/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutExecutionStep(int id, ExecutionSteps updated)
        {
            var step = await _context.ExecutionSteps.FindAsync(id);
            if (step == null) return NotFound();

            step.execution_status = updated.execution_status;
            step.actual_result = updated.actual_result;
            step.has_defect = updated.has_defect;
            step.severity = updated.severity;

            // a step keeps its ticket number once it has one; a Failed step gets the next DEF number
            if (string.IsNullOrWhiteSpace(step.ticket_id))
            {
                if (!string.IsNullOrWhiteSpace(updated.ticket_id))
                    step.ticket_id = updated.ticket_id.Trim();
                else if (updated.execution_status == "Failed" || updated.has_defect)
                    step.ticket_id = await GenerateTicketId();
            }

            step.comments = updated.comments;
            step.required_role = updated.required_role;

            await _context.SaveChangesAsync();
            return Ok(step);
        }
    }
}