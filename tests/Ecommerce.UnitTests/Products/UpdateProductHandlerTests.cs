using Ecommerce.Application.Common.OutBox;
using Ecommerce.Application.Products.Commands.UpdateProduct;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Interfaces;
using Ecommerce.Infrastructure.Messaging.Events;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace Ecommerce.UnitTests.Products;

public class UpdateProductHandlerTests
{
    private readonly IProductWriteRepository _repository = Substitute.For<IProductWriteRepository>();
    private readonly IOutboxWriter _outboxWriter = Substitute.For<IOutboxWriter>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    [Fact]
    public async Task Handle_WithExistingProduct_ShouldUpdateAndEnqueueEvent()
    {
        var product = new Product("Old name", "Old description", 10m, 1);
        product.UpdateDetails("New name", "New description", 20m, 5);
        _repository.UpdateAsync(product.Id, "New name", "New description", 20m, 5, 1, Arg.Any<CancellationToken>())
            .Returns(product);

        var handler = new UpdateProductCommandHandler(_repository, _unitOfWork, _outboxWriter);
        var command = new UpdateProductCommand(product.Id, "New name", "New description", 20m, 5, 1);

        await handler.Handle(command, CancellationToken.None);

        await _repository.Received(1).UpdateAsync(
            product.Id, "New name", "New description", 20m, 5, 1, Arg.Any<CancellationToken>());
        await _outboxWriter.Received(1).EnqueueAsync(
            "Product",
            Arg.Is<ProductUpdatedEvent>(e => e.Id == product.Id && e.Name == "New name"),
            Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenProductDoesNotExist_ShouldNotWriteOutboxOrSave()
    {
        var id = Guid.NewGuid();
        _repository.UpdateAsync(id, Arg.Any<string>(), Arg.Any<string>(), Arg.Any<decimal>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns((Product?)null);
        var handler = new UpdateProductCommandHandler(_repository, _unitOfWork, _outboxWriter);

        var act = () => handler.Handle(new UpdateProductCommand(id, "Name", "Description", 10m, 1, 1), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
        await _outboxWriter.DidNotReceiveWithAnyArgs().EnqueueAsync("Product", Arg.Any<ProductUpdatedEvent>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenSaveDetectsConcurrencyConflict_ShouldThrowInvalidOperation()
    {
        var product = new Product("Name", "Description", 10m, 1);
        _repository.UpdateAsync(product.Id, Arg.Any<string>(), Arg.Any<string>(), Arg.Any<decimal>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(product);
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromException<int>(new DbUpdateConcurrencyException()));
        var handler = new UpdateProductCommandHandler(_repository, _unitOfWork, _outboxWriter);

        var act = () => handler.Handle(
            new UpdateProductCommand(product.Id, "Name", "Description", 10m, 1, 1),
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*modified by another user*");
    }
}
