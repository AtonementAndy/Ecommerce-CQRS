using Ecommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Infrastructure.Data
{
    public class EcommerceDbContext(DbContextOptions<EcommerceDbContext> options) : DbContext(options)
    {
        public DbSet<Product> Products { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Product>(builder =>
            {
                builder.HasKey(p => p.Id);

                builder.Property(p => p.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                builder.Property(p => p.Description)
                    .HasMaxLength(500);

                builder.Property(p => p.Price)
                    .HasPrecision(18, 2); // Evita o warning de precisão de decimal do SQL Server

                builder.Property(p => p.Stock)
                    .IsRequired();
            });
        }
    }
}
