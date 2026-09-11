using MediatR;

namespace Eventra.Core.Abstractions;

public interface IDomainEventHandler<T> : INotificationHandler<T>
    where T : IDomainEvent
{
}