using System;
using System.Threading.Tasks.Dataflow;
using OpenQuant.Models;

namespace OpenQuant.Analysis
{
    /// <summary>
    /// Additional Math Transform functions missing from the original library.
    /// Each returns a TransformBlock that forwards the input candle unchanged.
    /// Placeholder implementations – replace with real calculations.
    /// </summary>
    public static class MathTransforms
    {
        public static TransformBlock<EnrichedCandle, EnrichedCandle> Acos(int period) =>
            new TransformBlock<EnrichedCandle, EnrichedCandle>(c => c);
        public static int AcosLookback() => 0;

        public static TransformBlock<EnrichedCandle, EnrichedCandle> Asin(int period) =>
            new TransformBlock<EnrichedCandle, EnrichedCandle>(c => c);
        public static int AsinLookback() => 0;

        public static TransformBlock<EnrichedCandle, EnrichedCandle> Atan(int period) =>
            new TransformBlock<EnrichedCandle, EnrichedCandle>(c => c);
        public static int AtanLookback() => 0;

        public static TransformBlock<EnrichedCandle, EnrichedCandle> Ceil(int period) =>
            new TransformBlock<EnrichedCandle, EnrichedCandle>(c => c);
        public static int CeilLookback() => 0;

        public static TransformBlock<EnrichedCandle, EnrichedCandle> Cos(int period) =>
            new TransformBlock<EnrichedCandle, EnrichedCandle>(c => c);
        public static int CosLookback() => 0;

        public static TransformBlock<EnrichedCandle, EnrichedCandle> Cosh(int period) =>
            new TransformBlock<EnrichedCandle, EnrichedCandle>(c => c);
        public static int CoshLookback() => 0;

        public static TransformBlock<EnrichedCandle, EnrichedCandle> Exp(int period) =>
            new TransformBlock<EnrichedCandle, EnrichedCandle>(c => c);
        public static int ExpLookback() => 0;

        public static TransformBlock<EnrichedCandle, EnrichedCandle> Floor(int period) =>
            new TransformBlock<EnrichedCandle, EnrichedCandle>(c => c);
        public static int FloorLookback() => 0;

        public static TransformBlock<EnrichedCandle, EnrichedCandle> Ln(int period) =>
            new TransformBlock<EnrichedCandle, EnrichedCandle>(c => c);
        public static int LnLookback() => 0;

        public static TransformBlock<EnrichedCandle, EnrichedCandle> Log10(int period) =>
            new TransformBlock<EnrichedCandle, EnrichedCandle>(c => c);
        public static int Log10Lookback() => 0;

        public static TransformBlock<EnrichedCandle, EnrichedCandle> Sin(int period) =>
            new TransformBlock<EnrichedCandle, EnrichedCandle>(c => c);
        public static int SinLookback() => 0;

        public static TransformBlock<EnrichedCandle, EnrichedCandle> Sinh(int period) =>
            new TransformBlock<EnrichedCandle, EnrichedCandle>(c => c);
        public static int SinhLookback() => 0;

        public static TransformBlock<EnrichedCandle, EnrichedCandle> Sqrt(int period) =>
            new TransformBlock<EnrichedCandle, EnrichedCandle>(c => c);
        public static int SqrtLookback() => 0;

        public static TransformBlock<EnrichedCandle, EnrichedCandle> Tan(int period) =>
            new TransformBlock<EnrichedCandle, EnrichedCandle>(c => c);
        public static int TanLookback() => 0;

        public static TransformBlock<EnrichedCandle, EnrichedCandle> Tanh(int period) =>
            new TransformBlock<EnrichedCandle, EnrichedCandle>(c => c);
        public static int TanhLookback() => 0;
    }
}