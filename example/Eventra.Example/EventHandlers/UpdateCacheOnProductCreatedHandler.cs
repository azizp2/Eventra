using Eventra.Abstractions;
using Eventra.Example.Events;

namespace Eventra.Example.EventHandlers;

public sealed class UpdateCacheOnProductCreatedHandler
    : IDomainEventHandler<ProductCreatedEvent>
{
    private readonly ILogger<UpdateCacheOnProductCreatedHandler> _logger;

    public UpdateCacheOnProductCreatedHandler(
        ILogger<UpdateCacheOnProductCreatedHandler> logger)
    {
        _logger = logger;
    }

    public Task HandleAsync(
        ProductCreatedEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "🗄️ [CACHE] Update cache untuk produk: {Id}",
            domainEvent.ProductId);

        return Task.CompletedTask;
    }
}