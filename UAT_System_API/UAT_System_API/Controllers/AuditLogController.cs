using Microsoft.AspNetCore.Mvc;
using UAT_System_API.Data;
using UAT_System_API.Models;
using Microsoft.EntityFrameworkCore;

namespace UAT_System_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuditLogController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AuditLogController(AppDbContext context)
        {
            _context = context;
        }

      
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AuditLog>>> GetAuditLogs()
        {
            return await _context.AuditLogs
                .OrderByDescending(a => a.DtCreated)
                .ToListAsync();
        }

        
        [HttpGet("{id}")]
        public async Task<ActionResult<AuditLog>> GetAuditLog(int id)
        {
            var log = await _context.AuditLogs.FindAsync(id);
            if (log == null)
            {
                return NotFound();
            }
            return log;
        }

       
        [HttpPost]
        public async Task<ActionResult<AuditLog>> PostAuditLog(AuditLog log)
        {
            log.DtCreated = DateTime.Now;
            _context.AuditLogs.Add(log);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetAuditLog), new { id = log.Id }, log);
        }
    }
}