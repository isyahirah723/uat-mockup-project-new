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
        public DbSet<SystemSettings> SystemSettings { get; set; }
        public DbSet<TestAssignments> TestAssignments { get; set; }
        public DbSet<TestRuns> TestRuns { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SystemSettings>(entity =>
            {
                entity.ToTable("system_settings", tb => tb.UseSqlOutputClause(false));
            });

            modelBuilder.Entity<ExecutionSteps>()
                .HasOne<TestRuns>()
                .WithMany()
                .HasForeignKey(e => e.run_id_fk)
                .OnDelete(DeleteBehavior.Cascade);

            base.OnModelCreating(modelBuilder);
        }
           

    }
}