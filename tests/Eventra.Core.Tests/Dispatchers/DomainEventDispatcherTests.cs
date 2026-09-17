using Eventra.Abstractions;
using Eventra.Core.BaseClasses;
using Eventra.Core.Dispatchers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Eventra.Core.Tests.Dispatchers;

public class DomainEventDispatcherTests
{
    // =================================================================
    // TEST FIXTURES
    // =================================================================
    private sealed record TestEvent(string Payload) : DomainEvent;

    private sealed record OtherTestEvent(string Payload) : DomainEvent;

    private sealed class TestEventHandler : IDomainEventHandler<TestEvent>
    {
        public List<TestEvent> HandledEvents { get; } = new();
        public Exception? ExceptionToThrow { get; set; }

        public Task HandleAsync(TestEvent domainEvent, CancellationToken cancellationToken = default)
        {
            HandledEvents.Add(domainEvent);

            if (ExceptionToThrow is not null)
                throw ExceptionToThrow;

            return Task.CompletedTask;
        }
    }

    private sealed class OtherTestEventHandler : IDomainEventHandler<OtherTestEvent>
    {
        public List<OtherTestEvent> HandledEvents { get; } = new();
        public Exception? ExceptionToThrow { get; set; }

        public Task HandleAsync(OtherTestEvent domainEvent, CancellationToken cancellationToken = default)
        {
            HandledEvents.Add(domainEvent);

            if (ExceptionToThrow is not null)
                throw ExceptionToThrow;

            return Task.CompletedTask;
        }
    }

    // =================================================================
    // HELPER
    // =================================================================
    private static (IDomainEventDispatcher Dispatcher, IServiceProvider ServiceProvider)
        BuildDispatcher(params IDomainEventHandler<TestEvent>[] handlers)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

        foreach (var handler in handlers)
        {
            services.AddScoped<IDomainEventHandler<TestEvent>>(_ => handler);
        }

        var sp = services.BuildServiceProvider();
        var dispatcher = sp.GetRequiredService<IDomainEventDispatcher>();

