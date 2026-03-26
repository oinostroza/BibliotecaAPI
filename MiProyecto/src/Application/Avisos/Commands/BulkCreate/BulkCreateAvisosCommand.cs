using MassTransit;
using MiProyecto.Application.Common.Interfaces;
using MiProyecto.Application.Common.Models;
using MiProyecto.Domain.Entities;
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

    public async Task<string> Handle(BulkCreateAvisosCommand request, CancellationToken ct)
    {
        var avisos = new List<Aviso>();
        int totalRegistros = 50000;
        int registrosPorMes = totalRegistros / 12; // Aprox 4,166 por mes
        int añoActual = DateTime.Now.Year;


        for (int mes = 1; mes <= 12; mes++)
        {
            for (int i = 0; i < registrosPorMes; i++)
            {
                var aviso = new Aviso
                {
                    NumeroPoliza = 1000000 + (mes * 10000) + i,
                    NumeroAviso = 2000000 + (mes * 10000) + i,
                    NombreCliente = $"Cliente Mes {mes} Num {i}",
                    RutCliente = $"12345678-{i % 9}",
                    FechaCancelacion = DateTime.SpecifyKind(new DateTime(añoActual, mes, 15), DateTimeKind.Utc), 
                    Detalles = new List<DetalleAviso>
                    {
                        new DetalleAviso { NumeroCuota = 1, TotalCuota = 15000 + i },
                        new DetalleAviso { NumeroCuota = 2, TotalCuota = 15000 + i }
                    }
                };
                avisos.Add(aviso);
            }
        }

        // Usar EFCore.BulkExtensions si lo tienes, o AddRange para 50k es aceptable en 1 min
        _context.Avisos.AddRange(avisos);
        await _context.SaveChangesAsync(ct);

        return $"✅ Éxito: Se crearon {avisos.Count} avisos distribuidos en 12 meses.";
    }
}
