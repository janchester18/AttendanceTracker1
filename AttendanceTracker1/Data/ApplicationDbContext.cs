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
    }
}