        return (dispatcher, sp);
    }

    // =================================================================
    // ✅ SKENARIO POSITIF
    // =================================================================

    // -----------------------------------------------------------------
    // POSITIF 1: Dispatch 1 event → 1 handler
    // -----------------------------------------------------------------
    [Fact]
    public async Task DispatchAsync_Should_Call_Single_Handler()
    {
        // Arrange
        var handler = new TestEventHandler();
        var (dispatcher, sp) = BuildDispatcher(handler);
        var domainEvent = new TestEvent("hello");

        // Act
        await dispatcher.DispatchAsync(new[] { domainEvent });

        // Assert
        Assert.Single(handler.HandledEvents);
        Assert.Same(domainEvent, handler.HandledEvents[0]);

        (sp as IDisposable)?.Dispose();
    }

    // -----------------------------------------------------------------
    // POSITIF 2: Dispatch 1 event → multiple handler
    // -----------------------------------------------------------------
    [Fact]
    public async Task DispatchAsync_Should_Call_All_Handlers()
    {
        // Arrange
        var handler1 = new TestEventHandler();
        var handler2 = new TestEventHandler();
        var handler3 = new TestEventHandler();

        var (dispatcher, sp) = BuildDispatcher(handler1, handler2, handler3);
        var domainEvent = new TestEvent("multi");

        // Act
        await dispatcher.DispatchAsync(new[] { domainEvent });

        // Assert
        Assert.Single(handler1.HandledEvents);
        Assert.Single(handler2.HandledEvents);
        Assert.Single(handler3.HandledEvents);

        (sp as IDisposable)?.Dispose();
    }

    // -----------------------------------------------------------------
    // POSITIF 3: Dispatch multiple events → handler dipanggil N kali
    // -----------------------------------------------------------------
    [Fact]
    public async Task DispatchAsync_Should_Handle_Multiple_Events()
    {
        // Arrange
        var handler = new TestEventHandler();
        var (dispatcher, sp) = BuildDispatcher(handler);

        var events = new IDomainEvent[]
        {
            new TestEvent("first"),
            new TestEvent("second"),
            new TestEvent("third")
        };

        // Act
        await dispatcher.DispatchAsync(events);

        // Assert
        Assert.Equal(3, handler.HandledEvents.Count);
        Assert.Equal(
            new[] { "first", "second", "third" },
            handler.HandledEvents.Select(e => e.Payload));

        (sp as IDisposable)?.Dispose();
    }

    // -----------------------------------------------------------------
    // POSITIF 4: Dispatch event tanpa handler → tidak error
    // -----------------------------------------------------------------
    [Fact]
    public async Task DispatchAsync_Should_Not_Throw_When_No_Handler_Registered()
    {
        // Arrange
        var (dispatcher, sp) = BuildDispatcher();  // tidak ada handler
        var domainEvent = new TestEvent("no-handler");

        // Act
        var exception = await Record.ExceptionAsync(() =>
            dispatcher.DispatchAsync(new[] { domainEvent }));

        // Assert
        Assert.Null(exception);

        (sp as IDisposable)?.Dispose();
    }

    // -----------------------------------------------------------------
    // POSITIF 5: Dispatch empty collection → tidak error
    // -----------------------------------------------------------------
    [Fact]
    public async Task DispatchAsync_Should_Handle_Empty_Collection()
    {
        // Arrange
        var handler = new TestEventHandler();
        var (dispatcher, sp) = BuildDispatcher(handler);

        // Act
        var exception = await Record.ExceptionAsync(() =>
            dispatcher.DispatchAsync(Array.Empty<IDomainEvent>()));

        // Assert
        Assert.Null(exception);
        Assert.Empty(handler.HandledEvents);

        (sp as IDisposable)?.Dispose();
    }

    // -----------------------------------------------------------------
    // POSITIF 6: Dispatch 2 event berbeda tipe → masing-masing handler dipanggil
    // -----------------------------------------------------------------
    [Fact]
    public async Task DispatchAsync_Should_Route_Events_To_Correct_Handlers()
    {
        // Arrange
        var testHandler = new TestEventHandler();
        var otherHandler = new OtherTestEventHandler();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
        services.AddScoped<IDomainEventHandler<TestEvent>>(_ => testHandler);
        services.AddScoped<IDomainEventHandler<OtherTestEvent>>(_ => otherHandler);

        var sp = services.BuildServiceProvider();
        var dispatcher = sp.GetRequiredService<IDomainEventDispatcher>();

        // Act
        await dispatcher.DispatchAsync(new IDomainEvent[]
        {
            new TestEvent("test"),
            new OtherTestEvent("other")
        });

        // Assert
        Assert.Single(testHandler.HandledEvents);
        Assert.Single(otherHandler.HandledEvents);
        Assert.Equal("test", testHandler.HandledEvents[0].Payload);
        Assert.Equal("other", otherHandler.HandledEvents[0].Payload);

        (sp as IDisposable)?.Dispose();
    }

    // =================================================================
    // ❌ SKENARIO NEGATIF
    // =================================================================

    // -----------------------------------------------------------------
    // NEGATIF 1: Error di 1 handler → SELURUH event di-stop (atomic per event)
    // -----------------------------------------------------------------
    [Fact]
    public async Task DispatchAsync_Should_Stop_All_Handlers_When_One_Throws()
    {
        // Arrange
        var failingHandler = new TestEventHandler
        {
            ExceptionToThrow = new InvalidOperationException("Boom!")
        };
        var succeedingHandler = new TestEventHandler();

        var (dispatcher, sp) = BuildDispatcher(failingHandler, succeedingHandler);
        var domainEvent = new TestEvent("fail");

        // Act
        var exception = await Record.ExceptionAsync(() =>
            dispatcher.DispatchAsync(new[] { domainEvent }));

        // Assert: exception tidak bocor ke caller.
        Assert.Null(exception);

        // ✅ Handler pertama dicoba (dan gagal).
        Assert.Single(failingHandler.HandledEvents);

        // ❌ Handler kedua TIDAK dipanggil (atomic per event).
        Assert.Empty(succeedingHandler.HandledEvents);

        (sp as IDisposable)?.Dispose();
    }

    // -----------------------------------------------------------------
    // NEGATIF 2: Error di 1 event → event berikutnya TETAP diproses
    // -----------------------------------------------------------------
    [Fact]
    public async Task DispatchAsync_Should_Continue_To_Next_Event_When_One_Fails()
    {
        // Arrange
        var failingHandler = new TestEventHandler
        {
            ExceptionToThrow = new InvalidOperationException("Boom!")
        };
        var otherHandler = new OtherTestEventHandler();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
        services.AddScoped<IDomainEventHandler<TestEvent>>(_ => failingHandler);
        services.AddScoped<IDomainEventHandler<OtherTestEvent>>(_ => otherHandler);

        var sp = services.BuildServiceProvider();
        var dispatcher = sp.GetRequiredService<IDomainEventDispatcher>();

        var events = new IDomainEvent[]
        {
            new TestEvent("will-fail"),
            new OtherTestEvent("will-succeed")
        };

        // Act
        var exception = await Record.ExceptionAsync(() =>
            dispatcher.DispatchAsync(events));

        // Assert
        Assert.Null(exception);

        // ✅ Event pertama dicoba (dan gagal).
        Assert.Single(failingHandler.HandledEvents);

        // ✅ Event kedua TETAP diproses (event-level isolation).
        Assert.Single(otherHandler.HandledEvents);
        Assert.Equal("will-succeed", otherHandler.HandledEvents[0].Payload);

        (sp as IDisposable)?.Dispose();
    }

    // -----------------------------------------------------------------
    // NEGATIF 3: Error di SEMUA handler → tidak ada exception yang bocor
    // -----------------------------------------------------------------
    [Fact]
    public async Task DispatchAsync_Should_Not_Throw_When_All_Handlers_Fail()
    {
        // Arrange
        var failingHandler1 = new TestEventHandler
        {
            ExceptionToThrow = new InvalidOperationException("Boom 1!")
        };
        var failingHandler2 = new TestEventHandler
        {
            ExceptionToThrow = new InvalidOperationException("Boom 2!")
        };

        var (dispatcher, sp) = BuildDispatcher(failingHandler1, failingHandler2);
        var domainEvent = new TestEvent("all-fail");

        // Act
        var exception = await Record.ExceptionAsync(() =>
            dispatcher.DispatchAsync(new[] { domainEvent }));

        // Assert
        Assert.Null(exception);
        Assert.Single(failingHandler1.HandledEvents);
        // Handler kedua TIDAK dipanggil (atomic per event).
        Assert.Empty(failingHandler2.HandledEvents);

        (sp as IDisposable)?.Dispose();
    }

    // -----------------------------------------------------------------
    // NEGATIF 4: Error di event pertama, event kedua & ketiga tetap jalan
    // -----------------------------------------------------------------
    [Fact]
    public async Task DispatchAsync_Should_Process_Remaining_Events_After_One_Fails()
    {
        // Arrange
        var failingHandler = new TestEventHandler
        {
            ExceptionToThrow = new InvalidOperationException("Boom!")
        };
        var otherHandler = new OtherTestEventHandler();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
        services.AddScoped<IDomainEventHandler<TestEvent>>(_ => failingHandler);
        services.AddScoped<IDomainEventHandler<OtherTestEvent>>(_ => otherHandler);

        var sp = services.BuildServiceProvider();
        var dispatcher = sp.GetRequiredService<IDomainEventDispatcher>();

        var events = new IDomainEvent[]
        {
            new TestEvent("fail-1"),
            new OtherTestEvent("success-1"),
            new OtherTestEvent("success-2")
        };

        // Act
        var exception = await Record.ExceptionAsync(() =>
            dispatcher.DispatchAsync(events));

        // Assert
        Assert.Null(exception);
        Assert.Single(failingHandler.HandledEvents);
        Assert.Equal(2, otherHandler.HandledEvents.Count);

        (sp as IDisposable)?.Dispose();
    }

    // -----------------------------------------------------------------
    // NEGATIF 5: Handler dengan cancellation token cancelled
    // -----------------------------------------------------------------
    [Fact]
    public async Task DispatchAsync_Should_Not_Throw_When_Cancellation_Requested()
    {
        // Arrange
        var handler = new TestEventHandler();
        var (dispatcher, sp) = BuildDispatcher(handler);

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        var exception = await Record.ExceptionAsync(() =>
            dispatcher.DispatchAsync(new[] { new TestEvent("ct") }, cts.Token));

        // Assert — tergantung implementasi handler.
        // Kalau handler tidak cek cancellation → tetap jalan.
        // Kalau handler cek → throw OperationCanceledException.
        // Di sini kita assert: tidak ada exception yang bocor (karena di-catch dispatcher).
        Assert.Null(exception);

        (sp as IDisposable)?.Dispose();
    }
}