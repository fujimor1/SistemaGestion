using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Termales.API.Authorization;
using Termales.BLL.Interfaces.Comedor;
using Termales.Common.DTOs.Comedor;

namespace Termales.API.Controllers.Comedor;

[ApiController]
[Route("api/comedor/mesas")]
[Authorize]
public class MesasController : ControllerBase
{
    private readonly IMesaService _service;

    public MesasController(IMesaService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> ObtenerTodas() =>
        Ok(await _service.ObtenerTodasAsync());

    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObtenerPorId(int id)
    {
        var resultado = await _service.ObtenerPorIdAsync(id);
        return resultado.Exito ? Ok(resultado) : NotFound(resultado);
    }

    [HttpPost]
    [Authorize(Roles = Modulos.Operaciones)]
    public async Task<IActionResult> Crear([FromBody] CrearMesaDto dto)
    {
        var resultado = await _service.CrearAsync(dto);
        if (!resultado.Exito) return BadRequest(resultado);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = resultado.Data!.MesaId }, resultado);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = Modulos.Operaciones)]
    public async Task<IActionResult> Actualizar(int id, [FromBody] ActualizarMesaDto dto)
    {
        if (id != dto.MesaId) return BadRequest("El ID no coincide");
        var resultado = await _service.ActualizarAsync(dto);
        return resultado.Exito ? Ok(resultado) : NotFound(resultado);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = Modulos.Operaciones)]
    public async Task<IActionResult> Desactivar(int id)
    {
        var resultado = await _service.DesactivarAsync(id);
        return resultado.Exito ? Ok(resultado) : NotFound(resultado);
    }

    [HttpPost("{id:int}/unir")]
    public async Task<IActionResult> Unir(int id, [FromBody] UnirMesaDto dto)
    {
        var resultado = await _service.UnirAsync(id, dto.MesaSecundariaId);
        return resultado.Exito ? Ok(resultado) : BadRequest(resultado);
    }

    [HttpPost("{id:int}/separar")]
    public async Task<IActionResult> Separar(int id)
    {
        var resultado = await _service.SepararAsync(id);
        return resultado.Exito ? Ok(resultado) : BadRequest(resultado);
    }
}
