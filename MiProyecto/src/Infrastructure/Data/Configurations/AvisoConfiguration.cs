using MiProyecto.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MiProyecto.Infrastructure.Data.Configurations;

public class AvisoConfiguration : IEntityTypeConfiguration<Aviso>
{
 
    public void Configure(EntityTypeBuilder<Aviso> builder)
    {
            
        builder.HasIndex(a => new { a.NumeroPoliza, a.NumeroAviso })
            .IsUnique();

        // Configuración de la relación 1 a N
        builder.HasMany(a => a.Detalles)
            .WithOne(d => d.Aviso)
            .HasForeignKey(d => d.AvisoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
