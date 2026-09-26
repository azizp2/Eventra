using Eventra.Abstractions;
using Eventra.Core.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Eventra.Core.Tests.Extensions;

public class ServiceCollectionExtensionsTests
{
    // -----------------------------------------------------------------
    // Test event & handler (public supaya ke-scan)
    // -----------------------------------------------------------------
    public sealed record ScanTestEvent(string Payload) : Eventra.Core.BaseClasses.DomainEvent;

    public sealed class ScanTestHandler : IDomainEventHandler<ScanTestEvent>
    {
        public Task HandleAsync(ScanTestEvent domainEvent, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    [Fact]
    public void AddEventra_Should_Register_Dispatcher()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddEventra(typeof(ScanTestHandler).Assembly);

        // Assert
        var descriptor = services.FirstOrDefault(d =>
            d.ServiceType == typeof(IDomainEventDispatcher));

        Assert.NotNull(descriptor);
    }

    [Fact]
    public void AddEventra_Should_Register_Handlers()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddEventra(typeof(ScanTestHandler).Assembly);

        // Assert
        var descriptor = services.FirstOrDefault(d =>
            d.ServiceType == typeof(IDomainEventHandler<ScanTestEvent>));

        Assert.NotNull(descriptor);
        Assert.Equal(typeof(ScanTestHandler), descriptor.ImplementationType);
    }

    [Fact]
    public void AddEventra_Should_Throw_When_Services_Null()
    {
        // Arrange
        IServiceCollection? services = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            services!.AddEventra(typeof(ScanTestHandler).Assembly));
    }

    [Fact]
    public void AddEventra_Should_Throw_When_Assemblies_Null()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            services.AddEventra(null!));
    }

    public sealed class AnotherScanTestHandler : IDomainEventHandler<ScanTestEvent>
    {
        public Task HandleAsync(ScanTestEvent domainEvent, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    [Fact]
    public void AddEventra_Should_Register_Multiple_Handlers_For_Same_Event()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddEventra(typeof(ScanTestHandler).Assembly);

        // Assert
        var descriptors = services.Where(d =>
            d.ServiceType == typeof(IDomainEventHandler<ScanTestEvent>)).ToList();

        Assert.True(descriptors.Count >= 2);
        Assert.Contains(descriptors, d => d.ImplementationType == typeof(ScanTestHandler));
        Assert.Contains(descriptors, d => d.ImplementationType == typeof(AnotherScanTestHandler));
    }
}