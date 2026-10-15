using Termales.BLL.Interfaces.Inventario;
using Termales.Common.DTOs.Inventario;
using Termales.DAL.UnitOfWork;

namespace Termales.BLL.Services.Inventario;

public class KardexService : IKardexService
{
    private readonly IUnitOfWork _uow;

    public KardexService(IUnitOfWork uow) => _uow = uow;

    public async Task<KardexInsumoDto?> ObtenerKardexInsumoAsync(int insumoId, DateTime desde, DateTime hasta)
    {
        var insumo = await _uow.Insumos.ObtenerPorIdAsync(insumoId);
        if (insumo is null) return null;

        var entradas = (await _uow.EntradasInsumo.ObtenerPorInsumoAsync(insumoId))
            .Where(e => e.Fecha >= desde && e.Fecha < hasta).ToList();

        var salidas = (await _uow.SalidasInsumo.ObtenerPorInsumoAsync(insumoId))
            .Where(s => s.Fecha >= desde && s.Fecha < hasta).ToList();

        var totalEntradasPeriodo = entradas.Sum(e => e.Cantidad);
        var totalSalidasPeriodo  = salidas.Sum(s => s.Cantidad);
        var saldoInicial = insumo.StockActual - totalEntradasPeriodo + totalSalidasPeriodo;
        var costo = insumo.PrecioReferencia;
        var valorSaldoInicial = Math.Round(saldoInicial * costo, 2);

        var movs = new List<KardexMovimientoDto>();
        foreach (var e in entradas)
            movs.Add(new KardexMovimientoDto
            {
                Fecha = e.Fecha, Tipo = "Entrada", Concepto = "Compra",
                Referencia = e.CompraId.HasValue ? $"Compra #{e.CompraId}" : null,
                CostoUnitario = e.PrecioUnitario, Entrada = e.Cantidad, Salida = 0,
                ValorEntrada = e.Total, Observacion = e.Observacion,
            });
        foreach (var s in salidas)
            movs.Add(new KardexMovimientoDto
            {
                Fecha = s.Fecha, Tipo = "Salida", Concepto = s.Motivo ?? "Consumo",
                CostoUnitario = costo, Entrada = 0, Salida = s.Cantidad,
                ValorSalida = Math.Round(s.Cantidad * costo, 2),
                Responsable = s.RegistradoPor, Observacion = s.Observacion,
            });

        movs = movs.OrderBy(m => m.Fecha).ToList();
        var saldo = saldoInicial;
        var valorSaldo = valorSaldoInicial;
        foreach (var m in movs)
        {
            saldo += m.Entrada - m.Salida;
            valorSaldo = Math.Round(valorSaldo + (m.ValorEntrada ?? 0) - (m.ValorSalida ?? 0), 2);
            m.Saldo = saldo; m.ValorSaldo = valorSaldo;
        }

        return new KardexInsumoDto
        {
            InsumoId = insumo.InsumoId, TipoArticulo = "insumo",
            Nombre = insumo.Nombre, Unidad = insumo.Unidad, TipoAmbiente = insumo.TipoAmbiente,
            StockActual = insumo.StockActual, SaldoInicial = saldoInicial,
            TotalEntradas = totalEntradasPeriodo, TotalSalidas = totalSalidasPeriodo,
            ValorSaldoInicial = valorSaldoInicial,
            ValorTotalEntradas = Math.Round(entradas.Sum(e => e.Total), 2),
            ValorTotalSalidas = Math.Round(totalSalidasPeriodo * costo, 2),
            ValorSaldoFinal = Math.Round(insumo.StockActual * costo, 2),
            Movimientos = movs,
        };
    }

