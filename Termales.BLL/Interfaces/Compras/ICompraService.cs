using Microsoft.AspNetCore.Http;
using Termales.Common.DTOs.Compras;

namespace Termales.BLL.Interfaces.Compras;

public interface ICompraService
{
    Task<CompraDto?> ObtenerPorIdAsync(int id);
    Task<(IEnumerable<CompraDto> Items, int Total)> ObtenerPaginadoAsync(
        int pagina, int tamanoPagina, int? proveedorId, string? estado);
    Task<CompraDto> RegistrarAsync(RegistrarCompraDto dto, string registradoPor);
    Task<CompraDto> PagarAsync(int id, PagarCompraDto dto, string registradoPor);
    Task<ResumenComprasDto> ObtenerResumenMesActualAsync();

    Task<List<CompraImagenDto>> AgregarImagenesAsync(int compraId, List<IFormFile> archivos);
    Task<List<CompraImagenDto>> ObtenerImagenesAsync(int compraId);
    Task<(byte[] Bytes, string ContentType, string NombreArchivo)?> ObtenerArchivoImagenAsync(int imagenId);
    Task EliminarImagenAsync(int imagenId);
}
