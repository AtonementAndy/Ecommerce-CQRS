using Ecommerce.Application.Common.Interfaces;
using Ecommerce.Application.Dtos;
using Ecommerce.Application.Products.Queries.GetProductById;
using Microsoft.Extensions.DependencyInjection;
using MediatR;
using NSubstitute;

namespace Ecommerce.UnitTests.Products;

public class GetProductByIdHandlerTests
{
    [Fact]
    public async Task Handle_OnCacheMiss_ShouldLoadRepositoryAndCacheResult()
    {
        var id = Guid.NewGuid();
        var repository = Substitute.For<IProductReadRepository>();
        repository.GetByIdAsync(id, Arg.Any<CancellationToken>())
            .Returns(new ProductReadModel
            {
                Id = id,
                Name = "Notebook",
                Description = "Portable computer",
                Price = 2999.99m,
                Stock = 4,
                CreatedAt = DateTime.UtcNow,
                Version = 1
            });
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
            .Returns((ProductReadModel?)null);
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
