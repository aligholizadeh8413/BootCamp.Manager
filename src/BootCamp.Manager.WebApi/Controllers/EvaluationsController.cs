using BootCamp.Manager.Application.DTOs;
using BootCamp.Manager.Application.Services;
using Microsoft.AspNetCore.Mvc;
namespace BootCamp.Manager.WebApi.Controllers;
[ApiController]
[Route("api/evaluations")]
public class EvaluationsController(EvaluationService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct) => Ok(await service.GetAllAsync(ct));
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct) => Ok(await service.GetByIdAsync(id, ct));
    [HttpPost]
    public async Task<IActionResult> Create(EvaluationRequest input, CancellationToken ct)
    {
        var result = await service.CreateAsync(input, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, EvaluationRequest input, CancellationToken ct)
    {
        await service.UpdateAsync(id, input, ct);
        return NoContent();
    }
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await service.DeleteAsync(id, ct);
        return NoContent();
    }
}
