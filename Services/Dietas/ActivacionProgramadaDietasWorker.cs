using NutriApi.Services.Dietas;

public class ActivacionProgramadaDietasWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    private readonly ILogger<ActivacionProgramadaDietasWorker> _logger;

    public ActivacionProgramadaDietasWorker(
        IServiceScopeFactory scopeFactory,
        ILogger<ActivacionProgramadaDietasWorker> logger
    )
    {
        _scopeFactory = scopeFactory;

        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();

                var dietaService = scope.ServiceProvider.GetRequiredService<IDietaService>();

                var cantidad = await dietaService.ProcesarActivacionesProgramadasAsync();

                if (cantidad > 0)
                {
                    _logger.LogInformation("Se activaron {Cantidad} dietas programadas.", cantidad);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error procesando activaciones programadas.");
            }

            await Task.Delay(TimeSpan.FromMinutes(15), stoppingToken);
        }
    }
}
