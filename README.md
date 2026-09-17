<div align="center">

# ⚡ Eventra

**Domain Events for .NET — Simple, Clean, Zero Dependency.**

[![NuGet](https://img.shields.io/nuget/v/Eventra.Core.svg)](https://www.nuget.org/packages/Eventra.Core)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Eventra.Core.svg?style=flat-square&logo=nuget)](https://www.nuget.org/packages/Eventra.Core)
[![License](https://img.shields.io/badge/license-MIT-green)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?style=flat-square&logo=dotnet)](https://dotnet.microsoft.com)

[Getting Started](#-getting-started) •
[Packages](#-packages) •
[Documentation](https://azizp2.github.io/EventraDocs) •
[Samples](https://azizp2.github.io/EventraDocs/) •
[Contributing](#-contributing)

</div>

---

## ✨ Apa Itu Eventra?

**Eventra** adalah framework **Domain Event** ringan untuk .NET dengan
**built-in dispatcher** — **tanpa MediatR**, **tanpa external dependency**.

Cocok untuk project yang menggunakan **Clean Architecture + DDD** dan ingin
menambahkan konsep **Domain Event** tanpa kerumitan Event Sourcing,
Outbox Pattern, atau distributed event.

> **Filosofi:** *"Domain events, made simple — zero dependency."*

📖 **Dokumentasi lengkap:** [eventra.dev/docs](https://azizp2.github.io/EventraDocs/)

---

## 🎯 Masalah yang Dipecahkan

Bayangkan Anda punya **toko online**. Ketika ada **pesanan baru**, ada banyak
hal yang harus dilakukan:

1. Kurangi stok barang.
2. Kirim email ke pembeli.
3. Beri tahu admin gudang.
4. Update cache.
5. Catat ke audit log.

Kalau semua ini dilakukan dalam **satu fungsi besar**, kodenya jadi panjang,
susah diubah, dan susah di-test.

**Eventra** menyelesaikannya dengan **Domain Event**:

- Fungsi utama hanya melakukan **satu hal**: "Buat Pesanan".
- Setelah pesanan dibuat, ia **berteriak**: `OrderCreatedEvent`.
- Bagian lain yang "mendengarkan" akan bereaksi sendiri-sendiri.

---

## 🚀 Fitur Utama

| Fitur | Deskripsi |
| :--- | :--- |
| ✅ **Zero Dependency** | Hanya butuh `Microsoft.Extensions.*` — tanpa MediatR. |
| ✅ **Built-in Dispatcher** | Dispatcher sendiri, tanpa pihak ketiga. |
| ✅ **Simple API** | `RaiseDomainEvent(...)` — selesai. |
| ✅ **Immutable Events** | Pakai `record`, aman dari mutasi. |
| ✅ **Auto Dispatch** | Event otomatis jalan setelah `SaveChanges`. |
| ✅ **Multi-Handler** | Satu event bisa punya banyak handler. |
| ✅ **Atomic per Event** | Error di handler tidak menggagalkan event lain. |
| ✅ **DI-Friendly** | Handler auto-register via assembly scanning. |
| ✅ **EF Core Integration** | Extension `SaveChangesAndDispatchEventsAsync`. |
| ✅ **Framework-Agnostic** | Bisa dipakai tanpa EF Core (Dapper, NHibernate, dll). |

---

## 📦 Packages

Eventra terdiri dari **3 package**:

| Package | Deskripsi | Dependency |
| :--- | :--- | :--- |
| **`Eventra.Abstractions`** | Interface only — **zero dependency**. | ❌ Tidak ada |
| **`Eventra.Core`** | Base class + built-in dispatcher. | `Microsoft.Extensions.*` |
| **`Eventra.EntityFrameworkCore`** | Integrasi EF Core. | `Microsoft.EntityFrameworkCore` |

### 📊 Install Matrix

| Skenario | Install |
| :--- | :--- |
| **Built-in + EF Core** | `Eventra.Core` + `Eventra.EntityFrameworkCore` |
| **Built-in tanpa EF Core** | `Eventra.Core` |
| **Interface only** (custom dispatcher) | `Eventra.Abstractions` |

---

## 📥 Instalasi

```bash
# Core (wajib)
dotnet add package Eventra.Core

# Integrasi EF Core (opsional)
dotnet add package Eventra.EntityFrameworkCore
```

---

## 🏁 Getting Started

### 1. Setup DI

```csharp
builder.Services.AddEventra(
    typeof(Program).Assembly,
    typeof(ProductCreatedEvent).Assembly);
```

### 2. Definisikan Event

```csharp
using Eventra.Core.BaseClasses;

public sealed record ProductCreatedEvent(
    Guid ProductId,
    string ProductName
) : DomainEvent;
```

### 3. Buat Entity

```csharp
using Eventra.Core.BaseClasses;

public class Product : Entity
{
    public string Name { get; private set; } = string.Empty;

    private Product() { }

    public static Product Create(string name)
    {
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = name
        };

        product.RaiseDomainEvent(new ProductCreatedEvent(product.Id, name));
        return product;
    }
}
```

### 4. Buat Handler

```csharp
using Eventra.Abstractions;
using Microsoft.Extensions.Logging;

public sealed class SendEmailOnProductCreatedHandler
    : IDomainEventHandler<ProductCreatedEvent>
{
    private readonly ILogger<SendEmailOnProductCreatedHandler> _logger;

    public SendEmailOnProductCreatedHandler(
        ILogger<SendEmailOnProductCreatedHandler> logger)
    {
        _logger = logger;
    }

    public Task HandleAsync(
        ProductCreatedEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "📧 Produk {Name} dibuat. Kirim email...",
            domainEvent.ProductName);

        return Task.CompletedTask;
    }
}
```

### 5. Simpan + Dispatch

```csharp
using Eventra.Abstractions;
using Eventra.EntityFrameworkCore.Extensions;

var product = Product.Create("Kopi Arabica");
_dbContext.Products.Add(product);

await _dbContext.SaveChangesAndDispatchEventsAsync(_dispatcher, ct);
```

**Selesai!** 🎉

📖 **Panduan lengkap:** [eventra.dev/docs/getting-started](https://azizp2.github.io/EventraDocs/getting-started)

---

## 🎯 Contoh Lain

### Banyak Handler untuk 1 Event

```csharp
// Handler 1: Kirim email
public sealed class SendEmailHandler : IDomainEventHandler<ProductCreatedEvent>
{
    public Task HandleAsync(ProductCreatedEvent e, CancellationToken ct = default)
        => _email.SendAsync(e.CustomerEmail, "Produk dibuat", ct);
}

// Handler 2: Audit log
public sealed class AuditLogHandler : IDomainEventHandler<ProductCreatedEvent>
{
    public Task HandleAsync(ProductCreatedEvent e, CancellationToken ct = default)
        => _audit.LogAsync($"ProductCreated: {e.ProductId}", ct);
}

// Handler 3: Update cache
public sealed class UpdateCacheHandler : IDomainEventHandler<ProductCreatedEvent>
{
    public Task HandleAsync(ProductCreatedEvent e, CancellationToken ct = default)
        => _cache.SetAsync($"product:{e.ProductId}", e.ProductName, ct);
}
```

### Aggregate Root

```csharp
public sealed class Order : AggregateRoot
{
    public Guid CustomerId { get; private set; }
    public OrderStatus Status { get; private set; }

    private Order() { }

    public static Order Create(Guid customerId)
    {
        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            Status = OrderStatus.Draft
        };

        order.RaiseDomainEvent(new OrderCreatedEvent(order.Id, customerId));
        return order;
    }

    public void Submit()
    {
        if (Status != OrderStatus.Draft)
            throw new InvalidOperationException("Order already submitted.");

        Status = OrderStatus.Submitted;
        RaiseDomainEvent(new OrderSubmittedEvent(Id, CustomerId));
    }
}
```

### EF Core — Ignore DomainEvents

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Product>()
        .Ignore(p => p.DomainEvents);
}
```

📖 **Contoh lainnya:** [eventra.dev/samples](https://azizp2.github.io/EventraDocs/)

---

## 🛡️ Error Handling

Eventra **atomic per event**:

- ✅ Semua handler untuk 1 event harus sukses.
- ❌ Jika 1 handler gagal → handler lain di event itu **tidak jalan**.
- ✅ Event berikutnya **tetap diproses**.
- ✅ Exception **tidak** dilempar ke caller — hanya di-log.

📖 **Detail:** [eventra.dev/docs/error-handling](https://azizp2.github.io/EventraDocs/error-handling)

---

## 📋 Requirements

| Requirement | Versi |
| :--- | :--- |
| **.NET** | 10.0 |
| **EF Core** | ≥ 10.0 (opsional) |

---

## 📄 License

Distributed under the **MIT License**. Lihat [LICENSE](LICENSE).

---

<div align="center">

**Made with ❤️ for the .NET community.**

🌐 [eventra docs](https://azizp2.github.io/EventraDocs/) •
📖 [Documentation](https://azizp2.github.io/EventraDocs/) •
🐛 [Issues](https://github.com/azizp2/Eventra/issues)

</div>