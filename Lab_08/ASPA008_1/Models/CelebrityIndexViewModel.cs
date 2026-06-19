namespace ASPA008_1.Models;

public sealed class CelebrityIndexViewModel
{
    public IReadOnlyList<CelebrityListItemViewModel> Celebrities { get; set; } = [];
    public string? ErrorMessage { get; set; }
}
