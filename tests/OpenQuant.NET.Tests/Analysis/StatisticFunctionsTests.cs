using System.Threading.Tasks.Dataflow;
using OpenQuant.Analysis;
using OpenQuant.Models;

namespace OpenQuant.Tests.Analysis;

public class StatisticFunctionsTests
{
    private static readonly DateTimeOffset BaseTime = new(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Var_ComputesPopulationVariance()
    {
        var results = new List<EnrichedCandle>();
        var block = StatisticFunctions.Var("VAR", 2);
        var sink = CreateSink(results);
        block.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        // [10, 30]: mean=20, var = ((10-20)²+(30-20)²)/2 = 200/2 = 100
        await SendCloses(block, [10m, 30m]);
        block.Complete();
        await sink.Completion;

        Assert.False(results[0].Indicators.ContainsKey("VAR"));
        Assert.Equal(100m, results[1].Indicators["VAR"]);
    }

    [Fact]
    public async Task StdDev_ComputesCorrectly()
    {
        var results = new List<EnrichedCandle>();
        var block = StatisticFunctions.StdDev("SD", 2);
        var sink = CreateSink(results);
        block.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        // [10, 30]: var = 100, sd = 10
        await SendCloses(block, [10m, 30m]);
        block.Complete();
        await sink.Completion;

        Assert.Equal(10m, results[1].Indicators["SD"]);
    }

    [Fact]
    public async Task StdDev_WithNbDev_Scales()
    {
        var results = new List<EnrichedCandle>();
        var block = StatisticFunctions.StdDev("SD2", 2, nbDev: 2m);
        var sink = CreateSink(results);
        block.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        // [10, 30]: sd = 10, sd*2 = 20
        await SendCloses(block, [10m, 30m]);
        block.Complete();
        await sink.Completion;

        Assert.Equal(20m, results[1].Indicators["SD2"]);
    }

    [Fact]
    public async Task LinearReg_ComputesEndOfRegressionLine()
    {
        var results = new List<EnrichedCandle>();
        var block = StatisticFunctions.LinearReg("LR", 3);
        var sink = CreateSink(results);
        block.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        // [10, 20, 30]: perfect linear, slope=10, intercept=10
        // LinearReg = 10 + 10*(3-1) = 30
        await SendCloses(block, [10m, 20m, 30m]);
        block.Complete();
        await sink.Completion;

        Assert.Equal(30m, results[2].Indicators["LR"]);
    }

    [Fact]
    public async Task LinearRegSlope_ComputesCorrectly()
    {
        var results = new List<EnrichedCandle>();
        var block = StatisticFunctions.LinearRegSlope("SLOPE", 3);
        var sink = CreateSink(results);
        block.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        // [10, 20, 30]: slope = 10
        await SendCloses(block, [10m, 20m, 30m]);
        block.Complete();
        await sink.Completion;

        Assert.Equal(10m, results[2].Indicators["SLOPE"]);
    }

    [Fact]
    public async Task LinearRegIntercept_ComputesCorrectly()
    {
        var results = new List<EnrichedCandle>();
        var block = StatisticFunctions.LinearRegIntercept("INTRCPT", 3);
        var sink = CreateSink(results);
        block.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        // [10, 20, 30]: intercept = 10
        await SendCloses(block, [10m, 20m, 30m]);
        block.Complete();
        await sink.Completion;

        Assert.Equal(10m, results[2].Indicators["INTRCPT"]);
    }

    [Fact]
    public async Task LinearRegAngle_ComputesCorrectly()
    {
        var results = new List<EnrichedCandle>();
        var block = StatisticFunctions.LinearRegAngle("ANGLE", 3);
        var sink = CreateSink(results);
        block.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        // [10, 20, 30]: slope=10, angle = atan(10) * 180/π ≈ 84.289
        await SendCloses(block, [10m, 20m, 30m]);
        block.Complete();
        await sink.Completion;

        var angle = results[2].Indicators["ANGLE"];
        Assert.Equal(84.289m, Math.Round(angle, 3));
    }

    [Fact]
    public async Task Tsf_ProjectsOneStepAhead()
    {
        var results = new List<EnrichedCandle>();
        var block = StatisticFunctions.Tsf("TSF", 3);
        var sink = CreateSink(results);
        block.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        // [10, 20, 30]: slope=10, intercept=10, TSF = 10 + 10*3 = 40
        await SendCloses(block, [10m, 20m, 30m]);
        block.Complete();
        await sink.Completion;

        Assert.Equal(40m, results[2].Indicators["TSF"]);
    }

    [Fact]
    public async Task Correl_PerfectPositiveCorrelation()
    {
        var results = new List<EnrichedCandle>();
        var block = StatisticFunctions.Correl(
            "CORREL",
            3,
            e => e.Candle.Close,
            e => e.Indicators["B"]);
        var sink = CreateSink(results);
        block.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        // Series A (Close): [10, 20, 30], Series B: [20, 40, 60]
        // Perfectly correlated → r = 1.0
        await block.SendAsync(MakeEnrichedWithIndicator(10m, "B", 20m, 0));
        await block.SendAsync(MakeEnrichedWithIndicator(20m, "B", 40m, 1));
        await block.SendAsync(MakeEnrichedWithIndicator(30m, "B", 60m, 2));
        block.Complete();
        await sink.Completion;

        Assert.Equal(1.0m, Math.Round(results[2].Indicators["CORREL"], 10));
    }

    [Fact]
    public async Task Beta_ComputesCorrectly()
    {
        var results = new List<EnrichedCandle>();
        var block = StatisticFunctions.Beta(
            "BETA",
            3,
            e => e.Candle.Close,
            e => e.Indicators["BENCH"]);
        var sink = CreateSink(results);
        block.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        // Asset (Close): [10, 20, 30], Benchmark: [20, 40, 60]
        // β = Cov/Var(B) = [nΣxy - ΣxΣy] / [nΣy² - (Σy)²]
        // nΣxy = 3*(10*20+20*40+30*60) = 3*2800 = 8400
        // ΣxΣy = 60*120 = 7200
        // nΣy² = 3*(400+1600+3600) = 16800
        // (Σy)² = 14400
        // β = (8400-7200)/(16800-14400) = 1200/2400 = 0.5
        await block.SendAsync(MakeEnrichedWithIndicator(10m, "BENCH", 20m, 0));
        await block.SendAsync(MakeEnrichedWithIndicator(20m, "BENCH", 40m, 1));
        await block.SendAsync(MakeEnrichedWithIndicator(30m, "BENCH", 60m, 2));
        block.Complete();
        await sink.Completion;

        Assert.Equal(0.5m, results[2].Indicators["BETA"]);
    }

    [Fact]
    public async Task Var_InsufficientData_ProducesNoValues()
    {
        var results = new List<EnrichedCandle>();
        var block = StatisticFunctions.Var("VAR", 5);
        var sink = CreateSink(results);
        block.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        await SendCloses(block, [10m, 20m]);
        block.Complete();
        await sink.Completion;

        Assert.All(results, r => Assert.Empty(r.Indicators));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Var_InvalidPeriod_Throws(int period)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => StatisticFunctions.Var("VAR", period));
    }

    [Fact]
    public void LinearReg_PeriodLessThanTwo_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => StatisticFunctions.LinearReg("LR", 1));
    }

    [Fact]
    public void Correl_PeriodLessThanTwo_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            StatisticFunctions.Correl("C", 1, e => e.Candle.Close, e => e.Candle.Close));
    }

    private static ActionBlock<EnrichedCandle> CreateSink(List<EnrichedCandle> results) =>
        new(item => results.Add(item));

    private static async Task SendCloses(TransformBlock<EnrichedCandle, EnrichedCandle> block, decimal[] closes)
    {
        for (var i = 0; i < closes.Length; i++)
        {
            await block.SendAsync(Enrich(closes[i], i));
        }
    }

    private static EnrichedCandle Enrich(decimal close, int dayOffset = 0) => new(new Candle
    {
        Timestamp = BaseTime.AddDays(dayOffset),
        Open = close,
        High = close,
        Low = close,
        Close = close,
        Volume = 1000,
    });

    private static EnrichedCandle MakeEnrichedWithIndicator(
        decimal close,
        string key,
        decimal value,
        int dayOffset = 0)
    {
        var enriched = Enrich(close, dayOffset);
        enriched.Indicators[key] = value;
        return enriched;
    }
}
