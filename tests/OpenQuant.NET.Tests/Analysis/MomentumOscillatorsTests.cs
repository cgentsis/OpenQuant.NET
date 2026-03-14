using System.Threading.Tasks.Dataflow;
using OpenQuant.Analysis;
using OpenQuant.Models;

namespace OpenQuant.Tests.Analysis;

public class MomentumOscillatorsTests
{
    private static readonly DateTimeOffset BaseTime = new(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Apo_WithValidPeriods_ReturnsExpectedValues()
    {
        var results = new List<EnrichedCandle>();
        var apo = MomentumOscillators.Apo("APO", 2, 3);
        var sink = CreateSink(results);
        apo.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        var candles = new[]
        {
            Enrich(10m, 0),
            Enrich(20m, 1),
            Enrich(30m, 2),
            Enrich(40m, 3),
            Enrich(50m, 4),
        };

        foreach (var candle in candles)
        {
            await apo.SendAsync(candle);
        }

        apo.Complete();
        await sink.Completion;

        Assert.False(results[0].Indicators.ContainsKey("APO"));
        Assert.False(results[1].Indicators.ContainsKey("APO"));
        Assert.Equal(5m, Math.Round(results[2].Indicators["APO"], 2));
        Assert.Equal(5m, Math.Round(results[4].Indicators["APO"], 2));
    }

    [Fact]
    public async Task Ppo_WithValidPeriods_ReturnsExpectedValues()
    {
        var results = new List<EnrichedCandle>();
        var ppo = MomentumOscillators.Ppo("PPO", 2, 3);
        var sink = CreateSink(results);
        ppo.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        var candles = new[]
        {
            Enrich(10m, 0),
            Enrich(20m, 1),
            Enrich(30m, 2),
            Enrich(40m, 3),
            Enrich(50m, 4),
        };

        foreach (var candle in candles)
        {
            await ppo.SendAsync(candle);
        }

        ppo.Complete();
        await sink.Completion;

        Assert.False(results[0].Indicators.ContainsKey("PPO"));
        Assert.False(results[1].Indicators.ContainsKey("PPO"));
        Assert.Equal(25m, Math.Round(results[2].Indicators["PPO"], 2));
        Assert.Equal(16.67m, Math.Round(results[3].Indicators["PPO"], 2));
        Assert.Equal(12.5m, Math.Round(results[4].Indicators["PPO"], 2));
    }

    [Fact]
    public async Task Macd_WithValidPeriods_ReturnsExpectedOutputs()
    {
        var results = new List<EnrichedCandle>();
        var macd = MomentumOscillators.Macd("MACD", 3, 5, 3);
        var sink = CreateSink(results);
        macd.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        var candles = new[]
        {
            Enrich(10m, 0),
            Enrich(20m, 1),
            Enrich(30m, 2),
            Enrich(40m, 3),
            Enrich(50m, 4),
            Enrich(60m, 5),
            Enrich(70m, 6),
            Enrich(80m, 7),
            Enrich(90m, 8),
            Enrich(100m, 9),
        };

        foreach (var candle in candles)
        {
            await macd.SendAsync(candle);
        }

        macd.Complete();
        await sink.Completion;

        Assert.False(results[3].Indicators.ContainsKey("MACD_MACD"));
        Assert.True(results[4].Indicators.ContainsKey("MACD_MACD"));
        Assert.False(results[4].Indicators.ContainsKey("MACD_Signal"));
        Assert.False(results[5].Indicators.ContainsKey("MACD_Signal"));
        Assert.Equal(10m, Math.Round(results[6].Indicators["MACD_MACD"], 2));
        Assert.Equal(10m, Math.Round(results[6].Indicators["MACD_Signal"], 2));
        Assert.Equal(0m, Math.Round(results[6].Indicators["MACD_Hist"], 2));
        Assert.Equal(10m, Math.Round(results[9].Indicators["MACD_MACD"], 2));
    }

    [Fact]
    public async Task Rsi_WithValidPeriod_ReturnsBoundedValues()
    {
        var results = new List<EnrichedCandle>();
        var rsi = MomentumOscillators.Rsi("RSI", 3);
        var sink = CreateSink(results);
        rsi.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        var candles = new[]
        {
            Enrich(44m, 0),
            Enrich(44.34m, 1),
            Enrich(44.09m, 2),
            Enrich(43.61m, 3),
            Enrich(44.33m, 4),
            Enrich(44.83m, 5),
            Enrich(45.10m, 6),
            Enrich(44.02m, 7),
            Enrich(44.17m, 8),
            Enrich(43.56m, 9),
        };

        foreach (var candle in candles)
        {
            await rsi.SendAsync(candle);
        }

        rsi.Complete();
        await sink.Completion;

        Assert.False(results[0].Indicators.ContainsKey("RSI"));
        Assert.False(results[1].Indicators.ContainsKey("RSI"));
        Assert.False(results[2].Indicators.ContainsKey("RSI"));
        Assert.Equal(31.78m, Math.Round(results[3].Indicators["RSI"], 2));

        for (var i = 3; i < results.Count; i++)
        {
            Assert.True(results[i].Indicators.ContainsKey("RSI"));
            Assert.InRange(results[i].Indicators["RSI"], 0m, 100m);
        }
    }

    [Fact]
    public async Task Cmo_WithAllUpMoves_ReturnsOneHundred()
    {
        var results = new List<EnrichedCandle>();
        var cmo = MomentumOscillators.Cmo("CMO", 3);
        var sink = CreateSink(results);
        cmo.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        var candles = new[]
        {
            Enrich(10m, 0),
            Enrich(12m, 1),
            Enrich(14m, 2),
            Enrich(16m, 3),
            Enrich(18m, 4),
        };

        foreach (var candle in candles)
        {
            await cmo.SendAsync(candle);
        }

        cmo.Complete();
        await sink.Completion;

        Assert.False(results[0].Indicators.ContainsKey("CMO"));
        Assert.False(results[1].Indicators.ContainsKey("CMO"));
        Assert.False(results[2].Indicators.ContainsKey("CMO"));
        Assert.Equal(100m, results[3].Indicators["CMO"]);
        Assert.Equal(100m, results[4].Indicators["CMO"]);
    }

    [Fact]
    public async Task Cci_WithTypicalPriceEqualToClose_ReturnsExpectedValue()
    {
        var results = new List<EnrichedCandle>();
        var cci = MomentumOscillators.Cci("CCI", 3);
        var sink = CreateSink(results);
        cci.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        var candles = new[]
        {
            MakeCandle(10m, 10m, 10m, 10m, dayOffset: 0),
            MakeCandle(20m, 20m, 20m, 20m, dayOffset: 1),
            MakeCandle(30m, 30m, 30m, 30m, dayOffset: 2),
        };

        foreach (var candle in candles)
        {
            await cci.SendAsync(candle);
        }

        cci.Complete();
        await sink.Completion;

        Assert.Equal(100m, results[2].Indicators["CCI"]);
    }

    [Fact]
    public async Task WillR_WithValidPeriod_ReturnsExpectedValue()
    {
        var results = new List<EnrichedCandle>();
        var willR = MomentumOscillators.WillR("WillR", 3);
        var sink = CreateSink(results);
        willR.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        var candles = new[]
        {
            MakeCandle(12m, 20m, 10m, 12m, dayOffset: 0),
            MakeCandle(18m, 22m, 11m, 18m, dayOffset: 1),
            MakeCandle(16m, 24m, 14m, 16m, dayOffset: 2),
        };

        foreach (var candle in candles)
        {
            await willR.SendAsync(candle);
        }

        willR.Complete();
        await sink.Completion;

        Assert.False(results[0].Indicators.ContainsKey("WillR"));
        Assert.False(results[1].Indicators.ContainsKey("WillR"));
        Assert.Equal(-57.14m, Math.Round(results[2].Indicators["WillR"], 2));
    }

    [Fact]
    public async Task Aroon_WithValidPeriod_ReturnsExpectedOutputs()
    {
        var results = new List<EnrichedCandle>();
        var aroon = MomentumOscillators.Aroon("Aroon", 3);
        var sink = CreateSink(results);
        aroon.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        var candles = new[]
        {
            MakeCandle(8m, 10m, 5m, 8m, dayOffset: 0),
            MakeCandle(15m, 20m, 10m, 15m, dayOffset: 1),
            MakeCandle(12m, 15m, 8m, 12m, dayOffset: 2),
            MakeCandle(20m, 25m, 12m, 20m, dayOffset: 3),
        };

        foreach (var candle in candles)
        {
            await aroon.SendAsync(candle);
        }

        aroon.Complete();
        await sink.Completion;

        Assert.False(results[0].Indicators.ContainsKey("Aroon_Up"));
        Assert.False(results[1].Indicators.ContainsKey("Aroon_Up"));
        Assert.False(results[2].Indicators.ContainsKey("Aroon_Up"));
        Assert.Equal(100m, results[3].Indicators["Aroon_Up"]);
        Assert.Equal(0m, results[3].Indicators["Aroon_Down"]);
    }

    [Fact]
    public async Task AroonOsc_WithValidPeriod_ReturnsExpectedValue()
    {
        var results = new List<EnrichedCandle>();
        var aroonOsc = MomentumOscillators.AroonOsc("AroonOsc", 3);
        var sink = CreateSink(results);
        aroonOsc.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        var candles = new[]
        {
            MakeCandle(8m, 10m, 5m, 8m, dayOffset: 0),
            MakeCandle(15m, 20m, 10m, 15m, dayOffset: 1),
            MakeCandle(12m, 15m, 8m, 12m, dayOffset: 2),
            MakeCandle(20m, 25m, 12m, 20m, dayOffset: 3),
        };

        foreach (var candle in candles)
        {
            await aroonOsc.SendAsync(candle);
        }

        aroonOsc.Complete();
        await sink.Completion;

        Assert.Equal(100m, results[3].Indicators["AroonOsc"]);
    }

    [Fact]
    public void Rsi_WithPeriodLessThanOne_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            MomentumOscillators.Rsi("RSI", 0));
    }

    [Fact]
    public void Apo_WithFastPeriodLessThanOne_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            MomentumOscillators.Apo("APO", 0, 3));
    }

    private static ActionBlock<EnrichedCandle> CreateSink(List<EnrichedCandle> results) =>
        new(item => results.Add(item));

    private static EnrichedCandle Enrich(decimal close, int dayOffset = 0) => new(new Candle
    {
        Timestamp = BaseTime.AddDays(dayOffset),
        Open = close,
        High = close,
        Low = close,
        Close = close,
        Volume = 1000,
    });

    private static EnrichedCandle MakeCandle(
        decimal open,
        decimal high,
        decimal low,
        decimal close,
        long volume = 1000,
        int dayOffset = 0) => new(new Candle
    {
        Timestamp = BaseTime.AddDays(dayOffset),
        Open = open,
        High = high,
        Low = low,
        Close = close,
        Volume = volume,
    });
}
