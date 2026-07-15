using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Termales.API.Authorization;
using Termales.BLL.Interfaces;
using Termales.BLL.Interfaces.Sunat;
using Termales.Common.DTOs.Comprobante;


namespace Termales.API.Controllers;

[ApiController]
[Route("api/comprobantes")]
[Authorize(Roles = Modulos.Operaciones)]
public class ComprobantesController : ControllerBase
{
    private readonly IComprobanteService _service;
    private readonly IFacturaElectronicaService _facturaElectronica;

    public ComprobantesController(IComprobanteService service, IFacturaElectronicaService facturaElectronica)
    {
        _service = service;
        _facturaElectronica = facturaElectronica;
    }

    [HttpPost("comedor/{ordenId:int}")]
    public async Task<IActionResult> GenerarComedor(int ordenId, [FromBody] GenerarComprobanteComedorDto dto)
    {
        try
        {
            var resultado = await _service.GenerarComprobanteComedor(ordenId, dto);
            return resultado.Exito ? Ok(resultado) : BadRequest(resultado);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { mensaje = $"Error interno: {ex.Message}" });
        }
    }

    [HttpPost("banio")]
    [Authorize(Roles = Modulos.BaniosHabitaciones)]
    public async Task<IActionResult> GenerarBanio([FromBody] GenerarComprobanteBanioDto dto)
    {
        try
        {
            var resultado = await _service.GenerarComprobanteBanio(dto);
            return resultado.Exito ? Ok(resultado) : BadRequest(resultado);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { mensaje = $"Error interno: {ex.Message}" });
        }
    }

    [HttpPost("habitacion/{habitacionId:int}")]
    [Authorize(Roles = Modulos.BaniosHabitaciones)]
    public async Task<IActionResult> GenerarHabitacion(int habitacionId, [FromBody] GenerarComprobanteDto dto)
    {
        try
        {
            var resultado = await _service.GenerarComprobanteHabitacion(habitacionId, dto);
            return resultado.Exito ? Ok(resultado) : BadRequest(resultado);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { mensaje = $"Error interno: {ex.Message}" });
        }
    }

    [HttpPost("tienda")]
    public async Task<IActionResult> GenerarTienda([FromBody] GenerarComprobanteTiendaDto dto)
    {
        try
        {
            var resultado = await _service.GenerarComprobanteTienda(dto);
            return resultado.Exito ? Ok(resultado) : BadRequest(resultado);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { mensaje = $"Error interno: {ex.Message}" });
        }
    }

    [HttpGet("anulaciones")]
    [Authorize(Roles = "Supervisor")]
    public async Task<IActionResult> ObtenerAnulaciones([FromQuery] string? desde, [FromQuery] string? hasta)
    {
        var resultado = await _service.ObtenerAnulacionesAsync(desde, hasta);
        return Ok(resultado);
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerPorFecha([FromQuery] string? fecha, [FromQuery] string? tipoAmbiente)
    {
        var resultado = await _service.ObtenerPorFechaAsync(fecha, tipoAmbiente);
        return Ok(resultado);
    }

    [HttpGet("pendientes")]
    public async Task<IActionResult> ObtenerPendientes()
    {
        var resultado = await _service.ObtenerPendientesDeCobroAsync();
        return Ok(resultado);
    }

    [HttpGet("{id:int}/detalle")]
    public async Task<IActionResult> ObtenerDetalle(int id)
    {
        var resultado = await _service.ObtenerDetalleAsync(id);
        return resultado.Exito ? Ok(resultado) : NotFound(resultado);
    }

    [HttpPatch("{id:int}/cobrar")]
    public async Task<IActionResult> MarcarCobrado(int id, [FromBody] MarcarCobradoDto dto)
    {
        var resultado = await _service.MarcarCobradoAsync(id, dto.MetodoPago);
        return resultado.Exito ? Ok(resultado) : BadRequest(resultado);
    }

    [HttpPost("{id:int}/solicitar-anulacion")]
    public async Task<IActionResult> SolicitarAnulacion(int id, [FromBody] SolicitarAnulacionDto dto)
    {
        var cajero = User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Name)?.Value ?? "---";
        var resultado = await _service.SolicitarAnulacionAsync(id, dto.Motivo, cajero);
        return resultado.Exito ? Ok(resultado) : BadRequest(resultado);
    }

    [HttpGet("pendientes-sunat")]
    public async Task<IActionResult> ObtenerPendientesSunat()
    {
        var resultado = await _service.ObtenerPendientesSunatAsync();
        return Ok(resultado);
    }

    [HttpGet("electronica")]
    public async Task<IActionResult> ObtenerFacturasBoletas([FromQuery] string? fecha)
    {
        var resultado = await _service.ObtenerFacturasBoletasAsync(fecha);
        return Ok(resultado);
    }

    [HttpGet("notas-credito")]
    public async Task<IActionResult> ObtenerNotasCredito([FromQuery] string? desde, [FromQuery] string? hasta)
    {
        var resultado = await _service.ObtenerNotasCreditoAsync(desde, hasta);
        return Ok(resultado);
    }

    [HttpPost("{id:int}/reenviar-sunat")]
    public async Task<IActionResult> ReenviarSunat(int id)
    {
        try
        {
            var resultado = await _service.ReenviarSunatAsync(id);
            return resultado.Exito ? Ok(resultado) : BadRequest(resultado);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { mensaje = $"Error interno: {ex.Message}" });
        }
    }

    [HttpGet("{id:int}/representacion-impresa")]
    public async Task<IActionResult> ObtenerRepresentacionImpresa(int id)
    {
        var resultado = await _facturaElectronica.ObtenerRepresentacionImpresaAsync(id);
        if (!resultado.Exito)
            return BadRequest(resultado);

        return File(resultado.Data!, "application/pdf", $"comprobante-{id}.pdf");
    }

    [HttpPost("{id:int}/nota-credito")]
    public async Task<IActionResult> EmitirNotaCredito(int id, [FromBody] EmitirNotaCreditoDto dto)
    {
        try
        {
            var resultado = await _service.EmitirNotaCreditoAsync(id, dto);
            return resultado.Exito ? Ok(resultado) : BadRequest(resultado);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { mensaje = $"Error interno: {ex.Message}" });
        }
    }
}
