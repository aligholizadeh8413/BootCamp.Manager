namespace BootCamp.Manager.Domain.Entities;

public class Evaluation
{
    public int Id { get; set; }
    public int SessionId { get; set; }
    public int StudentId { get; set; }
    public SessionParticipant Participant { get; set; } = null!;
    public int InstructorId { get; set; }
    public Instructor Instructor { get; set; } = null!;
    public int Score { get; set; }
    public string Notes { get; set; } = "";
}
