using OpenQuant.Analysis;

namespace OpenQuant.Tests.Analysis;

/// <summary>
/// Tests for the additional overlapping-study factories.
/// </summary>
public class OverlapStudiesExtraTests
{
    [Fact]
    public async Task AccelerationBands_OutputsAllBands()
    {
        var inputs = ExtraIndicatorTestData.Candles(new[] { 10m, 11m, 12m });

        var results = await ExtraIndicatorTestData.RunAsync(
            OverlapStudiesExtra.AccelerationBands(3, name: "AB"),
            inputs);

        Assert.True(results[2].Indicators.ContainsKey("AB_Middle"));
        Assert.True(results[2].Indicators.ContainsKey("AB_Upper"));
        Assert.True(results[2].Indicators.ContainsKey("AB_Lower"));
    }

    [Fact]
    public async Task ArnaudLegouxMA_OutputsAfterWarmup()
    {
        var inputs = ExtraIndicatorTestData.Candles(new[] { 10m, 11m, 12m });

        var results = await ExtraIndicatorTestData.RunAsync(
            OverlapStudiesExtra.ArnaudLegouxMA(3, name: "ALMA"),
            inputs);

        Assert.False(results[1].Indicators.ContainsKey("ALMA"));
        Assert.True(results[2].Indicators.ContainsKey("ALMA"));
    }

    [Fact]
    public async Task ChandeKrollStop_OutputsBothStops()
    {
        var inputs = ExtraIndicatorTestData.Candles(new[] { 10m, 11m, 12m, 13m });

        var results = await ExtraIndicatorTestData.RunAsync(
            OverlapStudiesExtra.ChandeKrollStop(2, rangePeriod: 2, name: "CKS"),
            inputs);

        Assert.True(results[3].Indicators.ContainsKey("CKS_Short"));
        Assert.True(results[3].Indicators.ContainsKey("CKS_Long"));
    }

    [Fact]
    public async Task DonchianChannels_OutputsUpperLowerAndMiddle()
    {
        var inputs = new[]
        {
            ExtraIndicatorTestData.Ohlcv(10m, 11m, 9m, 10m),
            ExtraIndicatorTestData.Ohlcv(12m, 13m, 11m, 12m),
            ExtraIndicatorTestData.Ohlcv(16m, 17m, 15m, 16m),
        };

        var results = await ExtraIndicatorTestData.RunAsync(
            OverlapStudiesExtra.DonchianChannels(3, "DC"),
            inputs);

        Assert.Equal(17m, results[2].Indicators["DC_Upper"]);
        Assert.Equal(9m, results[2].Indicators["DC_Lower"]);
        Assert.Equal(13m, results[2].Indicators["DC_Middle"]);
    }

    [Fact]
    public async Task FractalAdaptiveMA_OutputsAfterFullWindow()
    {
        var inputs = ExtraIndicatorTestData.Candles(new[] { 10m, 11m, 12m, 13m });

        var results = await ExtraIndicatorTestData.RunAsync(
            OverlapStudiesExtra.FractalAdaptiveMA(4, "FRAMA"),
            inputs);

        Assert.False(results[2].Indicators.ContainsKey("FRAMA"));
        Assert.True(results[3].Indicators.ContainsKey("FRAMA"));
    }

    [Fact]
    public async Task IchimokuKinkoHyo_OutputsAllFourLines()
    {
        var inputs = ExtraIndicatorTestData.Candles(new[] { 10m, 11m, 12m, 13m, 14m, 15m, 16m, 17m });

        var results = await ExtraIndicatorTestData.RunAsync(
            OverlapStudiesExtra.IchimokuKinkoHyo(2, name: "ICH"),
            inputs);

        Assert.True(results[7].Indicators.ContainsKey("ICH_Tenkan"));
        Assert.True(results[7].Indicators.ContainsKey("ICH_Kijun"));
        Assert.True(results[7].Indicators.ContainsKey("ICH_SpanA"));
        Assert.True(results[7].Indicators.ContainsKey("ICH_SpanB"));
    }

    [Fact]
    public async Task KeltnerChannels_OutputsAllBands()
    {
        var inputs = ExtraIndicatorTestData.Candles(new[] { 10m, 11m, 12m, 13m });

        var results = await ExtraIndicatorTestData.RunAsync(
            OverlapStudiesExtra.KeltnerChannels(3, name: "KC"),
            inputs);

        var upper = results[3].Indicators["KC_Upper"];
        var middle = results[3].Indicators["KC_Middle"];
        var lower = results[3].Indicators["KC_Lower"];

        Assert.True(upper > middle);
        Assert.True(middle > lower);
    }

    [Fact]
    public async Task MESAAdaptiveMA_OutputsFromFirstBar()
    {
        var results = await ExtraIndicatorTestData.RunAsync(
            OverlapStudiesExtra.MESAAdaptiveMA(3, "MAMA"),
            new[] { ExtraIndicatorTestData.Candle(10m) });

        Assert.Equal(10m, results[0].Indicators["MAMA"]);
    }

