using MiProyecto.Domain.Entities;
using MiProyecto.Domain.Events;

namespace MiProyecto.Application.Common.Models;

// Usamos un 'record' porque son inmutables y perfectos para mensajes de RabbitMQ
public class AvisoCreatedIntegrationEvent
{
    // Datos del Aviso (Cabecera)
    public int AvisoId { get; set; }
    public int NumeroPoliza { get; set; } 
    public int NumeroAviso { get; set; } 
    public string? NombreCliente { get; set; } = string.Empty;
    public string? RutCliente { get; set; } = string.Empty;
    public string Direccion { get; set; } = string.Empty;
    public DateTime FechaCancelacion { get; init; }


    // Colección de Detalles (Las N cuotas)
    public List<CuotaDto> Cuotas { get; set; } = new();
}
