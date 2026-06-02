namespace Termales.Common.Wrappers;

public class PagedResponse<T>
{
    public bool Exito { get; set; }
    public string Mensaje { get; set; } = string.Empty;
    public IEnumerable<T> Data { get; set; } = Enumerable.Empty<T>();
    public int Pagina { get; set; }
    public int TamanoPagina { get; set; }
    public int TotalRegistros { get; set; }
    public int TotalPaginas => (int)Math.Ceiling(TotalRegistros / (double)TamanoPagina);
    public bool TienePaginaAnterior => Pagina > 1;
    public bool TienePaginaSiguiente => Pagina < TotalPaginas;

    public static PagedResponse<T> Crear(IEnumerable<T> data, int pagina, int tamanoPagina, int total) =>
        new()
        {
            Exito = true,
            Mensaje = "Consulta exitosa",
            Data = data,
            Pagina = pagina,
            TamanoPagina = tamanoPagina,
            TotalRegistros = total
        };
}
