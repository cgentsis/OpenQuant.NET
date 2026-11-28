using OpenQuant.Analysis;

namespace OpenQuant.Tests.Analysis;

/// <summary>
/// Tests for the additional momentum and trend-strength factories.
/// </summary>
public class MomentumExtraTests
{
    [Fact]
    public async Task AwesomeOscillator_ComputesFastMinusSlowMedianAverage()
    {
        const decimal tolerance = 0.0001m;
        var inputs = ExtraIndicatorTestData.Candles(new[] { 10m, 11m, 12m });

        var results = await ExtraIndicatorTestData.RunAsync(
            MomentumExtra.AwesomeOscillator(2, slowPeriod: 3, name: "AO"),
            inputs);

        Assert.InRange(results[2].Indicators["AO"], 0.5m - tolerance, 0.5m + tolerance);
    }

    [Fact]
    public async Task AcceleratorOscillator_ComputesAwesomeOscillatorMinusItsAverage()
    {
        const decimal tolerance = 0.0001m;
        var inputs = ExtraIndicatorTestData.Candles(new[] { 10m, 11m, 12m, 13m });

        var results = await ExtraIndicatorTestData.RunAsync(
            MomentumExtra.AcceleratorOscillator(2, slowPeriod: 3, signalPeriod: 2, name: "AC"),
            inputs);

        Assert.InRange(results[3].Indicators["AC"], 0m - tolerance, 0m + tolerance);
    }

    [Fact]
    public async Task CenterOfGravity_ComputesNegativeWeightedAverage()
    {
        const decimal tolerance = 0.0001m;
        var inputs = ExtraIndicatorTestData.Candles(new[] { 10m, 20m, 30m });

        var results = await ExtraIndicatorTestData.RunAsync(
            MomentumExtra.CenterOfGravity(3, "COG"),
            inputs);

        Assert.InRange(results[2].Indicators["COG"], -2.3333m - tolerance, -2.3333m + tolerance);
    }

    [Fact]
    public async Task ChoppinessIndex_OutputsAfterWarmup()
    {
        var inputs = ExtraIndicatorTestData.Candles(new[] { 10m, 12m, 11m, 14m, 13m });

        var results = await ExtraIndicatorTestData.RunAsync(
            MomentumExtra.ChoppinessIndex(3, "CHI"),
            inputs);

        Assert.False(results[2].Indicators.ContainsKey("CHI"));
        Assert.True(results[3].Indicators.ContainsKey("CHI"));
    }

    [Fact]
    public async Task ChoppinessIndexTrueRangeBox_OutputsAfterWarmup()
    {
        var inputs = ExtraIndicatorTestData.Candles(new[] { 10m, 12m, 11m, 14m, 13m });

        var results = await ExtraIndicatorTestData.RunAsync(
            MomentumExtra.ChoppinessIndexTrueRangeBox(3, "CHITRB"),
            inputs);

        Assert.False(results[2].Indicators.ContainsKey("CHITRB"));
        Assert.True(results[3].Indicators.ContainsKey("CHITRB"));
    }

    [Fact]
    public async Task CoppockCurve_OutputsAfterRateOfChangeAndSmoothingWarmup()
    {
        var inputs = ExtraIndicatorTestData.Candles(new[] { 10m, 11m, 13m, 15m });

        var results = await ExtraIndicatorTestData.RunAsync(
            MomentumExtra.CoppockCurve(2, longPeriod: 1, shortPeriod: 2, name: "COP"),
            inputs);

        Assert.False(results[2].Indicators.ContainsKey("COP"));
        Assert.True(results[3].Indicators.ContainsKey("COP"));
    }

    [Fact]
    public async Task ConnorsRSI_OutputsAfterAllThreeComponentsAreReady()
    {
        var inputs = ExtraIndicatorTestData.Candles(new[] { 10m, 11m, 12m, 13m, 14m, 15m });

        var results = await ExtraIndicatorTestData.RunAsync(
            MomentumExtra.ConnorsRSI(2, rsiPeriod: 2, name: "CRSI"),
            inputs);

        Assert.False(results[1].Indicators.ContainsKey("CRSI"));
        Assert.True(results[2].Indicators.ContainsKey("CRSI"));
        Assert.InRange(results[5].Indicators["CRSI"], 0m, 100m);
    }

    [Fact]
    public async Task CorrelationTrendIndicator_ApproachesOneForMonotonicPrices()
    {
        const decimal tolerance = 0.0001m;
        var inputs = ExtraIndicatorTestData.Candles(new[] { 10m, 11m, 12m, 13m });

        var results = await ExtraIndicatorTestData.RunAsync(
            MomentumExtra.CorrelationTrendIndicator(3, "CTI"),
            inputs);

        Assert.InRange(results[3].Indicators["CTI"], 1m - tolerance, 1m + tolerance);
    }

    [Fact]
    public async Task DerivativeOscillator_ComputesFastMinusSlowAverage()
    {
        var inputs = ExtraIndicatorTestData.Candles(new[] { 10m, 20m, 30m });

        var results = await ExtraIndicatorTestData.RunAsync(
            MomentumExtra.DerivativeOscillator(2, slowPeriod: 3, name: "DO"),
            inputs);

        Assert.Equal(5m, results[2].Indicators["DO"]);
    }

    [Fact]
    public async Task DetrendedPriceOscillator_ComparesPastPriceWithCenteredAverage()
    {
        var inputs = ExtraIndicatorTestData.Candles(new[] { 10m, 11m, 12m, 13m, 14m });

        var results = await ExtraIndicatorTestData.RunAsync(
            MomentumExtra.DetrendedPriceOscillator(3, "DPO"),
            inputs);

        Assert.False(results[3].Indicators.ContainsKey("DPO"));
        Assert.Equal(1m, results[4].Indicators["DPO"]);
    }

    [Fact]
    public void Lookbacks_ReturnExpectedWarmupCounts()
    {
        Assert.Equal(33, MomentumExtra.AwesomeOscillatorLookback());
        Assert.Equal(37, MomentumExtra.AcceleratorOscillatorLookback());
        Assert.Equal(13, MomentumExtra.CenterOfGravityLookback());
        Assert.Equal(14, MomentumExtra.ChoppinessIndexLookback());
        Assert.Equal(14, MomentumExtra.ChoppinessIndexTrueRangeBoxLookback());
        Assert.Equal(27, MomentumExtra.CoppockCurveLookback());
        Assert.Equal(15, MomentumExtra.ConnorsRSILookback());
        Assert.Equal(13, MomentumExtra.CorrelationTrendIndicatorLookback());
        Assert.Equal(13, MomentumExtra.DerivativeOscillatorLookback());
        Assert.Equal(21, MomentumExtra.DetrendedPriceOscillatorLookback());
    }

    [Fact]
    public void Factories_RejectNonPositivePeriods()
    {
        var factories = new Action<int>[]
        {
            period => MomentumExtra.AwesomeOscillator(period),
            period => MomentumExtra.AcceleratorOscillator(period),
            period => MomentumExtra.CenterOfGravity(period),
            period => MomentumExtra.ChoppinessIndex(period),
            period => MomentumExtra.ChoppinessIndexTrueRangeBox(period),
            period => MomentumExtra.CoppockCurve(period),
            period => MomentumExtra.ConnorsRSI(period),
            period => MomentumExtra.CorrelationTrendIndicator(period),
            period => MomentumExtra.DerivativeOscillator(period),
            period => MomentumExtra.DetrendedPriceOscillator(period),
        };

        foreach (var factory in factories)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => factory(0));
            Assert.Throws<ArgumentOutOfRangeException>(() => factory(-1));
        }
    }
}
