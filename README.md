<div align="center">

# ⚡ Eventra

**Domain Events for .NET — Simple, Clean, Powerful.**

[![NuGet](https://img.shields.io/nuget/v/Eventra.Core.svg)](https://www.nuget.org/packages/Eventra.Core)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Eventra.svg?style=flat-square&logo=nuget)](https://www.nuget.org/packages/Eventra.Core)
[![License](https://img.shields.io/badge/license-MIT-green)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-8.0%20%7C%2010.0-512BD4?style=flat-square&logo=dotnet)](https://dotnet.microsoft.com)

[Getting Started](#-getting-started) •
[Documentation](#-documentation) •
[Samples](#-samples) •
[Contributing](#-contributing)

</div>

---

## ✨ Apa Itu Eventra?

**Eventra** adalah framework **Domain Event** ringan untuk .NET, dibangun di atas
[MediatR](https://github.com/jbogard/MediatR) dan terintegrasi dengan
**Entity Framework Core**.

Cocok untuk project yang menggunakan **Clean Architecture + CQRS** dan ingin
menambahkan konsep **Domain Event** ala DDD — tanpa kerumitan Event Sourcing,
Outbox Pattern, atau distributed event.

> **Filosofi:** *"Domain events, made simple."*

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
| ✅ **Simple API** | `RaiseDomainEvent(...)` — selesai. |
| ✅ **Immutable Events** | Pakai `record`, aman dari mutasi. |
| ✅ **Auto Dispatch** | Event otomatis jalan setelah `SaveChanges`. |
| ✅ **Multi-Handler** | Satu event bisa punya banyak handler. |
| ✅ **Error Isolation** | Error di handler tidak menggagalkan transaksi. |
| ✅ **DI-Friendly** | Handler auto-register via assembly scanning. |
| ✅ **EF Core Integration** | Extension `SaveChangesAndDispatchEventsAsync`. |
| ✅ **Framework-Agnostic** | Bisa dipakai tanpa EF Core (Dapper, NHibernate, dll). |
| ✅ **Multi-Target** | Support .NET 8 & .NET 10. |
| ✅ **Well-Documented** | XML docs untuk semua public API. |

---

## 📦 Instalasi

### Via .NET CLI

```bash
# Core (wajib)
dotnet add package Eventra.Core

# Integrasi EF Core (opsional)
dotnet add package Eventra.EntityFrameworkCore
```

### Via PackageReference

```xml
<ItemGroup>
  <PackageReference Include="Eventra-Core" Version="1.0.1" />
  <PackageReference Include="Eventra.EntityFrameworkCore" Version="1.0.1" />
</ItemGroup>
```

---

## 🏁 Getting Started

### 1. Setup DI

```csharp
var builder = WebApplication.CreateBuilder(args);

// Register DbContext.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

// Register Eventra + MediatR.
builder.Services.AddEventra(
    typeof(Program).Assembly,                    // assembly Anda (berisi handler)
    typeof(ProductCreatedEvent).Assembly);       // assembly event

var app = builder.Build();
app.Run();
```

### 2. Definisikan Event

Gunakan `record` agar immutable, dan warisi `DomainEvent`:

```csharp
using Eventra.Core.BaseClasses;

public sealed record ProductCreatedEvent(
    Guid ProductId,
    string ProductName
) : DomainEvent;
```

### 3. Buat Entity

Warisi `Entity` (atau `AggregateRoot`):

```csharp
using Eventra.Core.BaseClasses;

public class Product : Entity
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;

    private Product() { } // EF Core

    public static Product Create(string name)
    {
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = name
        };

        // Raise event — akan di-dispatch setelah SaveChanges sukses.
        product.RaiseDomainEvent(new ProductCreatedEvent(product.Id, name));

        return product;
    }
}
```

### 4. Buat Handler

Implement `IDomainEventHandler<T>`:

```csharp
using Eventra.Core.Abstractions;
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

    public Task Handle(
        ProductCreatedEvent notification,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "📧 Produk {Name} dibuat. Kirim email ke admin...",
            notification.ProductName);

        return Task.CompletedTask;
    }
}
```

### 5. Simpan + Dispatch

```csharp
using Eventra.Core.Abstractions;
using Eventra.EntityFrameworkCore.Extensions;

public sealed class CreateProductCommandHandler
    : IRequestHandler<CreateProductCommand, Guid>
{
    private readonly AppDbContext _dbContext;
    private readonly IDomainEventDispatcher _dispatcher;

    public CreateProductCommandHandler(
        AppDbContext dbContext,
        IDomainEventDispatcher dispatcher)
    {
        _dbContext = dbContext;
        _dispatcher = dispatcher;
    }

    public async Task<Guid> Handle(
        CreateProductCommand request,
        CancellationToken cancellationToken)
    {
        var product = Product.Create(request.Name);
        _dbContext.Products.Add(product);

        // Commit + dispatch event otomatis.
        await _dbContext.SaveChangesAndDispatchEventsAsync(
            _dispatcher,
            cancellationToken);

        return product.Id;
    }
}
```

**Selesai!** Event akan otomatis di-dispatch ke semua handler setelah commit.

---

## 📚 Documentation

### Konsep Dasar

| Konsep | Penjelasan |
| :--- | :--- |
| **Domain Event** | Sesuatu yang terjadi di domain, yang menarik untuk diketahui. |
| **Entity** | Objek domain yang punya identitas unik. |
| **Aggregate Root** | Entity utama yang jadi pintu masuk aggregate. |
| **Handler** | Kode yang bereaksi terhadap event. |
| **Dispatcher** | Yang mengirim event ke handler. |

### Arsitektur

```
┌─────────────────────────────────────────────────────────────┐
│                     Command Handler                         │
│  ┌───────────────────────────────────────────────────────┐  │
│  │ 1. product = Product.Create(name)                     │  │
│  │ 2. dbContext.Products.Add(product)                    │  │
│  │ 3. dbContext.SaveChangesAndDispatchEventsAsync(...)   │  │
│  └───────────────────────────────────────────────────────┘  │
└──────────────────────────┬──────────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────────────┐
│              DbContextExtensions                            │
│  ┌───────────────────────────────────────────────────────┐  │
│  │ 1. Kumpulkan entity yang punya DomainEvents           │  │
│  │ 2. Snapshot event-nya ke list                         │  │
│  │ 3. SaveChangesAsync() ──────► Database                │  │
│  │ 4. ClearDomainEvents() di entity                      │  │
│  │ 5. dispatcher.DispatchAsync(events)                   │  │
│  └───────────────────────────────────────────────────────┘  │
└──────────────────────────┬──────────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────────────┐
│              DomainEventDispatcher                          │
│  ┌───────────────────────────────────────────────────────┐  │
│  │ foreach event:                                        │  │
│  │   try { mediator.Publish(event) }                     │  │
│  │   catch { logger.LogError(...) }  ← tidak rethrow     │  │
│  └───────────────────────────────────────────────────────┘  │
└──────────────────────────┬──────────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────────────┐
│                        MediatR                              │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────┐ │
│  │ Handler A       │  │ Handler B       │  │ Handler C   │ │
│  │ (Send Email)    │  │ (Audit Log)     │  │ (Update     │ │
│  │                 │  │                 │  │  Cache)     │ │
│  └─────────────────┘  └─────────────────┘  └─────────────┘ │
└─────────────────────────────────────────────────────────────┘
```

### Struktur Project

```
Eventra/
 ┣ 📂 src
 ┃ ┣ 📂 Eventra.Core                    ← Abstraksi, base class, dispatcher
 ┃ ┗ 📂 Eventra.EntityFrameworkCore     ← Integrasi EF Core
 ┣ 📂 samples
 ┃ ┗ 📂 Eventra.SampleApp               ← Contoh console app
 ┣ 📂 tests
 ┃ ┗ 📂 Eventra.Core.Tests              ← Unit test
 ┣ 📜 Eventra.sln
 ┗ 📜 README.md
```

### Keputusan Desain Penting

| Keputusan | Alasan |
| :--- | :--- |
| Event di-snapshot **sebelum** `SaveChanges` | Agar event tidak terpengaruh oleh perubahan state EF Core setelah commit. |
| Event di-clear **setelah** `SaveChanges` | Jika `SaveChanges` gagal, event tetap tersimpan di entity (untuk potensi retry di masa depan). |
| Dispatch **setelah** commit | Memastikan handler hanya jalan kalau data benar-benar tersimpan. |
| Error di handler **di-catch** | Transaksi utama sudah commit; error di efek samping tidak boleh menggagalkan operasi bisnis. |
| Dispatcher di-register sebagai **Scoped** | Karena `IMediator` juga Scoped, dan dispatcher bergantung padanya. |
| Event pakai `record` | Immutable, value-based equality, cocok untuk event. |
| `Core` dan `EFCore` dipisah | Biar `Core` tidak terkunci ke EF Core. Bisa dipakai dengan Dapper, NHibernate, dll. |

---

## 🎮 Samples

Lihat folder [`samples/`](samples) untuk contoh lengkap.

### Menjalankan Sample

```bash
cd samples/Eventra.SampleApp
dotnet run
```

**Output:**

```
=== Membuat produk pertama ===
info: Eventra.Core.Dispatchers.DomainEventDispatcher[0]
      Dispatching domain event: ProductCreatedEvent (Id: ...)
info: Eventra.SampleApp.SendEmailOnProductCreatedHandler[0]
      📧 [EMAIL] Produk 'Kopi Arabica' (Id: ...) baru dibuat. Kirim email...
info: Eventra.SampleApp.LogOnProductCreatedHandler[0]
      📝 [AUDIT LOG] Produk 'Kopi Arabica' dibuat pada ... UTC.
Produk dibuat dengan Id: ...

=== Membuat produk kedua ===
...
```

---

## 🧪 Testing

```bash
dotnet test
```

Unit test mencakup:
1. Dispatch 1 event ke 1 handler.
2. Dispatch 1 event ke multiple handler.
3. Error di handler tidak menggagalkan dispatch.

---

## ⚠️ Non-Goals

Eventra **tidak** mendukung (dan tidak akan mendukung):

- ❌ Event Sourcing (menyimpan semua event di database)
- ❌ Outbox Pattern
- ❌ Distributed Event (Kafka, RabbitMQ)
- ❌ Event versioning
- ❌ Retry otomatis

Fokus pada **in-process domain event** yang sederhana dan mudah dipahami.

Kalau butuh fitur di atas, gunakan library khusus seperti:
- [EventFlow](https://github.com/eventflow/EventFlow) — Event Sourcing
- [Marten](https://martendb.io/) — Event Sourcing di PostgreSQL
- [MassTransit](https://masstransit.io/) — Distributed messaging

---

## 🤝 Contributing

Kontribusi selalu diterima! Silakan:

1. Fork repository ini.
2. Buat branch fitur (`git checkout -b feature/AmazingFeature`).
3. Commit perubahan (`git commit -m 'Add some AmazingFeature'`).
4. Push ke branch (`git push origin feature/AmazingFeature`).
5. Buat Pull Request.

Lihat [CONTRIBUTING.md](.github/CONTRIBUTING.md) untuk detail.

---

## 📋 Requirements

| Requirement | Versi |
| :--- | :--- |
| **.NET** | 8.0 atau 10.0 |
| **MediatR** | ≥ 12.0 |
| **EF Core** | ≥ 8.0 (opsional, hanya untuk `Eventra.EntityFrameworkCore`) |

---

## 📄 License

Distributed under the **MIT License**. Lihat [LICENSE](LICENSE) untuk detail.

---

## 📚 Referensi

- [Domain Events by Martin Fowler](https://martinfowler.com/eaaDev/DomainEvent.html)
- [MediatR GitHub](https://github.com/jbogard/MediatR)
- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [EF Core Docs](https://learn.microsoft.com/en-us/ef/core/)

---

## 🙏 Acknowledgements

- [Jimmy Bogard](https://github.com/jbogard) — MediatR
- [Martin Fowler](https://martinfowler.com/) — Domain Events concept
- [Robert C. Martin](https://blog.cleancoder.com/) — Clean Architecture

---

<div align="center">

**Made with ❤️ for the .NET community.**

[⬆ Back to Top](#-eventra)

</div>