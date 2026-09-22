namespace NutriApi.Configuracion;

public class EmailOptions
{
    public const string Seccion =
        "Email";


    public string ApiKey { get; set; } =
        string.Empty;


    public string FromEmail { get; set; } =
        string.Empty;


    public string FromName { get; set; } =
        "NutriPower";


    public string FrontendUrl { get; set; } =
        string.Empty;
}