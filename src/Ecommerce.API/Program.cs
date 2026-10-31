using Ecommerce.API.Filters;
using Ecommerce.API.Middlewares;
using Ecommerce.Application.Products.Commands.DeleteProduct;
using Ecommerce.Infrastructure.Data;
using Ecommerce.Infrastructure.DependencyInjection;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// 1.Configuração da Autenticação JWT
var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrWhiteSpace(jwtKey) || Encoding.UTF8.GetByteCount(jwtKey) < 32)
{
    throw new InvalidOperationException(
        "A chave JWT não está configurada ou tem menos de 32 bytes. Configure Jwt:Key usando um gerenciador de segredos ou a variável de ambiente Jwt__Key.");
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

builder.Services.AddAuthorizationBuilder()
    .SetFallbackPolicy(new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build())
    .AddPolicy("CatalogManagement", policy =>
        policy.RequireRole("Admin", "Manager")
              .RequireClaim("permission", "catalog:write"))
    .AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin"));

// Add services to the container.

// Ativar quando for conectar ao app React FrontEnd
//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("AllowReactApp", policy =>
//    {
//        policy.WithOrigins("http://localhost:5173", "http://localhost:3000") // Portas padrão do Vite/CRA
//              .AllowAnyHeader()
//              .AllowAnyMethod();
//    });
//});

builder.Services.AddValidatorsFromAssembly(typeof(DeleteProductValidation).Assembly);

builder.Services.AddControllers(options =>
{
    options.Filters.Add<ValidationFilter>();
    options.Filters.Add<LogActionFilter>();
    options.Filters.Add<ApiExceptionFilterAttribute>();
});

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes.Add("Bearer", new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjMiLCJyb2xlIjoiTWFuYWdlciIsInBlcm1pc3Npb24iOiJjYXRhbG9nOndyaXRlIiwiaXNzIjoiaHR0cHM6Ly9zZXUtaWRwLmNvbSIsImF1ZCI6InNob3AtYXBpIiwiZXhwIjoiMjAwMDAwMDAwMCIsImlhdCI6MTUxNjIzOTAyMn0.RtOEZIQq_kVkIXtxKRpmHVuCw4MzCl9ngPuWT6Y3HgE"
        });
        document.SecurityRequirements.Add(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Id = "Bearer",
                        Type = ReferenceType.SecurityScheme
                    }
                },
                Array.Empty<string>()
            }
        });

        return Task.CompletedTask;
    });
});

builder.Host.UseSerilog();
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseExceptionHandler();

if (!app.Environment.IsEnvironment("Testing"))
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<EcommerceDbContext>();
    await dbContext.Database.MigrateAsync();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseHsts();

    app.MapHealthChecks("/health");
    
    app.MapHealthChecks("/alive", new HealthCheckOptions
    {
        Predicate = r => r.Tags.Contains("live")
    });

    app.MapOpenApi().AllowAnonymous();

    app.MapScalarApiReference(options =>
    {
        options.Title = "Ecommerce CQRS API";
        //options.WithOpenApiRoutePattern("/swagger/v1/swagger.json");
        options.Theme = ScalarTheme.BluePlanet;
        options.DefaultHttpClient = new(ScalarTarget.Http, ScalarClient.Http11);
    }).AllowAnonymous();
}

// Ativar quando for conectar ao app React FrontEnd
//app.UseCors("AllowReactApp");

app.UseHttpsRedirection();
app.UseAuthentication(); // <-- ESSENCIAL: Lê e valida o Token JWT!
app.UseAuthorization(); // <-- Valida as Roles e Policies
app.MapControllers();

app.Run();

public partial class Program { }