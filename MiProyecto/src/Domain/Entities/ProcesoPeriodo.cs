
namespace MiProyecto.Domain.Entities;

public class ProcesoPeriodo : BaseAuditableEntity
{
    public int Mes { get; set; }
    public int Anio { get; set; }
    public string Estado { get; set; } = "PROCESANDO"; 
    public int TotalSolicitado { get; set; }
    public int ProcesadosOk { get; set; }
    public int ProcesadosError { get; set; }
    public DateTime FechaInicio { get; set; } = DateTime.UtcNow;
    public DateTime? FechaFin { get; set; }
}
