using Termales.Common.DTOs;

namespace Termales.BLL.Interfaces;

public interface IDashboardService
{
    Task<DashboardComedorDto> GetComedorAsync();
    Task<DashboardBaniosDto> GetBaniosAsync();
    Task<DashboardHabitacionesDto> GetHabitacionesAsync();
    Task<DashboardTiendaDto> GetTiendaAsync();
}
