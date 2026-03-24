using MiProyecto.Application.Common.Interfaces;
using MiProyecto.Application.Common.Models;

namespace MiProyecto.Application.Avisos.Queries.GetAvisosExcel;

public record GetAvisosExcelQuery(int Mes, int Anio) : IRequest<List<AvisoExcelDto>>;


public class GetAvisosExcelQueryHandler : IRequestHandler<GetAvisosExcelQuery, List<AvisoExcelDto>>
{
    private readonly IApplicationDbContext _context;
    public GetAvisosExcelQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<List<AvisoExcelDto>> Handle(GetAvisosExcelQuery request, CancellationToken cancellationToken)
    {
     return await _context.Avisos
            .Include(a => a.Detalles)
            .Where(a => a.FechaCancelacion.Month == request.Mes && a.FechaCancelacion.Year == request.Anio)
            .Select(a => new AvisoExcelDto(
                a.NumeroPoliza.ToString(),
                a.NumeroAviso.ToString(),
                a.NombreCliente ?? "",
                a.FechaCancelacion,
                a.Detalles.Select(d => new DetalleExcelDto(
                    a.NumeroPoliza.ToString(), 
                    a.NumeroAviso.ToString(), 
                    d.NumeroCuota, 
                    d.TotalCuota)).ToList()
            ))
            .ToListAsync(cancellationToken);
    }
}

