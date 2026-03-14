using System.Threading.Tasks.Dataflow;
using OpenQuant.Analysis;
using OpenQuant.Models;

namespace OpenQuant.Tests.Analysis;

public class StochasticIndicatorsTests
{
    private static readonly DateTimeOffset BaseTime = new(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Stoch_ProducesBothKeysAfterWarmup()
    {
        var results = new List<EnrichedCandle>();
        var block = StochasticIndicators.Stoch("STOCH", 3, 2, 2);
        var sink = CreateSink(results);
        block.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        await SendCloses(block, [10m, 12m, 13m, 14m, 15m]);
        block.Complete();
        await sink.Completion;

        Assert.False(results[2].Indicators.ContainsKey("STOCH_K"));
        Assert.False(results[3].Indicators.ContainsKey("STOCH_D"));
        Assert.True(results[4].Indicators.ContainsKey("STOCH_K"));
        Assert.True(results[4].Indicators.ContainsKey("STOCH_D"));
    }

    [Fact]
    public async Task StochF_ProducesBothKeysAfterWarmup()
    {
        var results = new List<EnrichedCandle>();
        var block = StochasticIndicators.StochF("STOCHF", 3, 2);
        var sink = CreateSink(results);
        block.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        await SendCloses(block, [10m, 12m, 13m, 14m]);
        block.Complete();
        await sink.Completion;

        Assert.False(results[1].Indicators.ContainsKey("STOCHF_K"));
        Assert.False(results[2].Indicators.ContainsKey("STOCHF_D"));
        Assert.True(results[3].Indicators.ContainsKey("STOCHF_K"));
        Assert.True(results[3].Indicators.ContainsKey("STOCHF_D"));
    }

    [Fact]
    public async Task StochRsi_ProducesBothKeysAfterWarmup()
    {
        var results = new List<EnrichedCandle>();
        var block = StochasticIndicators.StochRsi("STOCHRSI", 5, 5, 2, 2);
        var sink = CreateSink(results);
        block.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        await SendCloses(block, [10m, 12m, 11m, 13m, 12m, 14m, 11m, 15m, 13m, 16m, 14m, 17m, 15m, 18m, 16m, 19m, 17m, 20m, 18m, 21m, 19m, 22m]);
        block.Complete();
        await sink.Completion;

        Assert.False(results[9].Indicators.ContainsKey("STOCHRSI_K"));
        Assert.False(results[10].Indicators.ContainsKey("STOCHRSI_D"));
        Assert.True(results[21].Indicators.ContainsKey("STOCHRSI_K"));
        Assert.True(results[21].Indicators.ContainsKey("STOCHRSI_D"));
        Assert.InRange(results[21].Indicators["STOCHRSI_K"], 0m, 100m);
        Assert.InRange(results[21].Indicators["STOCHRSI_D"], 0m, 100m);
    }

    [Fact]
    public async Task Stoch_KAndDValues_AreBetweenZeroAndHundred()
    {
        var results = new List<EnrichedCandle>();
        var block = StochasticIndicators.Stoch("STOCH", 3, 2, 2);
        var sink = CreateSink(results);
        block.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        await SendCloses(block, [10m, 12m, 13m, 14m, 15m]);
        block.Complete();
        await sink.Completion;

        Assert.InRange(results[4].Indicators["STOCH_K"], 0m, 100m);
        Assert.InRange(results[4].Indicators["STOCH_D"], 0m, 100m);
    }

    [Fact]
    public async Task StochF_KValue_IsBetweenZeroAndHundred()
    {
        var results = new List<EnrichedCandle>();
        var block = StochasticIndicators.StochF("STOCHF", 3, 2);
        var sink = CreateSink(results);
        block.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        await SendCloses(block, [10m, 12m, 13m, 14m]);
        block.Complete();
        await sink.Completion;

        Assert.InRange(results[3].Indicators["STOCHF_K"], 0m, 100m);
    }

    private static ActionBlock<EnrichedCandle> CreateSink(List<EnrichedCandle> results) =>
        new(item => results.Add(item));

    private static async Task SendCloses(TransformBlock<EnrichedCandle, EnrichedCandle> block, decimal[] closes)
    {
        for (var i = 0; i < closes.Length; i++)
        {
            var close = closes[i];
            await block.SendAsync(MakeCandle(close - 0.5m, close + 1m, close - 1m, close, dayOffset: i));
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
