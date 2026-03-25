using System.Reflection;
using MiProyecto.Application.Common.Interfaces;
using MiProyecto.Domain.Entities;
using MiProyecto.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MassTransit;

namespace MiProyecto.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
    public DbSet<TodoList> TodoLists => Set<TodoList>();
    public DbSet<TodoItem> TodoItems => Set<TodoItem>();
    public DbSet<Aviso> Avisos => Set<Aviso>();
    public DbSet<DetalleAviso> DetalleAvisos => Set<DetalleAviso>();
    public DbSet<ProcesoAviso> ProcesosAvisos => Set<ProcesoAviso>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<ProcesoAviso>()
                .HasIndex(p => new { p.AvisoId, p.TipoProceso })
                .IsUnique();
        builder.AddTransactionalOutboxEntities();
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
