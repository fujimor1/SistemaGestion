using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Termales.API.Authorization;
using Termales.BLL.Interfaces.Comedor;
using Termales.Common.DTOs.Comedor;

namespace Termales.API.Controllers.Comedor;

[ApiController]
[Route("api/comedor/items")]
[Authorize]
public class ItemsMenuController : ControllerBase
{
    private readonly IItemMenuService _service;

    public ItemsMenuController(IItemMenuService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> ObtenerTodos() =>
        Ok(await _service.ObtenerTodosActivosAsync());

    [HttpGet("categoria/{categoriaId:int}")]
    public async Task<IActionResult> ObtenerPorCategoria(int categoriaId) =>
        Ok(await _service.ObtenerPorCategoriaAsync(categoriaId));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObtenerPorId(int id)
    {
        var resultado = await _service.ObtenerPorIdAsync(id);
        return resultado.Exito ? Ok(resultado) : NotFound(resultado);
    }

    [HttpPost]
    [Authorize(Roles = Modulos.Operaciones)]
    public async Task<IActionResult> Crear([FromBody] CrearItemMenuDto dto)
    {
        var resultado = await _service.CrearAsync(dto);
        if (!resultado.Exito) return BadRequest(resultado);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = resultado.Data!.ItemMenuId }, resultado);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = Modulos.Operaciones)]
    public async Task<IActionResult> Actualizar(int id, [FromBody] ActualizarItemMenuDto dto)
    {
        if (id != dto.ItemMenuId) return BadRequest("El ID no coincide");
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
}
