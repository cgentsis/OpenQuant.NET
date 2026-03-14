using System.Threading.Tasks.Dataflow;
using OpenQuant.Analysis;
using OpenQuant.Models;

namespace OpenQuant.Tests.Analysis;

public class RollingWindowTests
{
    private static readonly DateTimeOffset BaseTime = new(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Sum_WithValidPeriod_ReturnsCorrectValues()
    {
        var results = new List<EnrichedCandle>();
        var block = RollingWindow.Sum("SUM", 3);
        var sink = CreateSink(results);
        block.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        await SendCloses(block, [10m, 20m, 30m, 40m, 50m]);
        block.Complete();
        await sink.Completion;

        Assert.False(results[0].Indicators.ContainsKey("SUM"));
        Assert.False(results[1].Indicators.ContainsKey("SUM"));
        Assert.Equal(60m, results[2].Indicators["SUM"]);  // 10+20+30
        Assert.Equal(90m, results[3].Indicators["SUM"]);  // 20+30+40
        Assert.Equal(120m, results[4].Indicators["SUM"]); // 30+40+50
    }

    [Fact]
    public async Task Max_WithValidPeriod_ReturnsCorrectValues()
    {
        var results = new List<EnrichedCandle>();
        var block = RollingWindow.Max("MAX", 3);
        var sink = CreateSink(results);
        block.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        await SendCloses(block, [10m, 30m, 20m, 40m, 15m]);
        block.Complete();
        await sink.Completion;

        Assert.Equal(30m, results[2].Indicators["MAX"]);  // max(10,30,20)
        Assert.Equal(40m, results[3].Indicators["MAX"]);  // max(30,20,40)
        Assert.Equal(40m, results[4].Indicators["MAX"]);  // max(20,40,15)
    }

    [Fact]
    public async Task Min_WithValidPeriod_ReturnsCorrectValues()
    {
        var results = new List<EnrichedCandle>();
        var block = RollingWindow.Min("MIN", 3);
        var sink = CreateSink(results);
        block.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        await SendCloses(block, [30m, 10m, 20m, 5m, 40m]);
        block.Complete();
        await sink.Completion;

        Assert.Equal(10m, results[2].Indicators["MIN"]);  // min(30,10,20)
        Assert.Equal(5m, results[3].Indicators["MIN"]);   // min(10,20,5)
        Assert.Equal(5m, results[4].Indicators["MIN"]);   // min(20,5,40)
    }

    [Fact]
    public async Task MaxIndex_ReturnsCorrectIndex()
    {
        var results = new List<EnrichedCandle>();
        var block = RollingWindow.MaxIndex("MAXI", 3);
        var sink = CreateSink(results);
        block.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        // Window [10, 30, 20]: max=30 at index 1
        // Window [30, 20, 40]: max=40 at index 2
        // Window [20, 40, 15]: max=40 at index 1
        await SendCloses(block, [10m, 30m, 20m, 40m, 15m]);
        block.Complete();
        await sink.Completion;

        Assert.Equal(1m, results[2].Indicators["MAXI"]);
        Assert.Equal(2m, results[3].Indicators["MAXI"]);
        Assert.Equal(1m, results[4].Indicators["MAXI"]);
    }

    [Fact]
    public async Task MinIndex_ReturnsCorrectIndex()
    {
        var results = new List<EnrichedCandle>();
        var block = RollingWindow.MinIndex("MINI", 3);
        var sink = CreateSink(results);
        block.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        // Window [30, 10, 20]: min=10 at index 1
        // Window [10, 20, 5]: min=5 at index 2
        // Window [20, 5, 40]: min=5 at index 1
        await SendCloses(block, [30m, 10m, 20m, 5m, 40m]);
        block.Complete();
        await sink.Completion;

        Assert.Equal(1m, results[2].Indicators["MINI"]);
        Assert.Equal(2m, results[3].Indicators["MINI"]);
        Assert.Equal(1m, results[4].Indicators["MINI"]);
    }

    [Fact]
    public async Task MinMax_ReturnsMinAndMax()
    {
        var results = new List<EnrichedCandle>();
        var block = RollingWindow.MinMax("MM", 3);
        var sink = CreateSink(results);
        block.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        await SendCloses(block, [10m, 30m, 20m]);
        block.Complete();
        await sink.Completion;

        Assert.Equal(10m, results[2].Indicators["MM_Min"]);
        Assert.Equal(30m, results[2].Indicators["MM_Max"]);
    }

    [Fact]
    public async Task MinMaxIndex_ReturnsMinAndMaxIndexes()
    {
        var results = new List<EnrichedCandle>();
        var block = RollingWindow.MinMaxIndex("MMI", 3);
        var sink = CreateSink(results);
        block.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        // Window [10, 30, 20]: min=10 at 0, max=30 at 1
        await SendCloses(block, [10m, 30m, 20m]);
        block.Complete();
        await sink.Completion;

        Assert.Equal(0m, results[2].Indicators["MMI_MinIdx"]);
        Assert.Equal(1m, results[2].Indicators["MMI_MaxIdx"]);
    }

    [Fact]
    public async Task MidPoint_ComputesCorrectly()
    {
        var results = new List<EnrichedCandle>();
        var block = RollingWindow.MidPoint("MID", 3);
        var sink = CreateSink(results);
        block.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        // Window [10, 30, 20]: (30+10)/2 = 20
        await SendCloses(block, [10m, 30m, 20m]);
        block.Complete();
        await sink.Completion;

        Assert.Equal(20m, results[2].Indicators["MID"]);
    }

    [Fact]
    public async Task MidPrice_ComputesFromHighLow()
    {
        var results = new List<EnrichedCandle>();
        var block = RollingWindow.MidPrice("MIDP", 3);
        var sink = CreateSink(results);
        block.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        // H: [20, 22, 24], L: [10, 11, 14]
        // highest high = 24, lowest low = 10 → (24+10)/2 = 17
        await block.SendAsync(MakeCandle(10m, 20m, 10m, 12m, dayOffset: 0));
        await block.SendAsync(MakeCandle(12m, 22m, 11m, 18m, dayOffset: 1));
        await block.SendAsync(MakeCandle(18m, 24m, 14m, 16m, dayOffset: 2));
        block.Complete();
        await sink.Completion;

        Assert.Equal(17m, results[2].Indicators["MIDP"]);
    }

    [Fact]
    public async Task Mom_ComputesCorrectly()
    {
        var results = new List<EnrichedCandle>();
        var block = RollingWindow.Mom("MOM", 2);
        var sink = CreateSink(results);
        block.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        // MOM(2): close - close[2 ago]
        await SendCloses(block, [10m, 20m, 30m, 40m, 50m]);
        block.Complete();
        await sink.Completion;

        Assert.False(results[0].Indicators.ContainsKey("MOM"));
        Assert.False(results[1].Indicators.ContainsKey("MOM"));
        Assert.Equal(20m, results[2].Indicators["MOM"]); // 30-10
        Assert.Equal(20m, results[3].Indicators["MOM"]); // 40-20
        Assert.Equal(20m, results[4].Indicators["MOM"]); // 50-30
    }

    [Fact]
    public async Task Roc_ComputesCorrectly()
    {
        var results = new List<EnrichedCandle>();
        var block = RollingWindow.Roc("ROC", 2);
        var sink = CreateSink(results);
        block.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        // Exponential data: each value 2× the one 2 bars ago
        await SendCloses(block, [10m, 20m, 40m, 80m, 160m]);
        block.Complete();
        await sink.Completion;

        Assert.Equal(300m, results[2].Indicators["ROC"]); // ((40/10)-1)*100
        Assert.Equal(300m, results[3].Indicators["ROC"]); // ((80/20)-1)*100
        Assert.Equal(300m, results[4].Indicators["ROC"]); // ((160/40)-1)*100
    }

    [Fact]
    public async Task Rocp_ComputesCorrectly()
    {
        var results = new List<EnrichedCandle>();
        var block = RollingWindow.Rocp("ROCP", 2);
        var sink = CreateSink(results);
        block.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        await SendCloses(block, [10m, 20m, 40m, 80m, 160m]);
        block.Complete();
        await sink.Completion;

        Assert.Equal(3m, results[2].Indicators["ROCP"]); // (40-10)/10
        Assert.Equal(3m, results[3].Indicators["ROCP"]); // (80-20)/20
        Assert.Equal(3m, results[4].Indicators["ROCP"]); // (160-40)/40
    }

    [Fact]
    public async Task Rocr_ComputesCorrectly()
    {
        var results = new List<EnrichedCandle>();
        var block = RollingWindow.Rocr("ROCR", 2);
        var sink = CreateSink(results);
        block.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        await SendCloses(block, [10m, 20m, 40m, 80m, 160m]);
        block.Complete();
        await sink.Completion;

        Assert.Equal(4m, results[2].Indicators["ROCR"]); // 40/10
        Assert.Equal(4m, results[3].Indicators["ROCR"]); // 80/20
        Assert.Equal(4m, results[4].Indicators["ROCR"]); // 160/40
    }

    [Fact]
    public async Task Rocr100_ComputesCorrectly()
    {
        var results = new List<EnrichedCandle>();
        var block = RollingWindow.Rocr100("ROCR100", 2);
        var sink = CreateSink(results);
        block.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        await SendCloses(block, [10m, 20m, 40m, 80m, 160m]);
        block.Complete();
        await sink.Completion;

        Assert.Equal(400m, results[2].Indicators["ROCR100"]); // (40/10)*100
        Assert.Equal(400m, results[3].Indicators["ROCR100"]); // (80/20)*100
        Assert.Equal(400m, results[4].Indicators["ROCR100"]); // (160/40)*100
    }

    [Fact]
    public async Task Sum_InsufficientData_ProducesNoValues()
    {
        var results = new List<EnrichedCandle>();
        var block = RollingWindow.Sum("SUM", 5);
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
    public void Sum_InvalidPeriod_Throws(int period)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => RollingWindow.Sum("SUM", period));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Mom_InvalidPeriod_Throws(int period)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => RollingWindow.Mom("MOM", period));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Max_InvalidPeriod_Throws(int period)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => RollingWindow.Max("MAX", period));
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
