namespace NutriApi.Services.Email;

public interface IEmailService
{
    Task<ResultadoEmail> EnviarAsync(
        string destinatario,
        string asunto,
        string contenidoHtml,
        CancellationToken cancellationToken = default
    );
}