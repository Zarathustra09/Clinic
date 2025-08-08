using Clinic.Models;
using Microsoft.EntityFrameworkCore;

namespace Clinic.DataConnection
{
    public class SqlServerDbContext : DbContext
    {
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Branches> Branches { get; set; }
        public DbSet<TimeSlot> TimeSlots { get; set; }

        public SqlServerDbContext(DbContextOptions<SqlServerDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure table mappings
            modelBuilder.Entity<User>().ToTable("User", "Clinic");
            modelBuilder.Entity<Branches>().ToTable("Branches", "Clinic");
            modelBuilder.Entity<Appointment>().ToTable("Appointment", "Clinic");
            modelBuilder.Entity<TimeSlot>().ToTable("TimeSlot", "Clinic");

            // Configure Appointment relationships explicitly
            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.User)
                .WithMany(u => u.Appointments)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete

            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Doctor)
                .WithMany() // No navigation property on User for Doctor appointments
                .HasForeignKey(a => a.DoctorId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete

            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Branch)
                .WithMany(b => b.Appointments)
                .HasForeignKey(a => a.BranchId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.TimeSlot)
                .WithOne(ts => ts.Appointment)
                .HasForeignKey<Appointment>(a => a.TimeSlotId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure TimeSlot relationships
            modelBuilder.Entity<TimeSlot>()
                .HasOne(ts => ts.Doctor)
                .WithMany() // No navigation property on User for TimeSlots
                .HasForeignKey(ts => ts.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure unique constraint for TimeSlot-Appointment relationship
            modelBuilder.Entity<Appointment>()
                .HasIndex(a => a.TimeSlotId)
                .IsUnique();

            base.OnModelCreating(modelBuilder);
        }
    }
}