# Eventra.EntityFrameworkCore

**EF Core integration for Eventra — Save changes and dispatch domain events in one call.**

[![NuGet](https://img.shields.io/nuget/v/Eventra.EntityFrameworkCore.svg)](https://www.nuget.org/packages/Eventra.EntityFrameworkCore)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Eventra.EntityFrameworkCore.svg)](https://www.nuget.org/packages/Eventra.EntityFrameworkCore)
[![License](https://img.shields.io/badge/license-MIT-green)](LICENSE)

---

## 📖 Tentang

Package ini menyediakan **satu extension method** untuk menyatukan
`SaveChangesAsync` dengan dispatch domain event:

```csharp
await _dbContext.SaveChangesAndDispatchEventsAsync(_dispatcher, cancellationToken);