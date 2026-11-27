using System.Threading.Tasks.Dataflow;
using OpenQuant.Analysis;
using OpenQuant.Models;

namespace OpenQuant.Tests.Analysis;

/// <summary>
/// Smoke tests confirming every extra-indicator facade returns a usable dataflow block.
/// </summary>
public class ExtraIndicatorsTests
{
    [Fact]
    public void MathTransforms_ReturnsBlock()
    {
        Assert.IsType<TransformBlock<EnrichedCandle, EnrichedCandle>>(MathTransforms.Acos("test"));
    }

    [Fact]
    public void MathOperators_ReturnsBlock()
    {
        Assert.IsType<TransformBlock<EnrichedCandle, EnrichedCandle>>(MathOperators.Add("test"));
    }

    [Fact]
    public void OverlapStudiesExtra_ReturnsBlock()
    {
        Assert.IsType<TransformBlock<EnrichedCandle, EnrichedCandle>>(OverlapStudiesExtra.AccelerationBands("test", 10));
    }

    [Fact]
    public void MomentumExtra_ReturnsBlock()
    {
        Assert.IsType<TransformBlock<EnrichedCandle, EnrichedCandle>>(MomentumExtra.AcceleratorOscillator("test", 10));
    }

    [Fact]
    public void VolumeExtra_ReturnsBlock()
    {
        Assert.IsType<TransformBlock<EnrichedCandle, EnrichedCandle>>(VolumeExtra.ChaikinMoneyFlow("test", 10));
    }

    [Fact]
    public void VolatilityExtra_ReturnsBlock()
    {
        Assert.IsType<TransformBlock<EnrichedCandle, EnrichedCandle>>(VolatilityExtra.AverageDayRange("test", 10));
    }
}
