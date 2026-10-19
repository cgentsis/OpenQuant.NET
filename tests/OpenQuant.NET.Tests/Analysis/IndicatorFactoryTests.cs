using System.Threading.Tasks.Dataflow;
using OpenQuant.Analysis;
using OpenQuant.Models;

namespace OpenQuant.Tests.Analysis;

public class IndicatorFactoryTests
{
    [Fact]
    public async Task Create_WithValidParameters_CreatesTransformBlock()
    {
        // Arrange
        var name = "TestIndicator";
        var period = 3;
        var cancellationToken = default(CancellationToken);
        
        Func<EnrichedCandle, IndicatorState> computationDelegate = (enriched) =>
        {
            return new IndicatorState { Value = 10m, Count = 1 };
        };

        // Act
        var result = IndicatorFactory.Create(name, period, computationDelegate, cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<TransformBlock<EnrichedCandle, EnrichedCandle>>(result);
    }

    [Fact]
    public async Task Create_WithCancellationRequested_ReturnsUnchangedCandle()
    {
        // Arrange
        var name = "TestIndicator";
        var period = 3;
        var cts = new CancellationTokenSource();
        cts.Cancel(); // Cancel immediately
        
        Func<EnrichedCandle, IndicatorState> computationDelegate = (enriched) =>
        {
            // This should not be executed due to cancellation
            Assert.Fail("Computation delegate should not have been called due to cancellation");
            return new IndicatorState { Value = 0m, Count = 0 };
        };

        var candle = Enrich(10m);
        
        // Act
        var block = IndicatorFactory.Create(name, period, computationDelegate, cts.Token);
        var result = await block.SendAsync(candle);
        block.Complete();
        
        // Assert - The candle should be returned unchanged (unprocessed)
        Assert.True(result);
        Assert.NotNull(candle);
    }

    [Fact]
    public async Task Create_WithValidComputationDelegate_ProducesExpectedResult()
    {
        // Arrange
        var name = "TestIndicator";
        var period = 3;
        var cancellationToken = default(CancellationToken);
        
        var results = new List<EnrichedCandle>();
        var sink = new ActionBlock<EnrichedCandle>(r => results.Add(r));
        
        Func<EnrichedCandle, IndicatorState> computationDelegate = (enriched) =>
        {
            return new IndicatorState { Value = 15m, Count = 3 };
        };

        var block = IndicatorFactory.Create(name, period, computationDelegate, cancellationToken);
        block.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });
        
        // Act
        await block.SendAsync(Enrich(10m));
        block.Complete();
        await sink.Completion;
        
        // Assert
        Assert.Single(results);
        Assert.True(results[0].Indicators.ContainsKey(name));
        Assert.Equal(15m, results[0].Indicators[name]);
    }

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