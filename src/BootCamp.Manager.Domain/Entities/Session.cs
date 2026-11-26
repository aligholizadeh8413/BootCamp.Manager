namespace BootCamp.Manager.Domain.Entities;

public class Session
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public DateOnly Date { get; set; }
    public int InstructorId { get; set; }
    public Instructor Instructor { get; set; } = null!;
    public string TaskDescription { get; set; } = "";
    public int ProgressPercent { get; set; }
    public bool IsCompleted { get; set; }
    public List<SessionParticipant> Participants { get; set; } = [];
}
