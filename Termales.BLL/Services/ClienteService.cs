using Termales.BLL.Interfaces;
using Termales.Common.DTOs;
using Termales.Common.Wrappers;
using Termales.DAL.UnitOfWork;
using Termales.Entities.Models;

namespace Termales.BLL.Services;

public class ClienteService : IClienteService
{
    private readonly IUnitOfWork _uow;

    public ClienteService(IUnitOfWork uow) => _uow = uow;

    public async Task<ApiResponse<ClienteDto>> ObtenerPorIdAsync(int id)
    {
        var cliente = await _uow.Clientes.ObtenerPorIdAsync(id);
        if (cliente is null)
            return ApiResponse<ClienteDto>.Fallido("Cliente no encontrado");
        return ApiResponse<ClienteDto>.Exitoso(MapearDto(cliente));
    }

    public async Task<ApiResponse<ClienteDto>> ObtenerPorDniAsync(string dni)
    {
        var cliente = await _uow.Clientes.ObtenerPorDniAsync(dni);
        if (cliente is null)
            return ApiResponse<ClienteDto>.Fallido("Cliente no encontrado");
        return ApiResponse<ClienteDto>.Exitoso(MapearDto(cliente));
    }

    public async Task<PagedResponse<ClienteDto>> ObtenerPaginadoAsync(int pagina, int tamanoPagina, string? busqueda)
    {
        var (items, total) = await _uow.Clientes.ObtenerPaginadoAsync(pagina, tamanoPagina, busqueda);
        return PagedResponse<ClienteDto>.Crear(items.Select(MapearDto), pagina, tamanoPagina, total);
    }

    public async Task<ApiResponse<ClienteDto>> CrearAsync(CrearClienteDto dto)
    {
        var validacion = ValidarDocumento(dto);
        if (validacion is not null) return ApiResponse<ClienteDto>.Fallido(validacion);

        if (await _uow.Clientes.ExisteAsync(c => c.NumeroDocumento == dto.NumeroDocumento))
            return ApiResponse<ClienteDto>.Fallido($"Ya existe un cliente con {dto.TipoDocumento} {dto.NumeroDocumento}");

        var cliente = new Cliente
        {
            TipoDocumento  = dto.TipoDocumento,
            NumeroDocumento = dto.NumeroDocumento,
            Nombres        = dto.Nombres,
            Apellidos      = dto.Apellidos,
            RazonSocial    = dto.RazonSocial,
            Telefono       = dto.Telefono,
            Email          = dto.Email,
            Direccion      = dto.Direccion,
        };

        await _uow.Clientes.AgregarAsync(cliente);
        await _uow.GuardarCambiosAsync();
        return ApiResponse<ClienteDto>.Exitoso(MapearDto(cliente), "Cliente registrado exitosamente");
    }

    public async Task<ApiResponse<ClienteDto>> ActualizarAsync(ActualizarClienteDto dto)
    {
        var cliente = await _uow.Clientes.ObtenerPorIdAsync(dto.ClienteId);
        if (cliente is null)
            return ApiResponse<ClienteDto>.Fallido("Cliente no encontrado");

        var validacion = ValidarDocumento(dto);
        if (validacion is not null) return ApiResponse<ClienteDto>.Fallido(validacion);

        if (await _uow.Clientes.ExisteAsync(c => c.NumeroDocumento == dto.NumeroDocumento && c.ClienteId != dto.ClienteId))
            return ApiResponse<ClienteDto>.Fallido($"El {dto.TipoDocumento} {dto.NumeroDocumento} ya pertenece a otro cliente");

        cliente.TipoDocumento   = dto.TipoDocumento;
        cliente.NumeroDocumento = dto.NumeroDocumento;
        cliente.Nombres         = dto.Nombres;
        cliente.Apellidos       = dto.Apellidos;
        cliente.RazonSocial     = dto.RazonSocial;
        cliente.Telefono        = dto.Telefono;
        cliente.Email           = dto.Email;
        cliente.Direccion       = dto.Direccion;

        await _uow.Clientes.ActualizarAsync(cliente);
        await _uow.GuardarCambiosAsync();
        return ApiResponse<ClienteDto>.Exitoso(MapearDto(cliente), "Cliente actualizado exitosamente");
    }

    public async Task<ApiResponse> DesactivarAsync(int id)
    {
        var cliente = await _uow.Clientes.ObtenerPorIdAsync(id);
        if (cliente is null)
            return ApiResponse.Fallido("Cliente no encontrado");

        cliente.Activo = false;
        await _uow.Clientes.ActualizarAsync(cliente);
        await _uow.GuardarCambiosAsync();
        return ApiResponse.Exitoso("Cliente desactivado exitosamente");
    }

    private static string? ValidarDocumento(CrearClienteDto dto)
    {
        if (dto.TipoDocumento == "RUC")
        {
            if (string.IsNullOrWhiteSpace(dto.NumeroDocumento) || dto.NumeroDocumento.Length != 11 || !dto.NumeroDocumento.All(char.IsDigit))
                return "El RUC debe tener exactamente 11 dígitos";
            if (string.IsNullOrWhiteSpace(dto.RazonSocial))
                return "La razón social es requerida para empresas";
        }
        else
        {
            if (string.IsNullOrWhiteSpace(dto.NumeroDocumento) || dto.NumeroDocumento.Length != 8 || !dto.NumeroDocumento.All(char.IsDigit))
                return "El DNI debe tener exactamente 8 dígitos";
            if (string.IsNullOrWhiteSpace(dto.Nombres))
                return "Los nombres son requeridos para personas naturales";
            if (string.IsNullOrWhiteSpace(dto.Apellidos))
                return "Los apellidos son requeridos para personas naturales";
        }
        return null;
    }

    private static ClienteDto MapearDto(Cliente c) => new()
    {
        ClienteId       = c.ClienteId,
        TipoDocumento   = c.TipoDocumento,
        NumeroDocumento = c.NumeroDocumento,
        Nombres         = c.Nombres,
        Apellidos       = c.Apellidos,
        RazonSocial     = c.RazonSocial,
        Telefono        = c.Telefono,
        Email           = c.Email,
        Direccion       = c.Direccion,
        FechaRegistro   = c.FechaRegistro,
        Activo          = c.Activo,
    };
}
