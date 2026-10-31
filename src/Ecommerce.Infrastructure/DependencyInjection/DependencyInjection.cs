using Ecommerce.Application.Common.OutBox;
using Ecommerce.Domain.Interfaces;
using Ecommerce.Infrastructure.Data;
using Ecommerce.Infrastructure.Messaging.Consumers;
using Ecommerce.Infrastructure.Outbox;
using Ecommerce.Infrastructure.Repository;
using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using Serilog;

namespace Ecommerce.Infrastructure.DependencyInjection
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddDbContext<EcommerceDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("SqlServer"), sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null
                    );
                    sqlOptions.CommandTimeout(30);
                }));

            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IProductReadRepository, ProductReadRepository>();
            services.AddScoped<IProductWriteRepository, ProductWriteRepository>();
            services.AddScoped<IOutboxWriter, OutboxWriter>();
            services.AddHostedService<OutboxPublisher>();
            services.AddEndpointsApiExplorer();

            services.AddHsts(options =>
            {
                options.MaxAge = TimeSpan.FromDays(365);
                options.IncludeSubDomains = true;
                options.Preload = true;
            });

            // MongoDB (Proteção contra duplicar registro em Design-Time)
            try
            {
                BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
            }
            catch (BsonSerializationException)
            {
                // Ignora se já estiver registrado
            }

            services.AddSingleton<IMongoClient>(sp =>
            {
                var connectionString = configuration.GetConnectionString("MongoDb")
                    ?? configuration["MongoDbSettings:ConnectionString"]
                    ?? "mongodb://root:example@localhost:27017";

                return new MongoClient(connectionString);
            });

            services.AddScoped<IMongoDatabase>(sp =>
            {
                var mongoClient = sp.GetRequiredService<IMongoClient>();
                var databaseName = configuration["MongoDbSettings:DatabaseName"] ?? "EcommerceDb";
                return mongoClient.GetDatabase(databaseName);
            });

            // 3. MediatR
            services.AddMediatR(config =>
                config.RegisterServicesFromAssemblies(
                    typeof(Application.Products.Commands.CreateProduct.CreateProductCommand).Assembly));

            // 4. Redis Cache
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = configuration.GetConnectionString("Redis")
                    ?? "localhost:6379";
                options.InstanceName = "Ecommerce_";
            });

            // 5. MassTransit / RabbitMQ
            services.AddMassTransit(x =>
            {
                x.AddConsumer<CreatedProductConsumer>();
                x.AddConsumer<UpdatedProductConsumer>();
                x.AddConsumer<DeletedProductConsumer>();

                x.UsingRabbitMq((context, config) =>
                {
                    config.Host(configuration.GetConnectionString("RabbitMQ")
                        ?? "amqp://guest:guest@localhost:5672");
                    config.ConfigureEndpoints(context);
                });
            });

            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File("logs/ecommerce.log", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            

            return services;
        }
    }
}