using Eventra.Core.BaseClasses;

namespace Eventra.Core.Tests.BaseClasses;

public class EntityTests
{
    private sealed record TestEvent(string Name) : DomainEvent;

    private class TestEntity : Entity
    {
        public string Name { get; set; } = string.Empty;

        public void DoSomething(string name)
        {
            Name = name;
            RaiseDomainEvent(new TestEvent(name));
        }

        public void DoSomethingElse(string name)
        {
            RaiseDomainEvent(new TestEvent($"else-{name}"));
        }
    }

    [Fact]
    public void Entity_Should_Start_With_No_DomainEvents()
    {
        // Act
        var entity = new TestEntity();

        // Assert
        Assert.Empty(entity.DomainEvents);
    }

    [Fact]
    public void RaiseDomainEvent_Should_Add_Event()
    {
        // Arrange
        var entity = new TestEntity();

        // Act
        entity.DoSomething("hello");

        // Assert
        Assert.Single(entity.DomainEvents);
        Assert.IsType<TestEvent>(entity.DomainEvents.First());
    }

    [Fact]
    public void RaiseDomainEvent_Should_Add_Multiple_Events()
    {
        // Arrange
        var entity = new TestEntity();

        // Act
        entity.DoSomething("first");
        entity.DoSomethingElse("second");

        // Assert
        Assert.Equal(2, entity.DomainEvents.Count);
    }

    [Fact]
    public void ClearDomainEvents_Should_Remove_All()
    {
        // Arrange
        var entity = new TestEntity();
        entity.DoSomething("hello");
        entity.DoSomethingElse("world");

        // Act
        entity.ClearDomainEvents();

        // Assert
        Assert.Empty(entity.DomainEvents);
    }

    [Fact]
    public void DomainEvents_Should_Be_ReadOnly()
    {
        // Arrange
        var entity = new TestEntity();
        entity.DoSomething("hello");

        // Act & Assert
        // Harus return IReadOnlyCollection — tidak bisa di-cast ke List
        Assert.IsAssignableFrom<IReadOnlyCollection<Eventra.Abstractions.IDomainEvent>>(
            entity.DomainEvents);

        // Coba cast ke List<T> — harus null
        Assert.Null(entity.DomainEvents as List<Eventra.Abstractions.IDomainEvent>);
    }
}