using MassTransit;
using MiProyecto.Application.Common.Interfaces;
using MiProyecto.Application.Common.Models;
using MiProyecto.Domain.Events;

namespace MiProyecto.Application.Avisos.Commands.BulkCreate;

public record BulkCreateAvisosCommand : IRequest<string>;

public class BulkCreateAvisosCommandHandler : IRequestHandler<BulkCreateAvisosCommand, string>
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IApplicationDbContext _context;

    public BulkCreateAvisosCommandHandler(IPublishEndpoint publishEndpoint, IApplicationDbContext context)
    {
        _publishEndpoint = publishEndpoint;
        _context = context;
    }

    public async Task<string> Handle(BulkCreateAvisosCommand request, CancellationToken cancellationToken)
    {
        int totalAvisos = 3000;
        DateTime fechaInicio = new DateTime(2024, 1, 1); // Empezamos en Enero 2024
        Random rnd = new();

        for (int i = 1; i <= totalAvisos; i++)
        {
            // Distribuimos los avisos: los primeros 300 a Enero, luego 50 a Febrero, etc.
            // Para simplificar la lógica de tu ejemplo, usamos meses correlativos:
            int mesOffset = (i <= 300) ? 0 : (i <= 350) ? 1 : (i <= 360) ? 2 : (i % 12);
            var fechaAviso = fechaInicio.AddMonths(mesOffset);

            var eventMsg = new AvisoCreatedIntegrationEvent
            {
                NumeroPoliza = 2000 + i, // Poliza única para evitar tu validación de duplicados
                NumeroAviso = 1000 + i,
                NombreCliente = $"Cliente Dummy {i}",
                RutCliente = $"{rnd.Next(10, 25)}123456-K",
                FechaCancelacion = fechaAviso.AddDays(15).ToUniversalTime(), 
                Cuotas = new List<CuotaDto>
                {
                    new() { NumeroCuota = 1, TotalCuota = rnd.Next(10000, 50000), Observacion = "Cuota 1/2" },
                    new() { NumeroCuota = 2, TotalCuota = rnd.Next(10000, 50000), Observacion = "Cuota 2/2" }
                }
            }; 

            // Publicamos al Outbox
            await _publishEndpoint.Publish(eventMsg, cancellationToken);
            
            // Cada 500 registros guardamos el Outbox para no saturar la memoria
            if (i % 500 == 0) await _context.SaveChangesAsync(cancellationToken);
        }

        await _context.SaveChangesAsync(cancellationToken);
        return $"Encolados {totalAvisos} avisos con sus detalles.";
    }
}
