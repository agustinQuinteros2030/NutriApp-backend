namespace NutriApi.Services.Auth;

public class TokenResultado
{
    public string Token { get; set; } = string.Empty;

    public DateTime ExpiraEn { get; set; }
}