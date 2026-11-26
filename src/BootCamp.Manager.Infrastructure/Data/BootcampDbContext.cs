using BootCamp.Manager.Domain.Entities;
using Microsoft.EntityFrameworkCore;
namespace BootCamp.Manager.Infrastructure.Data;
public class BootcampDbContext(DbContextOptions<BootcampDbContext> options) : DbContext(options)
{
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Instructor> Instructors => Set<Instructor>();
    public DbSet<Session> Sessions => Set<Session>();
    public DbSet<SessionParticipant> SessionParticipants => Set<SessionParticipant>();
    public DbSet<Evaluation> Evaluations => Set<Evaluation>();
    protected override void OnModelCreating(ModelBuilder model)
    {
        model.Entity<Student>(e => {
            e.Property(x => x.FullName).HasMaxLength(100).IsRequired();
            e.Property(x => x.Email).HasMaxLength(200).IsRequired();
            e.HasIndex(x => x.Email).IsUnique();
        });
        model.Entity<Instructor>(e => {
            e.Property(x => x.FullName).HasMaxLength(100).IsRequired();
            e.Property(x => x.Email).HasMaxLength(200).IsRequired();
            e.Property(x => x.Expertise).HasMaxLength(150).IsRequired();
            e.HasIndex(x => x.Email).IsUnique();
        });
        model.Entity<Session>(e => {
            e.Property(x => x.Title).HasMaxLength(150).IsRequired();
            e.Property(x => x.TaskDescription).HasMaxLength(2000).IsRequired();
            e.HasOne(x => x.Instructor).WithMany().HasForeignKey(x => x.InstructorId).OnDelete(DeleteBehavior.Restrict);
            e.ToTable(t => t.HasCheckConstraint("CK_Sessions_Progress", "ProgressPercent BETWEEN 0 AND 100"));
        });
        model.Entity<SessionParticipant>(e => {
            e.HasKey(x => new { x.SessionId, x.StudentId });
            e.HasOne(x => x.Session).WithMany(x => x.Participants).HasForeignKey(x => x.SessionId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Student).WithMany().HasForeignKey(x => x.StudentId).OnDelete(DeleteBehavior.Restrict);
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            e.ToTable(t => t.HasCheckConstraint("CK_Participants_Status", "Status IN ('Pending', 'Present', 'Absent')"));
        });
        model.Entity<Evaluation>(e => {
            e.Property(x => x.Notes).HasMaxLength(2000).IsRequired();
            e.HasIndex(x => new { x.SessionId, x.StudentId }).IsUnique();
            e.HasOne(x => x.Participant).WithOne().HasForeignKey<Evaluation>(x => new { x.SessionId, x.StudentId }).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Instructor).WithMany().HasForeignKey(x => x.InstructorId).OnDelete(DeleteBehavior.Restrict);
            e.ToTable(t => t.HasCheckConstraint("CK_Evaluations_Score", "Score BETWEEN 0 AND 100"));
        });
    }
}
