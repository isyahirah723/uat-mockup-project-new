using Microsoft.AspNetCore.Mvc;
using UAT_System_API.Data;
using UAT_System_API.Models;
using Microsoft.EntityFrameworkCore;

namespace UAT_System_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsersController(AppDbContext context)
        {
            _context = context;
        }

        // GET api/Users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Users>>> GetUsers()
        {
            return await _context.Users.ToListAsync();
        }
    }
}