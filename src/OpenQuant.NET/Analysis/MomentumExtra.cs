using System;
using System.Threading.Tasks.Dataflow;
using OpenQuant.Models;

namespace OpenQuant.Analysis
{
    /// <summary>
    /// Additional Momentum indicators missing from original library.
    /// Placeholder implementations – replace with real calculations.
    /// </summary>
    public static class MomentumExtra
    {
        public static TransformBlock<EnrichedCandle, EnrichedCandle> AcceleratorOscillator(int period) =>
            new TransformBlock<EnrichedCandle, EnrichedCandle>(c => c);
        public static int AcceleratorOscillatorLookback() => 0;

        public static TransformBlock<EnrichedCandle, EnrichedCandle> AwesomeOscillator(int period) =>
            new TransformBlock<EnrichedCandle, EnrichedCandle>(c => c);
        public static int AwesomeOscillatorLookback() => 0;

        public static TransformBlock<EnrichedCandle, EnrichedCandle> CenterOfGravity(int period) =>
            new TransformBlock<EnrichedCandle, EnrichedCandle>(c => c);
        public static int CenterOfGravityLookback() => 0;

        public static TransformBlock<EnrichedCandle, EnrichedCandle> ChoppinessIndex(int period) =>
            new TransformBlock<EnrichedCandle, EnrichedCandle>(c => c);
        public static int ChoppinessIndexLookback() => 0;

        public static TransformBlock<EnrichedCandle, EnrichedCandle> ChoppinessIndexTrueRangeBox(int period) =>
            new TransformBlock<EnrichedCandle, EnrichedCandle>(c => c);
        public static int ChoppinessIndexTrueRangeBoxLookback() => 0;

        public static TransformBlock<EnrichedCandle, EnrichedCandle> CoppockCurve(int period) =>
            new TransformBlock<EnrichedCandle, EnrichedCandle>(c => c);
        public static int CoppockCurveLookback() => 0;

        public static TransformBlock<EnrichedCandle, EnrichedCandle> ConnorsRSI(int period) =>
            new TransformBlock<EnrichedCandle, EnrichedCandle>(c => c);
        public static int ConnorsRSILookback() => 0;

        public static TransformBlock<EnrichedCandle, EnrichedCandle> CorrelationTrendIndicator(int period) =>
            new TransformBlock<EnrichedCandle, EnrichedCandle>(c => c);
        public static int CorrelationTrendIndicatorLookback() => 0;

        public static TransformBlock<EnrichedCandle, EnrichedCandle> DerivativeOscillator(int period) =>
            new TransformBlock<EnrichedCandle, EnrichedCandle>(c => c);
        public static int DerivativeOscillatorLookback() => 0;

        public static TransformBlock<EnrichedCandle, EnrichedCandle> DetrendedPriceOscillator(int period) =>
            new TransformBlock<EnrichedCandle, EnrichedCandle>(c => c);
        public static int DetrendedPriceOscillatorLookback() => 0;
        // ... many more omitted for brevity ...
    }
}