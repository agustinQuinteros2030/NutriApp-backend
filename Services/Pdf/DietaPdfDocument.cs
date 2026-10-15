using System.Globalization;
using NutriApi.DTOs.Pdf;
using NutriApi.DTOs.PlanPaciente;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace NutriApi.Services.Pdf;

public class DietaPdfDocument
{
    private readonly DietaPdfDatos _datos;

    private const string VerdeOscuro = "#10130F";

    private const string Verde = "#C8FF00";

    private const string VerdeClaro = "#1A2310";

    private const string Fondo = "#151815";

    private const string Texto = "#F1F4EC";

    private const string TextoSecundario = "#ADB5A5";

    private const string Borde = "#303A29";

    // Paleta inspirada en la referencia: negro grafito + lima neón.
    private const string FondoPagina = "#090B09";

    private const string Tarjeta = "#121512";

    public DietaPdfDocument(DietaPdfDatos datos)
    {
        _datos = datos;
    }

    public byte[] Generar()
    {
        var documento = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);

                page.Margin(30);

                page.PageColor(FondoPagina);

                page.DefaultTextStyle(estilo => estilo.FontSize(10).FontColor(Texto));

                page.Header().Element(ConstruirEncabezado);

                page.Content()
                    .PaddingVertical(16)
                    .Column(columna =>
                    {
                        columna.Spacing(14);

                        columna.Item().Element(ConstruirResumen);

                        if (_datos.Plan.Hidratacion is not null)
                        {
                            columna.Item().Element(ConstruirHidratacion);
                        }

                        if (_datos.Plan.Suplementacion is not null)
                        {
                            columna.Item().Element(ConstruirSuplementacion);
                        }

                        foreach (var comida in _datos.Plan.Comidas)
                        {
                            columna
                                .Item()
                                .Element(contenedor => ConstruirComida(contenedor, comida));
                        }

                        if (!string.IsNullOrWhiteSpace(_datos.Plan.ObservacionesGenerales))
                        {
                            columna.Item().Element(ConstruirObservaciones);
                        }
                    });

