namespace BootCamp.Manager.Domain.Entities;

public class SessionParticipant
{
    public int SessionId { get; set; }
    public Session Session { get; set; } = null!;
    public int StudentId { get; set; }
    public Student Student { get; set; } = null!;
    public AttendanceStatus Status { get; set; } = AttendanceStatus.Pending;
}
