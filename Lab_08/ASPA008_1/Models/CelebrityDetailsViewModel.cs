using DAL_Celebrity_MSSQL;

namespace ASPA008_1.Models;

public sealed class CelebrityDetailsViewModel
{
    public Celebrity Celebrity { get; set; } = new();
    public IReadOnlyList<Lifeevent> Lifeevents { get; set; } = [];
    public IReadOnlyDictionary<string, string> WikiReferences { get; set; } = new Dictionary<string, string>();
    public string PhotoUrl { get; set; } = string.Empty;
}
