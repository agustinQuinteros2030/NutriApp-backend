using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using NutriApi.Services.Dashboard;

using System.Security.Claims;

namespace NutriApi.Controllers;

[ApiController]
[Route("api/nutricionista/dashboard")]
[Authorize(
    Roles =
        Roles.Nutricionista
)]
public class DashboardNutricionistaController
    : ControllerBase
{
    private readonly
        IDashboardNutricionistaService
        _service;


    public DashboardNutricionistaController(
        IDashboardNutricionistaService service)
    {
        _service =
            service;
    }


    [HttpGet]
    public async Task<IActionResult>
        Obtener()
    {
        var nutricionistaId =
            ObtenerUsuarioId();


        if (!nutricionistaId.HasValue)
        {
            return Unauthorized();
        }


        var dashboard =
            await _service
                .ObtenerAsync(
                    nutricionistaId.Value
                );


        return Ok(
            dashboard
        );
    }


    private int?
        ObtenerUsuarioId()
    {
        var claim =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier
            );


        if (!int.TryParse(
            claim,
            out var id))
        {
            return null;
        }


        return id;
    }
}