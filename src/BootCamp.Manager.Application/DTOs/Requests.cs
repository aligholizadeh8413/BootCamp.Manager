using System.ComponentModel.DataAnnotations;
using BootCamp.Manager.Domain.Entities;
namespace BootCamp.Manager.Application.DTOs;

public class StudentRequest
{
    [Required(ErrorMessage = "تکمیل این فیلد الزامی است."), StringLength(100, ErrorMessage = "طول متن بیشتر از 100 نویسه نباشد.")] public string FullName { get; set; } = "";
    [Required(ErrorMessage = "تکمیل این فیلد الزامی است."), EmailAddress(ErrorMessage = "ایمیل معتبر وارد کنید."), StringLength(200, ErrorMessage = "طول متن بیشتر از 200 نویسه نباشد.")] public string Email { get; set; } = "";
    public bool IsActive { get; set; } = true;
}
public class InstructorRequest : StudentRequest
{
    [Required(ErrorMessage = "تکمیل این فیلد الزامی است."), StringLength(150, ErrorMessage = "طول متن بیشتر از 150 نویسه نباشد.")] public string Expertise { get; set; } = "";
}
public class SessionRequest
{
    [Required(ErrorMessage = "تکمیل این فیلد الزامی است."), StringLength(150, ErrorMessage = "طول متن بیشتر از 150 نویسه نباشد.")] public string Title { get; set; } = "";
    [Required(ErrorMessage = "تکمیل این فیلد الزامی است.")] public DateOnly? Date { get; set; }
    [Range(1, int.MaxValue, ErrorMessage = "یک مورد معتبر انتخاب کنید.")] public int InstructorId { get; set; }
    [Required(AllowEmptyStrings = true, ErrorMessage = "این فیلد نباید null باشد."), StringLength(2000, ErrorMessage = "طول متن بیشتر از 2000 نویسه نباشد.")] public string TaskDescription { get; set; } = "";
    [Range(0, 100, ErrorMessage = "عددی صحیح بین صفر تا صد وارد کنید.")] public int ProgressPercent { get; set; }
    public bool IsCompleted { get; set; }
}
public class CreateSessionRequest : SessionRequest
{
    [Required(ErrorMessage = "تکمیل این فیلد الزامی است.")] public List<int> StudentIds { get; set; } = [];
}
public class AttendanceItem
{
    [Range(1, int.MaxValue, ErrorMessage = "یک مورد معتبر انتخاب کنید.")] public int StudentId { get; set; }
    [Required(ErrorMessage = "تکمیل این فیلد الزامی است."), EnumDataType(typeof(AttendanceStatus), ErrorMessage = "وضعیت حضور معتبر نیست.")] public AttendanceStatus? Status { get; set; }
}
public class AttendanceRequest
{
    [Required(ErrorMessage = "تکمیل این فیلد الزامی است.")] public List<AttendanceItem> Items { get; set; } = [];
}
public class EvaluationRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "یک مورد معتبر انتخاب کنید.")] public int SessionId { get; set; }
    [Range(1, int.MaxValue, ErrorMessage = "یک مورد معتبر انتخاب کنید.")] public int StudentId { get; set; }
    [Range(1, int.MaxValue, ErrorMessage = "یک مورد معتبر انتخاب کنید.")] public int InstructorId { get; set; }
    [Range(0, 100, ErrorMessage = "عددی صحیح بین صفر تا صد وارد کنید.")] public int Score { get; set; }
    [Required(AllowEmptyStrings = true, ErrorMessage = "این فیلد نباید null باشد."), StringLength(2000, ErrorMessage = "طول متن بیشتر از 2000 نویسه نباشد.")] public string Notes { get; set; } = "";
}
