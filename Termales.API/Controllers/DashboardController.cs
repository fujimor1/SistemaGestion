using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Termales.API.Authorization;
using Termales.BLL.Interfaces;

namespace Termales.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrador,Recepcionista,Supervisor")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _service;

    public DashboardController(IDashboardService service) => _service = service;

    [HttpGet("comedor")]
    public async Task<IActionResult> Comedor()
        => Ok(await _service.GetComedorAsync());

    [HttpGet("banios")]
    public async Task<IActionResult> Banios()
        => Ok(await _service.GetBaniosAsync());

    [HttpGet("habitaciones")]
    public async Task<IActionResult> Habitaciones()
        => Ok(await _service.GetHabitacionesAsync());

    [HttpGet("tienda")]
    public async Task<IActionResult> Tienda()
        => Ok(await _service.GetTiendaAsync());
}
