using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using MiProyecto.Application.Common.Interfaces;
using MiProyecto.Application.Common.Models;
using MiProyecto.Domain.Entities;
using MiProyecto.Domain.Enums;
using MiProyecto.Domain.Events;
using Microsoft.Extensions.Logging; // Para notificar a la Web

namespace MiProyecto.Infrastructure.Messaging.Consumers.Reports;

public class GenerateSinglePdfConsumer : IConsumer<Batch<GenerateSinglePdfRequest>>
{
    private readonly IApplicationDbContext _context;
    private readonly IPdfService _pdfService;
    private readonly IDistributedCache _cache;
    private readonly ProcessLabelSettings _labels;
    private readonly ILogger<GenerateSinglePdfConsumer> _logger;

    public GenerateSinglePdfConsumer(
        IApplicationDbContext context, 
        IPdfService pdfService, 
        IDistributedCache cache, 
        IOptions<ProcessLabelSettings> labels,
        ILogger<GenerateSinglePdfConsumer> logger)
    {
        _context = context;
        _pdfService = pdfService;
        _cache = cache;
        _labels = labels.Value;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<Batch<GenerateSinglePdfRequest>> context)
    {
        // Extraemos los mensajes del lote
        var messages = context.Message.Select(m => m.Message).ToList();
        var allIds = messages.Select(m => m.AvisoId).ToList();
        var listaResultados = new List<ProcesoAviso>();

        // 1. FILTRO DE REDIS (Idempotencia rápida)
        var idsAProcesar = new List<int>(); 
        foreach (var id in allIds)
        {
            var status = await _cache.GetStringAsync($"pdf_idemp_{id}", context.CancellationToken);
            if (status != "OK") idsAProcesar.Add(id);
        }

        if (!idsAProcesar.Any()) return;

        var avisosData = await _context.Avisos
            .Include(a => a.Detalles)
            .AsNoTracking()
            .Where(a => idsAProcesar.Contains(a.Id))
            .ToListAsync(context.CancellationToken);

        var folder = Path.Combine(Directory.GetCurrentDirectory(), "archivos", "pdfs");
        if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

        foreach (var aviso in avisosData)
        {
            try 
            {
                var bytes = await _pdfService.GenerarAvisoPdfAsync(MapToDto(aviso));
                
                var path = Path.Combine(folder, $"Aviso_{aviso.NumeroPoliza}_{aviso.Id}.pdf");
                await File.WriteAllBytesAsync(path, bytes, context.CancellationToken);

                // ÉXITO
                listaResultados.Add(new ProcesoAviso { 
                    AvisoId = aviso.Id, 
                    Estado = EstadoProceso.Completado, 
                    RutaArchivo = path, 
                    FechaFinalizacion = DateTime.UtcNow,
                    TipoProceso = "PDF" 
                });

                await _cache.SetStringAsync($"pdf_idemp_{aviso.Id}", "OK", 
                    new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(7) },
                    context.CancellationToken);
            } 
            catch (Exception ex) 
            {
                _logger.LogError(ex, "❌ Error en PDF Aviso {Id}", aviso.Id);
                
                listaResultados.Add(new ProcesoAviso { 
                    AvisoId = aviso.Id, 
                    Estado = EstadoProceso.Error, 
                    ErrorMensaje = ex.Message,
                    TipoProceso = "PDF",
                    FechaFinalizacion = DateTime.UtcNow
                });
                
                await _cache.RemoveAsync($"pdf_idemp_{aviso.Id}", context.CancellationToken);
            }
        }

        if (listaResultados.Any()) 
        {
            _context.ProcesosAvisos.AddRange(listaResultados);
            await _context.SaveChangesAsync(context.CancellationToken);
            
            // _logger.LogInformation("✅ [BATCH] Finalizado: {Ok} exitosos, {Err} fallidos. Etiqueta: {Msg}", 
                // listaResultados.Count(x => x.Estado == EstadoProceso.Completado),
                // listaResultados.Count(x => x.Estado == EstadoProceso.Error),
                // _labels.Completado);
        }
    }

    private AvisoPdfDto MapToDto(Aviso a) => new()
    {
        NumeroPoliza = a.NumeroPoliza.ToString(),
        NumeroAviso = a.NumeroAviso.ToString(),
        NombreCliente = a.NombreCliente ?? "Sin Nombre",
        RutCliente = a.RutCliente ?? "",
        FechaEmision = DateTime.UtcNow,
        Cuotas = a.Detalles.Select(d => new CuotaPdfDto {
            Numero = d.NumeroCuota,
            Vencimiento = a.FechaCancelacion,
            Monto = d.TotalCuota
        }).ToList()
    };
}
