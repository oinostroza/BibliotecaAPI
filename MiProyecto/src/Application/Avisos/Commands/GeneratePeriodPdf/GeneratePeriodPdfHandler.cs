using MassTransit;
using Microsoft.Extensions.Logging;
using MiProyecto.Application.Common.Interfaces;
using MiProyecto.Domain.Entities;
using MiProyecto.Domain.Events;

public record GeneratePeriodPdfCommand(int Mes, int Anio) : IRequest<int>;

public class GeneratePeriodPdfHandler : IRequestHandler<GeneratePeriodPdfCommand, int>
{
    private readonly IApplicationDbContext _context;
    //private readonly IPublishEndpoint _publish;
    private readonly IBus _bus;

    private readonly ILogger<GeneratePeriodPdfHandler> _logger; 

    // public GeneratePeriodPdfHandler(IApplicationDbContext context, IPublishEndpoint publish, ILogger<GeneratePeriodPdfHandler> logger)
    // {
    //     _context = context;
    //     _publish = publish;
    //     _logger = logger;
    // }
    public GeneratePeriodPdfHandler(IApplicationDbContext context, IBus bus, ILogger<GeneratePeriodPdfHandler> logger)
    {
        _context = context;
        _bus = bus;
        _logger = logger;
    }
   public async Task<int> Handle(GeneratePeriodPdfCommand request, CancellationToken ct)
    {
        _logger.LogInformation("🔍 [HANDLER] Iniciando validación para Periodo {Mes}/{Anio}", request.Mes, request.Anio);

        var procesoExistente = await _context.ProcesoPeriodo
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Mes == request.Mes && p.Anio == request.Anio, ct);

        if (procesoExistente != null && procesoExistente.Estado == "PROCESANDO")
        {
            _logger.LogWarning("⛔ [CANCELADO] El periodo {Mes}/{Anio} ya está en curso (ID: {Id})", request.Mes, request.Anio, procesoExistente.Id);
            throw new Exception("Este periodo ya se está procesando actualmente.");
        }

        _logger.LogInformation("Fetch: Buscando avisos en la base de datos...");
        var ids = await _context.Avisos
            .AsNoTracking()
            .Where(a => a.FechaCancelacion.Month == request.Mes && a.FechaCancelacion.Year == request.Anio)
            .Select(a => a.Id)
            .ToListAsync(ct);

        _logger.LogInformation("📊 [DATABASE] Se encontraron {Count} avisos para el periodo.", ids.Count);

        if (!ids.Any()) 
        {
             _logger.LogWarning("⚠️ [EMPTY] No hay registros para procesar en este mes/año.");
             return 0;
        }

        _logger.LogInformation("💾 Guardando registro de control ProcesoPeriodo...");
        var periodo = procesoExistente ?? new ProcesoPeriodo { Mes = request.Mes, Anio = request.Anio };
        periodo.TotalSolicitado = ids.Count;
        periodo.Estado = "PROCESANDO";
        periodo.ProcesadosOk = 0;
        periodo.ProcesadosError = 0;
        periodo.FechaInicio = DateTime.UtcNow;

        if (procesoExistente == null) _context.ProcesoPeriodo.Add(periodo);
        await _context.SaveChangesAsync(ct);
        _logger.LogInformation("✅ [DB-OK] Registro de control guardado con ID: {Id}", periodo.Id);

        _logger.LogInformation("🚀 [RABBITMQ] Intentando publicar {Count} mensajes vía PublishBatch...", ids.Count);
        
        try 
        {
            var mensajes = ids.Select(id => new GenerateSinglePdfRequest(id, periodo.Id)).ToList();          
            await _bus.PublishBatch(mensajes, ct);
            
            _logger.LogInformation("🏁 [SUCCESS] Mensajes entregados a RabbitMQ exitosamente.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ [RABBIT-ERROR] Falló el envío de mensajes al bus.");
            throw;
        }

        return ids.Count;
    }
}
