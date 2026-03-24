using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MiProyecto.Application.Common.Interfaces;
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
        //  _logger.LogInformation("🔍 [SPLITTER] Buscando periodos con datos en la base de datos...");
        // var periodos = await _context.Avisos
        //     .Select(a => new { a.FechaCancelacion.Month, a.FechaCancelacion.Year })
        //     .Distinct()
        //     .ToListAsync(context.CancellationToken);

        // foreach (var p in periodos){
        //     await _publishEndpoint.Publish(new GenerateMonthlyReportRequest(p.Month, p.Year));
        // }
        // _logger.LogInformation("✅ [SPLITTER] {Cant} mensajes de Excel encolados.", periodos.Count);

        var avisoIds = await _context.Avisos
            .OrderBy(a => a.Id)
            .Select(a => a.Id)
            .ToListAsync(context.CancellationToken); 

        _logger.LogInformation("Wait... Encolando {Cant} solicitudes de PDF individual.", avisoIds.Count);
        foreach (var id in avisoIds)
        {
            // Publicamos un mensaje por cada ID
            await _publishEndpoint.Publish(new GenerateSinglePdfRequest(id));
        }

        _logger.LogInformation("🏁 [SPLITTER] Proceso de encolado finalizado exitosamente.");


    }
}
