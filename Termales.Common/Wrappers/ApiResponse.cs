namespace Termales.Common.Wrappers;

public class ApiResponse<T>
{
    public bool Exito { get; set; }
    public string Mensaje { get; set; } = string.Empty;
    public T? Data { get; set; }
    public List<string> Errores { get; set; } = new();

    public static ApiResponse<T> Exitoso(T data, string mensaje = "Operación exitosa") =>
        new() { Exito = true, Mensaje = mensaje, Data = data };

    public static ApiResponse<T> Fallido(string mensaje, List<string>? errores = null) =>
        new() { Exito = false, Mensaje = mensaje, Errores = errores ?? new() };
}

public class ApiResponse : ApiResponse<object>
{
    public static ApiResponse Exitoso(string mensaje = "Operación exitosa") =>
        new() { Exito = true, Mensaje = mensaje };

    public static new ApiResponse Fallido(string mensaje, List<string>? errores = null) =>
        new() { Exito = false, Mensaje = mensaje, Errores = errores ?? new() };
}
