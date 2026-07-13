using Microsoft.AspNetCore.SignalR;
using Termales.BLL.Interfaces.Comedor;
using Termales.Common.DTOs.Comedor;
using Termales.Common.Helpers;
using Termales.Common.Wrappers;
using Termales.DAL.UnitOfWork;
using Termales.Entities.Enums;
using Termales.Entities.Models.Comedor;

namespace Termales.BLL.Services.Comedor;

public class OrdenService : IOrdenService
{
    private readonly IUnitOfWork _uow;
    private readonly IHubContext<ComandaHub> _hub;
    private readonly IComandaPrinterService _printer;

    public OrdenService(IUnitOfWork uow, IHubContext<ComandaHub> hub, IComandaPrinterService printer)
    {
        _uow = uow;
        _hub = hub;
        _printer = printer;
    }

    public async Task<ApiResponse<OrdenDto>> ObtenerPorIdAsync(int id)
    {
        var orden = await _uow.Ordenes.ObtenerConDetallesAsync(id);
        if (orden is null)
            return ApiResponse<OrdenDto>.Fallido("Orden no encontrada");
        return ApiResponse<OrdenDto>.Exitoso(MapearDto(orden));
    }

    public async Task<ApiResponse<OrdenDto>> ObtenerActivaPorMesaAsync(int mesaId)
    {
        var orden = await _uow.Ordenes.ObtenerActivaPorMesaAsync(mesaId);
        if (orden is null)
            return ApiResponse<OrdenDto>.Fallido("No hay orden activa en esta mesa");
        return ApiResponse<OrdenDto>.Exitoso(MapearDto(orden));
    }

    public async Task<ApiResponse<IEnumerable<OrdenDto>>> ObtenerPorEstadoAsync(EstadoOrden estado)
    {
        var ordenes = await _uow.Ordenes.ObtenerPorEstadoAsync(estado);
        return ApiResponse<IEnumerable<OrdenDto>>.Exitoso(ordenes.Select(MapearDto));
    }

    public async Task<ApiResponse<IEnumerable<OrdenDto>>> ObtenerPorFechaAsync(DateTime fecha)
    {
        var ordenes = await _uow.Ordenes.ObtenerPorFechaAsync(fecha);
        return ApiResponse<IEnumerable<OrdenDto>>.Exitoso(ordenes.Select(MapearDto));
    }

    public async Task<ApiResponse<OrdenDto>> CrearAsync(CrearOrdenDto dto)
    {
        var mesa = await _uow.Mesas.ObtenerPorIdAsync(dto.MesaId);
        if (mesa is null || !mesa.Activo)
            return ApiResponse<OrdenDto>.Fallido("Mesa no encontrada");

        var ordenActiva = await _uow.Ordenes.ObtenerActivaPorMesaAsync(dto.MesaId);
        if (ordenActiva is not null)
            return ApiResponse<OrdenDto>.Fallido("La mesa ya tiene una orden activa");

        var detalles = new List<OrdenDetalle>();
        decimal total = 0;

        foreach (var item in dto.Detalles)
        {
            var (detalle, error) = await ConstruirDetalleAsync(item);
            if (error is not null) return ApiResponse<OrdenDto>.Fallido(error);

            detalles.Add(detalle!);
            total += detalle!.PrecioUnitario * detalle.Cantidad;
        }

        var orden = new Orden
        {
            MesaId = dto.MesaId,
            UsuarioId = dto.UsuarioId,
            Estado = EstadoOrden.EnCocina,
            Total = total,
            Observaciones = dto.Observaciones,
            Detalles = detalles
        };

        mesa.Estado = EstadoMesa.Ocupada;
        await _uow.Mesas.ActualizarAsync(mesa);
        await _uow.Ordenes.AgregarAsync(orden);
        await _uow.GuardarCambiosAsync();

        var ordenCompleta = await _uow.Ordenes.ObtenerConDetallesAsync(orden.OrdenId);
        var ordenDto = MapearDto(ordenCompleta!);

        await _hub.Clients.Group("cocina").SendAsync("NuevaPedido", ordenDto);
        // Solo los platos de cocina van al ticket impreso — un producto de
        // tienda (gaseosa, snack, etc.) no necesita prepararse.
        var paraCocina = ordenCompleta!.Detalles.Where(d => d.ItemMenuId.HasValue).ToList();
        if (paraCocina.Count > 0)
            await _printer.ImprimirAsync(ordenCompleta, paraCocina, "COMANDA NUEVA");

        return ApiResponse<OrdenDto>.Exitoso(ordenDto, "Orden creada exitosamente");
    }

