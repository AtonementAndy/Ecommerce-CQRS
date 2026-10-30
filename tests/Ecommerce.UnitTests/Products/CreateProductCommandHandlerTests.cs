using Ecommerce.Application.Common.OutBox;
using Ecommerce.Application.Products.Commands.CreateProduct;
using Ecommerce.Application.Common.Events;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Interfaces;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Ecommerce.UnitTests.Products
{
    public class CreateProductCommandHandlerTests
    {
        private readonly IProductWriteRepository _repository;
        private readonly IOutboxWriter _outboxWriter;
        private readonly IUnitOfWork _unitOfWork;
        private readonly CreateProductCommandHandler _handler;

        public CreateProductCommandHandlerTests()
        {
            _repository = Substitute.For<IProductWriteRepository>();
            _outboxWriter = Substitute.For<IOutboxWriter>();
            _unitOfWork = Substitute.For<IUnitOfWork>();

            _handler = new CreateProductCommandHandler(_repository, _outboxWriter, _unitOfWork);
        }

        [Fact]
        public async Task Handle_WithValidCommand_ShouldSaveProductAndEnqueueOutboxEvent()
        {
            // Arrange
            var command = new CreateProductCommand(
                "Notebook", 
                "An Awesome computer", 
                2999.99m,
                10
            );

            _repository.AddAsync(Arg.Any<Product>(), Arg.Any<CancellationToken>())
                .Returns(call => call.Arg<Product>());

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeEmpty();
            await _repository.Received(1).AddAsync(Arg.Is<Product>(p =>
                p.Name == "Notebook" && 
                p.Description == "An Awesome computer" && 
                p.Price == 2999.99m && p.Stock == 10), CancellationToken.None);

            await _outboxWriter.Received(1).EnqueueAsync(
                "Product",
                Arg.Is<ProductCreatedEvent>(e => e.Name == "Notebook"),
                Arg.Any<CancellationToken>());

            await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_WhenDuplicateNameConstraintFails_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var command = new CreateProductCommand(
                "Notebook",
                "An Awesome computer",
                2999.99m,
                10
            );

            var innerException = new Exception(
                "Violation of UNIQUE KEY constraint 'IX_Products_Name'. Cannot insert duplicate key in object 'dbo.Products'.");

            var dbUpdateException = new DbUpdateException(
                "An error occurred while updating the entries.", innerException);

            _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
                .Returns(Task.FromException<int>(dbUpdateException));

            // Act
            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage($"*{command.Name}*");
        }
    }
}
