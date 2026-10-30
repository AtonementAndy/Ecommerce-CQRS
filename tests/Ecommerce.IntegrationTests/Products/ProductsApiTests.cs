using Ecommerce.Application.Dtos;
using Ecommerce.IntegrationTests.Fixtures;
using Microsoft.AspNetCore.Mvc.Testing;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Net.Http.Json;
using Microsoft.IdentityModel.Tokens;

namespace Ecommerce.IntegrationTests.Products;

public sealed class ProductsApiTests : IClassFixture<EcommerceWebApplicationFactory>
{
    private readonly EcommerceWebApplicationFactory _factory;

    public ProductsApiTests(EcommerceWebApplicationFactory factory)
    {
        _factory = factory;
        _factory.Store.Reset();
    }

    [Fact]
    public async Task ReadEndpoints_ShouldBeAnonymousAndWriteEndpoints_ShouldRequireAuthentication()
    {
        using var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var readResponse = await client.GetAsync("/api/Product");
        var writeResponse = await client.PostAsJsonAsync("/api/Product", ValidProduct());

        Assert.Equal(HttpStatusCode.OK, readResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, writeResponse.StatusCode);
    }

    [Fact]
    public async Task Create_ShouldRejectTokensWithoutRequiredClaimsOrWithWrongAudience()
    {
        using var client = CreateClientWithToken("Manager");

        var missingPermission = await client.PostAsJsonAsync("/api/Product", ValidProduct());
        using var wrongAudienceClient = CreateClientWithToken("Manager", "catalog:write", audience: "other-api");
        var wrongAudience = await wrongAudienceClient.PostAsJsonAsync("/api/Product", ValidProduct());

        Assert.Equal(HttpStatusCode.Forbidden, missingPermission.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, wrongAudience.StatusCode);
    }

    [Fact]
    public async Task CreateAndRead_ShouldRequireExpectedRoleAndSupportEntityTags()
    {
        using var client = CreateClientWithToken("Manager", "catalog:write");
        var createResponse = await client.PostAsJsonAsync("/api/Product", ValidProduct());
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var id = await createResponse.Content.ReadFromJsonAsync<Guid>();

        var readResponse = await client.GetAsync($"/api/Product/{id}");
        Assert.Equal(HttpStatusCode.OK, readResponse.StatusCode);
        Assert.Equal("\"1\"", readResponse.Headers.ETag!.Tag);
        var product = await readResponse.Content.ReadFromJsonAsync<ProductResponse>();
        Assert.Equal("Notebook", product?.Name);

        using var conditionalRequest = new HttpRequestMessage(HttpMethod.Get, $"/api/Product/{id}");
        conditionalRequest.Headers.IfNoneMatch.Add(readResponse.Headers.ETag);
        var notModified = await client.SendAsync(conditionalRequest);

        Assert.Equal(HttpStatusCode.NotModified, notModified.StatusCode);

        using var managerWithoutPermission = CreateClientWithToken("Manager");
        var missingIfMatch = await managerWithoutPermission.PutAsJsonAsync(
            $"/api/Product/{id}", ValidProduct("Updated"));
        Assert.Equal(HttpStatusCode.Forbidden, missingIfMatch.StatusCode);

        using var authorizedManager = CreateClientWithToken("Manager", "catalog:write");
        var updateWithoutIfMatch = await authorizedManager.PutAsJsonAsync(
            $"/api/Product/{id}", ValidProduct("Updated"));
        Assert.Equal(HttpStatusCode.BadRequest, updateWithoutIfMatch.StatusCode);

        using var updateRequest = new HttpRequestMessage(HttpMethod.Put, $"/api/Product/{id}")
        {
            Content = JsonContent.Create(ValidProduct("Updated"))
        };
        updateRequest.Headers.IfMatch.Add(new EntityTagHeaderValue("\"1\""));
        var updateResponse = await authorizedManager.SendAsync(updateRequest);
        Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);
    }

    [Fact]
    public async Task Update_ShouldRejectMalformedIfMatchHeader()
    {
        using var client = CreateClientWithToken("Manager", "catalog:write");
        using var request = new HttpRequestMessage(HttpMethod.Put, $"/api/Product/{Guid.NewGuid()}")
        {
            Content = JsonContent.Create(ValidProduct())
        };
        request.Headers.IfMatch.Add(new EntityTagHeaderValue("\"not-a-version\""));

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Delete_ShouldRequireAdminRoleAndIfMatchHeader()
    {
        using var creator = CreateClientWithToken("Manager", "catalog:write");
        var created = await creator.PostAsJsonAsync("/api/Product", ValidProduct());
        var id = await created.Content.ReadFromJsonAsync<Guid>();

        using var manager = CreateClientWithToken("Manager", "catalog:write");
        var forbidden = await manager.DeleteAsync($"/api/Product/{id}");
        Assert.Equal(HttpStatusCode.Forbidden, forbidden.StatusCode);

        using var admin = CreateClientWithToken("Admin");
        var missingIfMatch = await admin.DeleteAsync($"/api/Product/{id}");
        Assert.Equal(HttpStatusCode.BadRequest, missingIfMatch.StatusCode);

        using var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/Product/{id}");
        request.Headers.IfMatch.Add(new EntityTagHeaderValue("\"1\""));
        var deleted = await admin.SendAsync(request);

        Assert.Equal(HttpStatusCode.NoContent, deleted.StatusCode);
    }

    [Fact]
    public async Task Create_ShouldReturnBadRequestForInvalidProduct()
    {
        using var client = CreateClientWithToken("Manager", "catalog:write");
        var response = await client.PostAsJsonAsync("/api/Product", ValidProduct(name: "", price: 0));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private HttpClient CreateClientWithToken(
        string role,
        string? permission = null,
        string? audience = null)
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", CreateToken(role, permission, audience));
        return client;
    }

    private static string CreateToken(string role, string? permission, string? audience)
    {
        var claims = new List<Claim> { new(ClaimTypes.Role, role) };
        if (permission is not null)
            claims.Add(new Claim("permission", permission));

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(EcommerceWebApplicationFactory.JwtKey));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: EcommerceWebApplicationFactory.JwtIssuer,
            audience: audience ?? EcommerceWebApplicationFactory.JwtAudience,
            claims: claims,
            notBefore: DateTime.UtcNow.AddMinutes(-1),
            expires: DateTime.UtcNow.AddMinutes(5),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static object ValidProduct(string name = "Notebook", decimal price = 100m) => new
    {
        Name = name,
        Description = "Portable computer",
        Price = price,
        Stock = 5
    };
}
