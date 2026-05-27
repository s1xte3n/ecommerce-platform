using ECommerce.Domain.Entities;
using ECommerce.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Persistence.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("products");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Name).IsRequired().HasMaxLength(200);
        builder.Property(p => p.Description).IsRequired().HasColumnType("text");
        builder.Property(p => p.Slug).IsRequired().HasMaxLength(250);

        // Money as owned type
        builder.OwnsOne(p => p.Price, money =>
        {
            money.Property(m => m.Amount).HasColumnName("price_amount").HasColumnType("decimal(18,2)").IsRequired();
            money.Property(m => m.Currency).HasColumnName("price_currency").HasMaxLength(3).IsRequired();
        });

        // SKU as owned type
        builder.OwnsOne(p => p.Sku, sku =>
        {
            sku.Property(s => s.Value).HasColumnName("sku").HasMaxLength(50).IsRequired();
            sku.HasIndex(s => s.Value).IsUnique();
        });

        // ProductDimensions as owned type
        builder.OwnsOne(p => p.Dimensions, dims =>
        {
            dims.Property(d => d.Length).HasColumnName("dim_length").HasColumnType("decimal(10,2)");
            dims.Property(d => d.Width).HasColumnName("dim_width").HasColumnType("decimal(10,2)");
            dims.Property(d => d.Height).HasColumnName("dim_height").HasColumnType("decimal(10,2)");
            dims.Property(d => d.Weight).HasColumnName("dim_weight").HasColumnType("decimal(10,2)");
            dims.Property(d => d.Unit).HasColumnName("dim_unit").HasMaxLength(10);
        });

        builder.Property(p => p.StockQuantity).IsRequired().HasDefaultValue(0);
        builder.Property(p => p.ReservedQuantity).IsRequired().HasDefaultValue(0);
        builder.Property(p => p.IsActive).IsRequired().HasDefaultValue(true);

        builder.Property(p => p.RowVersion).IsRowVersion().IsConcurrencyToken();

        builder.HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // Reviews as owned collection (or separate table via HasMany)
        builder.HasMany(p => p.Reviews)
            .WithOne()
            .HasForeignKey("ProductId")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
