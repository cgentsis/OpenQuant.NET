# OpenQuant.NET

OpenQuant.NET is an open‑source .NET library providing unified financial data access, quantitative analysis tools, and extensible market‑data integrations.

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

## Features

- **Unified data model** – `Candle` record for OHLCV (Open, High, Low, Close, Volume) price data
- **Provider abstraction** – `IMarketDataProvider` interface to plug in any market‑data source
- **Yahoo Finance provider** – Built‑in provider for retrieving historical and latest candle data
- **Technical analysis** – 150+ indicators as chainable TPL Dataflow `TransformBlock` pipeline stages
  - Moving Averages: SMA, EMA, WMA, HMA, DEMA, TEMA, T3, TRIMA, KAMA, Median
  - Price Transforms: AVGPRICE, MEDPRICE, TYPPRICE, WCLPRICE, BOP, True Range
  - Rolling Window: SUM, MAX, MIN, MAXINDEX, MININDEX, MINMAX, MINMAXINDEX, MIDPOINT, MIDPRICE, MOM, ROC, ROCP, ROCR, ROCR100
  - Statistics: VAR, STDDEV, LINEARREG, LINEARREG_ANGLE, LINEARREG_INTERCEPT, LINEARREG_SLOPE, TSF, CORREL, BETA
  - Momentum: APO, PPO, MACD, RSI, CMO, CCI, Williams %R, Ultimate Oscillator, TRIX, Aroon, Aroon Oscillator
  - Directional Movement: +DM, −DM, +DI, −DI, DX, ADX, ADXR
  - Volatility: ATR, NATR, Bollinger Bands
  - Volume: OBV, Chaikin A/D, ADOSC, MFI
  - Stochastic: STOCH, STOCHF, StochRSI
  - Advanced: Parabolic SAR, Hilbert Transform (Trendline, DC Period, DC Phase, Phasor, Sine, Trend Mode)
  - Candlestick Patterns: 30+ patterns (Doji, Hammer, Engulfing, Harami, Morning/Evening Star, Three White Soldiers, Three Black Crows, and more)
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
// Moving Averages
var sma   = MovingAverage.SMA("SMA20", period: 20);
var ema   = MovingAverage.EMA("EMA12", period: 12);
var wma   = MovingAverage.WMA("WMA10", period: 10);
var hma   = MovingAverage.HMA("HMA16", period: 16);
var dema  = MovingAverage.DEMA("DEMA20", period: 20);
var tema  = MovingAverage.TEMA("TEMA20", period: 20);
var t3    = MovingAverage.T3("T3_5", period: 5);
var trima = MovingAverage.TRIMA("TRIMA20", period: 20);
var kama  = MovingAverage.KAMA("KAMA10", period: 10);
var median = MovingMedian.Median("Med5", period: 5);

// Price Transforms
var avgp = PriceTransform.AvgPrice("AVGPRICE");
var medp = PriceTransform.MedPrice("MEDPRICE");
var typp = PriceTransform.TypPrice("TYPPRICE");
var wclp = PriceTransform.WclPrice("WCLPRICE");
var bop  = PriceTransform.Bop("BOP");
var tr   = PriceTransform.TrueRange("TR");

// Momentum Oscillators
var macd = MomentumOscillators.Macd("MACD");          // outputs _MACD, _Signal, _Hist
var rsi  = MomentumOscillators.Rsi("RSI", period: 14);
var cci  = MomentumOscillators.Cci("CCI", period: 20);
var willr = MomentumOscillators.WillR("WILLR", period: 14);
var aroon = MomentumOscillators.Aroon("AROON", period: 25);  // outputs _Up, _Down

// Volatility
var atr    = VolatilityIndicators.Atr("ATR", period: 14);
var bbands = VolatilityIndicators.BBands("BB", period: 20, nbDev: 2);  // outputs _Upper, _Middle, _Lower

// Volume
var obv = VolumeIndicators.Obv("OBV");
var mfi = VolumeIndicators.Mfi("MFI", period: 14);

// Stochastic
var stoch = StochasticIndicators.Stoch("STOCH");  // outputs _K, _D

