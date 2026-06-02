using Termales.BLL.Interfaces;
using Termales.Common.DTOs;
using Termales.Common.Wrappers;
using Termales.DAL.UnitOfWork;
using Termales.Entities.Enums;
using Termales.Entities.Models;

namespace Termales.BLL.Services;

public class PagoService : IPagoService
{
    private readonly IUnitOfWork _uow;

    public PagoService(IUnitOfWork uow) => _uow = uow;

    public async Task<ApiResponse<PagoDto>> ObtenerPorReservaAsync(int reservaId)
    {
        var pago = await _uow.Pagos.ObtenerPorReservaAsync(reservaId);
        if (pago is null)
            return ApiResponse<PagoDto>.Fallido("No se encontró pago para esta reserva");
        return ApiResponse<PagoDto>.Exitoso(MapearDto(pago));
    }

    public async Task<ApiResponse<PagoDto>> RegistrarPagoAsync(RegistrarPagoDto dto)
    {
        var reserva = await _uow.Reservas.ObtenerPorIdAsync(dto.ReservaId);
        if (reserva is null)
            return ApiResponse<PagoDto>.Fallido("Reserva no encontrada");

        if (reserva.Estado == EstadoReserva.Cancelada)
            return ApiResponse<PagoDto>.Fallido("No se puede registrar pago de una reserva cancelada");

        var pagoExistente = await _uow.Pagos.ObtenerPorReservaAsync(dto.ReservaId);
        if (pagoExistente is not null)
            return ApiResponse<PagoDto>.Fallido("Esta reserva ya tiene un pago registrado");

        var pago = new Pago
        {
            ReservaId = dto.ReservaId,
            Monto = dto.Monto,
            TipoPago = dto.TipoPago,
            NumeroComprobante = dto.NumeroComprobante,
            Observaciones = dto.Observaciones
        };

        await _uow.Pagos.AgregarAsync(pago);

        reserva.Estado = EstadoReserva.Confirmada;
        await _uow.Reservas.ActualizarAsync(reserva);

        await _uow.GuardarCambiosAsync();
        return ApiResponse<PagoDto>.Exitoso(MapearDto(pago), "Pago registrado exitosamente");
    }

    public async Task<ApiResponse<decimal>> ObtenerTotalRecaudadoAsync(DateTime desde, DateTime hasta)
    {
        var total = await _uow.Pagos.ObtenerTotalRecaudadoAsync(desde, hasta);
        return ApiResponse<decimal>.Exitoso(total);
    }

    private static PagoDto MapearDto(Pago p) => new()
    {
        PagoId = p.PagoId,
        ReservaId = p.ReservaId,
        Monto = p.Monto,
        TipoPago = p.TipoPago,
        FechaPago = p.FechaPago,
        NumeroComprobante = p.NumeroComprobante,
        Observaciones = p.Observaciones
    };
}
