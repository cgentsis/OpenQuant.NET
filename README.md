# OpenQuant.NET

OpenQuant.NET is an open‑source .NET library providing unified financial data access, quantitative analysis tools, and extensible market‑data integrations.

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

## Features

- **Unified data model** – `Quote` record for OHLCV price data
- **Provider abstraction** – `IMarketDataProvider` interface to plug in any market‑data source
- **Technical analysis** – Built‑in indicators (SMA, with more planned)

## Getting Started

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download) or later

### Build

```bash
dotnet build
```

### Test

```bash
dotnet test
```

## Project Structure

```
OpenQuant.NET/
├── src/
│   └── OpenQuant.NET/          # Core library
│       ├── Models/              # Data models (Quote, etc.)
│       ├── Analysis/            # Technical analysis indicators
│       └── IMarketDataProvider.cs
├── tests/
│   └── OpenQuant.NET.Tests/    # Unit tests (xUnit)
├── Directory.Build.props        # Shared build settings
└── OpenQuant.NET.sln
```

## License

This project is licensed under the MIT License — see the [LICENSE](LICENSE) file for details.
