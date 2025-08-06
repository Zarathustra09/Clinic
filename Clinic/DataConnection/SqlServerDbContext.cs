using Clinic.Models;
using Microsoft.EntityFrameworkCore;

namespace Clinic.DataConnection
{
    public class SqlServerDbContext : DbContext
    {
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Branches> Branches { get; set; }

        public SqlServerDbContext(DbContextOptions<SqlServerDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().ToTable("User", "Clinic");
            modelBuilder.Entity<Branches>().ToTable("Branches", "Clinic");
            modelBuilder.Entity<Appointment>().ToTable("Appointment", "Clinic");
            base.OnModelCreating(modelBuilder);
        }
    }
}