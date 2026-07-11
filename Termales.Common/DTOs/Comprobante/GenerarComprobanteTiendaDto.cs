namespace Termales.Common.DTOs.Comprobante;

public class ItemTiendaDto
{
    public int ProductoId { get; set; }
    public int Cantidad { get; set; }
}

public class GenerarComprobanteTiendaDto : GenerarComprobanteDto
{
    public List<ItemTiendaDto> Items { get; set; } = new();
}
