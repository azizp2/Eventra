using Eventra.Core.Extensions;
using Eventra.Example.Commands;
using Eventra.Example.Data;
using Eventra.Example.EventHandlers;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ===== 1. DbContext =====
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("ProductSampleDb"));

// ===== 2. Eventra =====
builder.Services.AddEventra(
    typeof(Program).Assembly);

// ===== 3. Command Handler =====
builder.Services.AddScoped<
    ICommandHandler<CreateProductCommand, Guid>,
    CreateProductCommandHandler>();

var app = builder.Build();

// ===== 4. Endpoint =====
app.MapPost("/api/products", async (
    CreateProductCommand command,
    ICommandHandler<CreateProductCommand, Guid> handler,
    CancellationToken ct) =>
{
    var id = await handler.HandleAsync(command, ct);
    return Results.Created($"/api/products/{id}", new { id });
});

app.Run();