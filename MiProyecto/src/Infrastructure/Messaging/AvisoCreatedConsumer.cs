using MassTransit;
using MiProyecto.Application.Common.Interfaces;
using MiProyecto.Domain.Entities;
using Microsoft.Extensions.Logging;
using MiProyecto.Application.Common.Models;
using Microsoft.EntityFrameworkCore; 

namespace MiProyecto.Infrastructure.Messaging;

public class AvisoCreatedConsumer : IConsumer<AvisoCreatedIntegrationEvent>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<AvisoCreatedConsumer> _logger;

    public AvisoCreatedConsumer(IApplicationDbContext context, ILogger<AvisoCreatedConsumer> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<AvisoCreatedIntegrationEvent> context)
{
    var data = context.Message;

    // 1. VALIDACIÓN: ¿Existe ya en la base de datos?
    var existe = await _context.Avisos
        .AnyAsync(x => x.NumeroPoliza == data.NumeroPoliza 
                    && x.NumeroAviso == data.NumeroAviso, context.CancellationToken);

    if (existe)
    {
         _logger.LogWarning("⚠️ [CONSUMER] El Aviso {A} ya existe. Saltando procesamiento.", data.NumeroAviso);
        return; 
    }

    // 2. CREACIÓN ATÓMICA: Aviso + Detalles
    var nuevoAviso = new Aviso
    {
        NumeroPoliza = data.NumeroPoliza,
        NumeroAviso = data.NumeroAviso,
        NombreCliente = data.NombreCliente,
        RutCliente = data.RutCliente,
        FechaCancelacion = data.FechaCancelacion,
        // Insertamos los detalles aquí mismo
        Detalles = data.Cuotas.Select(c => new DetalleAviso 
        {
            NumeroCuota = c.NumeroCuota,
            TotalCuota = c.TotalCuota,
            Observacion = "Insertado por Worker"
        }).ToList()
    };

    try 
    {
        _context.Avisos.Add(nuevoAviso);       
        await _context.SaveChangesAsync(context.CancellationToken);
        
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "❌ Error al persistir en la base de datos.");
        throw; // Re-intento automático de RabbitMQ
    }
}




  
}
