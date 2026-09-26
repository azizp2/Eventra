<div align="center">

# ⚡ Eventra

**Domain Events for .NET — Simple, Clean, Zero Dependency.**

[![NuGet](https://img.shields.io/nuget/v/Eventra.Core.svg)](https://www.nuget.org/packages/Eventra.Core)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Eventra.Core.svg?style=flat-square&logo=nuget)](https://www.nuget.org/packages/Eventra.Core)
[![License](https://img.shields.io/badge/license-MIT-green)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?style=flat-square&logo=dotnet)](https://dotnet.microsoft.com)

[Getting Started](#-getting-started) •
[Packages](#-packages) •
[Documentation](https://azizp2.github.io/EventraDocs/) •
[Samples](example/Eventra.Example) •
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

📖 **Dokumentasi lengkap:** [azizp2.github.io/EventraDocs](https://azizp2.github.io/EventraDocs/)

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
| ✅ **Built-in Dispatcher** | Dispatcher bawaan berbasis **Delegate Caching** — performa tinggi tanpa refleksi berulang pada hot-path. |
| ✅ **Simple API** | Cukup panggil `RaiseDomainEvent(...)` di dalam entity — selesai. |
| ✅ **Immutable Events** | Menggunakan `record` C#, aman dari mutasi tak disengaja. |
| ✅ **Auto Dispatch** | Event otomatis dikirim setelah `SaveChanges` sukses. |
| ✅ **Multi-Handler** | Satu event bisa memiliki banyak handler terpisah. |
| ✅ **Atomic per Event** | Error di salah satu handler tidak menggagalkan event lain. |
| ✅ **DI-Friendly & Anti-Duplicate** | Assembly scanning aman dari duplikasi dengan dukungan opsi `ServiceLifetime`. |
| ✅ **EF Core Integration** | Extension method `SaveChangesAndDispatchEventsAsync`. |
| ✅ **Framework-Agnostic** | Bisa dipakai tanpa EF Core (Dapper, ADO.NET, NHibernate, dll). |

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