using System.Numerics;

namespace MiProyecto.Domain.Entities;

public class Aviso : BaseAuditableEntity
{
    public int NumeroPoliza { get; set; } // Negocio
    public int NumeroAviso { get; set; }  // Negocio
    public string? NombreCliente { get; set; }
    public string? RutCliente { get; set; }
    public string? Direccion { get; set; }
    public DateTime FechaCancelacion { get; set; }

    // Relación: Un aviso tiene muchos detalles
    public ICollection<DetalleAviso> Detalles { get; set; } = new List<DetalleAviso>();    
}