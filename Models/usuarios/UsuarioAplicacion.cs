using Microsoft.AspNetCore.Identity;

public class UsuarioAplicacion : IdentityUser<int>
{
    public string Nombre { get; set; } = string.Empty;

    public string Apellido { get; set; } = string.Empty;

    public bool Activo { get; set; } = true;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
} 