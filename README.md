# OpenQuant.NET

OpenQuant.NET is an open‑source .NET library providing unified financial data access, quantitative analysis tools, and extensible market‑data integrations.

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

## Features

- **Unified data model** – `Candle` record for OHLCV (Open, High, Low, Close, Volume) price data
- **Provider abstraction** – `IMarketDataProvider` interface to plug in any market‑data source
- **Yahoo Finance provider** – Built‑in provider for retrieving historical and latest candle data
- **Technical analysis** – TPL Dataflow‑based indicators (SMA, with more planned)
- **Code quality** – Enforced via StyleCop Analyzers and SonarAnalyzer on every build

## Getting Started

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download) or later

### Build

```bash
dotnet build
```

### Test

```bash
# Run unit tests only
dotnet test --filter "Category!=Integration"

# Run integration tests only (calls live Yahoo Finance API)
dotnet test --filter "Category=Integration"

# Run all tests
dotnet test
```

## Usage

### Fetching Historical Candles

```csharp
using OpenQuant.Providers.YahooFinance;

using var httpClient = new HttpClient();
var provider = new YahooFinanceProvider(httpClient);

var candles = await provider.GetHistoricalCandlesAsync(
    "AAPL",
    from: new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero),
    to: DateTimeOffset.UtcNow);

foreach (var candle in candles)
{
    Console.WriteLine($"{candle.Timestamp:yyyy-MM-dd}  O:{candle.Open}  H:{candle.High}  L:{candle.Low}  C:{candle.Close}  V:{candle.Volume}");
}
```

### Getting the Latest Candle

```csharp
var latest = await provider.GetLatestCandleAsync("MSFT");
Console.WriteLine($"MSFT latest close: {latest?.Close}");
```

### Computing a Simple Moving Average (SMA)

The SMA indicator is built on [TPL Dataflow](https://learn.microsoft.com/en-us/dotnet/standard/parallel-programming/dataflow-task-parallel-library) `ActionBlock<T>`, making it composable in streaming pipelines.

```csharp
using System.Threading.Tasks.Dataflow;
using OpenQuant.Analysis;

// Create an output buffer and a 20‑period SMA block
var output = new BufferBlock<(DateTimeOffset Timestamp, decimal Value)>();
var sma = MovingAverage.SMAActionBlockFactory(period: 20, target: output);

// Feed candles into the block
foreach (var candle in candles)
{
    await sma.SendAsync(candle);
}

sma.Complete();
await sma.Completion;

// Read SMA results
while (output.TryReceive(out var point))
{
    Console.WriteLine($"{point.Timestamp:yyyy-MM-dd}  SMA(20): {point.Value:F2}");
}
```

### Implementing a Custom Data Provider

Implement `IMarketDataProvider` to integrate any data source:

```csharp
using OpenQuant;
using OpenQuant.Models;

public class MyCustomProvider : IMarketDataProvider
{
    public string Name => "My Provider";

    public Task<IReadOnlyList<Candle>> GetHistoricalCandlesAsync(
        string symbol, DateTimeOffset from, DateTimeOffset to,
        CancellationToken cancellationToken = default)
    {
        // Your implementation here
    }

    public Task<Candle?> GetLatestCandleAsync(
        string symbol, CancellationToken cancellationToken = default)
    {
        // Your implementation here
    }
}
```

## Project Structure

```
OpenQuant.NET/
├── src/
│   └── OpenQuant.NET/                        # Core library
│       ├── Models/
│       │   └── Candle.cs                     # OHLCV price record
│       ├── Analysis/
│       │   └── MovingAverage.cs              # SMA via TPL Dataflow ActionBlock
│       ├── Providers/
│       │   └── YahooFinance/
│       │       ├── YahooFinanceProvider.cs   # Yahoo Finance v8 chart API client
│       │       └── Dto/                      # Internal JSON deserialization models
│       └── IMarketDataProvider.cs            # Provider abstraction interface
├── tests/
│   └── OpenQuant.NET.Tests/                  # xUnit tests
│       ├── Analysis/                         # Unit tests for indicators
│       └── Providers/YahooFinance/
│           ├── YahooFinanceProviderTests.cs  # Unit tests (mocked HTTP)
│           └── Integration/                  # Integration tests (live API)
├── Directory.Build.props                     # Shared build settings & analyzers
├── .editorconfig                             # Code style rules
├── stylecop.json                             # StyleCop configuration
└── OpenQuant.NET.slnx                        # Solution file
```

## Code Quality

The project enforces code quality on every build via:

| Analyzer | Purpose |
|---|---|
| [StyleCop.Analyzers](https://github.com/DotNetAnalyzers/StyleCopAnalyzers) | Code style, ordering, documentation |
| [SonarAnalyzer.CSharp](https://github.com/SonarSource/sonar-dotnet) | Bug detection, security, maintainability |

Both are configured in `Directory.Build.props` with `TreatWarningsAsErrors=true`, so any violation fails the build. Suppressed rules (for modern C# compatibility) are documented in `.editorconfig`.

## API Reference

### `Candle`

| Property | Type | Description |
|---|---|---|
| `Timestamp` | `DateTimeOffset` | Date/time of the candle |
| `Open` | `decimal` | Opening price |
| `High` | `decimal` | Highest price |
| `Low` | `decimal` | Lowest price |
| `Close` | `decimal` | Closing price |
| `Volume` | `long` | Trading volume |

### `IMarketDataProvider`

| Member | Returns | Description |
|---|---|---|
| `Name` | `string` | Display name of the provider |
| `GetHistoricalCandlesAsync(symbol, from, to)` | `Task<IReadOnlyList<Candle>>` | Retrieve candles for a date range |
| `GetLatestCandleAsync(symbol)` | `Task<Candle?>` | Retrieve the most recent candle |

### `MovingAverage`

| Method | Returns | Description |
|---|---|---|
| `SMAActionBlockFactory(period, target)` | `ActionBlock<Candle>` | Creates a Dataflow block computing the simple moving average |

### `YahooFinanceProvider`

Implements `IMarketDataProvider` using the Yahoo Finance v8 chart API. Accepts an `HttpClient` and an optional `baseUri` override for testing.

## License

This project is licensed under the MIT License — see the [LICENSE](LICENSE) file for details.
