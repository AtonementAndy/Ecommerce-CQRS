using Ecommerce.Application.Cart.Commands.AddItemToCart;
using Ecommerce.Application.Cart.Commands.ClearCart;
using Ecommerce.Application.Cart.Commands.CreateCart;
using Ecommerce.Application.Cart.Commands.RemoveItemFromCart;
using Ecommerce.Application.Cart.Queries;
using Ecommerce.Application.Dtos.Cart;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Carrinho geralmente exige usuário autenticado
public class CartController(ISender mediator) : ControllerBase
{
    /// <summary>
    /// Get the current user's cart
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(CartDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCart(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var cart = await mediator.Send(new GetCartByUserIdQuery(userId), cancellationToken);

        if (cart is null)
            return NotFound(new { Message = "Cart not found." });

        return Ok(cart);
    }

    /// <summary>
    /// Create a new cart for the user
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateCart(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var cartId = await mediator.Send(new CreateCartCommand(userId), cancellationToken);

        return CreatedAtAction(nameof(GetCart), new { }, cartId);
    }

    /// <summary>
    /// Add an item to the cart
    /// </summary>
    [HttpPost("items")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddItem([FromBody] AddItemToCartCommand command, CancellationToken cancellationToken)
    {
        // Garante que o UserId venha do token, não do body
        var commandWithUser = command with { UserId = GetUserId() };

        await mediator.Send(commandWithUser, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Remove an item from the cart
    /// </summary>
    [HttpDelete("items/{productId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveItem(Guid productId, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        await mediator.Send(new RemoveItemFromCartCommand(userId, productId), cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Clear all items from the cart
    /// </summary>
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ClearCart(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        await mediator.Send(new ClearCartCommand(userId), cancellationToken);
        return NoContent();
    }

    private Guid GetUserId()
    {
        // Extrai o UserId do token JWT (claim "sub" ou "userId")
        var userIdClaim = User.FindFirst("sub")?.Value 
                       ?? User.FindFirst("userId")?.Value;

        if (userIdClaim is null || !Guid.TryParse(userIdClaim, out var userId))
            throw new UnauthorizedAccessException("UserId not found in token.");

        return userId;
    }
}