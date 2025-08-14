using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using api_itm.Data.Entity;
using api_itm.Data.Entity.Disability;
using api_itm.Data.Entity.api_itm.Domain.Employees;

namespace api_itm
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; } 
        public DbSet<DisabilityType> DisabilityTypes { get; set; }
        public DbSet<DisabilityGrade> DisabilityGrades { get; set; }
        public DbSet<Person> People { get; set; }

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

            modelBuilder.Entity<User>()
         .ToTable("utilizator") // PostgreSQL table name
         .HasKey(u => u.IdUser); // C# key

            base.OnModelCreating(modelBuilder);

            // Auto-apply all IEntityTypeConfiguration<T> in this assembly
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
    }
}