    public async Task<ApiResponse<OrdenDto>> AgregarItemsAsync(int ordenId, AgregarItemsOrdenDto dto)
    {
        var orden = await _uow.Ordenes.ObtenerConDetallesAsync(ordenId);
        if (orden is null)
            return ApiResponse<OrdenDto>.Fallido("Orden no encontrada");

        if (orden.Estado == EstadoOrden.Pagada || orden.Estado == EstadoOrden.Cancelada)
            return ApiResponse<OrdenDto>.Fallido("No se pueden agregar items a una orden cerrada o cancelada");

        var nuevosDetalles = new List<OrdenDetalle>();

        foreach (var item in dto.Items)
        {
            var (detalle, error) = await ConstruirDetalleAsync(item);
            if (error is not null) return ApiResponse<OrdenDto>.Fallido(error);

            detalle!.OrdenId = ordenId;
            orden.Detalles.Add(detalle);
            nuevosDetalles.Add(detalle);
            orden.Total += detalle.PrecioUnitario * detalle.Cantidad;
        }

        // Si la orden ya estaba en un estado posterior a "en cocina" (lista,
        // para cobrar), agregar más ítems la regresa a cocina — hay comida
        // nueva que preparar (o, si son solo productos de tienda, igual
        // vuelve a "en cocina" para que el mesero repita el flujo normal de
        // "Marcar lista" antes de pasar a caja).
        orden.Estado = EstadoOrden.EnCocina;
        await _uow.Ordenes.ActualizarAsync(orden);
        await _uow.GuardarCambiosAsync();

        var ordenDto = MapearDto(orden);
        await _hub.Clients.Group("cocina").SendAsync("NuevaPedido", ordenDto);
        var paraCocina = nuevosDetalles.Where(d => d.ItemMenuId.HasValue).ToList();
        if (paraCocina.Count > 0)
            await _printer.ImprimirAsync(orden, paraCocina, "AGREGADO A COMANDA");

        return ApiResponse<OrdenDto>.Exitoso(ordenDto, "Items agregados exitosamente");
    }

    // Construye un OrdenDetalle a partir de la entrada del cliente: valida
    // que venga exactamente un tipo de ítem (plato de cocina O producto de
    // tienda), resuelve su precio, y aplica el descuento de stock que le
    // corresponda (receta de insumos para platos, stock directo para
    // productos).
    private async Task<(OrdenDetalle? Detalle, string? Error)> ConstruirDetalleAsync(CrearOrdenDetalleDto item)
    {
        if ((item.ItemMenuId is null) == (item.ProductoId is null))
            return (null, "Cada ítem debe ser un plato del menú o un producto de tienda (no ambos ni ninguno)");

        if (item.ItemMenuId is int itemMenuId)
        {
            var itemMenu = await _uow.ItemsMenu.ObtenerConRecetaAsync(itemMenuId);
            if (itemMenu is null || !itemMenu.Activo)
                return (null, $"El item de menú {itemMenuId} no existe o no está disponible");

            var detalle = new OrdenDetalle
            {
                ItemMenuId = itemMenuId,
                ItemMenu = itemMenu,
                Cantidad = item.Cantidad,
                PrecioUnitario = itemMenu.Precio,
                Observaciones = item.Observaciones,
                Estado = EstadoOrdenDetalle.Pendiente,
            };

            // Antes el cocinero registraba el consumo de insumos manualmente
            // al final del día; ahora se descuenta solo apenas se envía el
            // pedido a cocina, según la receta de cada plato.
            await ConsumirRecetaAsync(itemMenu, item.Cantidad);

            return (detalle, null);
        }

        var producto = await _uow.Productos.ObtenerPorIdAsync(item.ProductoId!.Value);
        if (producto is null || !producto.Activo)
            return (null, $"El producto {item.ProductoId} no existe o no está disponible");

        producto.Stock -= item.Cantidad;
        await _uow.Productos.ActualizarAsync(producto);

        return (new OrdenDetalle
        {
            ProductoId = producto.ProductoId,
            Producto = producto,
            Cantidad = item.Cantidad,
            PrecioUnitario = producto.Precio,
            Observaciones = item.Observaciones,
            Estado = EstadoOrdenDetalle.Pendiente,
        }, null);
    }

