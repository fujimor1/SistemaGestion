using Termales.BLL.Interfaces.Compras;
using Termales.Common.DTOs.Compras;
using Termales.Common.Wrappers;
using Termales.DAL.UnitOfWork;
using Termales.Entities.Models.Compras;

namespace Termales.BLL.Services.Compras;

public class ProveedorService : IProveedorService
{
    private readonly IUnitOfWork _uow;

    public ProveedorService(IUnitOfWork uow) => _uow = uow;

    public async Task<ApiResponse<ProveedorDto>> ObtenerPorIdAsync(int id)
    {
        var proveedor = await _uow.Proveedores.ObtenerPorIdAsync(id);
        if (proveedor is null)
            return ApiResponse<ProveedorDto>.Fallido("Proveedor no encontrado");
        return ApiResponse<ProveedorDto>.Exitoso(MapearDto(proveedor));
    }

    public async Task<ApiResponse<ProveedorDto>> ObtenerPorRucAsync(string ruc)
    {
        var proveedor = await _uow.Proveedores.ObtenerPorRucAsync(ruc);
        if (proveedor is null)
            return ApiResponse<ProveedorDto>.Fallido("Proveedor no encontrado");
        return ApiResponse<ProveedorDto>.Exitoso(MapearDto(proveedor));
    }

    public async Task<PagedResponse<ProveedorDto>> ObtenerPaginadoAsync(int pagina, int tamanoPagina, string? busqueda)
    {
        var (items, total) = await _uow.Proveedores.ObtenerPaginadoAsync(pagina, tamanoPagina, busqueda);
        return PagedResponse<ProveedorDto>.Crear(items.Select(MapearDto), pagina, tamanoPagina, total);
    }

    public async Task<ApiResponse<ProveedorDto>> CrearAsync(CrearProveedorDto dto)
    {
        if (await _uow.Proveedores.ExisteAsync(p => p.Ruc == dto.Ruc))
            return ApiResponse<ProveedorDto>.Fallido($"Ya existe un proveedor con RUC {dto.Ruc}");

        var proveedor = new Proveedor
        {
            Ruc = dto.Ruc,
            RazonSocial = dto.RazonSocial,
            NombreComercial = dto.NombreComercial,
            Direccion = dto.Direccion,
            Telefono = dto.Telefono,
            Email = dto.Email,
            Activo = true
        };

        await _uow.Proveedores.AgregarAsync(proveedor);
        await _uow.GuardarCambiosAsync();
        return ApiResponse<ProveedorDto>.Exitoso(MapearDto(proveedor), "Proveedor registrado exitosamente");
    }

    public async Task<ApiResponse<ProveedorDto>> ActualizarAsync(ActualizarProveedorDto dto)
    {
        var proveedor = await _uow.Proveedores.ObtenerPorIdAsync(dto.ProveedorId);
        if (proveedor is null)
            return ApiResponse<ProveedorDto>.Fallido("Proveedor no encontrado");

        if (await _uow.Proveedores.ExisteAsync(p => p.Ruc == dto.Ruc && p.ProveedorId != dto.ProveedorId))
            return ApiResponse<ProveedorDto>.Fallido($"El RUC {dto.Ruc} ya pertenece a otro proveedor");

        proveedor.Ruc = dto.Ruc;
        proveedor.RazonSocial = dto.RazonSocial;
        proveedor.NombreComercial = dto.NombreComercial;
        proveedor.Direccion = dto.Direccion;
        proveedor.Telefono = dto.Telefono;
        proveedor.Email = dto.Email;

        await _uow.Proveedores.ActualizarAsync(proveedor);
        await _uow.GuardarCambiosAsync();
        return ApiResponse<ProveedorDto>.Exitoso(MapearDto(proveedor), "Proveedor actualizado exitosamente");
    }

    public async Task<ApiResponse> DesactivarAsync(int id)
    {
        var proveedor = await _uow.Proveedores.ObtenerPorIdAsync(id);
        if (proveedor is null)
            return ApiResponse.Fallido("Proveedor no encontrado");

        proveedor.Activo = false;
        await _uow.Proveedores.ActualizarAsync(proveedor);
        await _uow.GuardarCambiosAsync();
        return ApiResponse.Exitoso("Proveedor desactivado exitosamente");
    }

    private static ProveedorDto MapearDto(Proveedor p) => new()
    {
        ProveedorId = p.ProveedorId,
        Ruc = p.Ruc,
        RazonSocial = p.RazonSocial,
        NombreComercial = p.NombreComercial,
        Direccion = p.Direccion,
        Telefono = p.Telefono,
        Email = p.Email,
        Activo = p.Activo
    };
}
