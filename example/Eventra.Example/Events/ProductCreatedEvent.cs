using Eventra.Core.BaseClasses;

namespace Eventra.Example.Events;

public sealed record ProductCreatedEvent(Guid ProductId, string ProductName) : DomainEvent;
