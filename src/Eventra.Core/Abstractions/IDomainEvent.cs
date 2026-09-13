using System;
using MediatR;

namespace Eventra.Core.Abstractions;

public interface IDomainEvent : INotification
{
    /// <summary>
    /// ID unik untuk event ini.
    /// </summary>
    Guid Id { get; }

    /// <summary>
    /// Waktu (UTC) saat event ini terjadi.
    /// </summary>
    DateTime OccurredOn { get; }
}