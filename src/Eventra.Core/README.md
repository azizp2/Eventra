# Eventra.Core

**Simple Domain Event framework for .NET — built-in dispatcher, Zero external dependency.**

[![NuGet](https://img.shields.io/nuget/v/Eventra.Core.svg)](https://www.nuget.org/packages/Eventra.Core)
[![License](https://img.shields.io/badge/license-MIT-green)](LICENSE)

---

## ✨ Fitur

- ✅ **Zero external dependency** — hanya `Microsoft.Extensions.*`.
- ✅ **Built-in dispatcher** (IServiceProvider + reflection).
- ✅ **Immutable events** — pakai `record`.
- ✅ **Auto dispatch** — setelah `SaveChanges` (dengan EF Core package).
- ✅ **Multi-handler** — satu event bisa punya banyak handler.
- ✅ **Error isolation** — error handler tidak menggagalkan transaksi.
- ✅ **DI-friendly** — handler auto-register via assembly scanning.

---

## 🚀 Instalasi

```bash
dotnet add package Eventra.Core