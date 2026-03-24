using MassTransit; 
using MediatR;
using Microsoft.Extensions.Logging;
using MiProyecto.Application.Common.Interfaces;
using MiProyecto.Application.Common.Models;
using MiProyecto.Domain.Entities;
using MiProyecto.Domain.Events;

namespace MiProyecto.Application.Avisos.Commands.CreateAviso;

public record CreateAvisoCommand : IRequest<int>
{
    public int NumeroPoliza { get; init; }
    public int NumeroAviso { get; init; }
    public string? NombreCliente { get; init; }
    public string? RutCliente { get; init; }
    public DateTime FechaCancelacion { get; init; }
    public List<CuotaDto> Cuotas { get; init; } = new();
}


public class CreateAvisoCommandHandler : IRequestHandler<CreateAvisoCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly IPublishEndpoint _publishEndpoint; 
    private readonly ILogger<CreateAvisoCommandHandler> _logger;


    public CreateAvisoCommandHandler(IApplicationDbContext context, 
                                    IPublishEndpoint publishEndpoint,
                                    ILogger<CreateAvisoCommandHandler> logger) {
        _context = context;
        _publishEndpoint = publishEndpoint;
        _logger =logger;
    }

    public async Task<int> Handle(CreateAvisoCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("📨 [API] Publicando solicitud de creación para Póliza: {P}", request.NumeroPoliza);  
        await _publishEndpoint.Publish(new AvisoCreatedIntegrationEvent
        {
            NumeroPoliza = request.NumeroPoliza,
            NumeroAviso = request.NumeroAviso,
            NombreCliente = request.NombreCliente,
            RutCliente = request.RutCliente,
            FechaCancelacion = request.FechaCancelacion,
            Cuotas = request.Cuotas // Importante: el DTO debe llevar la lista
        }, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        return 0;
    }
}
