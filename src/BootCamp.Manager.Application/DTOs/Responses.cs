using BootCamp.Manager.Domain.Entities;
namespace BootCamp.Manager.Application.DTOs;
public record StudentResponse(int Id, string FullName, string Email, bool IsActive);
public record InstructorResponse(int Id, string FullName, string Email, string Expertise, bool IsActive);
public record ParticipantResponse(int StudentId, string FullName, AttendanceStatus Status);
public record SessionResponse(int Id, string Title, DateOnly Date, int InstructorId,
    string InstructorName, string TaskDescription, int ProgressPercent, bool IsCompleted,
    int ParticipantCount, int PresentCount, int PendingCount, double? AttendancePercent,
    List<ParticipantResponse> Participants);
public record EvaluationResponse(int Id, int SessionId, string SessionTitle, int StudentId,
    string StudentName, int InstructorId, string InstructorName, int Score, string Notes);
