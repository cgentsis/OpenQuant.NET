using System.Threading.Tasks.Dataflow;
using OpenQuant.Analysis;
using OpenQuant.Models;

namespace OpenQuant.Tests.Analysis;

public class AdvancedIndicatorsTests
{
    private static readonly DateTimeOffset BaseTime = new(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Sar_ProducesValueAfterSecondCandle()
    {
        var results = new List<EnrichedCandle>();
        var block = AdvancedIndicators.Sar("SAR");
        var sink = CreateSink(results);
        block.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        await SendTrendCandles(block, [10m, 11m, 12m, 13m, 14m, 15m, 16m, 17m, 18m, 19m]);
        block.Complete();
        await sink.Completion;

        Assert.False(results[0].Indicators.ContainsKey("SAR"));
        Assert.True(results[1].Indicators.ContainsKey("SAR"));
        Assert.True(results[9].Indicators["SAR"] < results[9].Candle.Close);
    }

    [Fact]
    public async Task HtTrendline_ProducesValueAfterWarmup()
    {
        var results = new List<EnrichedCandle>();
        var block = AdvancedIndicators.HtTrendline("HTTL");
        var sink = CreateSink(results);
        block.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        await SendWaveCandles(block);
        block.Complete();
        await sink.Completion;

        Assert.False(results[5].Indicators.ContainsKey("HTTL"));
        Assert.True(results[6].Indicators.ContainsKey("HTTL"));
        Assert.True(results[19].Indicators["HTTL"] > 0m);
    }

    [Fact]
    public async Task HtDcPeriod_ProducesValueAfterWarmup()
    {
        var results = new List<EnrichedCandle>();
        var block = AdvancedIndicators.HtDcPeriod("DCPERIOD");
        var sink = CreateSink(results);
        block.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        await SendWaveCandles(block);
        block.Complete();
        await sink.Completion;

        Assert.False(results[8].Indicators.ContainsKey("DCPERIOD"));
        Assert.True(results[9].Indicators.ContainsKey("DCPERIOD"));
        Assert.True(results[19].Indicators["DCPERIOD"] > 0m);
    }

    [Fact]
    public async Task HtDcPhase_ProducesValueAfterWarmup()
    {
        var results = new List<EnrichedCandle>();
        var block = AdvancedIndicators.HtDcPhase("DCPHASE");
        var sink = CreateSink(results);
        block.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        await SendWaveCandles(block);
        block.Complete();
        await sink.Completion;

        Assert.False(results[8].Indicators.ContainsKey("DCPHASE"));
        Assert.True(results[9].Indicators.ContainsKey("DCPHASE"));
        Assert.InRange(results[19].Indicators["DCPHASE"], -180m, 360m);
    }

    [Fact]
    public async Task HtPhasor_StoresBothKeysAfterWarmup()
    {
        var results = new List<EnrichedCandle>();
        var block = AdvancedIndicators.HtPhasor("PHASOR");
        var sink = CreateSink(results);
        block.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        await SendWaveCandles(block);
        block.Complete();
        await sink.Completion;

        Assert.False(results[8].Indicators.ContainsKey("PHASOR_InPhase"));
        Assert.True(results[19].Indicators.ContainsKey("PHASOR_InPhase"));
        Assert.True(results[19].Indicators.ContainsKey("PHASOR_Quadrature"));
    }

    [Fact]
    public async Task HtSine_StoresBothKeysAfterWarmup()
    {
        var results = new List<EnrichedCandle>();
        var block = AdvancedIndicators.HtSine("SINE");
        var sink = CreateSink(results);
        block.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        await SendWaveCandles(block);
        block.Complete();
        await sink.Completion;

        Assert.False(results[8].Indicators.ContainsKey("SINE_Sine"));
        Assert.True(results[19].Indicators.ContainsKey("SINE_Sine"));
        Assert.True(results[19].Indicators.ContainsKey("SINE_LeadSine"));
        Assert.InRange(results[19].Indicators["SINE_Sine"], -1m, 1m);
        Assert.InRange(results[19].Indicators["SINE_LeadSine"], -1m, 1m);
    }

    [Fact]
    public async Task HtTrendMode_StoresBinaryValueAfterWarmup()
    {
        var results = new List<EnrichedCandle>();
        var block = AdvancedIndicators.HtTrendMode("TMODE");
        var sink = CreateSink(results);
        block.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        await SendWaveCandles(block);
        block.Complete();
        await sink.Completion;

        var value = results[19].Indicators["TMODE"];

        Assert.False(results[8].Indicators.ContainsKey("TMODE"));
        Assert.True(results[9].Indicators.ContainsKey("TMODE"));
        Assert.True(value == 0m || value == 1m);
    }

    private static ActionBlock<EnrichedCandle> CreateSink(List<EnrichedCandle> results) =>
        new(item => results.Add(item));

    private static async Task SendTrendCandles(TransformBlock<EnrichedCandle, EnrichedCandle> block, decimal[] closes)
    {
        for (var i = 0; i < closes.Length; i++)
        {
            var close = closes[i];
            await block.SendAsync(MakeCandle(close - 0.5m, close + 1m, close - 1m, close, dayOffset: i));
        }
    }

    private static async Task SendWaveCandles(TransformBlock<EnrichedCandle, EnrichedCandle> block)
    {
        var closes = new[] { 10m, 11m, 13m, 14m, 15m, 14m, 12m, 11m, 10m, 9m, 10m, 12m, 13m, 15m, 16m, 15m, 13m, 12m, 11m, 10m };

        for (var i = 0; i < closes.Length; i++)
        {
            var close = closes[i];
            await block.SendAsync(MakeCandle(close - 0.25m, close + 1m, close - 1m, close, dayOffset: i));
        }
    }

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
