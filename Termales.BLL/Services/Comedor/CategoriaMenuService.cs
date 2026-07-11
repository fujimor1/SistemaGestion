using Termales.BLL.Interfaces.Comedor;
using Termales.Common.DTOs.Comedor;
using Termales.Common.Wrappers;
using Termales.DAL.UnitOfWork;
using Termales.Entities.Models.Comedor;

namespace Termales.BLL.Services.Comedor;

public class CategoriaMenuService : ICategoriaMenuService
{
    private readonly IUnitOfWork _uow;

    public CategoriaMenuService(IUnitOfWork uow) => _uow = uow;

    public async Task<ApiResponse<IEnumerable<CategoriaMenuDto>>> ObtenerTodosAsync()
    {
        var categorias = await _uow.CategoriasMenu.ObtenerActivasAsync();
        return ApiResponse<IEnumerable<CategoriaMenuDto>>.Exitoso(categorias.Select(MapearDto));
    }

    public async Task<ApiResponse<CategoriaMenuDto>> ObtenerPorIdAsync(int id)
    {
        var categoria = await _uow.CategoriasMenu.ObtenerPorIdAsync(id);
        if (categoria is null)
            return ApiResponse<CategoriaMenuDto>.Fallido("Categoría no encontrada");
        return ApiResponse<CategoriaMenuDto>.Exitoso(MapearDto(categoria));
    }

    public async Task<ApiResponse<CategoriaMenuDto>> CrearAsync(CrearCategoriaMenuDto dto)
    {
        if (await _uow.CategoriasMenu.ExisteAsync(c => c.Nombre == dto.Nombre))
            return ApiResponse<CategoriaMenuDto>.Fallido($"Ya existe una categoría con el nombre '{dto.Nombre}'");

        var categoria = new CategoriaMenu { Nombre = dto.Nombre, Activo = true };
        await _uow.CategoriasMenu.AgregarAsync(categoria);
        await _uow.GuardarCambiosAsync();
        return ApiResponse<CategoriaMenuDto>.Exitoso(MapearDto(categoria), "Categoría creada exitosamente");
    }

    public async Task<ApiResponse<CategoriaMenuDto>> ActualizarAsync(ActualizarCategoriaMenuDto dto)
    {
        var categoria = await _uow.CategoriasMenu.ObtenerPorIdAsync(dto.CategoriaMenuId);
        if (categoria is null)
            return ApiResponse<CategoriaMenuDto>.Fallido("Categoría no encontrada");

        if (await _uow.CategoriasMenu.ExisteAsync(c => c.Nombre == dto.Nombre && c.CategoriaMenuId != dto.CategoriaMenuId))
            return ApiResponse<CategoriaMenuDto>.Fallido($"El nombre '{dto.Nombre}' ya está en uso");

        categoria.Nombre = dto.Nombre;
        await _uow.CategoriasMenu.ActualizarAsync(categoria);
        await _uow.GuardarCambiosAsync();
        return ApiResponse<CategoriaMenuDto>.Exitoso(MapearDto(categoria), "Categoría actualizada exitosamente");
    }

    public async Task<ApiResponse> DesactivarAsync(int id)
    {
        var categoria = await _uow.CategoriasMenu.ObtenerPorIdAsync(id);
        if (categoria is null)
            return ApiResponse.Fallido("Categoría no encontrada");

        categoria.Activo = false;
        await _uow.CategoriasMenu.ActualizarAsync(categoria);
        await _uow.GuardarCambiosAsync();
        return ApiResponse.Exitoso("Categoría desactivada exitosamente");
    }

    private static CategoriaMenuDto MapearDto(CategoriaMenu c) => new()
    {
        CategoriaMenuId = c.CategoriaMenuId,
        Nombre = c.Nombre,
        Activo = c.Activo
    };
}
