using NUnit.Framework;
using System.Threading.Tasks.Dataflow;
using OpenQuant.Analysis;
using OpenQuant.Models;

namespace OpenQuant.Tests.Analysis
{
    [TestFixture]
    public class ExtraIndicatorsTests
    {
        [Test]
        public void MathTransforms_ReturnsBlock()
        {
            var block = MathTransforms.Acos(10);
            Assert.IsInstanceOf<TransformBlock<EnrichedCandle, EnrichedCandle>>(block);
        }

        [Test]
        public void MathOperators_ReturnsBlock()
        {
            var block = MathOperators.Add(10);
            Assert.IsInstanceOf<TransformBlock<EnrichedCandle, EnrichedCandle>>(block);
        }

        [Test]
        public void OverlapStudiesExtra_ReturnsBlock()
        {
            var block = OverlapStudiesExtra.AccelerationBands(10);
            Assert.IsInstanceOf<TransformBlock<EnrichedCandle, EnrichedCandle>>(block);
        }

        [Test]
        public void MomentumExtra_ReturnsBlock()
        {
            var block = MomentumExtra.AcceleratorOscillator(10);
            Assert.IsInstanceOf<TransformBlock<EnrichedCandle, EnrichedCandle>>(block);
        }

        [Test]
        public void VolumeExtra_ReturnsBlock()
        {
            var block = VolumeExtra.ChaikinMoneyFlow(10);
            Assert.IsInstanceOf<TransformBlock<EnrichedCandle, EnrichedCandle>>(block);
        }

        [Test]
        public void VolatilityExtra_ReturnsBlock()
        {
            var block = VolatilityExtra.AverageDayRange(10);
            Assert.IsInstanceOf<TransformBlock<EnrichedCandle, EnrichedCandle>>(block);
        }
    }
}
