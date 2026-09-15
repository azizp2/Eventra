using Eventra.Core.Abstractions;
using Eventra.Core.BaseClasses;
using Eventra.Core.Dispatchers;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;

namespace Eventra.Core.Tests;

public class DomainEventDispatcherTests
{
    private sealed record TestEvent(string Payload) : DomainEvent
    {
        public string Payload { get; init; } = Payload;

    }

    // ---------------------------------------------------------------------
    // TEST 1: Dispatch 1 event ke 1 handler
    // ---------------------------------------------------------------------
    [Fact]
    public async Task DispatchAsync_Should_Publish_Event_To_Mediator()
    {
        // Arrange
        var domainEvent = new TestEvent("hello");
        var mediatorMock = new Mock<IMediator>();
        var loggerMock = new Mock<ILogger<DomainEventDispatcher>>();

        var dispatcher = new DomainEventDispatcher(
            mediatorMock.Object,
            loggerMock.Object);

        // Act
        await dispatcher.DispatchAsync(new[] { domainEvent });

        // Assert
        // ✅ PENTING: Setup pakai IDomainEvent, bukan TestEvent,
        //    karena dispatcher memanggil Publish<IDomainEvent>.
        mediatorMock.Verify(
            m => m.Publish(
                It.Is<IDomainEvent>(e => e == domainEvent),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // ---------------------------------------------------------------------
    // TEST 2: Dispatch multiple events
    // ---------------------------------------------------------------------
    [Fact]
    public async Task DispatchAsync_Should_Publish_Each_Event_Once()
    {
        // Arrange
        var events = new IDomainEvent[]
        {
            new TestEvent("first"),
            new TestEvent("second"),
            new TestEvent("third")
        };

        var mediatorMock = new Mock<IMediator>();
        var loggerMock = new Mock<ILogger<DomainEventDispatcher>>();

        var dispatcher = new DomainEventDispatcher(
            mediatorMock.Object,
            loggerMock.Object);

        // Act
        await dispatcher.DispatchAsync(events);

        // Assert
        mediatorMock.Verify(
            m => m.Publish(
                It.IsAny<IDomainEvent>(),
                It.IsAny<CancellationToken>()),
            Times.Exactly(3));
    }

    // ---------------------------------------------------------------------
    // TEST 3: Error di handler tidak menggagalkan dispatch
    // ---------------------------------------------------------------------
    [Fact]
    public async Task DispatchAsync_Should_Continue_When_One_Event_Throws()
    {
        // Arrange
        var failingEvent = new TestEvent("fail");
        var succeedingEvent = new TestEvent("success");

        var mediatorMock = new Mock<IMediator>();

        // ✅ Setup pakai IDomainEvent (bukan TestEvent).
        //    Kita match berdasarkan instance-nya.
        mediatorMock
            .Setup(m => m.Publish(
                It.Is<IDomainEvent>(e => e == failingEvent),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Boom!"));

        mediatorMock
            .Setup(m => m.Publish(
                It.Is<IDomainEvent>(e => e == succeedingEvent),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var loggerMock = new Mock<ILogger<DomainEventDispatcher>>();

        var dispatcher = new DomainEventDispatcher(
            mediatorMock.Object,
            loggerMock.Object);

        // Act
        var exception = await Record.ExceptionAsync(() =>
            dispatcher.DispatchAsync(new IDomainEvent[]
            {
                failingEvent,
                succeedingEvent
            }));

        // Assert: tidak ada exception yang bocor keluar.
        Assert.Null(exception);

        // Kedua event tetap dicoba untuk dipublish.
        mediatorMock.Verify(
            m => m.Publish(
                It.Is<IDomainEvent>(e => e == failingEvent),
                It.IsAny<CancellationToken>()),
            Times.Once);

        mediatorMock.Verify(
            m => m.Publish(
                It.Is<IDomainEvent>(e => e == succeedingEvent),
                It.IsAny<CancellationToken>()),
            Times.Once);

        // Error di-log.
        loggerMock.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}