using Eventra.Core.Extensions;
using Eventra.SampleApp.Commands;
using Eventra.SampleApp.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var services = new ServiceCollection();

// Logging ke console.
services.AddLogging(cfg =>
{
    cfg.AddSimpleConsole(opts => opts.SingleLine = true);
    cfg.SetMinimumLevel(LogLevel.Information);
});

// DbContext dengan InMemory database (untuk demo).
services.AddDbContext<AppDbContext>(opts =>
    opts.UseInMemoryDatabase("SampleDb"));

// Register Domain Events + MediatR.
// Assembly yang di-scan: SampleApp (handler + command) dan Core (dispatcher).
services.AddDomainEvents(
    typeof(Program).Assembly,
    typeof(Eventra.Core.Abstractions.IDomainEvent).Assembly);

var provider = services.BuildServiceProvider();

// Jalankan command.
using var scope = provider.CreateScope();
var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

Console.WriteLine("=== Membuat produk pertama ===");
var id1 = await mediator.Send(new CreateProductCommand("Kopi Arabica"));
Console.WriteLine($"Produk dibuat dengan Id: {id1}\n");

Console.WriteLine("=== Membuat produk kedua ===");
var id2 = await mediator.Send(new CreateProductCommand("Teh Hijau"));
Console.WriteLine($"Produk dibuat dengan Id: {id2}\n");

Console.WriteLine("Selesai. Tekan ENTER untuk keluar.");
Console.ReadLine();