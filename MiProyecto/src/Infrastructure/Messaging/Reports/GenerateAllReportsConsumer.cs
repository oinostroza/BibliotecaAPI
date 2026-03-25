using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MiProyecto.Application.Common.Interfaces;
using MiProyecto.Domain.Enums;
using MiProyecto.Domain.Events;

namespace MiProyecto.Infrastructure.Messaging.Consumers.Reports;

public class GenerateAllReportsConsumer : IConsumer<GenerateAllReportsRequest>
{
    private readonly IApplicationDbContext _context;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<GenerateAllReportsConsumer> _logger;

    public GenerateAllReportsConsumer(IApplicationDbContext context, IPublishEndpoint publishEndpoint,
         ILogger<GenerateAllReportsConsumer> logger)
    {
        _context = context;
        _publishEndpoint = publishEndpoint;
        _logger =logger;
        
    }

    public async Task Consume(ConsumeContext<GenerateAllReportsRequest> context)
    {
        _logger.LogInformation("🔍 [SPLITTER] Iniciando reparto masivo...");

        // var periodos = await _context.Avisos
        //     .Select(a => new { a.FechaCancelacion.Month, a.FechaCancelacion.Year })
        //     .Distinct()
        //     .ToListAsync(context.CancellationToken);

        // foreach (var p in periodos) {
        //     await _publishEndpoint.Publish(new GenerateMonthlyReportRequest(p.Month, p.Year));
        // }

        var todosLosIds = await _context.Avisos
            .AsNoTracking()
            .OrderBy(a => a.Id)
            .Select(a => a.Id)
            .ToListAsync(context.CancellationToken); 

        var idsYaProcesados = await _context.ProcesosAvisos
            .AsNoTracking()
            .Where(p => p.Estado == EstadoProceso.Completado)
            .Select(p => p.AvisoId)
            .ToListAsync(context.CancellationToken);

        var idsPendientes = todosLosIds.Except(idsYaProcesados).ToList();

        if (!idsPendientes.Any())
        {
            _logger.LogInformation("✅ [SPLITTER] Todos los PDFs ya están generados. Nada que encolar.");
            return;
        }

        _logger.LogInformation("🚀 [SPLITTER] Encolando solo faltantes: {Cant}", idsPendientes.Count);

        var tareasPdf = idsPendientes.Select(id => new GenerateSinglePdfRequest(id)).ToList();
        await _publishEndpoint.PublishBatch(tareasPdf, context.CancellationToken);
        
        _logger.LogInformation("🏁 [SPLITTER] Mensajes enviados.");
    }

}
