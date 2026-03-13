using System.Text.Json.Serialization;

namespace OpenQuant.Providers.YahooFinance.Dto;

internal sealed record YahooChart
{
    [JsonPropertyName("result")]
    public List<YahooChartResult>? Result { get; init; }

    [JsonPropertyName("error")]
    public YahooError? Error { get; init; }
}
