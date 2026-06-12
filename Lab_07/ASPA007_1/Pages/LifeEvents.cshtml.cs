using DAL_Celebrity_MSSQL;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ASPA007_1.Pages;

public sealed class LifeEventsModel : PageModel
{
    private readonly IRepository repository;

    public LifeEventsModel(IRepository repository)
    {
        this.repository = repository;
    }

    public IReadOnlyList<LifeEventRow> Events { get; private set; } = [];
    public string? ErrorMessage { get; private set; }

    public void OnGet()
    {
        try
        {
            Dictionary<int, string> celebrities = repository.GetAllCelebrities()
                .ToDictionary(celebrity => celebrity.Id, celebrity => celebrity.FullName);

            Events = repository.GetAllLifeevents()
                .Select(lifeevent => new LifeEventRow(
                    lifeevent.Id,
                    lifeevent.CelebrityId,
                    celebrities.GetValueOrDefault(lifeevent.CelebrityId, lifeevent.CelebrityId.ToString()),
                    lifeevent.Date,
                    lifeevent.Description))
                .ToList();
        }
        catch (Exception exception)
        {
            ErrorMessage = exception.Message;
            Events = [];
        }
    }

    public sealed record LifeEventRow(
        int Id,
        int CelebrityId,
        string CelebrityName,
        DateTime? Date,
        string Description);
}
