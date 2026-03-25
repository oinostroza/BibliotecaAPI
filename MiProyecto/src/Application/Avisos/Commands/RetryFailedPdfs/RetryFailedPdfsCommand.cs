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
        
        var idsFallidos = await _context.ProcesosAvisos
            .Where(p => p.Estado == EstadoProceso.Error && p.TipoProceso == "PDF")
            .Select(p => p.AvisoId)
            .ToListAsync(cancellationToken);

        if (!idsFallidos.Any()) return 0;

        foreach (var id in idsFallidos)
        {
            await _publishEndpoint.Publish(new GenerateSinglePdfRequest(id), cancellationToken);
        }

        return idsFallidos.Count;
    }
}
