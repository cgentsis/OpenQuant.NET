using System.Text.Json.Serialization;

namespace OpenQuant.Providers.YahooFinance.Dto;

internal sealed record YahooError
{
    [JsonPropertyName("code")]
    public string? Code { get; init; }

    [JsonPropertyName("description")]
    public string? Description { get; init; }
}
