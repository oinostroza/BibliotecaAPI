using MiProyecto.Application.Common.Interfaces;

namespace MiProyecto.Application.Avisos.Queries.GetPeriodoStatus;

public record GetPeriodoStatusQuery(int Mes, int Anio) : IRequest<PeriodoStatusVm?>;

public class GetPeriodoStatusQueryHandler : IRequestHandler<GetPeriodoStatusQuery, PeriodoStatusVm?>
{
    private readonly IApplicationDbContext _context;

    public GetPeriodoStatusQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PeriodoStatusVm?> Handle(GetPeriodoStatusQuery request, CancellationToken ct)
    {
        var periodo = await _context.ProcesoPeriodo
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Mes == request.Mes && p.Anio == request.Anio, ct);

        if (periodo == null) return null;

        // Cálculos de métricas en tiempo real
        var fin = periodo.FechaFin ?? DateTime.UtcNow;
        var duracion = fin - periodo.FechaInicio;
        var totalProcesados = periodo.ProcesadosOk + periodo.ProcesadosError;
        
        double porcentaje = periodo.TotalSolicitado > 0 
            ? (double)totalProcesados / periodo.TotalSolicitado 
            : 0;

        double velocidad = duracion.TotalSeconds > 0 
            ? totalProcesados / duracion.TotalSeconds 
            : 0;

        return new PeriodoStatusVm(
            periodo.Mes,
            periodo.Anio,
            periodo.Estado,
            periodo.TotalSolicitado,
            periodo.ProcesadosOk,
            periodo.ProcesadosError,
            Math.Round(porcentaje * 100, 2),
            Math.Round(velocidad, 2),
            duracion
        );
    }
}
