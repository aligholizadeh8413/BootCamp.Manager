using BootCamp.Manager.WebApi.Models;
using Microsoft.AspNetCore.Mvc;
namespace BootCamp.Manager.WebApi.Controllers;
[Route("learning/mvc")]
public class LearningController : Controller
{
    [HttpGet]
    public IActionResult Index() => View(new GreetingForm());
    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Index([FromForm] GreetingForm input)
    {
        if (!ModelState.IsValid) return View(input);
        ViewBag.Message = $"سلام {input.Name.Trim()}؛ فرم در Controller پردازش شد.";
        return View(input);
    }
}
