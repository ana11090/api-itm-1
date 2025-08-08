using Microsoft.EntityFrameworkCore;
using api_itm.Models;
using System.Diagnostics;

namespace api_itm
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
         .ToTable("utilizator") // PostgreSQL table name
         .HasKey(u => u.IdUser); // C# key

            modelBuilder.Entity<User>().Property(u => u.IdUser).HasColumnName("idutilizator");
            modelBuilder.Entity<User>().Property(u => u.Username).HasColumnName("userutilizator");
            modelBuilder.Entity<User>().Property(u => u.Password).HasColumnName("parolautilizator");
        }
    }
}
