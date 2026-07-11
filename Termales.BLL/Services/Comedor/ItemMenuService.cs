using Termales.BLL.Interfaces.Comedor;
using Termales.Common.DTOs.Comedor;
using Termales.Common.Wrappers;
using Termales.DAL.UnitOfWork;
using Termales.Entities.Models.Comedor;

namespace Termales.BLL.Services.Comedor;

public class ItemMenuService : IItemMenuService
{
    private readonly IUnitOfWork _uow;

    public ItemMenuService(IUnitOfWork uow) => _uow = uow;

    public async Task<ApiResponse<IEnumerable<ItemMenuDto>>> ObtenerTodosActivosAsync()
    {
        var items = await _uow.ItemsMenu.ObtenerActivosAsync();
        return ApiResponse<IEnumerable<ItemMenuDto>>.Exitoso(items.Select(MapearDto));
    }

    public async Task<ApiResponse<IEnumerable<ItemMenuDto>>> ObtenerPorCategoriaAsync(int categoriaId)
    {
        var items = await _uow.ItemsMenu.ObtenerPorCategoriaAsync(categoriaId);
        return ApiResponse<IEnumerable<ItemMenuDto>>.Exitoso(items.Select(MapearDto));
    }

    public async Task<ApiResponse<ItemMenuDto>> ObtenerPorIdAsync(int id)
    {
        var item = await _uow.ItemsMenu.ObtenerConRecetaAsync(id);
        if (item is null)
            return ApiResponse<ItemMenuDto>.Fallido("Item de menú no encontrado");
        return ApiResponse<ItemMenuDto>.Exitoso(MapearDto(item));
    }

    public async Task<ApiResponse<ItemMenuDto>> CrearAsync(CrearItemMenuDto dto)
    {
        var categoriaExiste = await _uow.CategoriasMenu.ExisteAsync(c => c.CategoriaMenuId == dto.CategoriaMenuId && c.Activo);
        if (!categoriaExiste)
            return ApiResponse<ItemMenuDto>.Fallido("La categoría especificada no existe");

        var errorReceta = await ValidarRecetaAsync(dto.Receta);
        if (errorReceta is not null)
            return ApiResponse<ItemMenuDto>.Fallido(errorReceta);

        var item = new ItemMenu
        {
            CategoriaMenuId = dto.CategoriaMenuId,
            Nombre = dto.Nombre,
            Descripcion = dto.Descripcion,
            Precio = dto.Precio,
            Activo = true,
            Receta = dto.Receta.Select(r => new RecetaInsumo { InsumoId = r.InsumoId, Cantidad = r.Cantidad }).ToList(),
        };

        await _uow.ItemsMenu.AgregarAsync(item);
        await _uow.GuardarCambiosAsync();

        var itemCompleto = await _uow.ItemsMenu.ObtenerConRecetaAsync(item.ItemMenuId);
        return ApiResponse<ItemMenuDto>.Exitoso(MapearDto(itemCompleto!), "Item de menú creado exitosamente");
    }

    public async Task<ApiResponse<ItemMenuDto>> ActualizarAsync(ActualizarItemMenuDto dto)
    {
        var item = await _uow.ItemsMenu.ObtenerConRecetaAsync(dto.ItemMenuId);
        if (item is null)
            return ApiResponse<ItemMenuDto>.Fallido("Item de menú no encontrado");

        var categoriaExiste = await _uow.CategoriasMenu.ExisteAsync(c => c.CategoriaMenuId == dto.CategoriaMenuId && c.Activo);
        if (!categoriaExiste)
            return ApiResponse<ItemMenuDto>.Fallido("La categoría especificada no existe");

        var errorReceta = await ValidarRecetaAsync(dto.Receta);
        if (errorReceta is not null)
            return ApiResponse<ItemMenuDto>.Fallido(errorReceta);

        item.CategoriaMenuId = dto.CategoriaMenuId;
        item.Nombre = dto.Nombre;
        item.Descripcion = dto.Descripcion;
        item.Precio = dto.Precio;

        item.Receta.Clear();
        foreach (var r in dto.Receta)
            item.Receta.Add(new RecetaInsumo { InsumoId = r.InsumoId, Cantidad = r.Cantidad });

        await _uow.ItemsMenu.ActualizarAsync(item);
        await _uow.GuardarCambiosAsync();

        var itemCompleto = await _uow.ItemsMenu.ObtenerConRecetaAsync(item.ItemMenuId);
        return ApiResponse<ItemMenuDto>.Exitoso(MapearDto(itemCompleto!), "Item de menú actualizado exitosamente");
    }

    private async Task<string?> ValidarRecetaAsync(List<RecetaInsumoInputDto> receta)
    {
        if (receta.Count == 0)
            return "Debe registrar al menos un insumo en la receta";

        foreach (var r in receta)
        {
            var insumoExiste = await _uow.Insumos.ExisteAsync(i => i.InsumoId == r.InsumoId && i.Activo);
            if (!insumoExiste)
                return $"El insumo {r.InsumoId} de la receta no existe o no está activo";
        }
        return null;
    }

    public async Task<ApiResponse> DesactivarAsync(int id)
    {
        var item = await _uow.ItemsMenu.ObtenerPorIdAsync(id);
        if (item is null)
            return ApiResponse.Fallido("Item de menú no encontrado");

        item.Activo = false;
        await _uow.ItemsMenu.ActualizarAsync(item);
        await _uow.GuardarCambiosAsync();
        return ApiResponse.Exitoso("Item de menú desactivado exitosamente");
    }

    private static ItemMenuDto MapearDto(ItemMenu i) => new()
    {
        ItemMenuId = i.ItemMenuId,
        CategoriaMenuId = i.CategoriaMenuId,
        NombreCategoria = i.Categoria?.Nombre ?? string.Empty,
        Nombre = i.Nombre,
        Descripcion = i.Descripcion,
        Precio = i.Precio,
        Activo = i.Activo,
        Receta = i.Receta.Select(r => new RecetaInsumoDto
        {
            RecetaInsumoId = r.RecetaInsumoId,
            InsumoId = r.InsumoId,
            NombreInsumo = r.Insumo?.Nombre ?? string.Empty,
            UnidadInsumo = r.Insumo?.Unidad,
            Cantidad = r.Cantidad,
        }).ToList()
    };
}
