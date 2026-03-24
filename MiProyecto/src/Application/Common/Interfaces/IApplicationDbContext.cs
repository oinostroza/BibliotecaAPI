using MiProyecto.Domain.Entities;

namespace MiProyecto.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<TodoList> TodoLists { get; }

    DbSet<TodoItem> TodoItems { get; }

    DbSet<Aviso> Avisos { get; }
    DbSet<DetalleAviso> DetalleAvisos { get; }


    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
