using Eventra.Core.BaseClasses;

namespace Eventra.Core.Tests.BaseClasses;

public class DomainEventTests
{
    private sealed record TestEvent : DomainEvent;

    [Fact]
    public void DomainEvent_Should_Have_Unique_Id()
    {
        // Act
        var evt1 = new TestEvent();
        var evt2 = new TestEvent();

        // Assert
        Assert.NotEqual(Guid.Empty, evt1.Id);
        Assert.NotEqual(Guid.Empty, evt2.Id);
        Assert.NotEqual(evt1.Id, evt2.Id);
    }

    [Fact]
    public void DomainEvent_Should_Have_OccurredOn_Close_To_Now()
    {
        // Arrange
        var before = DateTime.UtcNow;

        // Act
        var evt = new TestEvent();

        // Assert
        var after = DateTime.UtcNow;
        Assert.InRange(evt.OccurredOn, before, after);
    }

    [Fact]
    public void DomainEvent_Should_Use_TimeProvider()
    {
        // Arrange
        var fixedTime = new DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.Zero);
        var fakeTimeProvider = new FakeTimeProvider(fixedTime);

        // Act
        var evt = new TestEventWithTimeProvider(fakeTimeProvider);

        // Assert
        Assert.Equal(fixedTime.UtcDateTime, evt.OccurredOn);
    }

    // Event dengan TimeProvider untuk test
    private sealed record TestEventWithTimeProvider : DomainEvent
    {
        public TestEventWithTimeProvider(TimeProvider timeProvider)
            : base(timeProvider) { }
    }

    // Fake TimeProvider sederhana
    private sealed class FakeTimeProvider : TimeProvider
    {
        private readonly DateTimeOffset _now;
        public FakeTimeProvider(DateTimeOffset now) => _now = now;
        public override DateTimeOffset GetUtcNow() => _now;
    }
}