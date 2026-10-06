using Microsoft.AspNetCore.Mvc;
using UAT_System_API.Data;
using UAT_System_API.Models;
using Microsoft.EntityFrameworkCore;

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
        public async Task<ActionResult<IEnumerable<TestSteps>>> GetSteps([FromQuery] int testCaseId)
        {
            return await _context.TestSteps
                .Where(s => s.test_case_id == testCaseId)
                .OrderBy(s => s.step_order)
                .ToListAsync();
        }
    }
}