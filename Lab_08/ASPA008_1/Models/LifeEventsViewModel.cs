namespace ASPA008_1.Models;

public sealed class LifeEventsViewModel
{
    public IReadOnlyList<LifeEventRowViewModel> Events { get; set; } = [];
    public string? ErrorMessage { get; set; }
}

public sealed record LifeEventRowViewModel(
    int Id,
    int CelebrityId,
    string CelebrityName,
    DateTime? Date,
    string Description);
