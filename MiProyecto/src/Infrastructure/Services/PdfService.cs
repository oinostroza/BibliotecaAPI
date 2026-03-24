using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using MiProyecto.Application.Common.Interfaces;
using MiProyecto.Application.Common.Models;

namespace MiProyecto.Infrastructure.Services;

public class PdfService : IPdfService
{
    public Task<byte[]> GenerarAvisoPdfAsync(AvisoPdfDto datos)
    {
        // Licencia comunitaria (Gratis)
        QuestPDF.Settings.License = LicenseType.Community;

        var documento = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(1, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.Arial));

                // HEADER: Franja azul y Título
                page.Header().Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("MI EMPRESA S.A.").FontSize(16).Bold().FontColor(Colors.Blue.Medium);
                        col.Item().Text("Servicios Financieros").FontSize(9).Italic();
                    });

                    row.RelativeItem().AlignRight().Column(col =>
                    {
                        col.Item().Text("AVISO DE COBRANZA").FontSize(18).Bold().FontColor(Colors.Red.Medium);
                        col.Item().Text($"Póliza: {datos.NumeroPoliza}").Bold();
                    });
                });

                // CONTENIDO
                page.Content().PaddingVertical(20).Column(col =>
                {
                    // Cuadro de datos del cliente (Gris claro)
                    col.Item().Background(Colors.Grey.Lighten4).Padding(10).Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("DATOS DEL CLIENTE").FontSize(8).Bold().FontColor(Colors.Grey.Medium);
                            c.Item().Text(datos.NombreCliente).Bold();
                            c.Item().Text($"RUT: {datos.RutCliente}");
                        });
                        row.RelativeItem().AlignRight().Column(c =>
                        {
                            c.Item().Text("FECHA EMISIÓN").FontSize(8).Bold().FontColor(Colors.Grey.Medium);
                            c.Item().Text(DateTime.Now.ToString("dd/MM/yyyy HH:mm"));
                        });
                    });

                    // TABLA DE CUOTAS
                    col.Item().PaddingTop(15).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(3); // Descripción
                            columns.RelativeColumn(2); // Vencimiento
                            columns.RelativeColumn(2); // Monto
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(CellStyle).Text("Descripción");
                            header.Cell().Element(CellStyle).Text("Vencimiento");
                            header.Cell().Element(CellStyle).AlignRight().Text("Monto");

                            static IContainer CellStyle(IContainer container) => 
                                container.DefaultTextStyle(x => x.Bold().FontColor(Colors.White))
                                         .Background(Colors.Blue.Medium).Padding(5);
                        });

                        foreach (var cuota in datos.Cuotas)
                        {
                            table.Cell().Element(RowStyle).Text($"Cuota N° {cuota.Numero}");
                            table.Cell().Element(RowStyle).Text(cuota.Vencimiento.ToShortDateString());
                            table.Cell().Element(RowStyle).AlignRight().Text(cuota.Monto.ToString("C0"));

                            static IContainer RowStyle(IContainer container) => 
                                container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5);
                        }
                    });

                    // TOTAL
                    col.Item().AlignRight().PaddingTop(10).Text(text =>
                    {
                        text.Span("TOTAL A PAGAR: ").FontSize(12);
                        text.Span(datos.TotalPagar.ToString("C0")).FontSize(14).Bold().FontColor(Colors.Blue.Medium);
                    });

                    // NOTA INFORMATIVA
                    col.Item().PaddingTop(30).BorderLeft(3).BorderColor(Colors.Blue.Medium).PaddingLeft(10).Text(t =>
                    {
                        t.Span("Nota: ").Bold();
                        t.Span("Este documento es un aviso informativo. Realice su pago antes de la fecha de vencimiento para mantener su cobertura vigente.");
                    });
                });

                // FOOTER
                page.Footer().Column(f =>
                {
                    f.Item().AlignCenter().Text(x => {
                        x.Span("Página ");
                        x.CurrentPageNumber();
                    });
                    f.Item().Height(10).Background(Colors.Blue.Medium);
                });
            });
        });

        return Task.FromResult(documento.GeneratePdf());
    }
}
