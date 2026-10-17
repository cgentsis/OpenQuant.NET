using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks.Dataflow;
using OpenQuant.Analysis;
using OpenQuant.Models;
using Xunit;

namespace OpenQuant.Tests.Analysis;

public class CancellationTests
{
    [Fact]
    public async Task CancellationStopsPipeline()
    {
        var results = new List<EnrichedCandle>();
        using var cts = new CancellationTokenSource();
        var sma = MovingAverage.SMA("SMA", 3, cts.Token);
        var sink = CreateSink(results);
        sma.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        await sma.SendAsync(Enrich(10m, 0));
        await sma.SendAsync(Enrich(20m, 1));
        await sma.SendAsync(Enrich(30m, 2));

        // Wait briefly for processing
        await Task.Delay(10);

        await cts.CancelAsync();

        // These sends should not produce SMA values
        await sma.SendAsync(Enrich(40m, 3));
        await sma.SendAsync(Enrich(50m, 4));

        sma.Complete();
        await sink.Completion;

        // Only the third candle should have SMA value, others should not
        Assert.Equal(3, results.Count);
        Assert.False(results[0].Indicators.ContainsKey("SMA"));
        Assert.False(results[1].Indicators.ContainsKey("SMA"));
        Assert.Equal(20m, results[2].Indicators["SMA"]);
    }

    private static ActionBlock<EnrichedCandle> CreateSink(List<EnrichedCandle> results) =>
        new(item => results.Add(item));

    private static EnrichedCandle Enrich(decimal close, int dayOffset = 0) => new(new Candle
    {
        Timestamp = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero).AddDays(dayOffset),
        Open = close,
        High = close,
        Low = close,
        Close = close,
        Volume = 1000,
    });
}