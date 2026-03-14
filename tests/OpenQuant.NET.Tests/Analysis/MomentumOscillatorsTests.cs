using System.Threading.Tasks.Dataflow;
using OpenQuant.Analysis;
using OpenQuant.Models;

namespace OpenQuant.Tests.Analysis;

public class MomentumOscillatorsTests
{
    private static readonly DateTimeOffset BaseTime = new(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Apo_ProducesValueAfterWarmup()
    {
        var results = new List<EnrichedCandle>();
        var block = MomentumOscillators.Apo("APO", 3, 5);
        var sink = CreateSink(results);
        block.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        for (var i = 0; i < 10; i++)
        {
            var close = 100m + i;
            await block.SendAsync(MakeCandle(close - 1m, close + 1m, close - 2m, close, dayOffset: i));
        }

        block.Complete();
        await sink.Completion;

        for (var i = 0; i < 4; i++)
        {
            Assert.False(results[i].Indicators.ContainsKey("APO"));
        }

        Assert.True(results[4].Indicators.ContainsKey("APO"));
    }

    [Fact]
    public async Task Ppo_ProducesValueAfterWarmup()
    {
        var results = new List<EnrichedCandle>();
        var block = MomentumOscillators.Ppo("PPO", 3, 5);
        var sink = CreateSink(results);
        block.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        for (var i = 0; i < 10; i++)
        {
            var close = 100m + i;
            await block.SendAsync(MakeCandle(close - 1m, close + 1m, close - 2m, close, dayOffset: i));
        }

        block.Complete();
        await sink.Completion;

        for (var i = 0; i < 4; i++)
        {
            Assert.False(results[i].Indicators.ContainsKey("PPO"));
        }

        Assert.True(results[4].Indicators.ContainsKey("PPO"));
    }

    [Fact]
    public async Task Macd_StoresAllExpectedKeysAfterWarmup()
    {
        var results = new List<EnrichedCandle>();
        var block = MomentumOscillators.Macd("MACD", fastPeriod: 3, slowPeriod: 5, signalPeriod: 3);
        var sink = CreateSink(results);
        block.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        for (var i = 0; i < 10; i++)
        {
            var close = 100m + i;
            await block.SendAsync(MakeCandle(close - 1m, close + 1m, close - 2m, close, dayOffset: i));
        }

        block.Complete();
        await sink.Completion;

        for (var i = 0; i < 4; i++)
        {
            Assert.False(results[i].Indicators.ContainsKey("MACD_MACD"));
        }

        for (var i = 0; i < 6; i++)
        {
            Assert.False(results[i].Indicators.ContainsKey("MACD_Signal"));
            Assert.False(results[i].Indicators.ContainsKey("MACD_Hist"));
        }

        Assert.True(results[4].Indicators.ContainsKey("MACD_MACD"));
        Assert.True(results[6].Indicators.ContainsKey("MACD_MACD"));
        Assert.True(results[6].Indicators.ContainsKey("MACD_Signal"));
        Assert.True(results[6].Indicators.ContainsKey("MACD_Hist"));
    }

    [Fact]
    public async Task Rsi_WithIncreasingPrices_ProducesValueAboveFiftyAfterWarmup()
    {
        var results = new List<EnrichedCandle>();
        var block = MomentumOscillators.Rsi("RSI", 3);
        var sink = CreateSink(results);
        block.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        for (var i = 0; i < 10; i++)
        {
            var close = 100m + i;
            await block.SendAsync(MakeCandle(close - 1m, close + 1m, close - 2m, close, dayOffset: i));
        }

        block.Complete();
        await sink.Completion;

        for (var i = 0; i < 3; i++)
        {
            Assert.False(results[i].Indicators.ContainsKey("RSI"));
        }

        Assert.True(results[3].Indicators.ContainsKey("RSI"));
        Assert.True(results[results.Count - 1].Indicators["RSI"] > 50m);
    }

    [Fact]
    public async Task Rsi_WithDecreasingPrices_ProducesValueBelowFiftyAfterWarmup()
    {
        var results = new List<EnrichedCandle>();
        var block = MomentumOscillators.Rsi("RSI", 3);
        var sink = CreateSink(results);
        block.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        for (var i = 0; i < 10; i++)
        {
            var close = 110m - i;
            await block.SendAsync(MakeCandle(close + 1m, close + 2m, close - 1m, close, dayOffset: i));
        }

        block.Complete();
        await sink.Completion;

        for (var i = 0; i < 3; i++)
        {
            Assert.False(results[i].Indicators.ContainsKey("RSI"));
        }

        Assert.True(results[3].Indicators.ContainsKey("RSI"));
        Assert.True(results[results.Count - 1].Indicators["RSI"] < 50m);
    }

    [Fact]
    public async Task Cmo_ProducesValueAfterWarmup()
    {
        var results = new List<EnrichedCandle>();
        var block = MomentumOscillators.Cmo("CMO", 3);
        var sink = CreateSink(results);
        block.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        for (var i = 0; i < 10; i++)
        {
            var close = 100m + i;
            await block.SendAsync(MakeCandle(close - 1m, close + 1m, close - 2m, close, dayOffset: i));
        }

        block.Complete();
        await sink.Completion;

        for (var i = 0; i < 3; i++)
        {
            Assert.False(results[i].Indicators.ContainsKey("CMO"));
        }

        Assert.True(results[3].Indicators.ContainsKey("CMO"));
    }

    [Fact]
    public async Task Cci_ProducesValueAfterWarmup()
    {
        var results = new List<EnrichedCandle>();
        var block = MomentumOscillators.Cci("CCI", 3);
        var sink = CreateSink(results);
        block.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        for (var i = 0; i < 10; i++)
        {
            var close = 100m + i;
            await block.SendAsync(MakeCandle(close - 1m, close + 2m, close - 3m, close, dayOffset: i));
        }

        block.Complete();
        await sink.Completion;

        for (var i = 0; i < 2; i++)
        {
            Assert.False(results[i].Indicators.ContainsKey("CCI"));
        }

        Assert.True(results[2].Indicators.ContainsKey("CCI"));
    }

    [Fact]
    public async Task WillR_ProducesValueAfterWarmupWithinExpectedRange()
    {
        var results = new List<EnrichedCandle>();
        var block = MomentumOscillators.WillR("WILLR", 3);
        var sink = CreateSink(results);
        block.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        for (var i = 0; i < 10; i++)
        {
            var close = 100m + i;
            await block.SendAsync(MakeCandle(close - 1m, close + 3m, close - 3m, close, dayOffset: i));
        }

        block.Complete();
        await sink.Completion;

        for (var i = 0; i < 2; i++)
        {
            Assert.False(results[i].Indicators.ContainsKey("WILLR"));
        }

        Assert.True(results[2].Indicators.ContainsKey("WILLR"));
        Assert.InRange(results[results.Count - 1].Indicators["WILLR"], -100m, 0m);
    }

    [Fact]
    public async Task UltOsc_ProducesValueAfterWarmup()
    {
        var results = new List<EnrichedCandle>();
        var block = MomentumOscillators.UltOsc("ULT", period1: 3, period2: 4, period3: 5);
        var sink = CreateSink(results);
        block.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        for (var i = 0; i < 10; i++)
        {
            var close = 100m + i;
            await block.SendAsync(MakeCandle(close - 1m, close + 2m, close - 3m, close, dayOffset: i));
        }

        block.Complete();
        await sink.Completion;

        for (var i = 0; i < 5; i++)
        {
            Assert.False(results[i].Indicators.ContainsKey("ULT"));
        }

        Assert.True(results[5].Indicators.ContainsKey("ULT"));
    }

    [Fact]
    public async Task Trix_ProducesValueAfterWarmup()
    {
        var results = new List<EnrichedCandle>();
        var block = MomentumOscillators.Trix("TRIX", 3);
        var sink = CreateSink(results);
        block.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        for (var i = 0; i < 10; i++)
        {
            var close = 100m + i;
            await block.SendAsync(MakeCandle(close - 1m, close + 1m, close - 2m, close, dayOffset: i));
        }

        block.Complete();
        await sink.Completion;

        for (var i = 0; i < 7; i++)
        {
            Assert.False(results[i].Indicators.ContainsKey("TRIX"));
        }

        Assert.True(results[7].Indicators.ContainsKey("TRIX"));
    }

    [Fact]
    public async Task Aroon_StoresAllExpectedKeysAfterWarmup()
    {
        var results = new List<EnrichedCandle>();
        var block = MomentumOscillators.Aroon("AROON", 5);
        var sink = CreateSink(results);
        block.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        for (var i = 0; i < 10; i++)
        {
            var close = 100m + i;
            await block.SendAsync(MakeCandle(close - 1m, close + 2m, close - 3m, close, dayOffset: i));
        }

        block.Complete();
        await sink.Completion;

        for (var i = 0; i < 5; i++)
        {
            Assert.False(results[i].Indicators.ContainsKey("AROON_Up"));
            Assert.False(results[i].Indicators.ContainsKey("AROON_Down"));
        }

        Assert.True(results[5].Indicators.ContainsKey("AROON_Up"));
        Assert.True(results[5].Indicators.ContainsKey("AROON_Down"));
    }

    [Fact]
    public async Task AroonOsc_ProducesValueAfterWarmup()
    {
        var results = new List<EnrichedCandle>();
        var block = MomentumOscillators.AroonOsc("AROONOSC", 5);
        var sink = CreateSink(results);
        block.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        for (var i = 0; i < 10; i++)
        {
            var close = 100m + i;
            await block.SendAsync(MakeCandle(close - 1m, close + 2m, close - 3m, close, dayOffset: i));
        }

        block.Complete();
        await sink.Completion;

        for (var i = 0; i < 5; i++)
        {
            Assert.False(results[i].Indicators.ContainsKey("AROONOSC"));
        }

        Assert.True(results[5].Indicators.ContainsKey("AROONOSC"));
    }

    private static ActionBlock<EnrichedCandle> CreateSink(List<EnrichedCandle> results) =>
        new(item => results.Add(item));

    private static EnrichedCandle MakeCandle(
        decimal open,
        decimal high,
        decimal low,
        decimal close,
        long volume = 1000,
        int dayOffset = 0) =>
        new(new Candle
        {
            Timestamp = BaseTime.AddDays(dayOffset),
            Open = open,
            High = high,
            Low = low,
            Close = close,
            Volume = volume,
        });
}
