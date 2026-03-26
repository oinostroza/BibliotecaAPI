using Microsoft.EntityFrameworkCore.Infrastructure;
using MiProyecto.Domain.Entities;

namespace MiProyecto.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<TodoList> TodoLists { get; }
    DbSet<TodoItem> TodoItems { get; }
    DbSet<Aviso> Avisos { get; }
    DbSet<DetalleAviso> DetalleAvisos { get; }
    DbSet<ProcesoAviso> ProcesosAvisos { get; }
    DbSet<ProcesoPeriodo> ProcesoPeriodo { get; }
    DatabaseFacade Database { get; } 
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
