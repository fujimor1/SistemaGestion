using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Termales.API.Authorization;
using Termales.BLL.Interfaces;
using Termales.Common.DTOs;

namespace Termales.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = Modulos.Operaciones)]
public class PagosController : ControllerBase
{
    private readonly IPagoService _service;

    public PagosController(IPagoService service) => _service = service;

    [HttpGet("reserva/{reservaId:int}")]
    public async Task<IActionResult> ObtenerPorReserva(int reservaId)
    {
        var resultado = await _service.ObtenerPorReservaAsync(reservaId);
        return resultado.Exito ? Ok(resultado) : NotFound(resultado);
    }

    [HttpPost]
    public async Task<IActionResult> RegistrarPago([FromBody] RegistrarPagoDto dto)
    {
        var resultado = await _service.RegistrarPagoAsync(dto);
        return resultado.Exito ? Ok(resultado) : BadRequest(resultado);
    }

    [HttpGet("recaudado")]
    public async Task<IActionResult> ObtenerTotalRecaudado(
        [FromQuery] DateTime desde,
        [FromQuery] DateTime hasta)
    {
        var resultado = await _service.ObtenerTotalRecaudadoAsync(desde, hasta);
        return Ok(resultado);
    }
}
