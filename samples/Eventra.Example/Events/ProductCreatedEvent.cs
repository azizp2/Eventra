using Eventra.Core.BaseClasses;

namespace Eventra.SampleApp.Events;

public sealed record ProductCreatedEvent(Guid ProductId, string ProductName) : DomainEvent;
