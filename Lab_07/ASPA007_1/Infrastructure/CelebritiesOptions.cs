using DAL_Celebrity_MSSQL;

namespace ASPA007_1.Infrastructure;

public sealed class CelebritiesOptions
{
    public const string SectionName = "Celebrities";

    public string PhotosRequestPath { get; set; } = "/Photos";
    public string PhotosFolder { get; set; } = "Photos";
    public string ConnectionString { get; set; } = Init.DefaultConnectionString;
}
