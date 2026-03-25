using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using MiProyecto.Application.Common.Interfaces;
using MiProyecto.Application.Common.Models;
using MiProyecto.Domain.Entities;
using MiProyecto.Domain.Enums;
using MiProyecto.Domain.Events;

namespace MiProyecto.Infrastructure.Messaging.Consumers.Reports;

public class GenerateSinglePdfConsumer : IConsumer<Batch<GenerateSinglePdfRequest>>
{
    private readonly IApplicationDbContext _context;
    private readonly IPdfService _pdfService;
    private readonly IDistributedCache _cache;
    private readonly ProcessLabelSettings _labels;

    public GenerateSinglePdfConsumer(IApplicationDbContext context, IPdfService pdfService, 
                                    IDistributedCache cache, IOptions<ProcessLabelSettings> labels)
    {
        _context = context;
        _pdfService = pdfService;
        _cache = cache;
        _labels = labels.Value;
    }
    public async Task Consume(ConsumeContext<Batch<GenerateSinglePdfRequest>> context)
    {
      
      var messages = context.Message.Select(m => m.Message).ToList();
        var listaFinal = new List<ProcesoAviso>();

        foreach (var msg in messages)
        {
            // 1. Check rápido en REDIS
            var status = await _cache.GetStringAsync($"pdf_idemp_{msg.AvisoId}");
            if (status == "OK") continue;

            // 2. Proceso pesado
            try {
                var aviso = await _context.Avisos.Include(a => a.Detalles).AsNoTracking().FirstOrDefaultAsync(x => x.Id == msg.AvisoId);
                if (aviso == null) continue;

                var bytes = await _pdfService.GenerarAvisoPdfAsync(MapToDto(aviso));
                var path = Path.Combine(Directory.GetCurrentDirectory(), "archivos", "pdfs", $"Aviso_{aviso.Id}.pdf");
                await File.WriteAllBytesAsync(path, bytes);

                // 3. Preparar para Postgres
                listaFinal.Add(new ProcesoAviso { 
                    AvisoId = aviso.Id, Estado = EstadoProceso.Completado, 
                    RutaArchivo = path, FechaFinalizacion = DateTime.UtcNow 
                });

                // 4. Marcar en REDIS
                await _cache.SetStringAsync($"pdf_idemp_{msg.AvisoId}", "OK", new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(7) });
            } catch (Exception) { /* Log error labels.Error */ }
        }

        // 5. GUARDAR CADA 50 EN POSTGRES
        if (listaFinal.Any()) {
            _context.ProcesosAvisos.AddRange(listaFinal);
            await _context.SaveChangesAsync(context.CancellationToken);
        }  
    

    }

   private AvisoPdfDto MapToDto(Aviso a){
        return new AvisoPdfDto
        {
            NumeroPoliza = a.NumeroPoliza.ToString(),
            NumeroAviso = a.NumeroAviso.ToString(),
            NombreCliente = a.NombreCliente ?? "Sin Nombre",
            RutCliente = a.RutCliente ?? "",
            FechaEmision = DateTime.UtcNow,
            Cuotas = a.Detalles.Select(d => new CuotaPdfDto
            {
                Numero = d.NumeroCuota,
                Vencimiento = a.FechaCancelacion,
                Monto = d.TotalCuota

            }).ToList()
        };
    }

   
}
