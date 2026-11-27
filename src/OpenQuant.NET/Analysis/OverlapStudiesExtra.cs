using System;
using System.Threading.Tasks.Dataflow;
using OpenQuant.Models;

namespace OpenQuant.Analysis
{
    /// <summary>
    /// Additional Overlap Study indicators missing from original library.
    /// Placeholder implementations – replace with real calculations.
    /// </summary>
    public static class OverlapStudiesExtra
    {
        public static TransformBlock<EnrichedCandle, EnrichedCandle> AccelerationBands(int period) =>
            new TransformBlock<EnrichedCandle, EnrichedCandle>(c => c);
        public static int AccelerationBandsLookback() => 0;

        public static TransformBlock<EnrichedCandle, EnrichedCandle> ArnaudLegouxMA(int period) =>
            new TransformBlock<EnrichedCandle, EnrichedCandle>(c => c);
        public static int ArnaudLegouxMALookback() => 0;

        public static TransformBlock<EnrichedCandle, EnrichedCandle> ChandeKrollStop(int period) =>
            new TransformBlock<EnrichedCandle, EnrichedCandle>(c => c);
        public static int ChandeKrollStopLookback() => 0;

        public static TransformBlock<EnrichedCandle, EnrichedCandle> DonchianChannels(int period) =>
            new TransformBlock<EnrichedCandle, EnrichedCandle>(c => c);
        public static int DonchianChannelsLookback() => 0;

        public static TransformBlock<EnrichedCandle, EnrichedCandle> FractalAdaptiveMA(int period) =>
            new TransformBlock<EnrichedCandle, EnrichedCandle>(c => c);
        public static int FractalAdaptiveMALookback() => 0;

        public static TransformBlock<EnrichedCandle, EnrichedCandle> IchimokuKinkoHyo(int period) =>
            new TransformBlock<EnrichedCandle, EnrichedCandle>(c => c);
        public static int IchimokuKinkoHyoLookback() => 0;

        public static TransformBlock<EnrichedCandle, EnrichedCandle> KeltnerChannels(int period) =>
            new TransformBlock<EnrichedCandle, EnrichedCandle>(c => c);
        public static int KeltnerChannelsLookback() => 0;

        public static TransformBlock<EnrichedCandle, EnrichedCandle> MESAAdaptiveMA(int period) =>
            new TransformBlock<EnrichedCandle, EnrichedCandle>(c => c);
        public static int MESAAdaptiveMALookback() => 0;

        public static TransformBlock<EnrichedCandle, EnrichedCandle> McGinleyDynamic(int period) =>
            new TransformBlock<EnrichedCandle, EnrichedCandle>(c => c);
        public static int McGinleyDynamicLookback() => 0;

        public static TransformBlock<EnrichedCandle, EnrichedCandle> VWMA(int period) =>
            new TransformBlock<EnrichedCandle, EnrichedCandle>(c => c);
        public static int VWMA_Lookback() => 0;

        public static TransformBlock<EnrichedCandle, EnrichedCandle> ZIGZAG(int period) =>
            new TransformBlock<EnrichedCandle, EnrichedCandle>(c => c);
        public static int ZIGZAG_Lookback() => 0;
    }
}