    public async Task<ApiResponse<OrdenDetalleDto>> ActualizarEstadoDetalleAsync(int detalleId, ActualizarEstadoDetalleDto dto)
    {
        var detalle = await _uow.Ordenes.BuscarAsync(o =>
            o.Detalles.Any(d => d.OrdenDetalleId == detalleId));

        var orden = await _uow.Ordenes.ObtenerConDetallesAsync(
            detalle.FirstOrDefault()?.OrdenId ?? 0);

        if (orden is null)
            return ApiResponse<OrdenDetalleDto>.Fallido("Detalle no encontrado");

        var detalleOrden = orden.Detalles.FirstOrDefault(d => d.OrdenDetalleId == detalleId);
        if (detalleOrden is null)
            return ApiResponse<OrdenDetalleDto>.Fallido("Detalle no encontrado");

        detalleOrden.Estado = dto.Estado;

        // Cuando cocina termina todos los items → regresa al mozo como Lista
        if (orden.Detalles.All(d => d.Estado == EstadoOrdenDetalle.Listo))
        {
            orden.Estado = EstadoOrden.Lista;
            await _hub.Clients.Group($"mesero-{orden.UsuarioId}").SendAsync("PedidoListo", orden.OrdenId);
        }

        await _uow.Ordenes.ActualizarAsync(orden);
        await _uow.GuardarCambiosAsync();

        var detalleDto = MapearDetalleDto(detalleOrden);
        await _hub.Clients.Group($"mesero-{orden.UsuarioId}").SendAsync("ItemActualizado", detalleDto);

        return ApiResponse<OrdenDetalleDto>.Exitoso(detalleDto, "Estado actualizado exitosamente");
    }

    // Sin pantalla de cocina (se reemplazó por el ticket impreso), es el
    // propio mesero quien marca la orden como lista una vez que la retira
    // de cocina, en vez de que cocina vaya marcando item por item.
    public async Task<ApiResponse<OrdenDto>> MarcarListaAsync(int ordenId)
    {
        var orden = await _uow.Ordenes.ObtenerConDetallesAsync(ordenId);
        if (orden is null)
            return ApiResponse<OrdenDto>.Fallido("Orden no encontrada");

        if (orden.Estado != EstadoOrden.EnCocina)
            return ApiResponse<OrdenDto>.Fallido("Solo se pueden marcar como listas las órdenes que están en cocina");

        foreach (var detalle in orden.Detalles)
            detalle.Estado = EstadoOrdenDetalle.Listo;

        orden.Estado = EstadoOrden.Lista;
        await _uow.Ordenes.ActualizarAsync(orden);
        await _uow.GuardarCambiosAsync();

        return ApiResponse<OrdenDto>.Exitoso(MapearDto(orden), "Orden marcada como lista");
    }

    public async Task<ApiResponse<OrdenDto>> PasarACajaAsync(int ordenId)
    {
        var orden = await _uow.Ordenes.ObtenerConDetallesAsync(ordenId);
        if (orden is null)
            return ApiResponse<OrdenDto>.Fallido("Orden no encontrada");

        if (orden.Estado != EstadoOrden.Lista)
            return ApiResponse<OrdenDto>.Fallido("Solo se pueden pasar a caja órdenes que cocina marcó como listas");

        orden.Estado = EstadoOrden.ParaCobrar;
        await _uow.Ordenes.ActualizarAsync(orden);
        await _uow.GuardarCambiosAsync();

        var ordenDto = MapearDto(orden);
        await _hub.Clients.Group("cajero").SendAsync("OrdenParaCobrar", ordenDto);

        return ApiResponse<OrdenDto>.Exitoso(ordenDto, "Orden enviada a caja");
    }

