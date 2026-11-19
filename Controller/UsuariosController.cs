using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NutriApi.DTOs.Usuarios;
using NutriApi.Validaciones;

namespace NutriApi.Controllers;

[ApiController]
[Route("api/usuarios")]
[Authorize]
public class UsuariosController : ControllerBase
{
    private readonly UserManager<UsuarioAplicacion> _userManager;

    public UsuariosController(UserManager<UsuarioAplicacion> userManager)
    {
        _userManager = userManager;
    }

    [HttpPut("me/avatar")]
    public async Task<IActionResult> ActualizarMiAvatar([FromBody] ActualizarAvatarDto dto)
    {
        if (!AvatarCatalogo.EsValido(dto.AvatarId))
        {
            return BadRequest(new { mensaje = "El avatar seleccionado no es válido." });
        }

        var usuario = await _userManager.GetUserAsync(User);

        if (usuario is null)
        {
            return Unauthorized(new { mensaje = "Usuario no autenticado." });
        }

        usuario.AvatarId = dto.AvatarId;

        var resultado = await _userManager.UpdateAsync(usuario);

        if (!resultado.Succeeded)
        {
            return BadRequest(new { mensaje = "No se pudo actualizar el avatar." });
        }

        return Ok(new { avatarId = usuario.AvatarId });
    }
}
