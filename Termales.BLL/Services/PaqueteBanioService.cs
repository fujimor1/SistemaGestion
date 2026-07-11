using Termales.BLL.Interfaces;
using Termales.Common.DTOs;
using Termales.Common.Wrappers;
using Termales.DAL.UnitOfWork;
using Termales.Entities.Models;

namespace Termales.BLL.Services;

public class PaqueteBanioService : IPaqueteBanioService
{
    private readonly IUnitOfWork _uow;

    public PaqueteBanioService(IUnitOfWork uow) => _uow = uow;

    public async Task<ApiResponse<IEnumerable<PaqueteBanioDto>>> ObtenerActivosAsync()
    {
        var paquetes = await _uow.PaquetesBanio.ObtenerActivosConTiposAsync();
        return ApiResponse<IEnumerable<PaqueteBanioDto>>.Exitoso(paquetes.Select(MapearDto));
    }

    public async Task<ApiResponse<PaqueteBanioDto>> CrearAsync(CrearPaqueteBanioDto dto)
    {
        var idsUnicos = dto.TipoServicioIds.Distinct().ToList();
        var tiposValidos = await _uow.TiposServicio.ContarAsync(t => idsUnicos.Contains(t.TipoServicioId) && t.Activo);
        if (tiposValidos != idsUnicos.Count)
            return ApiResponse<PaqueteBanioDto>.Fallido("Alguno de los tipos de servicio no existe o está inactivo");

        var paquete = new PaqueteBanio
        {
            Nombre = dto.Nombre,
            Precio = dto.Precio,
            Activo = true,
            Tipos = idsUnicos.Select(id => new PaqueteBanioTipoServicio { TipoServicioId = id }).ToList()
        };

        await _uow.PaquetesBanio.AgregarAsync(paquete);
        await _uow.GuardarCambiosAsync();
        var creado = await _uow.PaquetesBanio.ObtenerConTiposAsync(paquete.PaqueteBanioId);
        return ApiResponse<PaqueteBanioDto>.Exitoso(MapearDto(creado!), "Paquete registrado exitosamente");
    }

    public async Task<ApiResponse<PaqueteBanioDto>> ActualizarAsync(ActualizarPaqueteBanioDto dto)
    {
        var paquete = await _uow.PaquetesBanio.ObtenerConTiposAsync(dto.PaqueteBanioId);
        if (paquete is null)
            return ApiResponse<PaqueteBanioDto>.Fallido("Paquete no encontrado");

        var idsUnicos = dto.TipoServicioIds.Distinct().ToList();
        var tiposValidos = await _uow.TiposServicio.ContarAsync(t => idsUnicos.Contains(t.TipoServicioId) && t.Activo);
        if (tiposValidos != idsUnicos.Count)
            return ApiResponse<PaqueteBanioDto>.Fallido("Alguno de los tipos de servicio no existe o está inactivo");

        paquete.Nombre = dto.Nombre;
        paquete.Precio = dto.Precio;

        var tiposAEliminar = paquete.Tipos.Where(t => !idsUnicos.Contains(t.TipoServicioId)).ToList();
        foreach (var t in tiposAEliminar)
            paquete.Tipos.Remove(t);

        var idsExistentes = paquete.Tipos.Select(t => t.TipoServicioId).ToHashSet();
        foreach (var id in idsUnicos.Where(id => !idsExistentes.Contains(id)))
            paquete.Tipos.Add(new PaqueteBanioTipoServicio { PaqueteBanioId = paquete.PaqueteBanioId, TipoServicioId = id });

        await _uow.PaquetesBanio.ActualizarAsync(paquete);
        await _uow.GuardarCambiosAsync();
        return ApiResponse<PaqueteBanioDto>.Exitoso(MapearDto(paquete), "Paquete actualizado exitosamente");
    }

    public async Task<ApiResponse> DesactivarAsync(int id)
    {
        var paquete = await _uow.PaquetesBanio.ObtenerPorIdAsync(id);
        if (paquete is null)
            return ApiResponse.Fallido("Paquete no encontrado");

        paquete.Activo = false;
        await _uow.PaquetesBanio.ActualizarAsync(paquete);
        await _uow.GuardarCambiosAsync();
        return ApiResponse.Exitoso("Paquete desactivado exitosamente");
    }

    private static PaqueteBanioDto MapearDto(PaqueteBanio p) => new()
    {
        PaqueteBanioId = p.PaqueteBanioId,
        Nombre = p.Nombre,
        Precio = p.Precio,
        Activo = p.Activo,
        TipoServicioIds = p.Tipos.Select(t => t.TipoServicioId).ToList()
    };
}
