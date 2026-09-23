using System.ComponentModel.DataAnnotations;

namespace NutriApi.DTOs.Turnos;

public class CrearTurnoDto
{
    public DateTimeOffset FechaHora { get; set; }


    public ModalidadTurno Modalidad { get; set; }


    [MaxLength(
        200,
        ErrorMessage =
            "El lugar no puede superar los 200 caracteres."
    )]
    public string? Lugar { get; set; }


    [MaxLength(
        500,
        ErrorMessage =
            "El link de reunión no puede superar los 500 caracteres."
    )]
    public string? LinkReunion { get; set; }


    [MaxLength(
        250,
        ErrorMessage =
            "El motivo no puede superar los 250 caracteres."
    )]
    public string? Motivo { get; set; }


    [MaxLength(
        1000,
        ErrorMessage =
            "Las observaciones no pueden superar los 1000 caracteres."
    )]
    public string? Observaciones { get; set; }
}
