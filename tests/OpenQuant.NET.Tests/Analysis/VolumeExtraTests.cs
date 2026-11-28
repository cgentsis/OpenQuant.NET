using OpenQuant.Analysis;

namespace OpenQuant.Tests.Analysis;

/// <summary>
/// Tests for the additional volume-based factories.
/// </summary>
public class VolumeExtraTests
{
    [Fact]
    public async Task ChaikinMoneyFlow_ComputesVolumeWeightedCloseLocationValue()
    {
        var inputs = new[]
        {
            ExtraIndicatorTestData.Ohlcv(10m, 11m, 9m, 11m, 100L),
            ExtraIndicatorTestData.Ohlcv(10m, 11m, 9m, 9m, 100L),
        };

        var results = await ExtraIndicatorTestData.RunAsync(
            VolumeExtra.ChaikinMoneyFlow(2, "CMF"),
            inputs);

        Assert.Equal(0m, results[1].Indicators["CMF"]);
    }

    [Fact]
    public async Task ElderForceIndex_ComputesPriceChangeTimesVolume()
    {
        var inputs = ExtraIndicatorTestData.Candles(
            new[] { 10m, 12m, 11m },
            new long[] { 100L, 200L, 300L });

        var results = await ExtraIndicatorTestData.RunAsync(
            VolumeExtra.ElderForceIndex(2, "EFI"),
            inputs);

        Assert.False(results[0].Indicators.ContainsKey("EFI"));
        Assert.Equal(400m, results[1].Indicators["EFI"]);
    }

    [Fact]
    public async Task ArmsEaseOfMovement_ComputesMidpointMovePerBoxRatio()
    {
        var inputs = new[]
        {
            ExtraIndicatorTestData.Ohlcv(10m, 11m, 9m, 10m, 100L),
            ExtraIndicatorTestData.Ohlcv(12m, 13m, 11m, 12m, 100L),
            ExtraIndicatorTestData.Ohlcv(14m, 15m, 13m, 14m, 100L),
        };

        var results = await ExtraIndicatorTestData.RunAsync(
            VolumeExtra.ArmsEaseOfMovement(2, name: "EOM"),
            inputs);

        Assert.Equal(400m, results[2].Indicators["EOM"]);
    }

    [Fact]
    public async Task KlingerVolumeOscillator_OutputsAfterBothEmaSeedsAreReady()
    {
        var inputs = ExtraIndicatorTestData.Candles(new[] { 10m, 11m, 12m, 13m, 14m });

        var results = await ExtraIndicatorTestData.RunAsync(
            VolumeExtra.KlingerVolumeOscillator(2, slowPeriod: 3, name: "KVO"),
            inputs);

        Assert.False(results[2].Indicators.ContainsKey("KVO"));
        Assert.True(results[3].Indicators.ContainsKey("KVO"));
    }

    [Fact]
    public async Task MarketFacilitationIndex_ComputesScaledRangePerVolume()
    {
        var input = ExtraIndicatorTestData.Ohlcv(10m, 11m, 9m, 10m, 100L);

        var results = await ExtraIndicatorTestData.RunAsync(
            VolumeExtra.MarketFacilitationIndex(14, scale: 100m, name: "MFI"),
            new[] { input });

        Assert.Equal(2m, results[0].Indicators["MFI"]);
    }

    [Fact]
    public async Task PositiveVolumeIndex_UpdatesOnlyOnHigherVolumeBars()
    {
        var inputs = ExtraIndicatorTestData.Candles(
            new[] { 10m, 11m, 12m },
            new long[] { 100L, 200L, 150L });

        var results = await ExtraIndicatorTestData.RunAsync(
            VolumeExtra.PositiveVolumeIndex(14, "PVI"),
            inputs);

        Assert.Equal(100m, results[0].Indicators["PVI"]);
        Assert.Equal(110m, results[1].Indicators["PVI"]);
        Assert.Equal(110m, results[2].Indicators["PVI"]);
    }

    [Fact]
    public async Task PositiveVolumeOscillator_ComputesPercentageChangeInPositiveVolumeIndex()
    {
        const decimal tolerance = 0.0001m;
        var inputs = ExtraIndicatorTestData.Candles(
            new[] { 10m, 11m, 12m, 13m },
            new long[] { 100L, 200L, 150L, 300L });

        var results = await ExtraIndicatorTestData.RunAsync(
            VolumeExtra.PositiveVolumeOscillator(2, "PVO"),
            inputs);

        Assert.InRange(results[3].Indicators["PVO"], 8.3333m - tolerance, 8.3333m + tolerance);
    }

    [Fact]
    public void Lookbacks_ReturnExpectedWarmupCounts()
    {
        Assert.Equal(13, VolumeExtra.ChaikinMoneyFlowLookback());
        Assert.Equal(1, VolumeExtra.ElderForceIndexLookback());
        Assert.Equal(14, VolumeExtra.ArmsEaseOfMovementLookback());
        Assert.Equal(34, VolumeExtra.KlingerVolumeOscillatorLookback());
        Assert.Equal(0, VolumeExtra.MarketFacilitationIndexLookback());
        Assert.Equal(0, VolumeExtra.PositiveVolumeIndexLookback());
        Assert.Equal(14, VolumeExtra.PositiveVolumeOscillatorLookback());
    }

    [Fact]
    public void Factories_RejectNonPositivePeriods()
    {
        var factories = new Action<int>[]
        {
            period => VolumeExtra.ChaikinMoneyFlow(period),
            period => VolumeExtra.ElderForceIndex(period),
            period => VolumeExtra.ArmsEaseOfMovement(period),
            period => VolumeExtra.KlingerVolumeOscillator(period),
            period => VolumeExtra.MarketFacilitationIndex(period),
            period => VolumeExtra.PositiveVolumeIndex(period),
            period => VolumeExtra.PositiveVolumeOscillator(period),
        };

        foreach (var factory in factories)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => factory(0));
            Assert.Throws<ArgumentOutOfRangeException>(() => factory(-1));
        }
    }
}
