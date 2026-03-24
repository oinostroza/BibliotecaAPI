namespace MiProyecto.Domain.Entities;

public class DetalleAviso : BaseAuditableEntity
{

    public int NumeroCuota { get; set; }
    public decimal TotalCuota { get; set; }
    public string? Observacion { get; set; }

    // Id (PK Técnica) viene de la base
    public int AvisoId { get; set; } // FK técnica que apunta al Id de Aviso
    public Aviso Aviso { get; set; } = null!;
}