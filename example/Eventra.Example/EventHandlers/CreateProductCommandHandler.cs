using Eventra.Abstractions;
using Eventra.EntityFrameworkCore.Extensions;
using Eventra.Example.Commands;
using Eventra.Example.Data;
using Eventra.Example.Entity;

namespace Eventra.Example.EventHandlers;

public sealed class CreateProductCommandHandler
    : ICommandHandler<CreateProductCommand, Guid>
{
    private readonly AppDbContext _dbContext;
    private readonly IDomainEventDispatcher _dispatcher;

    public CreateProductCommandHandler(
        AppDbContext dbContext,
        IDomainEventDispatcher dispatcher)
    {
        _dbContext = dbContext;
        _dispatcher = dispatcher;
    }

    public async Task<Guid> HandleAsync(CreateProductCommand request, CancellationToken cancellationToken = default)
    {
        var product = Product.Create(request.Name);

        _dbContext.Products.Add(product);

        // Commit + dispatch event otomatis.
        await _dbContext.SaveChangesAndDispatchEventsAsync(
            _dispatcher,
            cancellationToken);

        return product.Id;
    }
}