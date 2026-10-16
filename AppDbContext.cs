using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Models;

namespace RaceDay.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<RaceEvent> Events { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<EventRoute> Routes { get; set; }
        public DbSet<Enrolment> Enrolments { get; set; }
        public DbSet<RaceResult> Results { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
            modelBuilder.Entity<User>().Property(u => u.Email).HasMaxLength(200);
            modelBuilder.Entity<User>().Property(u => u.Role).HasMaxLength(20);

            modelBuilder.Entity<RaceEvent>().HasKey(e => e.EventId);
            modelBuilder.Entity<EventRoute>().HasKey(r => r.RouteId);
            modelBuilder.Entity<RaceResult>().HasKey(r => r.ResultId);

            modelBuilder.Entity<RaceEvent>().ToTable("Events");
            modelBuilder.Entity<RaceEvent>().Property(e => e.EventDate).HasColumnType("date");
            modelBuilder.Entity<RaceEvent>().Property(e => e.Distance).HasPrecision(6, 2);

            modelBuilder.Entity<Category>().Property(c => c.Distance).HasPrecision(6, 2);

            modelBuilder.Entity<EventRoute>().ToTable("Routes");

            modelBuilder.Entity<Enrolment>()
                .HasIndex(e => new { e.ParticipantId, e.CategoryId }).IsUnique();

            modelBuilder.Entity<RaceResult>().ToTable("Results");
            modelBuilder.Entity<RaceResult>().HasIndex(r => r.EnrolmentId).IsUnique();

            // Foreign keys (cascade is turned off - the controllers delete dependent rows themselves)
            modelBuilder.Entity<RaceEvent>().HasOne<User>().WithMany()
                .HasForeignKey(e => e.OrganiserId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Category>().HasOne<RaceEvent>().WithMany()
                .HasForeignKey(c => c.EventId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<EventRoute>().HasOne<RaceEvent>().WithMany()
                .HasForeignKey(r => r.EventId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Enrolment>().HasOne<User>().WithMany()
                .HasForeignKey(e => e.ParticipantId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Enrolment>().HasOne<Category>().WithMany()
                .HasForeignKey(e => e.CategoryId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RaceResult>().HasOne<Enrolment>().WithMany()
                .HasForeignKey(r => r.EnrolmentId).OnDelete(DeleteBehavior.Restrict);
        }
    }
}