// Candlestick Patterns (100 = bullish, −100 = bearish)
var doji      = CandlestickPatterns.Doji("DOJI");
var engulfing = CandlestickPatterns.Engulfing("ENGULF");
var hammer    = CandlestickPatterns.Hammer("HAMMER");
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
│       │   ├── MovingAverage.cs              # SMA, EMA, WMA, HMA, DEMA, TEMA, T3, TRIMA, KAMA
│       │   ├── MovingMedian.cs               # Moving Median
│       │   ├── PriceTransform.cs             # AVGPRICE, MEDPRICE, TYPPRICE, WCLPRICE, BOP, TrueRange
│       │   ├── RollingWindow.cs              # SUM, MAX, MIN, MOM, ROC, MIDPOINT, MIDPRICE, etc.
│       │   ├── StatisticFunctions.cs         # VAR, STDDEV, LINEARREG, TSF, CORREL, BETA
│       │   ├── MomentumOscillators.cs        # APO, PPO, MACD, RSI, CMO, CCI, WILLR, TRIX, Aroon
│       │   ├── DirectionalMovement.cs        # +DM, −DM, +DI, −DI, DX, ADX, ADXR
│       │   ├── VolatilityIndicators.cs       # ATR, NATR, Bollinger Bands
│       │   ├── VolumeIndicators.cs           # OBV, AD, ADOSC, MFI
│       │   ├── StochasticIndicators.cs       # STOCH, STOCHF, StochRSI
│       │   ├── AdvancedIndicators.cs         # SAR, Hilbert Transform family
│       │   └── CandlestickPatterns.cs        # 30+ candlestick pattern recognizers
│       ├── Providers/
│       │   └── YahooFinance/
│       │       ├── YahooFinanceProvider.cs   # Yahoo Finance v8 chart API client
│       │       └── Dto/                      # Internal JSON deserialization models
│       └── IMarketDataProvider.cs            # Provider abstraction interface
├── tests/
│   └── OpenQuant.NET.Tests/                  # xUnit tests
│       ├── Analysis/                         # Unit tests for all indicators
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
| `GetCandlesAsync(symbol, asOf, count)` | `Task<IReadOnlyList<Candle>>` | Retrieve the last N trading-day candles up to a date |
| `GetLatestCandleAsync(symbol)` | `Task<Candle?>` | Retrieve the most recent candle |

### `MovingAverage`

| Method | Description |
|---|---|
| `SMA(name, period)` | Simple moving average over `period` closing prices |
| `EMA(name, period)` | Exponential moving average; seeds with SMA, smoothing factor `2/(period+1)` |
| `WMA(name, period)` | Weighted moving average with linearly increasing weights |
| `HMA(name, period)` | Hull moving average — period ≥ 2 |
| `DEMA(name, period)` | Double exponential moving average: `2×EMA − EMA(EMA)` |
| `TEMA(name, period)` | Triple exponential moving average |
| `T3(name, period, volumeFactor)` | T3 triple-smoothed EMA with volume factor (default 0.7) |
| `TRIMA(name, period)` | Triangular moving average (SMA of SMA) — period ≥ 2 |
| `KAMA(name, period)` | Kaufman adaptive moving average — period ≥ 2 |

### `MovingMedian`

| Method | Description |
|---|---|
| `Median(name, period)` | Moving median; averages two middle values for even periods |

### `PriceTransform`

| Method | Description |
|---|---|
| `AvgPrice(name)` | Average price: (O+H+L+C)/4 |
| `MedPrice(name)` | Median price: (H+L)/2 |
| `TypPrice(name)` | Typical price: (H+L+C)/3 |
| `WclPrice(name)` | Weighted close: (H+L+2C)/4 |
| `Bop(name)` | Balance of Power: (C−O)/(H−L) |
| `TrueRange(name)` | True Range (lookback 1) |

### `RollingWindow`

| Method | Description |
|---|---|
| `Sum(name, period)` | Rolling sum |
| `Max(name, period)` / `Min(name, period)` | Rolling max / min |
| `MaxIndex(name, period)` / `MinIndex(name, period)` | Index of max / min in window |
| `MinMax(name, period)` | Both min and max (outputs `_Min`, `_Max`) |
| `MinMaxIndex(name, period)` | Indexes of both (outputs `_MinIdx`, `_MaxIdx`) |
| `MidPoint(name, period)` | (max close + min close) / 2 |
| `MidPrice(name, period)` | (highest high + lowest low) / 2 |
| `Mom(name, period)` | Momentum: close − close[n ago] |
| `Roc` / `Rocp` / `Rocr` / `Rocr100` | Rate of change variants |

### `StatisticFunctions`

| Method | Description |
|---|---|
| `Var(name, period)` | Population variance |
| `StdDev(name, period, nbDev)` | Standard deviation × nbDev |
| `LinearReg(name, period)` | Linear regression end value |
| `LinearRegAngle` / `LinearRegSlope` / `LinearRegIntercept` | Regression components |
| `Tsf(name, period)` | Time series forecast (regression + 1 step) |
| `Correl(name, period, seriesA, seriesB)` | Pearson correlation between two series |
| `Beta(name, period, asset, benchmark)` | Beta coefficient |

### `MomentumOscillators`

