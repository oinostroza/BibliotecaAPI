using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using MiProyecto.Application.Avisos.Queries.GetAvisosExcel;
using MiProyecto.Application.Common.Interfaces;
using MiProyecto.Domain.Events;

namespace MiProyecto.Infrastructure.Messaging.Consumers.Reports;

public class GenerateMonthlyReportConsumer : IConsumer<GenerateMonthlyReportRequest>
{
    private readonly ISender _sender;
    private readonly IExcelService _excelService;
    private readonly ILogger<GenerateMonthlyReportConsumer> _logger;

    

    public GenerateMonthlyReportConsumer(ISender sender, IExcelService excelService,ILogger<GenerateMonthlyReportConsumer> logger)
    {
        _sender = sender;
        _excelService = excelService;
        _logger =logger;
    }

    public async Task Consume(ConsumeContext<GenerateMonthlyReportRequest> context)
    {
        var msg = context.Message;
        _logger.LogInformation("📥 [WORKER] Iniciando reporte para el periodo: {Mes}/{Anio}", msg.Mes, msg.Anio);   

        var datos = await _sender.Send(new GetAvisosExcelQuery(msg.Mes, msg.Anio));

        if (!datos.Any())
        {
             _logger.LogWarning("⚠️ [WORKER] No se encontraron datos para {Mes}/{Anio}. Cancelando archivo.", msg.Mes, msg.Anio);
             return;
        }
        
        _logger.LogInformation("📊 [WORKER] Procesando {Cant} avisos para el Excel de {Mes}/{Anio}...", datos.Count, msg.Mes, msg.Anio);
        var bytes = _excelService.GenerarArchivoUnico(datos, $"{msg.Mes}_{msg.Anio}");

        // Carpeta donde se guardarán (asegúrate que exista)
        var folder = Path.Combine(Directory.GetCurrentDirectory(), "archivos");
        if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

        var fileName = $"Reporte_{msg.Anio}_{msg.Mes}.xlsx";
        var path = Path.Combine(folder, fileName);
        
        await File.WriteAllBytesAsync(path, bytes);
        _logger.LogInformation("✅ [WORKER] Archivo guardado exitosamente: {Path}", fileName);
    }
}
