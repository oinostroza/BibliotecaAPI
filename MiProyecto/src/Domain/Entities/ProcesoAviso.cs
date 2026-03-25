using MiProyecto.Domain.Common;
using MiProyecto.Domain.Enums;

namespace MiProyecto.Domain.Entities;

public class ProcesoAviso : BaseAuditableEntity
{
    public int AvisoId { get; set; }
    public string TipoProceso { get; set; } = "PDF"; 
    public EstadoProceso Estado { get; set; }
    public string? RutaArchivo { get; set; }
    public string? ErrorMensaje { get; set; }
    public DateTime? FechaFinalizacion { get; set; }
    public Aviso Aviso { get; set; } = null!;
}
