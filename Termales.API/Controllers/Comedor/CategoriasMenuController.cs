using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Termales.API.Authorization;
using Termales.BLL.Interfaces.Comedor;
using Termales.Common.DTOs.Comedor;

namespace Termales.API.Controllers.Comedor;

[ApiController]
[Route("api/comedor/categorias")]
[Authorize]
public class CategoriasMenuController : ControllerBase
{
    private readonly ICategoriaMenuService _service;

    public CategoriasMenuController(ICategoriaMenuService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> ObtenerTodos() =>
        Ok(await _service.ObtenerTodosAsync());

    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObtenerPorId(int id)
    {
        var resultado = await _service.ObtenerPorIdAsync(id);
        return resultado.Exito ? Ok(resultado) : NotFound(resultado);
    }

    [HttpPost]
    [Authorize(Roles = Modulos.Operaciones)]
    public async Task<IActionResult> Crear([FromBody] CrearCategoriaMenuDto dto)
    {
        var resultado = await _service.CrearAsync(dto);
        if (!resultado.Exito) return BadRequest(resultado);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = resultado.Data!.CategoriaMenuId }, resultado);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = Modulos.Operaciones)]
    public async Task<IActionResult> Actualizar(int id, [FromBody] ActualizarCategoriaMenuDto dto)
    {
        if (id != dto.CategoriaMenuId) return BadRequest("El ID no coincide");
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
