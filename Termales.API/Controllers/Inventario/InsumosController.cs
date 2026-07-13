using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Termales.API.Authorization;
using Termales.BLL.Interfaces;
using Termales.BLL.Interfaces.Inventario;
using Termales.Common.DTOs.Inventario;

namespace Termales.API.Controllers.Inventario;

[ApiController]
[Route("api/inventario/[controller]")]
[Authorize(Roles = Modulos.Operaciones)]
public class InsumosController : ControllerBase
{
    private readonly IInsumoService _service;
    private readonly IEntradaInsumoService _entradaService;
    private readonly ISalidaInsumoService _salidaService;
    private readonly IReciboPrinterService _reciboPrinter;

    public InsumosController(
        IInsumoService service, IEntradaInsumoService entradaService,
        ISalidaInsumoService salidaService, IReciboPrinterService reciboPrinter)
    {
        _service = service;
        _entradaService = entradaService;
        _salidaService = salidaService;
        _reciboPrinter = reciboPrinter;
    }

    [HttpGet("{ambiente}")]
    public async Task<IActionResult> ObtenerPorAmbiente(string ambiente)
    {
        var insumos = await _service.ObtenerPorAmbienteAsync(ambiente);
        return Ok(insumos);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObtenerPorId(int id)
    {
        var insumo = await _service.ObtenerPorIdAsync(id);
        return insumo is null ? NotFound() : Ok(insumo);
    }

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] CrearInsumoDto dto)
    {
        var insumo = await _service.CrearAsync(dto);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = insumo.InsumoId }, insumo);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Actualizar(int id, [FromBody] ActualizarInsumoDto dto)
    {
        var insumo = await _service.ActualizarAsync(id, dto);
        return insumo is null ? NotFound() : Ok(insumo);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        var ok = await _service.EliminarAsync(id);
        return ok ? NoContent() : NotFound();
    }

    // ── Entradas ──────────────────────────────────────────────────────────────

    [HttpGet("{id:int}/entradas")]
    public async Task<IActionResult> ObtenerEntradas(int id)
    {
        var entradas = await _entradaService.ObtenerPorInsumoAsync(id);
        return Ok(entradas);
    }

    [HttpPost("{id:int}/entradas")]
    public async Task<IActionResult> RegistrarEntrada(int id, [FromBody] RegistrarEntradaInsumoDto dto)
    {
        dto.InsumoId = id;
        var entrada = await _entradaService.RegistrarAsync(dto);
        return Ok(entrada);
    }

    // ── Salidas ───────────────────────────────────────────────────────────────

    [HttpGet("{id:int}/salidas")]
    public async Task<IActionResult> ObtenerSalidas(int id)
    {
        var salidas = await _salidaService.ObtenerPorInsumoAsync(id);
        return Ok(salidas);
    }

    [HttpPost("{id:int}/salidas")]
    public async Task<IActionResult> RegistrarSalida(int id, [FromBody] RegistrarSalidaInsumoDto dto)
    {
        dto.InsumoId = id;
        try
        {
            var salida = await _salidaService.RegistrarAsync(dto);
            return Ok(salida);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    // ── Consumo diario (para la vista del cocinero) ───────────────────────────

    [HttpGet("consumo-diario")]
    public async Task<IActionResult> ConsumoDiario([FromQuery] DateTime? fecha)
    {
        var dia = fecha?.ToUniversalTime() ?? DateTime.UtcNow;
        var salidas = await _salidaService.ObtenerPorFechaAsync(dia);
        return Ok(salidas);
    }

    // ── Consumo actual: sale ya mismo, a diferencia del cierre diario que se
    // registra en bloque al final del turno. Emite un ticket de referencia. ──

    [HttpPost("consumo-actual")]
    public async Task<IActionResult> RegistrarConsumoActual([FromBody] RegistrarConsumoActualDto dto)
    {
        if (dto.Items is null || dto.Items.Count == 0)
            return BadRequest(new { mensaje = "Selecciona al menos un insumo" });

        // Verifica que todos tengan stock suficiente ANTES de descontar
        // cualquiera, para no dejar el registro a medias si uno falla.
        foreach (var item in dto.Items)
        {
            var insumo = await _service.ObtenerPorIdAsync(item.InsumoId);
            if (insumo is null)
                return BadRequest(new { mensaje = $"Insumo {item.InsumoId} no encontrado" });
            if (insumo.StockActual < item.Cantidad)
                return BadRequest(new { mensaje = $"Stock insuficiente de {insumo.Nombre}. Disponible: {insumo.StockActual} {insumo.Unidad}" });
        }

        var resultados = new List<Termales.Common.DTOs.Inventario.SalidaInsumoDto>();
        foreach (var item in dto.Items)
        {
            var salida = await _salidaService.RegistrarAsync(new RegistrarSalidaInsumoDto
            {
                InsumoId = item.InsumoId,
                Cantidad = item.Cantidad,
                Observacion = $"Consumo actual — {dto.Ambiente}",
            });
            resultados.Add(salida);
        }

        var detalle = string.Join("\n", resultados.Select(r => $"{r.NombreInsumo}: {r.Cantidad} {r.Unidad}".Trim()));
        await _reciboPrinter.ImprimirTicketControlAsync($"SALIDA DE INSUMOS - {dto.Ambiente.ToUpperInvariant()}", detalle);

        return Ok(resultados);
    }
}