    [Fact]
    public async Task McGinleyDynamic_OutputsFromFirstBar()
    {
        var results = await ExtraIndicatorTestData.RunAsync(
            OverlapStudiesExtra.McGinleyDynamic(3, "MD"),
            new[] { ExtraIndicatorTestData.Candle(10m) });

        Assert.Equal(10m, results[0].Indicators["MD"]);
    }

    [Fact]
    public async Task Vwma_ComputesVolumeWeightedAverage()
    {
        const decimal tolerance = 0.0001m;
        var inputs = ExtraIndicatorTestData.Candles(
            new[] { 10m, 20m, 30m },
            new long[] { 1L, 2L, 3L });

        var results = await ExtraIndicatorTestData.RunAsync(
            OverlapStudiesExtra.Vwma(3, "VWMA3"),
            inputs);

        Assert.InRange(results[2].Indicators["VWMA3"], 23.3333m - tolerance, 23.3333m + tolerance);
    }

    [Fact]
    public async Task Zigzag_OutputsPriceAndDirection()
    {
        var results = await ExtraIndicatorTestData.RunAsync(
            OverlapStudiesExtra.Zigzag(2, name: "ZZ"),
            new[] { ExtraIndicatorTestData.Candle(10m) });

        Assert.Equal(10m, results[0].Indicators["ZZ_Zigzag"]);
        Assert.Equal(1m, results[0].Indicators["ZZ_Direction"]);
    }

    [Fact]
    public async Task ObsoleteAliases_RemainAvailableWithAliasKeys()
    {
#pragma warning disable CS0618
        var vwmaResults = await ExtraIndicatorTestData.RunAsync(
            OverlapStudiesExtra.VWMA(2),
            ExtraIndicatorTestData.Candles(new[] { 10m, 20m }));

        var zigzagResults = await ExtraIndicatorTestData.RunAsync(
            OverlapStudiesExtra.ZIGZAG(2),
            new[] { ExtraIndicatorTestData.Candle(10m) });
#pragma warning restore CS0618

        Assert.True(vwmaResults[1].Indicators.ContainsKey("VWMA"));
        Assert.True(zigzagResults[0].Indicators.ContainsKey("ZIGZAG_Zigzag"));
        Assert.True(zigzagResults[0].Indicators.ContainsKey("ZIGZAG_Direction"));
    }

    [Fact]
    public void Lookbacks_CanBeCalledWithoutArguments()
    {
        Assert.Equal(13, OverlapStudiesExtra.AccelerationBandsLookback());
        Assert.Equal(13, OverlapStudiesExtra.ArnaudLegouxMALookback());
        Assert.Equal(13, OverlapStudiesExtra.ChandeKrollStopLookback());
        Assert.Equal(13, OverlapStudiesExtra.DonchianChannelsLookback());
        Assert.Equal(13, OverlapStudiesExtra.FractalAdaptiveMALookback());
        Assert.Equal(41, OverlapStudiesExtra.IchimokuKinkoHyoLookback());
        Assert.Equal(14, OverlapStudiesExtra.KeltnerChannelsLookback());
        Assert.Equal(0, OverlapStudiesExtra.MESAAdaptiveMALookback());
        Assert.Equal(0, OverlapStudiesExtra.McGinleyDynamicLookback());
        Assert.Equal(13, OverlapStudiesExtra.VwmaLookback());
        Assert.Equal(0, OverlapStudiesExtra.ZigzagLookback());

#pragma warning disable CS0618
        Assert.Equal(19, OverlapStudiesExtra.VWMA_Lookback());
        Assert.Equal(0, OverlapStudiesExtra.ZIGZAG_Lookback());
#pragma warning restore CS0618
    }

    [Fact]
    public void Factories_RejectNonPositivePeriods()
    {
        var factories = new Action<int>[]
        {
            period => OverlapStudiesExtra.AccelerationBands(period),
            period => OverlapStudiesExtra.ArnaudLegouxMA(period),
            period => OverlapStudiesExtra.ChandeKrollStop(period),
            period => OverlapStudiesExtra.DonchianChannels(period),
            period => OverlapStudiesExtra.FractalAdaptiveMA(period),
            period => OverlapStudiesExtra.IchimokuKinkoHyo(period),
            period => OverlapStudiesExtra.KeltnerChannels(period),
            period => OverlapStudiesExtra.MESAAdaptiveMA(period),
            period => OverlapStudiesExtra.McGinleyDynamic(period),
            period => OverlapStudiesExtra.Vwma(period),
            period => OverlapStudiesExtra.Zigzag(period),
        };

        foreach (var factory in factories)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => factory(0));
            Assert.Throws<ArgumentOutOfRangeException>(() => factory(-1));
        }
    }
}
