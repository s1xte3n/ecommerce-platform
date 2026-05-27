using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ECommerce.Infrastructure.Persistence;

public class ECommerceDbContextFactory : IDesignTimeDbContextFactory<ECommerceDbContext>
{
    public ECommerceDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ECommerceDbContext>();
        optionsBuilder.UseNpgsql(
            "Host=localhost;Port=5432;Database=ecommerce_dev;Username=ecommerce_user;Password=dev_password_change_in_production;SSL Mode=Disable;Trust Server Certificate=true"
        );
        return new ECommerceDbContext(optionsBuilder.Options);
    }
}
