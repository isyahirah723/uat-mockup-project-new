using Microsoft.EntityFrameworkCore;
using UAT_System_API.Models;
namespace UAT_System_API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<Users> Users { get; set; }
        public DbSet<TestCycles> TestCycles { get; set; }
        public DbSet<TestCases> TestCases { get; set; }
        public DbSet<TestSteps> TestSteps { get; set; }
        public DbSet<TestFeedbacks> TestFeedbacks { get; set; }
        public DbSet<ExecutionSteps> ExecutionSteps { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<ExecutionAttachments> ExecutionAttachments { get; set; }
    }
}