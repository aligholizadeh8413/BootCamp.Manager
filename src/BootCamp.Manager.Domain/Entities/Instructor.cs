namespace BootCamp.Manager.Domain.Entities;

public class Instructor
{
    public int Id { get; set; }
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Expertise { get; set; } = "";
    public bool IsActive { get; set; } = true;
}
