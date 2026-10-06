using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UAT_System_API.Data;
using UAT_System_API.Models;

namespace UAT_System_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestCyclesController : ControllerBase
    {
        private readonly AppDbContext _context;
        public TestCyclesController(AppDbContext context) { _context = context; }

       
        private async Task<string> ResolveUserName(int? userId)
        {
            if (!userId.HasValue) return "System";
            var user = await _context.Users.FindAsync(userId.Value);
            return user?.full_name ?? userId.Value.ToString();
        }

        
        // Next free code for today: CY + YYMMDD + 3-digit running number
        private async Task<string> GenerateCycleCode()
        {
            var prefix = "CY" + DateTime.Now.ToString("yyMMdd");

            var codes = await _context.TestCycles
                .Where(c => c.cycle_code != null && c.cycle_code.StartsWith(prefix))
                .Select(c => c.cycle_code!)
                .ToListAsync();

            var max = 0;
            foreach (var code in codes)
            {
                if (int.TryParse(code.Substring(prefix.Length), out var n) && n > max)
                    max = n;
            }

            return prefix + (max + 1).ToString("000");
        }

        [HttpGet]
        public async Task<ActionResult> GetTestCycles()
        {
            var query = from c in _context.TestCycles
                        join u in _context.Users on c.created_by equals u.id into userJoin
                        from user in userJoin.DefaultIfEmpty()
                        join a in _context.Users on c.assigned_to equals a.id into assignJoin
                        from assign in assignJoin.DefaultIfEmpty()
                        select new
                        {
                            c.id,
                            c.cycle_code,
                            c.name,
                            c.department,
                            c.dt_start,
                            c.dt_end,
                            c.status,
                            c.created_date,
                            c.auto_assign_rule,
                            c.module,
                            c.priority,
                            c.match_department,
                            c.created_at,
                            c.updated_at,
                            c.created_by,
                            c.assigned_to,
                            created_by_name = user != null ? user.full_name : "System",
                            assigned_to_name = assign != null ? assign.full_name : "Unassigned"
                        };
            return Ok(await query.ToListAsync());
        }

       
        [HttpGet("{id}")]
        public async Task<ActionResult<TestCycles>> GetTestCycle(int id)
        {
            var cycle = await _context.TestCycles.FindAsync(id);
            if (cycle == null) return NotFound();
            return cycle;
        }

        // POST: api/TestCycles
        [HttpPost]
        public async Task<ActionResult> PostTestCycle(TestCycles cycle)
        {
            // Cycle ID format: CY + YYMMDD + 3-digit running number (e.g. CY260928001).
            // Use the code sent by the frontend, but generate a fresh one if it is blank or already taken.
            if (string.IsNullOrWhiteSpace(cycle.cycle_code) ||
                await _context.TestCycles.AnyAsync(c => c.cycle_code == cycle.cycle_code))
            {
                cycle.cycle_code = await GenerateCycleCode();
            }

            if (cycle.created_date == null)
            {
                cycle.created_date = DateTime.Now.Date;
            }

            // created_by must be a real user, otherwise the fk_cycle_creator constraint fails
            // (this used to be hardcoded to 14, which no longer exists in Users).
            if (!cycle.created_by.HasValue || cycle.created_by.Value <= 0)
            {
                var defaultUser = await _context.Users.OrderBy(u => u.id).FirstOrDefaultAsync();
                if (defaultUser == null)
                    return BadRequest(new { message = "Tiada sebarang rekod User di dalam pangkalan data. Sila masukkan sekurang-kurangnya satu user." });

                cycle.created_by = defaultUser.id;
            }
            else if (!await _context.Users.AnyAsync(u => u.id == cycle.created_by.Value))
            {
                return BadRequest(new { message = $"User dengan ID {cycle.created_by} tidak wujud dalam pangkalan data." });
            }

            if (cycle.assigned_to.HasValue &&
                !await _context.Users.AnyAsync(u => u.id == cycle.assigned_to.Value))
            {
                return BadRequest(new { message = $"User dengan ID {cycle.assigned_to} tidak wujud dalam pangkalan data." });
            }

            cycle.created_at = DateTime.Now;
            cycle.updated_at = DateTime.Now;

            _context.TestCycles.Add(cycle);
            await _context.SaveChangesAsync();

            _context.AuditLogs.Add(new AuditLog
            {
                RunId = cycle.cycle_code,
                Action = "CREATE",
                StatusOld = null,
                StatusNew = cycle.status,
                Title = cycle.name,
                Details = "Test cycle created",
                CrtUserId = await ResolveUserName(cycle.created_by),
                DtCreated = DateTime.Now
            });
            await _context.SaveChangesAsync();

            
            var query = from c in _context.TestCycles
                        join u in _context.Users on c.created_by equals u.id into userJoin
                        from user in userJoin.DefaultIfEmpty()
                        where c.id == cycle.id
                        select new
                        {
                            c.id,
                            c.cycle_code,
                            c.name,
                            c.department,
                            c.dt_start,
                            c.dt_end,
                            c.status,
                            c.created_date,
                            c.auto_assign_rule,
                            c.module,
                            c.priority,
                            c.match_department,
                            c.created_at,
                            c.updated_at,
                            c.created_by,
                            c.assigned_to,
                            created_by_name = user != null ? user.full_name : "System",
                            assigned_to_name = "Unassigned"
                        };

            var result = await query.FirstOrDefaultAsync();
            return CreatedAtAction(nameof(GetTestCycle), new { id = cycle.id }, result);
        }

        // PUT: api/TestCycles/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTestCycle(int id, TestCycles cycle)
        {
            if (id != cycle.id) return BadRequest();

            var existingCycle = await _context.TestCycles.FindAsync(id);
            if (existingCycle == null) return NotFound();

            var oldStatus = existingCycle.status;

            existingCycle.cycle_code = cycle.cycle_code;
            existingCycle.name = cycle.name;
            existingCycle.department = cycle.department;
            existingCycle.dt_start = cycle.dt_start;
            existingCycle.dt_end = cycle.dt_end;
            existingCycle.status = cycle.status;
            existingCycle.auto_assign_rule = cycle.auto_assign_rule;
            existingCycle.module = cycle.module;
            existingCycle.priority = cycle.priority;
            existingCycle.match_department = cycle.match_department;
            existingCycle.assigned_to = cycle.assigned_to;
            existingCycle.updated_at = DateTime.Now;

            try
            {
                await _context.SaveChangesAsync();

                _context.AuditLogs.Add(new AuditLog
                {
                    RunId = existingCycle.cycle_code,
                    Action = "UPDATE",
                    StatusOld = oldStatus,
                    StatusNew = existingCycle.status,
                    Title = existingCycle.name,
                    Details = "Test cycle updated",
                    CrtUserId = await ResolveUserName(existingCycle.created_by),
                    DtCreated = DateTime.Now
                });
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.TestCycles.Any(e => e.id == id)) return NotFound();
                else throw;
            }
            return NoContent();
        }

        
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTestCycle(int id)
        {
            var cycle = await _context.TestCycles.FindAsync(id);
            if (cycle == null) return NotFound();

            _context.TestCycles.Remove(cycle);
            await _context.SaveChangesAsync();

            _context.AuditLogs.Add(new AuditLog
            {
                RunId = cycle.cycle_code,
                Action = "DELETE",
                StatusOld = cycle.status,
                StatusNew = null,
                Title = cycle.name,
                Details = "Test cycle deleted",
                CrtUserId = await ResolveUserName(cycle.created_by),
                DtCreated = DateTime.Now
            });
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}