namespace NutriApi.Validaciones;

public static class AvatarCatalogo
{
    private static readonly HashSet<string> AvataresPermitidos = Enumerable
        .Range(1, 40)
        .Select(numero => $"avatar-{numero:D2}")
        .ToHashSet(StringComparer.Ordinal);

    public static bool EsValido(string? avatarId)
    {
        return !string.IsNullOrWhiteSpace(avatarId) && AvataresPermitidos.Contains(avatarId);
    }
}
