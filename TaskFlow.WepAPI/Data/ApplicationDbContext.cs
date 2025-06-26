
using Microsoft.EntityFrameworkCore;
using TaskFlow.WebAPI.Models; // So it knows about TaskItem

namespace TaskFlow.WebAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // This DbSet will represent the "TaskItems" table in our database
        public DbSet<TaskItem> TaskItems { get; set; }

        // You can add more DbSet<T> properties here for other models/tables

        public DbSet<User> Users { get; set; }
    }
}