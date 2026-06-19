using DAL_Celebrity_MSSQL;

namespace ASPA008_1.Infrastructure;

public sealed class CelebritiesOptions
{
    public const string SectionName = "Celebrities";

    public string PhotosRequestPath { get; set; } = "/Photos";
    public string PhotosFolder { get; set; } = "Photos";
    public string CountryCodesPath { get; set; } = "CountryCodes/iso3166-1-alpha2-country-codes.json";
    public string ConnectionString { get; set; } = Init.DefaultConnectionString;
}
