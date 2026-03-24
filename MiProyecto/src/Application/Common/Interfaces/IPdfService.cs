using MiProyecto.Application.Common.Models;

namespace MiProyecto.Application.Common.Interfaces;

public interface IPdfService
{
    Task<byte[]> GenerarAvisoPdfAsync(AvisoPdfDto datos);
}
