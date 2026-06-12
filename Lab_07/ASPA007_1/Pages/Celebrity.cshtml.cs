using ASPA007_1.Infrastructure;
using DAL_Celebrity_MSSQL;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ASPA007_1.Pages;

public sealed class CelebrityModel : PageModel
{
    private readonly IRepository repository;
    private readonly CelebritiesPathService paths;

    public CelebrityModel(IRepository repository, CelebritiesPathService paths)
    {
        this.repository = repository;
        this.paths = paths;
    }

    public Celebrity? Celebrity { get; private set; }
    public string PhotoUrl => paths.GetPhotoUrl(Celebrity?.ReqPhotoPath);

    public void OnGet(int id)
    {
        Celebrity = repository.GetCelebrityById(id);
    }
}
