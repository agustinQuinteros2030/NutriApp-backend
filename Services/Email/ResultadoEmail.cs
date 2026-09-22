namespace NutriApi.Services.Email;

public class ResultadoEmail
{
    public bool Exitoso { get; set; }

    public string? Error { get; set; }


    public static ResultadoEmail Ok()
    {
        return new ResultadoEmail
        {
            Exitoso = true
        };
    }


    public static ResultadoEmail Fallo(
        string error)
    {
        return new ResultadoEmail
        {
            Exitoso = false,
            Error = error
        };
    }
}