using Eventra.Core.BaseClasses;

namespace Eventra.Core.Tests.BaseClasses;

public class AggregateRootTests
{
    private sealed record OrderCreatedEvent(Guid OrderId) : DomainEvent;

    private class OrderAggregate : AggregateRoot
    {
        public OrderAggregate(Guid id)
        {
            Id = id;
            RaiseDomainEvent(new OrderCreatedEvent(id));
        }
    }

    [Fact]
    public void AggregateRoot_Should_Inherit_From_Entity()
    {
        var id = Guid.NewGuid();
        var order = new OrderAggregate(id);

        Assert.IsAssignableFrom<Entity>(order);
        Assert.IsAssignableFrom<Entity<Guid>>(order);
        Assert.Equal(id, order.Id);
    }

    [Fact]
    public void AggregateRoot_Should_Manage_DomainEvents()
    {
        var id = Guid.NewGuid();
        var order = new OrderAggregate(id);

        Assert.Single(order.DomainEvents);
        Assert.IsType<OrderCreatedEvent>(order.DomainEvents.First());

        order.ClearDomainEvents();
        Assert.Empty(order.DomainEvents);
    }
}
