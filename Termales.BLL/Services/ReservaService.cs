using Termales.BLL.Interfaces;
using Termales.Common.DTOs;
using Termales.Common.Helpers;
using Termales.Common.Wrappers;
using Termales.DAL.UnitOfWork;
using Termales.Entities.Enums;
using Termales.Entities.Models;

namespace Termales.BLL.Services;

public class ReservaService : IReservaService
{
    private readonly IUnitOfWork _uow;

    public ReservaService(IUnitOfWork uow) => _uow = uow;

    public async Task<ApiResponse<ReservaDto>> ObtenerPorIdAsync(int id)
    {
        var reserva = await _uow.Reservas.ObtenerConDetallesAsync(id);
        if (reserva is null)
            return ApiResponse<ReservaDto>.Fallido("Reserva no encontrada");
        return ApiResponse<ReservaDto>.Exitoso(MapearDto(reserva));
    }

    public async Task<PagedResponse<ReservaDto>> ObtenerPaginadoAsync(FiltroReserva filtro)
    {
        var (items, total) = await _uow.Reservas.ObtenerPaginadoAsync(filtro);
        return PagedResponse<ReservaDto>.Crear(items.Select(MapearDto), filtro.Pagina, filtro.TamanoPagina, total);
    }

    public async Task<ApiResponse<IEnumerable<ReservaDto>>> ObtenerPorClienteAsync(int clienteId)
    {
        var reservas = await _uow.Reservas.ObtenerPorClienteAsync(clienteId);
        return ApiResponse<IEnumerable<ReservaDto>>.Exitoso(reservas.Select(MapearDto));
    }

    public async Task<ApiResponse<ReservaDto>> CrearAsync(CrearReservaDto dto)
    {
        if (dto.FechaSalida <= dto.FechaIngreso)
            return ApiResponse<ReservaDto>.Fallido("La fecha de salida debe ser posterior al ingreso");

        var cliente = await _uow.Clientes.ObtenerPorIdAsync(dto.ClienteId);
        if (cliente is null)
            return ApiResponse<ReservaDto>.Fallido("Cliente no encontrado");

        var piscina = await _uow.Piscinas.ObtenerPorIdAsync(dto.PiscinaId);
        if (piscina is null || !piscina.Disponible)
            return ApiResponse<ReservaDto>.Fallido("Piscina no disponible");

        if (dto.NumeroPersonas > piscina.CapacidadPersonas)
            return ApiResponse<ReservaDto>.Fallido($"La piscina tiene capacidad máxima de {piscina.CapacidadPersonas} personas");

        if (await _uow.Reservas.ExisteConflictoHorarioAsync(dto.PiscinaId, dto.FechaIngreso, dto.FechaSalida))
            return ApiResponse<ReservaDto>.Fallido("La piscina ya está reservada en ese horario");

        var horas = (decimal)(dto.FechaSalida - dto.FechaIngreso).TotalHours;
        var montoBase = piscina.TarifaPorHora * horas;

        var reserva = new Reserva
        {
            ClienteId = dto.ClienteId,
            PiscinaId = dto.PiscinaId,
            FechaReserva = DateTime.UtcNow,
            FechaIngreso = dto.FechaIngreso,
            FechaSalida = dto.FechaSalida,
            NumeroPersonas = dto.NumeroPersonas,
            Observaciones = dto.Observaciones,
            Estado = EstadoReserva.Pendiente
        };

        if (dto.Servicios.Any())
        {
            decimal montoServicios = 0;
            foreach (var svc in dto.Servicios)
            {
                var servicio = await _uow.Servicios.ObtenerPorIdAsync(svc.ServicioId);
                if (servicio is null || !servicio.Activo) continue;

                reserva.ReservaServicios.Add(new ReservaServicio
                {
                    ServicioId = svc.ServicioId,
                    Cantidad = svc.Cantidad,
                    PrecioUnitario = servicio.Precio
                });
                montoServicios += servicio.Precio * svc.Cantidad;
            }
            reserva.MontoTotal = montoBase + montoServicios;
        }
        else
        {
            reserva.MontoTotal = montoBase;
        }

        await _uow.Reservas.AgregarAsync(reserva);
        await _uow.GuardarCambiosAsync();

        var reservaCreada = await _uow.Reservas.ObtenerConDetallesAsync(reserva.ReservaId);
        return ApiResponse<ReservaDto>.Exitoso(MapearDto(reservaCreada!), "Reserva creada exitosamente");
    }

    public async Task<ApiResponse<ReservaDto>> ActualizarEstadoAsync(int reservaId, ActualizarEstadoReservaDto dto)
    {
        var reserva = await _uow.Reservas.ObtenerConDetallesAsync(reservaId);
        if (reserva is null)
            return ApiResponse<ReservaDto>.Fallido("Reserva no encontrada");

        if (reserva.Estado == EstadoReserva.Cancelada)
            return ApiResponse<ReservaDto>.Fallido("No se puede modificar una reserva cancelada");

        reserva.Estado = dto.NuevoEstado;
        if (!string.IsNullOrWhiteSpace(dto.Observaciones))
            reserva.Observaciones = dto.Observaciones;

        await _uow.Reservas.ActualizarAsync(reserva);
        await _uow.GuardarCambiosAsync();
        return ApiResponse<ReservaDto>.Exitoso(MapearDto(reserva), "Estado actualizado exitosamente");
    }

    public async Task<ApiResponse> CancelarAsync(int reservaId, string? motivo)
    {
        var reserva = await _uow.Reservas.ObtenerPorIdAsync(reservaId);
        if (reserva is null)
            return ApiResponse.Fallido("Reserva no encontrada");
        if (reserva.Estado == EstadoReserva.Cancelada)
            return ApiResponse.Fallido("La reserva ya está cancelada");

        reserva.Estado = EstadoReserva.Cancelada;
        if (!string.IsNullOrWhiteSpace(motivo))
            reserva.Observaciones = motivo;

        await _uow.Reservas.ActualizarAsync(reserva);
        await _uow.GuardarCambiosAsync();
        return ApiResponse.Exitoso("Reserva cancelada exitosamente");
    }

    private static ReservaDto MapearDto(Reserva r) => new()
    {
        ReservaId = r.ReservaId,
        ClienteId = r.ClienteId,
        NombreCliente = r.Cliente is not null ? $"{r.Cliente.Nombres} {r.Cliente.Apellidos}" : string.Empty,
        PiscinaId = r.PiscinaId,
        NombrePiscina = r.Piscina?.Nombre ?? string.Empty,
        FechaReserva = r.FechaReserva,
        FechaIngreso = r.FechaIngreso,
        FechaSalida = r.FechaSalida,
        NumeroPersonas = r.NumeroPersonas,
        MontoTotal = r.MontoTotal,
        Estado = r.Estado,
        Observaciones = r.Observaciones,
        Servicios = r.ReservaServicios.Select(rs => new ReservaServicioDto
        {
            ServicioId = rs.ServicioId,
            NombreServicio = rs.Servicio?.Nombre ?? string.Empty,
            Cantidad = rs.Cantidad,
            PrecioUnitario = rs.PrecioUnitario
        }).ToList()
    };
}
