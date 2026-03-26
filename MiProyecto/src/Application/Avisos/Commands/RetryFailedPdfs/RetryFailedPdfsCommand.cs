using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MiProyecto.Application.Common.Interfaces;
using MiProyecto.Domain.Enums;
using MiProyecto.Domain.Events;

namespace MiProyecto.Application.Avisos.Commands.RetryFailedPdfs;

public record RetryFailedPdfsCommand : IRequest<int>;

public class RetryFailedPdfsCommandHandler : IRequestHandler<RetryFailedPdfsCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly IPublishEndpoint _publishEndpoint;

    public RetryFailedPdfsCommandHandler(IApplicationDbContext context, IPublishEndpoint publishEndpoint)
    {
        _context = context;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<int> Handle(RetryFailedPdfsCommand request, CancellationToken cancellationToken)
    {
        
        var fallidos = await _context.ProcesosAvisos
            .AsNoTracking()
            .Where(p => p.Estado == EstadoProceso.Error && p.TipoProceso == "PDF")
            .Select(p => new { p.AvisoId, p.Id })
            .ToListAsync(cancellationToken);

        if (!fallidos.Any()) return 0;


        var mensajes = fallidos.Select(f => new GenerateSinglePdfRequest(f.AvisoId, 0)).ToList(); 
        
        await _publishEndpoint.PublishBatch(mensajes, cancellationToken);


        return fallidos.Count;
    }
}
