using Ecommerce.Application.Common.Cache;
using Ecommerce.Application.Common.Events;
using Ecommerce.Application.Dtos;
using MassTransit;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

public class DeletedProductConsumer(
    ILogger<DeletedProductConsumer> logger,
    IMongoDatabase mongoDatabase,
    IDistributedCache cache) : IConsumer<ProductDeletedEvent>
{
    private readonly IMongoCollection<ProductReadModel> _mongoCollection =
        mongoDatabase.GetCollection<ProductReadModel>("ProductsReadModel");

    public async Task Consume(ConsumeContext<ProductDeletedEvent> context)
    {
        var id = context.Message.Id;
        var ct = context.CancellationToken;

        var result = await _mongoCollection.DeleteOneAsync(p => p.Id == id, ct);

        if (result.DeletedCount == 0)
        {
            logger.LogWarning("[RabbitMQ] Product not found in read DB for deletion (ID: {Id})", id);
        }
        else
        {
            logger.LogInformation("[RabbitMQ] Read DB document removed (ID: {Id})", id);
        }

        // Invalida cache — não deixa Redis derrubar o consumer
        try
        {
            await CacheInvalidator.InvalidateProductCachesAsync(cache, id, ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to invalid cache fater delete (ID: {Id})", id);
        }

    }
}