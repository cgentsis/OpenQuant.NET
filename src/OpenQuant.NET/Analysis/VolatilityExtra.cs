using System;
using System.Threading.Tasks.Dataflow;
using OpenQuant.Models;

namespace OpenQuant.Analysis
{
    /// <summary>
    /// Additional Volatility indicators missing from original library.
    /// Placeholder implementations – replace with real calculations.
    /// </summary>
    public static class VolatilityExtra
    {
        public static TransformBlock<EnrichedCandle, EnrichedCandle> AverageDayRange(int period) =>
            new TransformBlock<EnrichedCandle, EnrichedCandle>(c => c);
        public static int AverageDayRangeLookback() => 0;

        public static TransformBlock<EnrichedCandle, EnrichedCandle> BollingerBandWidth(int period) =>
            new TransformBlock<EnrichedCandle, EnrichedCandle>(c => c);
        public static int BollingerBandWidthLookback() => 0;

        public static TransformBlock<EnrichedCandle, EnrichedCandle> ChaikinVolatility(int period) =>
            new TransformBlock<EnrichedCandle, EnrichedCandle>(c => c);
        public static int ChaikinVolatilityLookback() => 0;

        public static TransformBlock<EnrichedCandle, EnrichedCandle> MassIndex(int period) =>
            new TransformBlock<EnrichedCandle, EnrichedCandle>(c => c);
        public static int MassIndexLookback() => 0;

        public static TransformBlock<EnrichedCandle, EnrichedCandle> PercentB(int period) =>
            new TransformBlock<EnrichedCandle, EnrichedCandle>(c => c);
        public static int PercentBLookback() => 0;
        // ... more omitted ...
    }
}