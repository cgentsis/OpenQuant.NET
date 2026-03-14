using System.Threading.Tasks.Dataflow;

namespace OpenQuant.Analysis;

/// <summary>
/// Internal helpers for TPL Dataflow block wiring.
/// </summary>
internal static class DataflowHelpers
{
    /// <summary>
    /// Awaits completion of <paramref name="source"/> and propagates the outcome
    /// (success or fault) to <paramref name="target"/>. Replaces the error-prone
    /// <c>ContinueWith</c> pattern with a straightforward async helper.
    /// </summary>
    internal static async Task PropagateCompletionAsync(IDataflowBlock source, IDataflowBlock target)
    {
        try
        {
            await source.Completion.ConfigureAwait(false);
            target.Complete();
        }
        catch (Exception ex)
        {
            target.Fault(ex);
        }
    }
}
