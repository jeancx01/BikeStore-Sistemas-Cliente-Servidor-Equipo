using BikeStore.Domain.Entities;
using BikeStore.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BikeStore.Infrastructure.Data;

internal sealed class BicycleConfiguration
    : IEntityTypeConfiguration<Bicycle>
{
    public void Configure(
        EntityTypeBuilder<Bicycle> entity)
    {
        entity.ToTable("Bicicleta");

        entity.HasKey(x => x.Id)
            .HasName("PK_Bicicleta");

        entity.Property(x => x.Id)
            .HasColumnName("IdBicicleta");

        entity.Property(x => x.CategoryId)
            .HasColumnName("IdCategoria")
            .IsRequired();

        entity.Property(x => x.Brand)
            .HasColumnName("Marca")
            .HasMaxLength(100)
            .IsRequired();

        entity.Property(x => x.Model)
            .HasColumnName("Modelo")
            .HasMaxLength(100)
            .IsRequired();

        entity.Property(x => x.Price)
            .HasColumnName("Precio")
            .HasPrecision(10, 2)
            .IsRequired();

        entity.Property(x => x.Stock)
            .HasColumnName("Stock")
            .IsRequired();

        entity.Property(x => x.Status)
            .HasColumnName("Estado")
            .HasConversion(
                status =>
                    status == BicycleStatus.BajoStock
                        ? "Bajo stock"
                        : status == BicycleStatus.Agotado
                            ? "Agotado"
                            : status == BicycleStatus.Inactivo
                                ? "Inactivo"
                                : "Disponible",
                value =>
                    value == "Bajo stock"
                        ? BicycleStatus.BajoStock
                        : value == "Agotado"
                            ? BicycleStatus.Agotado
                            : value == "Inactivo"
                                ? BicycleStatus.Inactivo
                                : BicycleStatus.Disponible)
            .HasMaxLength(20)
            .IsRequired();

        entity.HasIndex(x => new
        {
            x.Brand,
            x.Model
        })
        .HasDatabaseName("IX_Bicicleta_Marca_Modelo");

        entity.HasIndex(x => x.Stock)
            .HasDatabaseName("IX_Bicicleta_Stock");
    }
}