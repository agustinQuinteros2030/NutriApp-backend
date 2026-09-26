namespace NutriApi.DTOs.Pdf;

public class ArchivoPdfDieta
{
    public byte[] Contenido { get; set; } = Array.Empty<byte>();

    public string NombreArchivo { get; set; } = string.Empty;
}