    public async Task<KardexInsumoDto?> ObtenerKardexProductoAsync(int productoId, DateTime desde, DateTime hasta)
    {
        var producto = await _uow.Productos.ObtenerPorIdAsync(productoId);
        if (producto is null) return null;

        var entradas = (await _uow.EntradasProducto.ObtenerPorProductoAsync(productoId))
            .Where(e => e.Fecha >= desde && e.Fecha < hasta).ToList();

        var salidas = (await _uow.SalidasProducto.ObtenerPorProductoAsync(productoId))
            .Where(s => s.Fecha >= desde && s.Fecha < hasta).ToList();

        var totalEntradasPeriodo = entradas.Sum(e => e.Cantidad);
        var totalSalidasPeriodo  = salidas.Sum(s => s.Cantidad);
        var saldoInicial = producto.Stock - totalEntradasPeriodo + totalSalidasPeriodo;
        var costo = producto.PrecioCompra;
        var valorSaldoInicial = Math.Round(saldoInicial * costo, 2);

        var movs = new List<KardexMovimientoDto>();
        foreach (var e in entradas)
            movs.Add(new KardexMovimientoDto
            {
                Fecha = e.Fecha, Tipo = "Entrada", Concepto = "Compra",
                Referencia = e.CompraId.HasValue ? $"Compra #{e.CompraId}" : null,
                CostoUnitario = e.PrecioUnitario, Entrada = e.Cantidad, Salida = 0,
                ValorEntrada = e.Total, Observacion = e.Observacion,
            });
        foreach (var s in salidas)
            movs.Add(new KardexMovimientoDto
            {
                Fecha = s.Fecha, Tipo = "Salida", Concepto = "Venta",
                CostoUnitario = costo, Entrada = 0, Salida = s.Cantidad,
                ValorSalida = Math.Round(s.Cantidad * costo, 2),
                Observacion = s.Observacion,
            });

        movs = movs.OrderBy(m => m.Fecha).ToList();
        decimal saldo = saldoInicial;
        var valorSaldo = valorSaldoInicial;
        foreach (var m in movs)
        {
            saldo += m.Entrada - m.Salida;
            valorSaldo = Math.Round(valorSaldo + (m.ValorEntrada ?? 0) - (m.ValorSalida ?? 0), 2);
            m.Saldo = saldo; m.ValorSaldo = valorSaldo;
        }

        return new KardexInsumoDto
        {
            InsumoId = producto.ProductoId, TipoArticulo = "producto",
            Nombre = producto.Nombre, Unidad = "unid", TipoAmbiente = "tienda",
            StockActual = producto.Stock, SaldoInicial = saldoInicial,
            TotalEntradas = totalEntradasPeriodo, TotalSalidas = totalSalidasPeriodo,
            ValorSaldoInicial = valorSaldoInicial,
            ValorTotalEntradas = Math.Round(entradas.Sum(e => e.Total), 2),
            ValorTotalSalidas = Math.Round(totalSalidasPeriodo * costo, 2),
            ValorSaldoFinal = Math.Round(producto.Stock * costo, 2),
            Movimientos = movs,
        };
    }

    public async Task<IEnumerable<KardexResumenDto>> ObtenerResumenGeneralAsync(
        DateTime desde, DateTime hasta, string? tipoAmbiente)
    {
        var resultado = new List<KardexResumenDto>();
        var incluirInsumos = string.IsNullOrWhiteSpace(tipoAmbiente) || tipoAmbiente != "tienda";
        var incluirProductos = string.IsNullOrWhiteSpace(tipoAmbiente) || tipoAmbiente == "tienda";

        if (incluirInsumos)
        {
            var ambientes = string.IsNullOrWhiteSpace(tipoAmbiente)
                ? new[] { "comedor", "banio", "habitacion" }
                : new[] { tipoAmbiente };

            foreach (var amb in ambientes)
            {
                var insumos = (await _uow.Insumos.ObtenerPorAmbienteAsync(amb)).Where(i => i.Activo);
                foreach (var ins in insumos)
                {
                    var ent = (await _uow.EntradasInsumo.ObtenerPorInsumoAsync(ins.InsumoId))
                        .Where(e => e.Fecha >= desde && e.Fecha < hasta).Sum(e => e.Cantidad);
                    var sal = (await _uow.SalidasInsumo.ObtenerPorInsumoAsync(ins.InsumoId))
                        .Where(s => s.Fecha >= desde && s.Fecha < hasta).Sum(s => s.Cantidad);
                    var costo = ins.PrecioReferencia;
                    resultado.Add(new KardexResumenDto
                    {
                        InsumoId = ins.InsumoId, TipoArticulo = "insumo",
                        Nombre = ins.Nombre, Unidad = ins.Unidad, TipoAmbiente = ins.TipoAmbiente,
                        SaldoInicial = ins.StockActual - ent + sal,
                        TotalEntradas = ent, TotalSalidas = sal, SaldoFinal = ins.StockActual,
                        StockMinimo = ins.StockMinimo, CostoUnitario = costo,
                        ValorSaldoFinal = Math.Round(ins.StockActual * costo, 2),
                    });
                }
            }
        }

        if (incluirProductos)
        {
            var productos = (await _uow.Productos.ObtenerActivosAsync());
            foreach (var prod in productos)
            {
                var ent = (await _uow.EntradasProducto.ObtenerPorProductoAsync(prod.ProductoId))
                    .Where(e => e.Fecha >= desde && e.Fecha < hasta).Sum(e => e.Cantidad);
                var sal = (await _uow.SalidasProducto.ObtenerPorProductoAsync(prod.ProductoId))
                    .Where(s => s.Fecha >= desde && s.Fecha < hasta).Sum(s => s.Cantidad);
                var costo = prod.PrecioCompra;
                resultado.Add(new KardexResumenDto
                {
                    InsumoId = prod.ProductoId, TipoArticulo = "producto",
                    Nombre = prod.Nombre, Unidad = "unid", TipoAmbiente = "tienda",
                    SaldoInicial = prod.Stock - ent + sal,
                    TotalEntradas = ent, TotalSalidas = sal, SaldoFinal = prod.Stock,
                    StockMinimo = prod.StockMinimo, CostoUnitario = costo,
                    ValorSaldoFinal = Math.Round(prod.Stock * costo, 2),
                });
            }
        }

        return resultado.OrderBy(r => r.TipoAmbiente).ThenBy(r => r.Nombre);
    }
}
