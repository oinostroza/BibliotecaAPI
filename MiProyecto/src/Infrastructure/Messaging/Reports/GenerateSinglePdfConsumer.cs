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

        _logger.LogInformation("📥 [CONSUMER] Recibido lote de {Count} mensajes.", context.Message.Length);

        var messages = context.Message.Select(m => m.Message).ToList();
        var periodoId = messages.First().PeriodoId; 
        var listaFinal = new List<ProcesoAviso>();

        var idsAProcesar = new List<int>();
        foreach (var m in messages) {
            if (await _cache.GetStringAsync($"pdf_ok_{m.AvisoId}") == null) 
                idsAProcesar.Add(m.AvisoId);
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
                listaFinal.Add(new ProcesoAviso { 
                    AvisoId = aviso.Id, 
                    Estado = EstadoProceso.Completado, 
                    RutaArchivo = path, 
                    FechaFinalizacion = DateTime.UtcNow,
                    TipoProceso = "PDF" 
                });

                await _cache.SetStringAsync($"pdf_ok_{aviso.Id}", "OK", 
                    new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(7) },
                    context.CancellationToken);
            } 
            catch (Exception ex) 
            {
                _logger.LogError(ex, "❌ Error en PDF Aviso {Id}", aviso.Id);
                
                listaFinal.Add(new ProcesoAviso { 
                    AvisoId = aviso.Id, 
                    Estado = EstadoProceso.Error, 
                    ErrorMensaje = ex.Message,
                    TipoProceso = "PDF",
                    FechaFinalizacion = DateTime.UtcNow
                });
                
                await _cache.RemoveAsync($"pdf_idemp_{aviso.Id}", context.CancellationToken);
            }
        }

        if (listaFinal.Any()) 
        {
            _context.ProcesosAvisos.AddRange(listaFinal);
            await _context.SaveChangesAsync(context.CancellationToken);
            
            var oks = listaFinal.Count(x => x.Estado == EstadoProceso.Completado);
            var errs = listaFinal.Count(x => x.Estado == EstadoProceso.Error);

            await _context.Database.ExecuteSqlInterpolatedAsync($@"
                UPDATE ""ProcesoPeriodo"" 
                SET ""ProcesadosOk"" = ""ProcesadosOk"" + {oks}, 
                    ""ProcesadosError"" = ""ProcesadosError"" + {errs}
                WHERE ""Id"" = {periodoId}");
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
