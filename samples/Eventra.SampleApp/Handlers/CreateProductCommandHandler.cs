using Eventra.Core.Abstractions;
using Eventra.EntityFrameworkCore.Extensions;
using Eventra.SampleApp.Commands;
using Eventra.SampleApp.Data;
using Eventra.SampleApp.Entity;
using MediatR;

namespace Eventra.SampleApp.Handlers;

public sealed class CreateProductCommandHandler
    : IRequestHandler<CreateProductCommand, Guid>
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

    public async Task<Guid> Handle(
        CreateProductCommand request,
        CancellationToken cancellationToken)
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