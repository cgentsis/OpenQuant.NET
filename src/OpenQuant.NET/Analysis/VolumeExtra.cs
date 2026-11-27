using System;
using System.Threading.Tasks.Dataflow;
using OpenQuant.Models;

namespace OpenQuant.Analysis
{
    /// <summary>
    /// Additional Volume indicators missing from original library.
    /// Placeholder implementations – replace with real calculations.
    /// </summary>
    public static class VolumeExtra
    {
        public static TransformBlock<EnrichedCandle, EnrichedCandle> ChaikinMoneyFlow(int period) =>
            new TransformBlock<EnrichedCandle, EnrichedCandle>(c => c);
        public static int ChaikinMoneyFlowLookback() => 0;

        public static TransformBlock<EnrichedCandle, EnrichedCandle> ElderForceIndex(int period) =>
            new TransformBlock<EnrichedCandle, EnrichedCandle>(c => c);
        public static int ElderForceIndexLookback() => 0;

        public static TransformBlock<EnrichedCandle, EnrichedCandle> ArmsEaseOfMovement(int period) =>
            new TransformBlock<EnrichedCandle, EnrichedCandle>(c => c);
        public static int ArmsEaseOfMovementLookback() => 0;

        public static TransformBlock<EnrichedCandle, EnrichedCandle> KlingerVolumeOscillator(int period) =>
            new TransformBlock<EnrichedCandle, EnrichedCandle>(c => c);
        public static int KlingerVolumeOscillatorLookback() => 0;

        public static TransformBlock<EnrichedCandle, EnrichedCandle> MarketFacilitationIndex(int period) =>
            new TransformBlock<EnrichedCandle, EnrichedCandle>(c => c);
        public static int MarketFacilitationIndexLookback() => 0;

        public static TransformBlock<EnrichedCandle, EnrichedCandle> PositiveVolumeIndex(int period) =>
            new TransformBlock<EnrichedCandle, EnrichedCandle>(c => c);
        public static int PositiveVolumeIndexLookback() => 0;

        public static TransformBlock<EnrichedCandle, EnrichedCandle> PositiveVolumeOscillator(int period) =>
            new TransformBlock<EnrichedCandle, EnrichedCandle>(c => c);
        public static int PositiveVolumeOscillatorLookback() => 0;
        // ... more omitted ...
    }
}