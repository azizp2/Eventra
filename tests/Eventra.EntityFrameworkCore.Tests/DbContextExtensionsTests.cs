using Eventra.Abstractions;
using Eventra.Core.BaseClasses;
using Eventra.EntityFrameworkCore.Extensions;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Eventra.EntityFrameworkCore.Tests;

public class DbContextExtensionsTests
{
    // =================================================================
    // TEST FIXTURES & ENTITIES
    // =================================================================

    public sealed record ProductCreatedEvent(Guid ProductId, string Name) : DomainEvent;
    public sealed record ProductNameChangedEvent(Guid ProductId, string NewName) : DomainEvent;

    public class TestProduct : Entity
    {
        public string Name { get; private set; } = string.Empty;

        private TestProduct() { }

        public static TestProduct Create(string name)
        {
            var product = new TestProduct
            {
                Id = Guid.NewGuid(),
                Name = name
            };
            product.RaiseDomainEvent(new ProductCreatedEvent(product.Id, name));
            return product;
        }

        public void ChangeName(string newName)
        {
            Name = newName;
            RaiseDomainEvent(new ProductNameChangedEvent(Id, newName));
        }
    }

    public class TestDbContext : DbContext
    {
        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }

        public DbSet<TestProduct> Products => Set<TestProduct>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TestProduct>().Ignore(p => p.DomainEvents);
        }
    }

    public class FailingDbContext : TestDbContext
    {
        public FailingDbContext(DbContextOptions<TestDbContext> options) : base(options) { }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            throw new DbUpdateException("Simulated database failure.", new Exception("Inner DB error"));
        }
    }

    private static TestDbContext CreateDbContext(string databaseName)
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: databaseName)
            .Options;

        return new TestDbContext(options);
    }

    private static FailingDbContext CreateFailingDbContext(string databaseName)
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: databaseName)
            .Options;

        return new FailingDbContext(options);
    }

    // =================================================================
    // TESTS: SUCCESS SCENARIOS
    // =================================================================

    [Fact]
    public async Task SaveChangesAndDispatchEventsAsync_Should_Dispatch_And_Clear_Events_On_Success()
    {
        // Arrange
        using var context = CreateDbContext(nameof(SaveChangesAndDispatchEventsAsync_Should_Dispatch_And_Clear_Events_On_Success));
        var mockDispatcher = new Mock<IDomainEventDispatcher>();
        List<IDomainEvent>? capturedEvents = null;

        mockDispatcher
            .Setup(d => d.DispatchAsync(It.IsAny<IEnumerable<IDomainEvent>>(), It.IsAny<CancellationToken>()))
            .Callback<IEnumerable<IDomainEvent>, CancellationToken>((events, _) =>
            {
                capturedEvents = events.ToList();
            })
            .Returns(Task.CompletedTask);

        var product = TestProduct.Create("Mechanical Keyboard");
        context.Products.Add(product);

        // Act
        var result = await context.SaveChangesAndDispatchEventsAsync(mockDispatcher.Object);

        // Assert
        Assert.True(result > 0);
        mockDispatcher.Verify(d => d.DispatchAsync(It.IsAny<IEnumerable<IDomainEvent>>(), It.IsAny<CancellationToken>()), Times.Once);
        Assert.NotNull(capturedEvents);
        Assert.Single(capturedEvents);
        Assert.IsType<ProductCreatedEvent>(capturedEvents[0]);

        // Event pada entity harus sudah bersih
        Assert.Empty(product.DomainEvents);
    }

    [Fact]
    public async Task SaveChangesAndDispatchEventsAsync_Should_Collect_Events_From_Multiple_Entities()
    {
        // Arrange
        using var context = CreateDbContext(nameof(SaveChangesAndDispatchEventsAsync_Should_Collect_Events_From_Multiple_Entities));
        var mockDispatcher = new Mock<IDomainEventDispatcher>();
        List<IDomainEvent>? capturedEvents = null;

        mockDispatcher
            .Setup(d => d.DispatchAsync(It.IsAny<IEnumerable<IDomainEvent>>(), It.IsAny<CancellationToken>()))
            .Callback<IEnumerable<IDomainEvent>, CancellationToken>((events, _) =>
            {
                capturedEvents = events.ToList();
            })
            .Returns(Task.CompletedTask);

        var product1 = TestProduct.Create("Item 1");
        product1.ChangeName("Item 1 Updated"); // 2 events (create + update)
        var product2 = TestProduct.Create("Item 2"); // 1 event (create)

        context.Products.AddRange(product1, product2);

        // Act
        var result = await context.SaveChangesAndDispatchEventsAsync(mockDispatcher.Object);

        // Assert
        Assert.True(result > 0);
        mockDispatcher.Verify(d => d.DispatchAsync(It.IsAny<IEnumerable<IDomainEvent>>(), It.IsAny<CancellationToken>()), Times.Once);
        Assert.NotNull(capturedEvents);
        Assert.Equal(3, capturedEvents.Count);

        // Keduanya harus sudah bersih
        Assert.Empty(product1.DomainEvents);
        Assert.Empty(product2.DomainEvents);
    }

    [Fact]
    public async Task SaveChangesAndDispatchEventsAsync_When_No_Events_Should_Save_Without_Calling_Dispatcher()
    {
        // Arrange
        using var context = CreateDbContext(nameof(SaveChangesAndDispatchEventsAsync_When_No_Events_Should_Save_Without_Calling_Dispatcher));
        var mockDispatcher = new Mock<IDomainEventDispatcher>();

        var product = TestProduct.Create("Item");
        product.ClearDomainEvents(); // Kosongkan event sebelum ditambahkan
        context.Products.Add(product);

        // Act
        var result = await context.SaveChangesAndDispatchEventsAsync(mockDispatcher.Object);

        // Assert
        Assert.True(result > 0);
        mockDispatcher.Verify(d => d.DispatchAsync(It.IsAny<IEnumerable<IDomainEvent>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // =================================================================
    // TESTS: FAILURE & MITIGATION SCENARIOS
    // =================================================================

    [Fact]
    public async Task SaveChangesAndDispatchEventsAsync_When_SaveChanges_Fails_Should_NOT_Dispatch_Or_Clear_Events()
    {
        // Arrange
        using var context = CreateFailingDbContext(nameof(SaveChangesAndDispatchEventsAsync_When_SaveChanges_Fails_Should_NOT_Dispatch_Or_Clear_Events));
        var mockDispatcher = new Mock<IDomainEventDispatcher>();

        var product = TestProduct.Create("Failing Item");
        context.Products.Add(product);

        // Act & Assert
        await Assert.ThrowsAsync<DbUpdateException>(() =>
            context.SaveChangesAndDispatchEventsAsync(mockDispatcher.Object));

        // Dispatcher TIDAK boleh dipanggil jika DB save gagal
        mockDispatcher.Verify(d => d.DispatchAsync(It.IsAny<IEnumerable<IDomainEvent>>(), It.IsAny<CancellationToken>()), Times.Never);

        // Event HARUS TETAP ADA di entity agar bisa di-retry nanti
        Assert.Single(product.DomainEvents);
        Assert.IsType<ProductCreatedEvent>(product.DomainEvents.First());
    }

    [Fact]
    public async Task SaveChangesAndDispatchEventsAsync_Should_Pass_CancellationToken()
    {
        // Arrange
        using var context = CreateDbContext(nameof(SaveChangesAndDispatchEventsAsync_Should_Pass_CancellationToken));
        var mockDispatcher = new Mock<IDomainEventDispatcher>();
        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        var product = TestProduct.Create("Token Test");
        context.Products.Add(product);

        // Act
        await context.SaveChangesAndDispatchEventsAsync(mockDispatcher.Object, token);

        // Assert
        mockDispatcher.Verify(d => d.DispatchAsync(It.IsAny<IEnumerable<IDomainEvent>>(), token), Times.Once);
    }

    [Fact]
    public async Task SaveChangesAndDispatchEventsAsync_When_Context_Null_Should_Throw_ArgumentNullException()
    {
        // Arrange
        DbContext? nullContext = null;
        var mockDispatcher = new Mock<IDomainEventDispatcher>();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            nullContext!.SaveChangesAndDispatchEventsAsync(mockDispatcher.Object));
    }

    [Fact]
    public async Task SaveChangesAndDispatchEventsAsync_When_Dispatcher_Null_Should_Throw_ArgumentNullException()
    {
        // Arrange
        using var context = CreateDbContext(nameof(SaveChangesAndDispatchEventsAsync_When_Dispatcher_Null_Should_Throw_ArgumentNullException));

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            context.SaveChangesAndDispatchEventsAsync(null!));
    }

    [Fact]
    public async Task SaveChangesAndDispatchEventsAsync_Consecutive_Calls_Should_Only_Dispatch_New_Events()
    {
        // Arrange
        using var context = CreateDbContext(nameof(SaveChangesAndDispatchEventsAsync_Consecutive_Calls_Should_Only_Dispatch_New_Events));
        var mockDispatcher = new Mock<IDomainEventDispatcher>();
        var dispatchedBatches = new List<List<IDomainEvent>>();

        mockDispatcher
            .Setup(d => d.DispatchAsync(It.IsAny<IEnumerable<IDomainEvent>>(), It.IsAny<CancellationToken>()))
            .Callback<IEnumerable<IDomainEvent>, CancellationToken>((events, _) =>
            {
                dispatchedBatches.Add(events.ToList());
            })
            .Returns(Task.CompletedTask);

        // First round: create product
        var product = TestProduct.Create("Item First");
        context.Products.Add(product);
        await context.SaveChangesAndDispatchEventsAsync(mockDispatcher.Object);

        // Second round: modify product
        product.ChangeName("Item Renamed");
        await context.SaveChangesAndDispatchEventsAsync(mockDispatcher.Object);

        // Assert
        Assert.Equal(2, dispatchedBatches.Count);
        Assert.Single(dispatchedBatches[0]);
        Assert.IsType<ProductCreatedEvent>(dispatchedBatches[0][0]);
        Assert.Single(dispatchedBatches[1]);
        Assert.IsType<ProductNameChangedEvent>(dispatchedBatches[1][0]);
        Assert.Empty(product.DomainEvents);
    }
}
