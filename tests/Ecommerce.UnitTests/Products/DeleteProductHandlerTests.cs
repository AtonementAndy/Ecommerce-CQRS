using Ecommerce.Application.Common.Events;
using Ecommerce.Application.Common.OutBox;
using Ecommerce.Application.Products.Commands.DeleteProduct;
using Ecommerce.Domain.Interfaces;
using FluentAssertions;
using NSubstitute;

namespace Ecommerce.UnitTests.Products;

public class DeleteProductHandlerTests
{
    private readonly IProductWriteRepository _repository = Substitute.For<IProductWriteRepository>();
    private readonly IOutboxWriter _outboxWriter = Substitute.For<IOutboxWriter>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    [Fact]
    public async Task Handle_WhenProductIsDeleted_ShouldEnqueueEventAndSave()
    {
        var id = Guid.NewGuid();
        _repository.DeleteAsync(id, 4, Arg.Any<CancellationToken>()).Returns(true);
        var handler = new DeleteProductCommandHandler(_repository, _unitOfWork, _outboxWriter);

        await handler.Handle(new DeleteProductCommand(id, 4), CancellationToken.None);

        await _outboxWriter.Received(1).EnqueueAsync(
            "Product",
            Arg.Is<ProductDeletedEvent>(message => message.Id == id && message.Version == 5),
            Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenProductDoesNotExist_ShouldNotEnqueueOrSave()
    {
        var id = Guid.NewGuid();
        _repository.DeleteAsync(id, 1, Arg.Any<CancellationToken>()).Returns(false);
        var handler = new DeleteProductCommandHandler(_repository, _unitOfWork, _outboxWriter);

        var act = () => handler.Handle(new DeleteProductCommand(id, 1), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
        await _outboxWriter.DidNotReceiveWithAnyArgs().EnqueueAsync("Product", Arg.Any<ProductDeletedEvent>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
