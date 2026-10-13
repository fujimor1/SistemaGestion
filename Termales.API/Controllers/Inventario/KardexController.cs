using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Termales.BLL.Interfaces.Inventario;
using Termales.API.Authorization;
using Termales.Common.Utils;

namespace Termales.API.Controllers.Inventario;

[ApiController]
[Route("api/inventario/kardex")]
[Authorize(Roles = Modulos.Operaciones)]
public class KardexController : ControllerBase
{
    private readonly IKardexService _kardex;

    public KardexController(IKardexService kardex) => _kardex = kardex;

    [HttpGet("insumo/{id:int}")]
    public async Task<IActionResult> KardexInsumo(
        int id,
        [FromQuery] DateTime? desde,
        [FromQuery] DateTime? hasta)
    {
        var d = PeruTime.DayRange((desde ?? PeruTime.Today().AddDays(-30)).Date).inicio;
        var h = PeruTime.DayRange((hasta ?? PeruTime.Today()).Date).fin;
        var kardex = await _kardex.ObtenerKardexInsumoAsync(id, d, h);
        return kardex is null ? NotFound() : Ok(kardex);
    }

    [HttpGet("general")]
    public async Task<IActionResult> ResumenGeneral(
        [FromQuery] DateTime? desde,
        [FromQuery] DateTime? hasta,
        [FromQuery] string? tipoAmbiente)
    {
        var d = PeruTime.DayRange((desde ?? PeruTime.Today().AddDays(-30)).Date).inicio;
        var h = PeruTime.DayRange((hasta ?? PeruTime.Today()).Date).fin;
        var resumen = await _kardex.ObtenerResumenGeneralAsync(d, h, tipoAmbiente);
        return Ok(resumen);
    }
}
