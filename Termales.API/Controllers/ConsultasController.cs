using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Termales.API.Authorization;
using Termales.BLL.Interfaces;

namespace Termales.API.Controllers;

[ApiController]
[Route("api/consultas")]
[Authorize(Roles = Modulos.Operaciones)]
public class ConsultasController : ControllerBase
{
    private readonly IConsultaDocumentoService _service;

    public ConsultasController(IConsultaDocumentoService service) => _service = service;

    [HttpGet("dni/{dni}")]
    public async Task<IActionResult> ConsultarDni(string dni)
    {
        var resultado = await _service.ConsultarDniAsync(dni);
        return resultado.Exito ? Ok(resultado) : NotFound(resultado);
    }

    [HttpGet("ruc/{ruc}")]
    public async Task<IActionResult> ConsultarRuc(string ruc)
    {
        var resultado = await _service.ConsultarRucAsync(ruc);
        return resultado.Exito ? Ok(resultado) : NotFound(resultado);
    }
}
