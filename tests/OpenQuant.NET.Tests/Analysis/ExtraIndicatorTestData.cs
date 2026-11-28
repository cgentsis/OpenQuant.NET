using System.Threading.Tasks.Dataflow;
using OpenQuant.Models;

namespace OpenQuant.Tests.Analysis;

/// <summary>
/// Shared candle construction and dataflow execution helpers for extra-indicator tests.
/// </summary>
internal static class ExtraIndicatorTestData
{
    public static EnrichedCandle Candle(
        decimal close,
        decimal highOffset = 1m,
        decimal lowOffset = 1m,
        long volume = 1000,
        int dayOffset = 0) =>
        Ohlcv(close, close + highOffset, close - lowOffset, close, volume, dayOffset);

    public static EnrichedCandle Ohlcv(
        decimal open,
        decimal high,
        decimal low,
        decimal close,
        long volume = 1000,
        int dayOffset = 0) =>
        new(new Candle
        {
            Timestamp = DateTimeOffset.UnixEpoch.AddDays(dayOffset),
            Open = open,
            High = high,
            Low = low,
            Close = close,
            Volume = volume,
        });

    public static async Task<List<EnrichedCandle>> RunAsync(
        TransformBlock<EnrichedCandle, EnrichedCandle> block,
        IEnumerable<EnrichedCandle> inputs)
    {
        var results = new List<EnrichedCandle>();
        var sink = new ActionBlock<EnrichedCandle>(results.Add);

        block.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });
        foreach (var input in inputs)
        {
            await block.SendAsync(input);
        }

        block.Complete();
        await sink.Completion;
        return results;
    }

    public static EnrichedCandle[] Candles(decimal[] closes, IReadOnlyList<long>? volumes = null)
    {
        var candles = new EnrichedCandle[closes.Length];
        for (var index = 0; index < closes.Length; index++)
        {
            var volume = volumes is null ? 1000L : volumes[index];
            candles[index] = Candle(closes[index], volume: volume, dayOffset: index);
        }

        return candles;
    }
}
