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
        if (await _uow.Clientes.ExisteAsync(c => c.Dni == dto.Dni))
            return ApiResponse<ClienteDto>.Fallido($"Ya existe un cliente con DNI {dto.Dni}");

        var cliente = new Cliente
        {
            Nombres = dto.Nombres,
            Apellidos = dto.Apellidos,
            Dni = dto.Dni,
            Telefono = dto.Telefono,
            Email = dto.Email,
            Direccion = dto.Direccion
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

        if (await _uow.Clientes.ExisteAsync(c => c.Dni == dto.Dni && c.ClienteId != dto.ClienteId))
            return ApiResponse<ClienteDto>.Fallido($"El DNI {dto.Dni} ya pertenece a otro cliente");

        cliente.Nombres = dto.Nombres;
        cliente.Apellidos = dto.Apellidos;
        cliente.Dni = dto.Dni;
        cliente.Telefono = dto.Telefono;
        cliente.Email = dto.Email;
        cliente.Direccion = dto.Direccion;

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

    private static ClienteDto MapearDto(Cliente c) => new()
    {
        ClienteId = c.ClienteId,
        Nombres = c.Nombres,
        Apellidos = c.Apellidos,
        Dni = c.Dni,
        Telefono = c.Telefono,
        Email = c.Email,
        Direccion = c.Direccion,
        FechaRegistro = c.FechaRegistro,
        Activo = c.Activo
    };
}
