using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using NutriApi.Services.PlanPaciente;

using System.Security.Claims;

namespace NutriApi.Controllers;

[ApiController]
[Authorize(Roles = Roles.Paciente)]
[Route("api/paciente")]
public class MiPlanController
    : ControllerBase
{
    private readonly IPlanPacienteService
        _planPacienteService;


    public MiPlanController(
        IPlanPacienteService planPacienteService)
    {
        _planPacienteService =
            planPacienteService;
    }


    // ==========================================
    // MI PLAN
    // ==========================================

    [HttpGet("mi-plan")]
    public async Task<IActionResult>
        ObtenerMiPlan()
    {
        var pacienteId =
            ObtenerPacienteId();


        if (!pacienteId.HasValue)
        {
            return Unauthorized();
        }


        var plan =
            await _planPacienteService
                .ObtenerMiPlanAsync(
                    pacienteId.Value
                );


        if (plan is null)
        {
            return NotFound(
                new
                {
                    error =
                        "No tenés una dieta activa asignada."
                }
            );
        }


        return Ok(
            plan
        );
    }


    // ==========================================
    // CLAIM
    // ==========================================

    private int?
        ObtenerPacienteId()
    {
        var claim =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier
            );


        if (!int.TryParse(
            claim,
            out var pacienteId))
        {
            return null;
        }


        return pacienteId;
    }
}