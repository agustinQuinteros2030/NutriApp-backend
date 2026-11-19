using System.ComponentModel.DataAnnotations;

namespace NutriApi.DTOs.Usuarios;

public class ActualizarAvatarDto
{
    [Required]
    [MaxLength(32)]
    public string AvatarId { get; set; } = string.Empty;
}
