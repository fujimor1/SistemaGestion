using Termales.BLL.Interfaces;
using Termales.Common.DTOs;
using Termales.Common.Wrappers;
using Termales.DAL.UnitOfWork;
using Termales.Entities.Enums;
using Termales.Entities.Models;

namespace Termales.BLL.Services;

public class TurnoService : ITurnoService
{
    private readonly IUnitOfWork _uow;

    public TurnoService(IUnitOfWork uow) => _uow = uow;

    public async Task<ApiResponse<TurnoDto>> RegistrarIngresoAsync(RegistrarTurnoDto dto)
    {
        var tipoServicio = await _uow.TiposServicio.ObtenerPorIdAsync(dto.TipoServicioId);
        if (tipoServicio is null || !tipoServicio.Activo)
            return ApiResponse<TurnoDto>.Fallido("Tipo de servicio no disponible");

        var fechaSolo = dto.FechaHora.Date;
        var disponibilidad = await VerificarDisponibilidadAsync(dto.TipoServicioId, fechaSolo, dto.CantidadPersonas);
        if (!disponibilidad.Exito)
            return ApiResponse<TurnoDto>.Fallido(disponibilidad.Mensaje);

        var aforo = await _uow.Aforos.ObtenerPorTipoYFechaAsync(dto.TipoServicioId, fechaSolo);
        if (aforo is null)
        {
            aforo = new Aforo
            {
                TipoServicioId = dto.TipoServicioId,
                Fecha = fechaSolo,
                CapacidadMaxima = tipoServicio.CapacidadMaxima,
                OcupacionActual = 0
            };
            await _uow.Aforos.AgregarAsync(aforo);
        }

        aforo.OcupacionActual += dto.CantidadPersonas;
        await _uow.Aforos.ActualizarAsync(aforo);

        var turno = new Turno
        {
            TipoServicioId = dto.TipoServicioId,
            FechaHora = dto.FechaHora,
            CantidadPersonas = dto.CantidadPersonas,
            MontoTotal = tipoServicio.PrecioPorPersona * dto.CantidadPersonas,
            EstadoPago = EstadoPago.Pagado,
            MetodoPago = dto.MetodoPago,
            UsuarioId = dto.UsuarioId
        };

        await _uow.Turnos.AgregarAsync(turno);
        await _uow.GuardarCambiosAsync();

        var turnoCreado = await _uow.Turnos.ObtenerConDetallesAsync(turno.TurnoId);
        return ApiResponse<TurnoDto>.Exitoso(MapearTurnoDto(turnoCreado!), "Ingreso registrado exitosamente");
    }

    public async Task<ApiResponse<DisponibilidadDto>> VerificarDisponibilidadAsync(
        int tipoServicioId, DateTime fecha, int cantidadPersonas)
    {
        var tipoServicio = await _uow.TiposServicio.ObtenerPorIdAsync(tipoServicioId);
        if (tipoServicio is null || !tipoServicio.Activo)
            return ApiResponse<DisponibilidadDto>.Fallido("Tipo de servicio no disponible");

        var aforo = await _uow.Aforos.ObtenerPorTipoYFechaAsync(tipoServicioId, fecha.Date);
        var ocupacionActual = aforo?.OcupacionActual ?? 0;
        var lugaresDisponibles = tipoServicio.CapacidadMaxima - ocupacionActual;

        if (cantidadPersonas > lugaresDisponibles)
            return ApiResponse<DisponibilidadDto>.Fallido(
                $"Sin disponibilidad. Lugares disponibles: {lugaresDisponibles}");

        var dto = new DisponibilidadDto
        {
            TipoServicioId = tipoServicioId,
            NombreTipoServicio = tipoServicio.Nombre,
            Fecha = fecha.Date,
            CapacidadMaxima = tipoServicio.CapacidadMaxima,
            OcupacionActual = ocupacionActual
        };

        return ApiResponse<DisponibilidadDto>.Exitoso(dto);
    }

    public async Task<ApiResponse<IEnumerable<DisponibilidadDto>>> ObtenerAforoDelDiaAsync(DateTime fecha)
    {
        var tipos = await _uow.TiposServicio.ObtenerActivosAsync();
        var aforos = await _uow.Aforos.ObtenerPorFechaAsync(fecha.Date);

        var resultado = tipos.Select(t =>
        {
            var aforo = aforos.FirstOrDefault(a => a.TipoServicioId == t.TipoServicioId);
            return new DisponibilidadDto
            {
                TipoServicioId = t.TipoServicioId,
                NombreTipoServicio = t.Nombre,
                Fecha = fecha.Date,
                CapacidadMaxima = t.CapacidadMaxima,
                OcupacionActual = aforo?.OcupacionActual ?? 0
            };
        });

        return ApiResponse<IEnumerable<DisponibilidadDto>>.Exitoso(resultado);
    }

    public async Task<ApiResponse<TurnoDto>> ObtenerPorIdAsync(int id)
    {
        var turno = await _uow.Turnos.ObtenerConDetallesAsync(id);
        if (turno is null)
            return ApiResponse<TurnoDto>.Fallido("Turno no encontrado");
        return ApiResponse<TurnoDto>.Exitoso(MapearTurnoDto(turno));
    }

    public async Task<ApiResponse<IEnumerable<TurnoDto>>> ObtenerPorTipoYFechaAsync(int tipoServicioId, DateTime fecha)
    {
        var turnos = await _uow.Turnos.ObtenerPorTipoYFechaAsync(tipoServicioId, fecha);
        return ApiResponse<IEnumerable<TurnoDto>>.Exitoso(turnos.Select(MapearTurnoDto));
    }

    public async Task<ApiResponse<IEnumerable<TipoServicioDto>>> ObtenerTiposServicioAsync()
    {
        var tipos = await _uow.TiposServicio.ObtenerActivosAsync();
        return ApiResponse<IEnumerable<TipoServicioDto>>.Exitoso(tipos.Select(MapearTipoServicioDto));
    }

    private static TurnoDto MapearTurnoDto(Turno t) => new()
    {
        TurnoId = t.TurnoId,
        TipoServicioId = t.TipoServicioId,
        NombreTipoServicio = t.TipoServicio?.Nombre ?? string.Empty,
        FechaHora = t.FechaHora,
        CantidadPersonas = t.CantidadPersonas,
        MontoTotal = t.MontoTotal,
        EstadoPago = t.EstadoPago,
        MetodoPago = t.MetodoPago,
        UsuarioId = t.UsuarioId,
        FechaCreacion = t.FechaCreacion
    };

    private static TipoServicioDto MapearTipoServicioDto(TipoServicio t) => new()
    {
        TipoServicioId = t.TipoServicioId,
        Nombre = t.Nombre,
        Descripcion = t.Descripcion,
        CapacidadMaxima = t.CapacidadMaxima,
        PrecioPorPersona = t.PrecioPorPersona,
        Activo = t.Activo
    };
}