                page.Footer()
                    .BorderTop(1)
                    .BorderColor(Borde)
                    .PaddingTop(8)
                    .Row(fila =>
                    {
                        fila.RelativeItem()
                            .Text($"Plan nutricional · {_datos.NombrePaciente}")
                            .FontSize(8)
                            .FontColor(TextoSecundario);

                        fila.AutoItem()
                            .Text(texto =>
                            {
                                texto.DefaultTextStyle(estilo =>
                                    estilo.FontSize(8).FontColor(TextoSecundario)
                                );

                                texto.Span("Página ");

                                texto.CurrentPageNumber();

                                texto.Span(" de ");

                                texto.TotalPages();
                            });
                    });
            });
        });

        return documento.GeneratePdf();
    }

    // ==========================================
    // ENCABEZADO
    // ==========================================

    private void ConstruirEncabezado(IContainer container)
    {
        container
            .Background(VerdeOscuro)
            .BorderBottom(3)
            .BorderColor(Verde)
            .Padding(18)
            .Row(fila =>
            {
                fila.RelativeItem()
                    .Column(columna =>
                    {
                        columna
                            .Item()
                            .Text("NUTRIPOWER · PLAN PERSONALIZADO")
                            .FontSize(9)
                            .SemiBold()
                            .FontColor(Verde);

                        columna
                            .Item()
                            .PaddingTop(3)
                            .Text("PLAN NUTRICIONAL")
                            .FontSize(14)
                            .SemiBold()
                            .FontColor(Texto);

                        columna
                            .Item()
                            .PaddingTop(3)
                            .Text(_datos.NombrePaciente.ToUpperInvariant())
                            .FontSize(23)
                            .Bold()
                            .FontColor(Verde);
                    });

                fila.AutoItem()
                    .AlignBottom()
                    .Text($"VERSIÓN {_datos.Plan.Version}")
                    .FontSize(9)
                    .SemiBold()
                    .FontColor(Verde);
            });
    }

    // ==========================================
    // RESUMEN
    // ==========================================

    private void ConstruirResumen(IContainer container)
    {
        container
            .Border(1)
            .BorderColor(Borde)
            .Background(Fondo)
            .Padding(14)
            .Column(columna =>
            {
                columna.Spacing(7);

                columna.Item().Text(_datos.Plan.Nombre).FontSize(16).Bold().FontColor(Verde);

                if (!string.IsNullOrWhiteSpace(_datos.Plan.Descripcion))
                {
                    columna.Item().Text(_datos.Plan.Descripcion).FontColor(TextoSecundario);
                }

                columna.Item().Text($"Nutricionista: {_datos.NombreNutricionista}").SemiBold();

                columna
                    .Item()
                    .Text(
                        $"Desde {FormatearFecha(_datos.Plan.FechaInicio)}"
                            + (
                                _datos.Plan.FechaFin.HasValue
                                    ? $" hasta {FormatearFecha(_datos.Plan.FechaFin.Value)}"
                                    : string.Empty
                            )
                    );

                columna.Item().PaddingTop(5).Element(ConstruirTotales);
            });
    }

    private void ConstruirTotales(IContainer container)
    {
        var totales = _datos.Plan.Totales;

        container
            .Background(VerdeClaro)
            .Padding(10)
            .Row(fila =>
            {
                CrearMetrica(fila, FormatearNumero(totales.Calorias), "kcal");

                CrearMetrica(fila, FormatearNumero(totales.Proteinas), "Proteínas");

                CrearMetrica(fila, FormatearNumero(totales.Carbohidratos), "Carbohidratos");

                CrearMetrica(fila, FormatearNumero(totales.Grasas), "Grasas");
            });
    }

    private static void CrearMetrica(RowDescriptor fila, string valor, string label)
    {
        fila.RelativeItem()
            .AlignCenter()
            .Column(columna =>
            {
                columna.Item().AlignCenter().Text(valor).Bold().FontSize(14).FontColor(Verde);

                columna.Item().AlignCenter().Text(label).FontSize(8).FontColor(TextoSecundario);
            });
    }

    // ==========================================
    // COMIDA
    // ==========================================

    private void ConstruirComida(IContainer container, MiPlanComidaDto comida)
    {
        container
            .Border(1)
            .BorderColor(Borde)
            .Background(Tarjeta)
            .Column(columna =>
            {
                columna
                    .Item()
                    .Background(VerdeOscuro)
                    .BorderLeft(3)
                    .BorderColor(Verde)
                    .Padding(10)
                    .Row(fila =>
                    {
                        fila.RelativeItem()
                            .Text(comida.Nombre)
                            .FontSize(15)
                            .Bold()
                            .FontColor(Colors.White);

                        fila.AutoItem().Text(comida.Tipo).FontSize(9).FontColor(Verde);
                    });

                columna
                    .Item()
                    .Padding(12)
                    .Column(contenido =>
                    {
                        contenido.Spacing(10);

                        if (!string.IsNullOrWhiteSpace(comida.Observaciones))
                        {
                            contenido.Item().Text(comida.Observaciones).FontColor(TextoSecundario);
                        }

                        foreach (var seccion in comida.Secciones)
                        {
                            contenido.Item().Element(c => ConstruirSeccion(c, seccion));
                        }
                    });
            });
    }

    private void ConstruirSeccion(IContainer container, MiPlanSeccionDto seccion)
    {
        container.Column(columna =>
        {
            columna.Spacing(7);

            columna.Item().Text(seccion.Nombre).SemiBold().FontSize(11).FontColor(Verde);

            if (!string.IsNullOrWhiteSpace(seccion.Observaciones))
            {
                columna.Item().Text(seccion.Observaciones).FontSize(9).FontColor(TextoSecundario);
            }

            foreach (var opcion in seccion.Opciones)
            {
                columna.Item().Element(c => ConstruirOpcion(c, opcion));
            }
        });
    }

    private void ConstruirOpcion(IContainer container, MiPlanOpcionDto opcion)
    {
        container
            .PreventPageBreak()
            .Border(1)
            .BorderColor(opcion.EsPredeterminada ? Verde : Borde)
            .Background(opcion.EsPredeterminada ? VerdeClaro : Fondo)
            .Padding(10)
            .Column(columna =>
            {
                columna.Spacing(6);

                columna
                    .Item()
                    .Row(fila =>
                    {
                        fila.RelativeItem().Text(opcion.Nombre).SemiBold().FontColor(Texto);

                        if (opcion.EsPredeterminada)
                        {
                            fila.AutoItem()
                                .Text("MENÚ PRINCIPAL")
                                .FontSize(8)
                                .SemiBold()
                                .FontColor(Verde);
                        }
                    });

                if (!string.IsNullOrWhiteSpace(opcion.Observaciones))
                {
                    columna
                        .Item()
                        .Text(opcion.Observaciones)
                        .FontSize(9)
                        .FontColor(TextoSecundario);
                }

                foreach (var item in opcion.Items)
                {
                    columna.Item().Element(c => ConstruirItem(c, item));
                }
            });
    }

    private void ConstruirItem(IContainer container, MiPlanItemDto item)
    {
        container
            .PaddingVertical(3)
            .Column(columna =>
            {
                columna
                    .Item()
                    .Row(fila =>
                    {
                        fila.RelativeItem().Text(item.Alimento).SemiBold();

                        fila.AutoItem()
                            .Text(
                                $"{FormatearNumero(item.Cantidad)} {FormatearUnidad(item.UnidadMedida)}"
                            )
                            .SemiBold()
                            .FontColor(Verde);
                    });

                if (!string.IsNullOrWhiteSpace(item.Indicaciones))
                {
                    columna.Item().Text(item.Indicaciones).FontSize(8).FontColor(TextoSecundario);
                }

                if (item.Alternativas.Count > 0)
                {
                    columna
                        .Item()
                        .PaddingTop(3)
                        .Text("Equivalencias")
                        .FontSize(8)
                        .SemiBold()
                        .FontColor(Verde);

                    foreach (var alternativa in item.Alternativas)
                    {
                        columna
                            .Item()
                            .PaddingLeft(10)
                            .Text(
                                $"• {alternativa.Alimento}: "
                                    + $"{FormatearNumero(alternativa.CantidadCalculada)} "
                                    + $"{FormatearUnidad(alternativa.UnidadMedida)}"
                            )
                            .FontSize(8)
                            .FontColor(TextoSecundario);
                    }
                }
            });
    }

    // ==========================================
    // HIDRATACIÓN
    // ==========================================

    private void ConstruirHidratacion(IContainer container)
    {
        var hidratacion = _datos.Plan.Hidratacion!;

        container
            .Background(VerdeClaro)
            .Border(1)
            .BorderColor(Borde)
            .Padding(12)
            .Column(columna =>
            {
                columna.Item().Text("HIDRATACIÓN").Bold().FontSize(13).FontColor(Verde);

                var pauta = new List<string>();
                if (hidratacion.MililitrosDiarios.HasValue)
                    pauta.Add($"{hidratacion.MililitrosDiarios.Value} ml diarios");
                if (hidratacion.VasosDiarios.HasValue)
                    pauta.Add($"{hidratacion.VasosDiarios.Value} vasos");
                if (pauta.Count > 0)
                    columna.Item().PaddingTop(4).Text(string.Join(" · ", pauta));

                if (!string.IsNullOrWhiteSpace(hidratacion.Observaciones))
                {
                    columna
                        .Item()
                        .PaddingTop(3)
                        .Text(hidratacion.Observaciones)
                        .FontColor(TextoSecundario);
                }
            });
    }

    // ==========================================
    // SUPLEMENTACIÓN
    // ==========================================

    private void ConstruirSuplementacion(IContainer container)
    {
        var suplementacion = _datos.Plan.Suplementacion!;

        container
            .Background(VerdeClaro)
            .Border(1)
            .BorderColor(Borde)
            .Padding(12)
            .Column(columna =>
            {
                columna.Spacing(5);

                columna.Item().Text("SUPLEMENTACIÓN").Bold().FontSize(13).FontColor(Verde);

                if (!string.IsNullOrWhiteSpace(suplementacion.ObservacionesGenerales))
                {
                    columna
                        .Item()
                        .Text(suplementacion.ObservacionesGenerales)
                        .FontColor(TextoSecundario);
                }

                foreach (var item in suplementacion.Items)
                {
                    var cantidad = item.Cantidad.HasValue
                        ? $"{FormatearNumero(item.Cantidad.Value)} {item.Unidad}"
                        : "Cantidad no indicada";

                    columna
                        .Item()
                        .Text(
                            $"• {item.Nombre} — {cantidad}"
                                + (
                                    !string.IsNullOrWhiteSpace(item.Momento)
                                        ? $" · {item.Momento}"
                                        : string.Empty
                                )
                        );

                    if (!string.IsNullOrWhiteSpace(item.Indicaciones))
                    {
                        columna
                            .Item()
                            .PaddingLeft(10)
                            .Text(item.Indicaciones)
                            .FontSize(9)
                            .FontColor(TextoSecundario);
                    }
                }
            });
    }

    // ==========================================
    // OBSERVACIONES GENERALES
    // ==========================================

    private void ConstruirObservaciones(IContainer container)
    {
        container
            .BorderTop(1)
            .BorderColor(Borde)
            .PaddingTop(10)
            .Column(columna =>
            {
                columna.Item().Text("INDICACIONES GENERALES").Bold().FontColor(Verde);

                columna.Item().PaddingTop(4).Text(_datos.Plan.ObservacionesGenerales!);
            });
    }

    // ==========================================
    // FORMATOS
    // ==========================================

    private static string FormatearNumero(decimal? numero)
    {
        if (!numero.HasValue)
        {
            return "—";
        }

        return numero.Value.ToString("0.##", CultureInfo.GetCultureInfo("es-AR"));
    }

    private static string FormatearFecha(DateOnly fecha)
    {
        return fecha.ToString("dd/MM/yyyy");
    }

    private static string FormatearUnidad(string unidad)
    {
        return unidad switch
        {
            "Gramos" => "g",
            "Mililitros" => "ml",
            "Unidad" => "u.",
            "Unidades" => "u.",
            _ => unidad,
        };
    }
}
