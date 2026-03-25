using MiProyecto.Application.Common.Interfaces;
using MiProyecto.Domain.Enums;

namespace MiProyecto.Application.Avisos.Queries.GetProcesosStatus;

public record ProcesosStatusVm(
    Dictionary<string, int> Resumen,
    List<ErrorDetalleDto> UltimosErrores
);

public record ErrorDetalleDto(int AvisoId, string? Mensaje);

public record GetProcesosStatusQuery : IRequest<ProcesosStatusVm>;

public class GetProcesosStatusQueryHandler : IRequestHandler<GetProcesosStatusQuery, ProcesosStatusVm>
{
    private readonly IApplicationDbContext _context;

    public GetProcesosStatusQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ProcesosStatusVm> Handle(GetProcesosStatusQuery request, CancellationToken cancellationToken)
    {
        var stats = await _context.ProcesosAvisos
            .AsNoTracking()
            .GroupBy(p => p.Estado)
            .Select(g => new { Estado = g.Key.ToString(), Cantidad = g.Count() })
            .ToListAsync(cancellationToken);

        var errores = await _context.ProcesosAvisos
            .AsNoTracking()
            .Where(p => p.Estado == EstadoProceso.Error)
            .OrderByDescending(p => p.Id) // 
            .Take(10)
            .Select(p => new ErrorDetalleDto(p.AvisoId, p.ErrorMensaje))
            .ToListAsync(cancellationToken);

        return new ProcesosStatusVm(
            stats.ToDictionary(x => x.Estado, x => x.Cantidad),
            errores
        );
    }
}
