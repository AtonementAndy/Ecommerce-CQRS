using Ecommerce.Application.Products.Queries.GetProductById;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace Ecommerce.UnitTests.Products;

public class GetProductByIdHandlerTests
{
    [Fact]
    public async Task Handle_OnCacheMiss_ShouldLoadRepositoryAndCacheResult()
    {
        var id = Guid.NewGuid();
        var repository = Substitute.For<IProductReadRepository>();
        var createdAt = DateTime.UtcNow;
        repository.GetByIdAsync(id, Arg.Any<CancellationToken>())
            .Returns(new Product(id, "Notebook", "Portable computer", 2999.99m, 4, createdAt, createdAt, 1));
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddMediatR(configuration =>
            configuration.RegisterServicesFromAssembly(typeof(GetProductByIdQuery).Assembly));
        services.AddSingleton(repository);
        services.AddDistributedMemoryCache();

        await using var provider = services.BuildServiceProvider();
        var sender = provider.GetRequiredService<ISender>();

        var firstResult = await sender.Send(new GetProductByIdQuery(id));
        var cachedResult = await sender.Send(new GetProductByIdQuery(id));

        Assert.Equal("Notebook", firstResult?.Name);
        Assert.Equal(firstResult, cachedResult);
        await repository.Received(1).GetByIdAsync(id, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRepositoryDoesNotFindProduct_ShouldReturnNull()
    {
        var repository = Substitute.For<IProductReadRepository>();
        var id = Guid.NewGuid();
        repository.GetByIdAsync(id, Arg.Any<CancellationToken>())
            .Returns((Product?)null);
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddMediatR(configuration =>
            configuration.RegisterServicesFromAssembly(typeof(GetProductByIdQuery).Assembly));
        services.AddSingleton(repository);
        services.AddDistributedMemoryCache();

        await using var provider = services.BuildServiceProvider();
        var result = await provider.GetRequiredService<ISender>().Send(new GetProductByIdQuery(id));

        Assert.Null(result);
        await repository.Received(1).GetByIdAsync(id, Arg.Any<CancellationToken>());
    }
}
