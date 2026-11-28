using OpenQuant.Analysis;

namespace OpenQuant.Tests.Analysis;

/// <summary>
/// Tests for the additional volatility factories.
/// </summary>
public class VolatilityExtraTests
{
    [Fact]
    public async Task AverageDayRange_ComputesAverageHighLowRange()
    {
        var inputs = new[]
        {
            ExtraIndicatorTestData.Ohlcv(10m, 11m, 9m, 10m),
            ExtraIndicatorTestData.Ohlcv(12m, 14m, 10m, 12m),
            ExtraIndicatorTestData.Ohlcv(14m, 17m, 11m, 14m),
        };

        var results = await ExtraIndicatorTestData.RunAsync(
            VolatilityExtra.AverageDayRange(3, "ADR"),
            inputs);

        Assert.Equal(4m, results[2].Indicators["ADR"]);
    }

    [Fact]
    public async Task BollingerBandWidth_ComputesBandsAndRelativeWidth()
    {
        const decimal tolerance = 0.0001m;
        var inputs = ExtraIndicatorTestData.Candles(new[] { 10m, 20m });

        var results = await ExtraIndicatorTestData.RunAsync(
            VolatilityExtra.BollingerBandWidth(2, nbDev: 2m, name: "BW"),
            inputs);

        var width = results[1].Indicators["BW"];
        Assert.InRange(width, 133.3333m - tolerance, 133.3333m + tolerance);
        Assert.Equal(width, results[1].Indicators["BW_Width"]);
        Assert.Equal(25m, results[1].Indicators["BW_Upper"]);
        Assert.Equal(15m, results[1].Indicators["BW_Middle"]);
        Assert.Equal(5m, results[1].Indicators["BW_Lower"]);
    }

    [Fact]
    public async Task ChaikinVolatility_OutputsAfterEmaAndDisplacementWarmup()
    {
        var inputs = ExtraIndicatorTestData.Candles(new[] { 10m, 12m, 15m });

        var results = await ExtraIndicatorTestData.RunAsync(
            VolatilityExtra.ChaikinVolatility(1, emaPeriod: 2, name: "CV"),
            inputs);

        Assert.False(results[1].Indicators.ContainsKey("CV"));
        Assert.True(results[2].Indicators.ContainsKey("CV"));
    }

    [Fact]
    public async Task MassIndex_OutputsAfterEmaAndSummationWarmup()
    {
        var inputs = ExtraIndicatorTestData.Candles(new[] { 10m, 12m, 15m });

        var results = await ExtraIndicatorTestData.RunAsync(
            VolatilityExtra.MassIndex(2, emaPeriod: 2, name: "MI"),
            inputs);

        Assert.False(results[1].Indicators.ContainsKey("MI"));
        Assert.True(results[2].Indicators.ContainsKey("MI"));
        Assert.True(results[2].Indicators["MI"] > 0m);
    }

    [Fact]
    public async Task PercentB_ComputesPositionWithinBands()
    {
        const decimal tolerance = 0.0001m;
        var inputs = ExtraIndicatorTestData.Candles(new[] { 10m, 20m });

        var results = await ExtraIndicatorTestData.RunAsync(
            VolatilityExtra.PercentB(2, nbDev: 2m, name: "PB"),
            inputs);

        Assert.InRange(results[1].Indicators["PB"], 0.75m - tolerance, 0.75m + tolerance);
        Assert.Equal(25m, results[1].Indicators["PB_Upper"]);
        Assert.Equal(15m, results[1].Indicators["PB_Middle"]);
        Assert.Equal(5m, results[1].Indicators["PB_Lower"]);
    }

    [Fact]
    public void Lookbacks_ReturnExpectedWarmupCounts()
    {
        Assert.Equal(13, VolatilityExtra.AverageDayRangeLookback());
        Assert.Equal(13, VolatilityExtra.BollingerBandWidthLookback());
        Assert.Equal(23, VolatilityExtra.ChaikinVolatilityLookback());
        Assert.Equal(32, VolatilityExtra.MassIndexLookback());
        Assert.Equal(13, VolatilityExtra.PercentBLookback());
    }

    [Fact]
    public void Factories_RejectNonPositivePeriods()
    {
        var factories = new Action<int>[]
        {
            period => VolatilityExtra.AverageDayRange(period),
            period => VolatilityExtra.BollingerBandWidth(period),
            period => VolatilityExtra.ChaikinVolatility(period),
            period => VolatilityExtra.MassIndex(period),
            period => VolatilityExtra.PercentB(period),
        };

        foreach (var factory in factories)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => factory(0));
            Assert.Throws<ArgumentOutOfRangeException>(() => factory(-1));
        }
    }
}
