using System.ComponentModel.DataAnnotations;
namespace BootCamp.Manager.WebApi.Models;
public class GreetingForm
{
    [Required(ErrorMessage = "نام را وارد کنید."), StringLength(60)]
    public string Name { get; set; } = "";
}
