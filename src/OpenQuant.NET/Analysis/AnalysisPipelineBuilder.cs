using System.Threading.Tasks.Dataflow;
using OpenQuant.Models;

namespace OpenQuant.Analysis;

/// <summary>
/// Fluent builder that selects which analysis indicators to include, runs them in parallel
/// over a list of candles, and returns every candle enriched with the computed values.
/// The builder is reusable — each <see cref="RunAsync"/> call creates an independent pipeline.
/// </summary>
public sealed class AnalysisPipelineBuilder
{
    private readonly List<(string Name, IndicatorFactory Factory)> _indicators = [];
    private readonly HashSet<string> _names = [];

    /// <summary>
    /// Registers a custom indicator factory under the given name.
    /// </summary>
    /// <param name="name">Unique display name used as key in <see cref="EnrichedCandle.Indicators"/>.</param>
    /// <param name="factory">A delegate that creates the indicator's <see cref="ActionBlock{Candle}"/>.</param>
    /// <returns>This builder instance for chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="name"/> has already been added.</exception>
    public AnalysisPipelineBuilder Add(string name, IndicatorFactory factory)
    {
        if (!_names.Add(name))
        {
            throw new ArgumentException(
                $"Duplicate indicator name '{name}'. Each indicator must have a unique name.",
                nameof(name));
        }

        _indicators.Add((name, factory));
        return this;
    }

    /// <summary>Adds a Simple Moving Average indicator.</summary>
    public AnalysisPipelineBuilder AddSMA(string name, int period)
        => Add(name, (target, ct) => MovingAverage.SMAActionBlockFactory(period, target, ct));

    /// <summary>Adds an Exponential Moving Average indicator.</summary>
    public AnalysisPipelineBuilder AddEMA(string name, int period)
        => Add(name, (target, ct) => MovingAverage.EMAActionBlockFactory(period, target, ct));

    /// <summary>Adds a Weighted Moving Average indicator.</summary>
    public AnalysisPipelineBuilder AddWMA(string name, int period)
        => Add(name, (target, ct) => MovingAverage.WMAActionBlockFactory(period, target, ct));

    /// <summary>Adds a Hull Moving Average indicator.</summary>
    public AnalysisPipelineBuilder AddHMA(string name, int period)
        => Add(name, (target, ct) => MovingAverage.HMAActionBlockFactory(period, target, ct));

    /// <summary>Adds a Moving Median indicator.</summary>
    public AnalysisPipelineBuilder AddMedian(string name, int period)
        => Add(name, (target, ct) => MovingMedian.MedianActionBlockFactory(period, target, ct));

    /// <summary>
    /// Feeds every candle to all registered indicators in parallel, awaits completion,
    /// and returns the candles enriched with computed indicator values.
    /// </summary>
    /// <param name="candles">Candles in chronological order.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>One <see cref="EnrichedCandle"/> per input candle, preserving order.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no indicators have been added.</exception>
    /// <exception cref="AggregateException">Thrown when one or more indicators fault during processing.</exception>
    public async Task<IReadOnlyList<EnrichedCandle>> RunAsync(
        IReadOnlyList<Candle> candles,
        CancellationToken cancellationToken = default)
    {
        if (_indicators.Count == 0)
        {
            throw new InvalidOperationException("At least one indicator must be added before running the pipeline.");
        }

        // Snapshot indicator list so the builder stays reusable.
        var indicators = _indicators.ToArray();
        var indicatorCount = indicators.Length;

        var buffers = new BufferBlock<(DateTimeOffset Timestamp, decimal Value)>[indicatorCount];
        var blocks = new ActionBlock<Candle>[indicatorCount];

        for (var i = 0; i < indicatorCount; i++)
        {
            buffers[i] = new BufferBlock<(DateTimeOffset Timestamp, decimal Value)>();
            blocks[i] = indicators[i].Factory(buffers[i], cancellationToken);
        }

        // Feed every candle to every indicator block in parallel.
        foreach (var candle in candles)
        {
            var sendTasks = new Task<bool>[indicatorCount];

            for (var i = 0; i < indicatorCount; i++)
            {
                sendTasks[i] = blocks[i].SendAsync(candle, cancellationToken);
            }

            await Task.WhenAll(sendTasks);
        }

        // Signal completion and wait for all blocks to finish processing.
        foreach (var block in blocks)
        {
            block.Complete();
        }

        await Task.WhenAll(blocks.Select(b => b.Completion));

        // Surface any faulted indicators.
        var faults = new List<Exception>();

        for (var i = 0; i < indicatorCount; i++)
        {
            if (blocks[i].Completion.IsFaulted)
            {
                faults.Add(new InvalidOperationException(
                    $"Indicator '{indicators[i].Name}' faulted.",
                    blocks[i].Completion.Exception));
            }
        }

        if (faults.Count > 0)
        {
            throw new AggregateException("One or more indicators faulted during processing.", faults);
        }

        // Drain each buffer into a timestamp-keyed lookup.
        var indicatorValues = new Dictionary<string, Dictionary<DateTimeOffset, decimal>>(indicatorCount);

        for (var i = 0; i < indicatorCount; i++)
        {
            var values = new Dictionary<DateTimeOffset, decimal>();

            while (buffers[i].TryReceive(out var item))
            {
                values[item.Timestamp] = item.Value;
            }

            indicatorValues[indicators[i].Name] = values;
        }

        // Build enriched candles preserving input order.
        var result = new List<EnrichedCandle>(candles.Count);

        foreach (var candle in candles)
        {
            var values = new Dictionary<string, decimal>();

            foreach (var (name, lookup) in indicatorValues)
            {
                if (lookup.TryGetValue(candle.Timestamp, out var value))
                {
                    values[name] = value;
                }
            }

            result.Add(new EnrichedCandle
            {
                Candle = candle,
                Indicators = values,
            });
        }

        return result;
    }
}
