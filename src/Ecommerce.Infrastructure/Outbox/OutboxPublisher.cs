using Ecommerce.Application.Common.OutBox;
using Ecommerce.Domain.Enums;
using Ecommerce.Infrastructure.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Ecommerce.Infrastructure.Outbox
{
    public class OutboxPublisher(
        IServiceScopeFactory scopeFactory,
        IBus bus,
        ILogger<OutboxPublisher> logger
    ) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = scopeFactory.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<EcommerceDbContext>();

                    var pending = await context.Outbox
                        .Where(o => o.Status == OutboxStatus.Pending)
                        .OrderBy(o => o.CreatedAt)
                        .Take(50)
                        .ToListAsync(stoppingToken);                    

                    foreach (var message in pending)
                    {
                        try
                        {
                            var eventType = OutboxEventRegistry.Resolve(message.EventType);

                            var outboxEvent = JsonSerializer.Deserialize(message.Payload, eventType)
                                ?? throw new InvalidOperationException("Null Payload");

                            await bus.Publish(outboxEvent, eventType, stoppingToken);

                            message.MarkAsPublished();
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, "Failed to publish outbox {Id}", message.Id);
                            message.MarkAsFailed(ex.Message);
                        }
                    }

                    if (pending.Count > 0)
                        await context.SaveChangesAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error in the OutboxPublisher loop");
                }

                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }
}
