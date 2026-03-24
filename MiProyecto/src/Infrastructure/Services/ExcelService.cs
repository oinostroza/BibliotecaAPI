using ClosedXML.Excel;
using MiProyecto.Application.Avisos.Queries.GetAvisosExcel;
using MiProyecto.Application.Common.Interfaces;
using MiProyecto.Application.Common.Models;

namespace MiProyecto.Infrastructure.Services;

public class ExcelService : IExcelService
{
    public byte[] GenerarArchivoUnico(List<AvisoExcelDto> data, string nombreMes)
    {
        using var workbook = new XLWorkbook();
        var wsAvisos = workbook.Worksheets.Add("Cabecera Avisos");
        var wsDetalles = workbook.Worksheets.Add("Detalle Cuotas");

        // Cabeceras Hoja 1
        string[] h1 = { "Póliza", "Aviso", "Cliente", "Fecha Cancelación" };
        for (int i = 0; i < h1.Length; i++) wsAvisos.Cell(1, i + 1).Value = h1[i];
        wsAvisos.Row(1).Style.Font.Bold = true;

        // Cabeceras Hoja 2
        string[] h2 = { "Póliza", "Aviso", "N° Cuota", "Monto Cuota" };
        for (int i = 0; i < h2.Length; i++) wsDetalles.Cell(1, i + 1).Value = h2[i];
        wsDetalles.Row(1).Style.Font.Bold = true;

        int fA = 2, fD = 2;
        foreach (var aviso in data) {
            wsAvisos.Cell(fA++, 1).Value = aviso.Poliza;
            wsAvisos.Cell(fA - 1, 2).Value = aviso.Aviso;
            wsAvisos.Cell(fA - 1, 3).Value = aviso.Cliente;
            wsAvisos.Cell(fA - 1, 4).Value = aviso.Fecha.ToShortDateString();

            foreach (var c in aviso.Cuotas) {
                wsDetalles.Cell(fD++, 1).Value = c.Poliza;
                wsDetalles.Cell(fD - 1, 2).Value = c.Aviso;
                wsDetalles.Cell(fD - 1, 3).Value = c.NumeroCuota;
                wsDetalles.Cell(fD - 1, 4).Value = c.TotalCuota;
            }
        }
        wsAvisos.Columns().AdjustToContents();
        wsDetalles.Columns().AdjustToContents();
        Console.WriteLine($"📑 [EXCEL-SERVICE] Binario generado para {nombreMes} con {data.Count} registros.");
        using var ms = new MemoryStream();
        workbook.SaveAs(ms);
        return ms.ToArray();
    }
}
