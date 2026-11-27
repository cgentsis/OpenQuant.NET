using System;
using System.Threading.Tasks.Dataflow;
using OpenQuant.Models;

namespace OpenQuant.Analysis
{
    /// <summary>
    /// Additional Math Operators (ADD, DIV, MULT, SUB, CUMSUM) missing from original library.
    /// Placeholder implementations – replace with real calculations.
    /// </summary>
    public static class MathOperators
    {
        public static TransformBlock<EnrichedCandle, EnrichedCandle> Add(int period) =>
            new TransformBlock<EnrichedCandle, EnrichedCandle>(c => c);
        public static int AddLookback() => 0;

        public static TransformBlock<EnrichedCandle, EnrichedCandle> Div(int period) =>
            new TransformBlock<EnrichedCandle, EnrichedCandle>(c => c);
        public static int DivLookback() => 0;

        public static TransformBlock<EnrichedCandle, EnrichedCandle> Mult(int period) =>
            new TransformBlock<EnrichedCandle, EnrichedCandle>(c => c);
        public static int MultLookback() => 0;

        public static TransformBlock<EnrichedCandle, EnrichedCandle> Sub(int period) =>
            new TransformBlock<EnrichedCandle, EnrichedCandle>(c => c);
        public static int SubLookback() => 0;

        public static TransformBlock<EnrichedCandle, EnrichedCandle> CumSum(int period) =>
            new TransformBlock<EnrichedCandle, EnrichedCandle>(c => c);
        public static int CumSumLookback() => 0;
    }
}