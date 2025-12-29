using GroceryMVC.Models;
using Microsoft.EntityFrameworkCore;

namespace GroceryMVC.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<GroceryItem> GroceryItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // optional explicit key mapping if you prefer fluent API:
            modelBuilder.Entity<GroceryItem>()
                .HasKey(g => g.GroceryId);

            // optional - map column names if they differ:
            // modelBuilder.Entity<GroceryItem>().Property(g => g.GroceryId).HasColumnName("GroceryId");
        }
    }
}
