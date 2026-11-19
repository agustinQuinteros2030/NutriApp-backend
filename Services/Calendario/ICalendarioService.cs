using NutriApi.DTOs.Calendario;

namespace NutriApi.Services.Calendario;

public interface ICalendarioService
{
    Task<List<EventoCalendarioDto>> ObtenerEventosAsync(
        int nutricionistaId,
        DateOnly desde,
        DateOnly hasta
    );
}
