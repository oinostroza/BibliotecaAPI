using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MiProyecto.Application.Common.Interfaces;
using MiProyecto.Application.Common.Models;
using MiProyecto.Domain.Events;

namespace MiProyecto.Infrastructure.Messaging.Consumers.Reports;

public class GenerateSinglePdfConsumer : IConsumer<GenerateSinglePdfRequest>
{
    private readonly IApplicationDbContext _context;
    private readonly IPdfService _pdfService;
    private readonly ILogger<GenerateSinglePdfConsumer> _logger;
    private static int _pdfCount = 0;


    public GenerateSinglePdfConsumer(IApplicationDbContext context, IPdfService pdfService, ILogger<GenerateSinglePdfConsumer> logger)
    {
        _context = context;
        _pdfService = pdfService;
        _logger = logger;
    }
    public async Task Consume(ConsumeContext<GenerateSinglePdfRequest> context)
    {
        // 1. Consulta optimizada (Solo lectura)
        var aviso = await _context.Avisos
            .Include(a => a.Detalles)
            .AsNoTracking() 
            .FirstOrDefaultAsync(a => a.Id == context.Message.AvisoId);

        if (aviso == null) return;

        var dto = new AvisoPdfDto {
            NumeroPoliza = aviso.NumeroPoliza.ToString(),
            NumeroAviso = aviso.NumeroAviso.ToString(),
            NombreCliente = aviso.NombreCliente ?? "Sin Nombre",
            RutCliente = aviso.RutCliente ?? "",
            FechaEmision = DateTime.UtcNow, // Usa UtcNow por estándar
            Cuotas = aviso.Detalles.Select(d => new CuotaPdfDto {
                Numero = d.NumeroCuota,
                Vencimiento = aviso.FechaCancelacion, // Usa la fecha real del aviso
                Monto = d.TotalCuota
            }).ToList()
        };

        var pdfBytes = await _pdfService.GenerarAvisoPdfAsync(dto);
        var folder = Path.Combine(Directory.GetCurrentDirectory(), "archivos", "pdfs");
        
        if (!Directory.Exists(folder)) {
            Directory.CreateDirectory(folder);
        }

        var path = Path.Combine(folder, $"Aviso_{aviso.NumeroPoliza}_{aviso.Id}.pdf");
        await File.WriteAllBytesAsync(path, pdfBytes, context.CancellationToken);
        var current = Interlocked.Increment(ref _pdfCount);
        if (current % 100 == 0)
        {
            _logger.LogInformation("🚀 [PROGRESS-PDF] Completados {Cant} ", current);
        }
        
    

    }

   
   
}
