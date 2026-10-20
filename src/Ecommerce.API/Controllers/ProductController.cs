using Ecommerce.Application.Dtos;
using Ecommerce.Application.Products.Commands.CreateProduct;
using Ecommerce.Application.Products.Commands.DeleteProduct;
using Ecommerce.Application.Products.Commands.UpdateProduct;
using Ecommerce.Application.Products.Queries.GetAllProducts;
using Ecommerce.Application.Products.Queries.GetProductById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController(ISender mediator) : ControllerBase
    {
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(IEnumerable<ProductDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
        {
            var products = await mediator.Send(new GetAllProductsQuery(pageNumber, pageSize), cancellationToken);
            return Ok(products);
        }

        [HttpGet("{id:guid}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            var product = await mediator.Send(new GetProductByIdQuery(id), cancellationToken);

            if (product is null)
                return NotFound(new { Message = $"Product ID {id} wasn't found." });

            var etag = $"\"{product.Version}\"";
            Response.Headers.ETag = etag;

            if (Request.Headers.IfNoneMatch == etag)
                return StatusCode(StatusCodes.Status304NotModified);

            return Ok(product);
        }

        [HttpPost]
        [Authorize(Policy = "CatalogManagement")]
        [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateProductCommand command, CancellationToken cancellationToken)
        {
            var productId = await mediator.Send(command, cancellationToken);

            return CreatedAtAction(nameof(GetById), new { id = productId }, productId);
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "CatalogManagement")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductRequest request, CancellationToken cancellationToken)
        {
            var command = new UpdateProductCommand(
                id,
                request.Name,
                request.Description,
                request.Price,
                request.Stock
            );
            await mediator.Send(command, cancellationToken);

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            var ifMatch = Request.Headers.IfMatch.ToString();

            if (string.IsNullOrWhiteSpace(ifMatch))
            {
                return BadRequest(new { Message = "If-Match header is required for delete operations." });
            }

            // Remove aspas e faz parse
            var versionString = ifMatch.Trim('"');
            if (!int.TryParse(versionString, out var version))
            {
                return BadRequest(new { Message = "Invalid If-Match header format. Expected a numeric version." });
            }

            await mediator.Send(new DeleteProductCommand(id, version), cancellationToken);
            return NoContent();
        }

    }
}
