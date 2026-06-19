using System.Text.Json;
using ASPA008_1.Infrastructure;
using Microsoft.Extensions.Options;

namespace ASPA008_1.CountryCodes;

public sealed class CountryCodesService
{
    public CountryCodesService(IWebHostEnvironment environment, IOptions<CelebritiesOptions> options)
    {
        string filePath = options.Value.CountryCodesPath;
        if (!Path.IsPathRooted(filePath))
        {
            filePath = Path.Combine(environment.ContentRootPath, filePath);
        }

        string json = File.ReadAllText(filePath);
        Countries = JsonSerializer.Deserialize<List<CountryCode>>(json) ?? [];
    }

    public IReadOnlyList<CountryCode> Countries { get; }
}
