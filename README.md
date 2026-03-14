# OpenQuant.NET

OpenQuant.NET is an open‑source .NET library providing unified financial data access, quantitative analysis tools, and extensible market‑data integrations.

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

## Features

- **Unified data model** – `Candle` record for OHLCV (Open, High, Low, Close, Volume) price data
- **Provider abstraction** – `IMarketDataProvider` interface to plug in any market‑data source
- **Yahoo Finance provider** – Built‑in provider for retrieving historical and latest candle data
- **Technical analysis** – TPL Dataflow‑based indicators (SMA, EMA, WMA, HMA, Moving Median)
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

### Computing Technical Indicators

All indicators are built on [TPL Dataflow](https://learn.microsoft.com/en-us/dotnet/standard/parallel-programming/dataflow-task-parallel-library) `TransformBlock<EnrichedCandle, EnrichedCandle>`, making them chainable pipeline stages via `LinkTo`.

```csharp
using System.Threading.Tasks.Dataflow;
using OpenQuant.Analysis;
using OpenQuant.Models;

// Create a pipeline: SMA(20) → EMA(12) → sink
var sma = MovingAverage.SMA("SMA20", period: 20);
var ema = MovingAverage.EMA("EMA12", period: 12);
var results = new List<EnrichedCandle>();
var sink = new ActionBlock<EnrichedCandle>(e => results.Add(e));

sma.LinkTo(ema, new DataflowLinkOptions { PropagateCompletion = true });
ema.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

// Feed candles — each one flows through every stage
foreach (var candle in candles)
{
    await sma.SendAsync(new EnrichedCandle(candle));
}

sma.Complete();
await sink.Completion;

// Each EnrichedCandle has the original candle + computed indicators
foreach (var e in results)
{
    Console.WriteLine($"{e.Candle.Timestamp:yyyy-MM-dd}  Close={e.Candle.Close}  {string.Join("  ", e.Indicators)}");
}
```

Available indicator factories:

```csharp
// Simple Moving Average
var sma = MovingAverage.SMA("SMA20", period: 20);

// Exponential Moving Average
var ema = MovingAverage.EMA("EMA12", period: 12);

// Weighted Moving Average
var wma = MovingAverage.WMA("WMA10", period: 10);

// Hull Moving Average (period must be ≥ 2)
var hma = MovingAverage.HMA("HMA16", period: 16);

// Moving Median
var median = MovingMedian.Median("Med5", period: 5);
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
│       │   ├── Candle.cs                     # OHLCV price record
│       │   └── EnrichedCandle.cs             # Candle enriched with indicator values
│       ├── Analysis/
│       │   ├── MovingAverage.cs              # SMA, EMA, WMA, HMA via TPL Dataflow
│       │   └── MovingMedian.cs               # Moving Median via TPL Dataflow
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

### `EnrichedCandle`

| Property | Type | Description |
|---|---|---|
| `Candle` | `Candle` | The original OHLCV candle |
| `Indicators` | `Dictionary<string, decimal>` | Computed indicator values keyed by name |

### `IMarketDataProvider`

| Member | Returns | Description |
|---|---|---|
| `Name` | `string` | Display name of the provider |
| `GetHistoricalCandlesAsync(symbol, from, to)` | `Task<IReadOnlyList<Candle>>` | Retrieve candles for a date range |
| `GetLatestCandleAsync(symbol)` | `Task<Candle?>` | Retrieve the most recent candle |

### `MovingAverage`

| Method | Returns | Description |
|---|---|---|
| `SMA(name, period)` | `TransformBlock<EnrichedCandle, EnrichedCandle>` | Simple moving average over `period` closing prices |
| `EMA(name, period)` | `TransformBlock<EnrichedCandle, EnrichedCandle>` | Exponential moving average; seeds with SMA, smoothing factor `2/(period+1)` |
| `WMA(name, period)` | `TransformBlock<EnrichedCandle, EnrichedCandle>` | Weighted moving average with linearly increasing weights |
| `HMA(name, period)` | `TransformBlock<EnrichedCandle, EnrichedCandle>` | Hull moving average — `WMA(√n)` of `2×WMA(n/2) − WMA(n)` — period ≥ 2 |

### `MovingMedian`

| Method | Returns | Description |
|---|---|---|
| `Median(name, period)` | `TransformBlock<EnrichedCandle, EnrichedCandle>` | Moving median; averages the two middle values for even periods |

### `YahooFinanceProvider`

Implements `IMarketDataProvider` using the Yahoo Finance v8 chart API. Accepts an `HttpClient` and an optional `baseUri` override for testing.

## License

This project is licensed under the MIT License — see the [LICENSE](LICENSE) file for details.
