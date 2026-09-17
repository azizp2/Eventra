using Eventra.Abstractions;
using Eventra.Example.Events;

namespace Eventra.Example.EventHandlers;

/// <summary>
/// Handler yang mensimulasikan pengiriman email saat produk dibuat.
/// </summary>
public sealed class SendEmailOnProductCreatedHandler(ILogger<SendEmailOnProductCreatedHandler> logger)
    : IDomainEventHandler<ProductCreatedEvent>
{
    public Task HandleAsync(ProductCreatedEvent notification, CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "📧 [EMAIL] Produk '{Name}' (Id: {Id}) baru dibuat. Mengirim email ke admin...",
            notification.ProductName,
            notification.ProductId);

        return Task.CompletedTask;
    }
}