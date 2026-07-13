using Termales.Common.Wrappers;
using Termales.Common.DTOs.Consultas;

namespace Termales.BLL.Interfaces;

public interface IConsultaDocumentoService
{
    Task<ApiResponse<ConsultaDniResultDto>> ConsultarDniAsync(string dni);
    Task<ApiResponse<ConsultaRucResultDto>> ConsultarRucAsync(string ruc);
}
