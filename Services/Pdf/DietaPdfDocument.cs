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

    private const string VerdeOscuro = "#164E3F";

    private const string Verde = "#2F7D64";

    private const string VerdeClaro = "#EAF5F0";

    private const string Fondo = "#F7FAF8";

    private const string Texto = "#18332C";

    private const string TextoSecundario = "#62756F";

    private const string Borde = "#D9E6E0";

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

                page.PageColor(Colors.White);

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
            .Padding(18)
            .Row(fila =>
            {
                fila.RelativeItem()
                    .Column(columna =>
                    {
                        columna
                            .Item()
                            .Text("NUTRIPOWER")
                            .FontSize(10)
                            .SemiBold()
                            .FontColor("#BFE3D5");

                        columna
                            .Item()
                            .PaddingTop(3)
                            .Text("Plan nutricional")
                            .FontSize(22)
                            .Bold()
                            .FontColor(Colors.White);

                        columna
                            .Item()
                            .PaddingTop(3)
                            .Text(_datos.NombrePaciente)
                            .FontSize(13)
                            .FontColor(Colors.White);
                    });

                fila.AutoItem()
                    .AlignBottom()
                    .Text($"Versión {_datos.Plan.Version}")
                    .FontSize(9)
                    .FontColor("#BFE3D5");
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

                columna.Item().Text(_datos.Plan.Nombre).FontSize(16).Bold().FontColor(VerdeOscuro);

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
                columna.Item().AlignCenter().Text(valor).Bold().FontSize(13);

                columna.Item().AlignCenter().Text(label).FontSize(8);
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
            .Column(columna =>
            {
                columna
                    .Item()
                    .Background(VerdeOscuro)
                    .Padding(10)
                    .Row(fila =>
                    {
                        fila.RelativeItem()
                            .Text(comida.Nombre)
                            .FontSize(15)
                            .Bold()
                            .FontColor(Colors.White);

                        fila.AutoItem().Text(comida.Tipo).FontSize(9).FontColor("#CDE6DC");
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
            .Background(opcion.EsPredeterminada ? VerdeClaro : Colors.White)
            .Padding(10)
            .Column(columna =>
            {
                columna.Spacing(6);

                columna
                    .Item()
                    .Row(fila =>
                    {
                        fila.RelativeItem().Text(opcion.Nombre).SemiBold();

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
                            .SemiBold();
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
            .Background("#EDF7FB")
            .Border(1)
            .BorderColor("#CAE5F0")
            .Padding(12)
            .Column(columna =>
            {
                columna.Item().Text("Hidratación").Bold().FontSize(13).FontColor("#23647B");

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
            .Background("#FFF8E8")
            .Border(1)
            .BorderColor("#ECDDB6")
            .Padding(12)
            .Column(columna =>
            {
                columna.Spacing(5);

                columna.Item().Text("Suplementación").Bold().FontSize(13).FontColor("#7A5A16");

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
                        columna.Item().PaddingLeft(10).Text(item.Indicaciones)
                            .FontSize(9).FontColor(TextoSecundario);
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
                columna.Item().Text("Indicaciones generales").Bold().FontColor(VerdeOscuro);

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