    public async Task<ApiResponse<OrdenDto>> CerrarOrdenAsync(int ordenId)
    {
        var orden = await _uow.Ordenes.ObtenerConDetallesAsync(ordenId);
        if (orden is null)
            return ApiResponse<OrdenDto>.Fallido("Orden no encontrada");

        var sinCobrar = orden.Detalles.Any(d =>
            d.Estado != EstadoOrdenDetalle.Cancelado && d.ComprobanteId is null);
        if (sinCobrar)
            return ApiResponse<OrdenDto>.Fallido("Hay platos sin cobrar, no se puede cerrar la mesa");

        orden.Estado = EstadoOrden.Pagada;
        orden.FechaCierre = DateTime.UtcNow;

        var mesa = await _uow.Mesas.ObtenerPorIdAsync(orden.MesaId);
        if (mesa is not null)
        {
            mesa.Estado = EstadoMesa.Disponible;
            await _uow.Mesas.ActualizarAsync(mesa);
        }

        await _uow.Ordenes.ActualizarAsync(orden);
        await _uow.GuardarCambiosAsync();

        await _hub.Clients.All.SendAsync("MesaDisponible", orden.MesaId);

        return ApiResponse<OrdenDto>.Exitoso(MapearDto(orden), "Orden cerrada exitosamente");
    }

    public async Task<ApiResponse> CancelarAsync(int ordenId, string motivo)
    {
        var orden = await _uow.Ordenes.ObtenerConDetallesAsync(ordenId);
        if (orden is null)
            return ApiResponse.Fallido("Orden no encontrada");

        if (orden.Estado == EstadoOrden.Pagada)
            return ApiResponse.Fallido("No se puede cancelar una orden ya pagada");

        // El insumo de esta orden ya se descontó al enviarla a cocina (ver
        // CrearAsync/AgregarItemsAsync); al cancelarla se devuelve, porque
        // nunca se llegó a preparar/entregar.
        await DevolverRecetaAsync(orden);

        orden.Estado = EstadoOrden.Cancelada;
        orden.MotivoCancelacion = motivo;
        orden.FechaCierre = DateTime.UtcNow;

        var mesa = await _uow.Mesas.ObtenerPorIdAsync(orden.MesaId);
        if (mesa is not null)
        {
            mesa.Estado = EstadoMesa.Disponible;
            await _uow.Mesas.ActualizarAsync(mesa);
        }

        await _uow.Ordenes.ActualizarAsync(orden);
        await _uow.GuardarCambiosAsync();

        await _hub.Clients.All.SendAsync("MesaDisponible", orden.MesaId);

        return ApiResponse.Exitoso("Orden cancelada exitosamente");
    }

    // Saca un solo plato/producto de una orden ya enviada (el cliente cambió
    // de opinión, pidió otra cosa, etc.), sin cancelar toda la orden. Devuelve
    // el stock/insumo que se había descontado, igual que una cancelación
    // total, pero solo para esta línea.
    public async Task<ApiResponse<OrdenDto>> EliminarDetalleAsync(int detalleId)
    {
        var ordenesConDetalle = await _uow.Ordenes.BuscarAsync(o => o.Detalles.Any(d => d.OrdenDetalleId == detalleId));
        var ordenBase = ordenesConDetalle.FirstOrDefault();
        if (ordenBase is null)
            return ApiResponse<OrdenDto>.Fallido("Detalle no encontrado");

        var orden = await _uow.Ordenes.ObtenerConDetallesAsync(ordenBase.OrdenId);
        var detalle = orden!.Detalles.FirstOrDefault(d => d.OrdenDetalleId == detalleId);
        if (detalle is null)
            return ApiResponse<OrdenDto>.Fallido("Detalle no encontrado");

        if (detalle.Estado == EstadoOrdenDetalle.Cancelado)
            return ApiResponse<OrdenDto>.Fallido("Este ítem ya fue eliminado");
        if (detalle.ComprobanteId is not null)
            return ApiResponse<OrdenDto>.Fallido("No se puede eliminar un ítem que ya fue cobrado");

        await DevolverStockDetalleAsync(detalle);

        orden.Total -= detalle.PrecioUnitario * detalle.Cantidad;
        detalle.Estado = EstadoOrdenDetalle.Cancelado;

        await _uow.Ordenes.ActualizarAsync(orden);
        await _uow.GuardarCambiosAsync();

        var ordenDto = MapearDto(orden);
        await _hub.Clients.Group($"mesero-{orden.UsuarioId}").SendAsync("ItemActualizado", MapearDetalleDto(detalle));

        return ApiResponse<OrdenDto>.Exitoso(ordenDto, "Ítem eliminado de la orden");
    }

