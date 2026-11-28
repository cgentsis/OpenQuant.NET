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
        Assert.IsType<TransformBlock<EnrichedCandle, EnrichedCandle>>(MathTransforms.Acos(10));
    }

    [Fact]
    public void MathOperators_ReturnsBlock()
    {
        Assert.IsType<TransformBlock<EnrichedCandle, EnrichedCandle>>(MathOperators.Add(10));
    }

    [Fact]
    public void OverlapStudiesExtra_ReturnsBlock()
    {
        Assert.IsType<TransformBlock<EnrichedCandle, EnrichedCandle>>(OverlapStudiesExtra.AccelerationBands(10));
    }

    [Fact]
    public void MomentumExtra_ReturnsBlock()
    {
        Assert.IsType<TransformBlock<EnrichedCandle, EnrichedCandle>>(MomentumExtra.AcceleratorOscillator(10));
    }

    [Fact]
    public void VolumeExtra_ReturnsBlock()
    {
        Assert.IsType<TransformBlock<EnrichedCandle, EnrichedCandle>>(VolumeExtra.ChaikinMoneyFlow(10));
    }

    [Fact]
    public void VolatilityExtra_ReturnsBlock()
    {
        Assert.IsType<TransformBlock<EnrichedCandle, EnrichedCandle>>(VolatilityExtra.AverageDayRange(10));
    }
}
