using AttendanceTracker1.Models;
using Microsoft.EntityFrameworkCore;

namespace AttendanceTracker1.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base (options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<Leave> Leaves { get; set; }
        public DbSet<OvertimeConfig> OvertimeConfigs { get; set; }
        public DbSet<Overtime> Overtimes { get; set; }
        public DbSet<Log> Logs { get; set; }
        public DbSet<Holiday> Holidays { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<CashAdvanceRequest> CashAdvanceRequests { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Attendance>()
                .HasOne(a => a.User)
                .WithMany(u => u.Attendances)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>()
                .HasMany(u => u.Leaves)
                .WithOne(l => l.User)
                .HasForeignKey(l => l.UserId)
                .OnDelete(DeleteBehavior.Restrict); // Prevents multiple cascade paths

            modelBuilder.Entity<User>()
                .HasMany(u => u.Approvals)
                .WithOne(l => l.Approver)
                .HasForeignKey(l => l.ReviewedBy)
                .OnDelete(DeleteBehavior.Restrict); // Prevents multiple cascade paths

            // 🔹 Overtime Request Relationship
            modelBuilder.Entity<Overtime>()
                .HasOne(o => o.User)
                .WithMany(u => u.OvertimeRequests)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Cascade); // Delete overtime records when a user is deleted

            // 🔹 Overtime Approval Relationship
            modelBuilder.Entity<Overtime>()
                .HasOne(o => o.Approver)
                .WithMany(u => u.OvertimeApprovals)
                .HasForeignKey(o => o.ReviewedBy)
                .OnDelete(DeleteBehavior.Restrict); // Prevent multiple cascade paths

            base.OnModelCreating(modelBuilder);
        }
    }
}
