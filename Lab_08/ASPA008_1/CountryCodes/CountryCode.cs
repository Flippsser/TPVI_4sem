using System.Text.Json.Serialization;

namespace ASPA008_1.CountryCodes;

public sealed class CountryCode
{
    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;

    [JsonPropertyName("countryLabel")]
    public string CountryLabel { get; set; } = string.Empty;
}