    // Descuenta el stock de cada insumo de la receta de `itemMenu`, según la
    // cantidad vendida. No bloquea la orden si el stock queda negativo — eso
    // solo se refleja como alerta en Inventario, no debe frenar a un mesero
    // que ya está tomando un pedido.
    private async Task ConsumirRecetaAsync(ItemMenu itemMenu, int cantidadVendida)
    {
        foreach (var receta in itemMenu.Receta)
        {
            var cantidad = ConversionUnidades.RecetaAStockInsumo(receta.Cantidad, receta.Insumo.Unidad) * cantidadVendida;
            receta.Insumo.StockActual -= cantidad;
            await _uow.Insumos.ActualizarAsync(receta.Insumo);
        }
    }

    // Reverso de ConsumirRecetaAsync / del descuento de stock de tienda,
    // para cuando se cancela una orden que ya había descontado stock.
    private async Task DevolverRecetaAsync(Orden orden)
    {
        foreach (var detalle in orden.Detalles)
            await DevolverStockDetalleAsync(detalle);
    }

    // Misma reversa que DevolverRecetaAsync pero para un solo detalle —
    // usada tanto al cancelar la orden completa como al sacar un ítem suelto.
    private async Task DevolverStockDetalleAsync(OrdenDetalle detalle)
    {
        if (detalle.ItemMenuId is int itemMenuId)
        {
            var itemMenu = await _uow.ItemsMenu.ObtenerConRecetaAsync(itemMenuId);
            if (itemMenu is null) return;

            foreach (var receta in itemMenu.Receta)
            {
                var cantidad = ConversionUnidades.RecetaAStockInsumo(receta.Cantidad, receta.Insumo.Unidad) * detalle.Cantidad;
                receta.Insumo.StockActual += cantidad;
                await _uow.Insumos.ActualizarAsync(receta.Insumo);
            }
        }
        else if (detalle.ProductoId is int productoId)
        {
            var producto = await _uow.Productos.ObtenerPorIdAsync(productoId);
            if (producto is null) return;

            producto.Stock += detalle.Cantidad;
            await _uow.Productos.ActualizarAsync(producto);
        }
    }

    private static OrdenDto MapearDto(Orden o) => new()
    {
        OrdenId = o.OrdenId,
        MesaId = o.MesaId,
        NumeroMesa = o.Mesa?.Numero ?? 0,
        UsuarioId = o.UsuarioId,
        NombreMesero = o.Usuario?.Empleado is not null ? $"{o.Usuario.Empleado.Nombres} {o.Usuario.Empleado.Apellidos}" : string.Empty,
        Estado = o.Estado,
        Total = o.Total,
        Observaciones = o.Observaciones,
        MotivoCancelacion = o.MotivoCancelacion,
        FechaApertura = o.FechaApertura,
        FechaCierre = o.FechaCierre,
        Detalles = o.Detalles.Select(MapearDetalleDto).ToList()
    };

    private static OrdenDetalleDto MapearDetalleDto(OrdenDetalle d) => new()
    {
        OrdenDetalleId = d.OrdenDetalleId,
        ItemMenuId = d.ItemMenuId,
        ProductoId = d.ProductoId,
        Origen = d.ItemMenuId.HasValue ? "cocina" : "tienda",
        NombreItem = d.ItemMenuId.HasValue ? (d.ItemMenu?.Nombre ?? string.Empty) : (d.Producto?.Nombre ?? string.Empty),
        Cantidad = d.Cantidad,
        PrecioUnitario = d.PrecioUnitario,
        Estado = d.Estado,
        Observaciones = d.Observaciones,
        ComprobanteId = d.ComprobanteId,
    };
}
