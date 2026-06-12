using ASPA007_1.Infrastructure;
using DAL_Celebrity_MSSQL;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ASPA007_1.Pages;

public sealed class CelebritiesModel : PageModel
{
    private readonly IRepository repository;
    private readonly CelebritiesPathService paths;

    public CelebritiesModel(IRepository repository, CelebritiesPathService paths)
    {
        this.repository = repository;
        this.paths = paths;
    }

    public IReadOnlyList<Celebrity> Celebrities { get; private set; } = [];
    public string? ErrorMessage { get; private set; }

    public void OnGet()
    {
        try
        {
            Celebrities = repository.GetAllCelebrities();
        }
        catch (Exception exception)
        {
            ErrorMessage = exception.Message;
            Celebrities = [];
        }
    }

    public string PhotoUrl(string? fileName) => paths.GetPhotoUrl(fileName);
}
