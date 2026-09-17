# Eventra.Abstractions

**Abstractions for Eventra — Simple Domain Event framework for .NET.**

[![NuGet](https://img.shields.io/nuget/v/Eventra.Abstractions.svg)](https://www.nuget.org/packages/Eventra.Abstractions)
[![License](https://img.shields.io/badge/license-MIT-green)](LICENSE)

---

## 📖 Tentang

`Eventra.Abstractions` berisi **interface only** untuk Eventra —
**zero dependency**. Tidak ada MediatR, tidak ada EF Core, tidak ada
`Microsoft.Extensions.*`.

Cocok untuk:

- 🔌 **Consumer yang mau bikin custom dispatcher sendiri.**
- 🧩 **Library lain** yang mau refer ke Eventra tanpa bawa dependency besar.
- 📦 **Package paling ringan** untuk konsumsi interface saja.

---

## 📦 Isi Package

| Interface | Deskripsi |
| :--- | :--- |
| `IDomainEvent` | Marker untuk semua domain event. |
| `IDomainEventHandler<T>` | Handler untuk domain event tertentu. |
| `IDomainEventDispatcher` | Kontrak dispatcher. |
| `IHasDomainEvents` | Kontrak entity yang punya domain event pending. |

---

## 🚀 Instalasi

```bash
dotnet add package Eventra.Abstractions