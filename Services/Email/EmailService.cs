using Microsoft.Extensions.Options;

using NutriApi.Configuracion;

using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace NutriApi.Services.Email;

public class EmailService
    : IEmailService
{
    private readonly HttpClient _httpClient;

    private readonly EmailOpciones _options;

    private readonly ILogger<EmailService>
        _logger;


    public EmailService(
        HttpClient httpClient,
        IOptions<EmailOpciones> options,
        ILogger<EmailService> logger)
    {
        _httpClient =
            httpClient;

        _options =
            options.Value;

        _logger =
            logger;
    }


    public async Task<ResultadoEmail>
        EnviarAsync(
            string destinatario,
            string asunto,
            string contenidoHtml,
            CancellationToken cancellationToken = default)
    {
        // ==========================================
        // VALIDACIONES
        // ==========================================

        if (string.IsNullOrWhiteSpace(
            destinatario))
        {
            return ResultadoEmail.Fallo(
                "El destinatario del email es obligatorio."
            );
        }


        if (string.IsNullOrWhiteSpace(
            asunto))
        {
            return ResultadoEmail.Fallo(
                "El asunto del email es obligatorio."
            );
        }


        if (string.IsNullOrWhiteSpace(
            contenidoHtml))
        {
            return ResultadoEmail.Fallo(
                "El contenido del email es obligatorio."
            );
        }


        if (string.IsNullOrWhiteSpace(
            _options.ApiKey))
        {
            _logger.LogError(
                "No se configuró Email:ApiKey."
            );


            return ResultadoEmail.Fallo(
                "El servicio de email no está configurado."
            );
        }


        if (string.IsNullOrWhiteSpace(
            _options.FromEmail))
        {
            _logger.LogError(
                "No se configuró Email:FromEmail."
            );


            return ResultadoEmail.Fallo(
                "El remitente del servicio de email no está configurado."
            );
        }


        // ==========================================
        // REQUEST
        // ==========================================

        var from =
            string.IsNullOrWhiteSpace(
                _options.FromName)
                ? _options.FromEmail
                : $"{_options.FromName} <{_options.FromEmail}>";


        var body =
            new
            {
                from,

                to =
                    new[]
                    {
                        destinatario.Trim()
                    },

                subject =
                    asunto,

                html =
                    contenidoHtml
            };


        using var request =
            new HttpRequestMessage(
                HttpMethod.Post,
                "emails"
            );


        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                _options.ApiKey
            );


        request.Content =
            JsonContent.Create(
                body
            );


        // ==========================================
        // ENVÍO
        // ==========================================

        try
        {
            using var response =
                await _httpClient
                    .SendAsync(
                        request,
                        cancellationToken
                    );


            if (response.IsSuccessStatusCode)
            {
                return ResultadoEmail.Ok();
            }


            var respuestaProveedor =
                await response.Content
                    .ReadAsStringAsync(
                        cancellationToken
                    );


            _logger.LogError(
                "Error enviando email. Status: {StatusCode}. Respuesta del proveedor: {Respuesta}",
                (int)response.StatusCode,
                respuestaProveedor
            );


            return ResultadoEmail.Fallo(
                "No se pudo enviar el email."
            );
        }
        catch (TaskCanceledException)
            when (cancellationToken
                .IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Ocurrió un error al comunicarse con el proveedor de email."
            );


            return ResultadoEmail.Fallo(
                "No se pudo enviar el email."
            );
        }
    }
}