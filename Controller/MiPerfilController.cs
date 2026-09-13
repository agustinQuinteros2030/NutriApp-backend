using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NutriApi.Services.MiPerfil;
using System.Security.Claims;

namespace NutriApi.Controllers;

[ApiController]
[Authorize(Roles = "Paciente")]
[Route("api/paciente")]
public class MiPerfilController
    : ControllerBase
{
    private readonly IPerfilPacienteService
        _service;


    public MiPerfilController(
        IPerfilPacienteService service)
    {
        _service = service;
    }


    [HttpGet("mi-perfil")]
    public async Task<IActionResult>
        ObtenerMiPerfil()
    {
        var claim =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier
            );


        if (!int.TryParse(
            claim,
            out var pacienteId))
        {
            return Unauthorized();
        }


        var perfil =
            await _service
                .ObtenerMiPerfilAsync(
                    pacienteId
                );


        if (perfil is null)
        {
            return NotFound(
                new
                {
                    error =
                        "Paciente no encontrado."
                }
            );
        }


        return Ok(
            perfil
        );
    }
}