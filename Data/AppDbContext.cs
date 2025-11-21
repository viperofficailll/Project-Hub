using Microsoft.EntityFrameworkCore;
using ProjectHub.Models;

namespace ProjectHub.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)

    {
        public DbSet<User>Users { get; set; }
        public DbSet<Project> Projects { get; set; }
    }
}