| Method | Description |
|---|---|
| `Apo(name, fast, slow)` | Absolute Price Oscillator |
| `Ppo(name, fast, slow)` | Percentage Price Oscillator |
| `Macd(name, fast, slow, signal)` | MACD (outputs `_MACD`, `_Signal`, `_Hist`) |
| `Rsi(name, period)` | Relative Strength Index (Wilder's smoothing) |
| `Cmo(name, period)` | Chande Momentum Oscillator |
| `Cci(name, period)` | Commodity Channel Index |
| `WillR(name, period)` | Williams %R |
| `UltOsc(name, p1, p2, p3)` | Ultimate Oscillator |
| `Trix(name, period)` | 1-day ROC of triple-smoothed EMA |
| `Aroon(name, period)` | Aroon (outputs `_Up`, `_Down`) |
| `AroonOsc(name, period)` | Aroon Oscillator |

### `DirectionalMovement`

| Method | Description |
|---|---|
| `PlusDM` / `MinusDM` | Smoothed directional movement |
| `PlusDI` / `MinusDI` | Directional indicators |
| `Dx(name, period)` | Directional Movement Index |
| `Adx(name, period)` | Average Directional Index |
| `Adxr(name, period)` | ADX Rating |

### `VolatilityIndicators`

| Method | Description |
|---|---|
| `Atr(name, period)` | Average True Range |
| `Natr(name, period)` | Normalized ATR: (ATR/Close)×100 |
| `BBands(name, period, nbDev)` | Bollinger Bands (outputs `_Upper`, `_Middle`, `_Lower`) |

### `VolumeIndicators`

| Method | Description |
|---|---|
| `Obv(name)` | On Balance Volume |
| `Ad(name)` | Chaikin A/D Line |
| `AdOsc(name, fast, slow)` | Chaikin A/D Oscillator |
| `Mfi(name, period)` | Money Flow Index |

### `StochasticIndicators`

| Method | Description |
|---|---|
| `Stoch(name, fastK, slowK, slowD)` | Stochastic Oscillator (outputs `_K`, `_D`) |
| `StochF(name, fastK, fastD)` | Fast Stochastic |
| `StochRsi(name, rsi, stoch, k, d)` | Stochastic RSI |

### `AdvancedIndicators`

| Method | Description |
|---|---|
| `Sar(name, accel, max)` | Parabolic SAR |
| `HtTrendline(name)` | Hilbert Transform - Instantaneous Trendline |
| `HtDcPeriod(name)` | Hilbert Transform - Dominant Cycle Period |
| `HtDcPhase(name)` | Hilbert Transform - Dominant Cycle Phase |
| `HtPhasor(name)` | Phasor Components (outputs `_InPhase`, `_Quadrature`) |
| `HtSine(name)` | SineWave (outputs `_Sine`, `_LeadSine`) |
| `HtTrendMode(name)` | Trend vs Cycle Mode (1 or 0) |

### `CandlestickPatterns`

59 pattern recognizers including: `Doji`, `DragonflyDoji`, `GravestoneDoji`, `LongLeggedDoji`, `Hammer`, `InvertedHammer`, `HangingMan`, `ShootingStar`, `Marubozu`, `ClosingMarubozu`, `SpinningTop`, `Engulfing`, `Harami`, `HaramiCross`, `Piercing`, `DarkCloudCover`, `MorningStar`, `EveningStar`, `MorningDojiStar`, `EveningDojiStar`, `ThreeWhiteSoldiers`, `ThreeBlackCrows`, `LongLine`, `ShortLine`, `DojiStar`, `BeltHold`, `Kicking`, `MatchingLow`, `HomingPigeon`, `Counterattack`, `RickshawMan`, `HighWave`, `Tristar`, `StickSandwich`, `Thrusting`, `InNeck`, `OnNeck`, `SeparatingLines`, `TwoCrows`, `ThreeInside`, `ThreeLineStrike`, `ThreeStarsInSouth`, `AbandonedBaby`, `AdvanceBlock`, `Breakaway`, `ConcealingBabySwallow`, `GapSideSideWhite`, `Hikkake`, `HikkakeMod`, `IdenticalThreeCrows`, `KickingByLength`, `LadderBottom`, `MatHold`, `RisingFallingThreeMethods`, `StalledPattern`, `Takuri`, `TasukiGap`, `Unique3River`, `UpsideGap2Crows`, `XSideGap3Methods`.

### `YahooFinanceProvider`

Implements `IMarketDataProvider` using the Yahoo Finance v8 chart API. Accepts an `HttpClient` and an optional `baseUri` override for testing.

## License

This project is licensed under the MIT License — see the [LICENSE](LICENSE) file for details.
