using Ecommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Infrastructure.Data
{
    public class EcommerceDbContext(DbContextOptions<EcommerceDbContext> options) : DbContext(options)
    {
        public DbSet<Product> Products { get; set; }
        public DbSet<OutboxMessage> Outbox { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Product>()
                .HasIndex(p => p.Name)
                .IsUnique()
                .HasDatabaseName("IX_Products_Name");

            modelBuilder.Entity<Product>(builder =>
            {
                builder.HasKey(p => p.Id);

                builder.Property(p => p.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                builder.Property(p => p.Description)
                    .HasMaxLength(500);

                builder.Property(p => p.Price)
                    .HasPrecision(18, 2); // Evita warning de precisão de decimal do SQL Server

                builder.Property(p => p.Stock)
                    .IsRequired();

                builder.Property(p => p.Version)
                    .IsConcurrencyToken()
                    .HasDefaultValue(1);
            });

            modelBuilder.Entity<OutboxMessage>(builder =>
            {
                builder.HasKey(o => o.Id);

                builder.HasIndex(o => new { o.Status, o.CreatedAt }); // worker consulta por isso

                builder.Property(o => o.AggregateType)
                    .HasMaxLength(100)
                    .IsRequired();

                builder.Property(o => o.EventType)
                    .HasConversion<string>()
                    .HasMaxLength(50);

                builder.Property(o => o.Payload)
                    .IsRequired(); // nvarchar(max)

                builder.Property(o => o.Status)
                    .HasConversion<string>()
                    .HasMaxLength(20);

                builder.Property(o => o.Error)
                    .HasMaxLength(2000);
            });
        }
    }
}
