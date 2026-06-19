using System.Text;
using ASPA008_1.Filters;
using ASPA008_1.Infrastructure;
using ASPA008_1.Models;
using DAL_Celebrity_MSSQL;
using Microsoft.AspNetCore.Mvc;

namespace ASPA008_1.Controllers;

public sealed class CelebritiesController : Controller
{
    private static readonly HashSet<string> AllowedPhotoExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg",
        ".jpeg",
        ".png",
        ".gif",
        ".webp"
    };

    private readonly IRepository repository;
    private readonly CelebritiesPathService paths;

    public CelebritiesController(IRepository repository, CelebritiesPathService paths)
    {
        this.repository = repository;
        this.paths = paths;
    }

    public IActionResult Index()
    {
        CelebrityIndexViewModel model = new();

        try
        {
            model.Celebrities = repository.GetAllCelebrities()
                .Select(celebrity => new CelebrityListItemViewModel
                {
                    Id = celebrity.Id,
                    FullName = celebrity.FullName,
                    PhotoUrl = paths.GetPhotoUrl(celebrity.ReqPhotoPath)
                })
                .ToList();
        }
        catch (Exception exception)
        {
            model.ErrorMessage = exception.Message;
            model.Celebrities = [];
        }

        return View(model);
    }

    public IActionResult LifeEvents()
    {
        LifeEventsViewModel model = new();

        try
        {
            Dictionary<int, string> celebrities = repository.GetAllCelebrities()
                .ToDictionary(celebrity => celebrity.Id, celebrity => celebrity.FullName);

            model.Events = repository.GetAllLifeevents()
                .Select(lifeevent => new LifeEventRowViewModel(
                    lifeevent.Id,
                    lifeevent.CelebrityId,
                    celebrities.GetValueOrDefault(lifeevent.CelebrityId, lifeevent.CelebrityId.ToString()),
                    lifeevent.Date,
                    lifeevent.Description))
                .ToList();
        }
        catch (Exception exception)
        {
            model.ErrorMessage = exception.Message;
            model.Events = [];
        }

        return View(model);
    }

    [HttpGet]
    public IActionResult NewHumanForm()
    {
        return View(new CelebrityFormModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> NewHumanForm(CelebrityFormModel model)
    {
        NormalizeModel(model);

        if (model.Upload is null || model.Upload.Length == 0)
        {
            ModelState.AddModelError(nameof(model.Upload), "Select a photo.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        string? tempPhoto = await SaveTempPhotoAsync(model.Upload!);
        if (tempPhoto is null)
        {
            ModelState.AddModelError(nameof(model.Upload), "Use jpg, png, gif, or webp photo.");
            return View(model);
        }

        model.TempPhotoFileName = tempPhoto;
        model.OriginalPhotoName = Path.GetFileName(model.Upload!.FileName);
        model.TempPhotoUrl = paths.GetPhotoUrl(tempPhoto);

        ModelState.Clear();
        return View("ConfirmNewHuman", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ConfirmNewHuman(CelebrityFormModel model, string submit)
    {
        NormalizeModel(model);

        if (string.Equals(submit, "cancel", StringComparison.OrdinalIgnoreCase))
        {
            DeletePhotoIfTemporary(model.TempPhotoFileName);
            return RedirectToAction(nameof(Index));
        }

        if (!ModelState.IsValid || string.IsNullOrWhiteSpace(model.TempPhotoFileName))
        {
            model.TempPhotoUrl = paths.GetPhotoUrl(model.TempPhotoFileName);
            return View(model);
        }

        string tempPath = paths.GetPhotoPath(model.TempPhotoFileName);
        if (!System.IO.File.Exists(tempPath))
        {
            ModelState.AddModelError(string.Empty, "Uploaded photo was not found.");
            return View(model);
        }

        string finalFileName = CreateFinalPhotoFileName(model.FullName, Path.GetExtension(tempPath));
        System.IO.File.Move(tempPath, paths.GetPhotoPath(finalFileName));

        repository.AddCelebrity(new Celebrity
        {
            FullName = model.FullName,
            Nationality = model.Nationality,
            ReqPhotoPath = finalFileName
        });

        return RedirectToAction(nameof(Index));
    }

    [InfoAsyncActionFilter(InfoAsyncActionFilter.Wikipedia)]
    public IActionResult Human(int id)
    {
        Celebrity? celebrity = repository.GetCelebrityById(id);
        if (celebrity is null)
        {
            return NotFound();
        }

        IReadOnlyDictionary<string, string> wikiReferences =
            HttpContext.Items[InfoAsyncActionFilter.Wikipedia] as IReadOnlyDictionary<string, string> ??
            new Dictionary<string, string>();

        CelebrityDetailsViewModel model = new()
        {
            Celebrity = celebrity,
            Lifeevents = repository.GetLifeeventsByCelebrityId(id),
            PhotoUrl = paths.GetPhotoUrl(celebrity.ReqPhotoPath),
            WikiReferences = wikiReferences
        };

        return View(model);
    }

    [HttpGet]
    public IActionResult Edit(int id)
    {
        Celebrity? celebrity = repository.GetCelebrityById(id);
        if (celebrity is null)
        {
            return NotFound();
        }

        return View(ToFormModel(celebrity));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CelebrityFormModel model)
    {
        Celebrity? current = repository.GetCelebrityById(id);
        if (current is null)
        {
            return NotFound();
        }

        NormalizeModel(model);
        ModelState.Remove(nameof(model.Upload));

        if (!ModelState.IsValid)
        {
            model.Id = id;
            model.ExistingPhotoFileName = current.ReqPhotoPath;
            model.ExistingPhotoUrl = paths.GetPhotoUrl(current.ReqPhotoPath);
            return View(model);
        }

        string? photoFileName = current.ReqPhotoPath;
        if (model.Upload is not null && model.Upload.Length > 0)
        {
            string? savedPhoto = await SaveFinalPhotoAsync(model.FullName, model.Upload);
            if (savedPhoto is null)
            {
                ModelState.AddModelError(nameof(model.Upload), "Use jpg, png, gif, or webp photo.");
                model.Id = id;
                model.ExistingPhotoFileName = current.ReqPhotoPath;
                model.ExistingPhotoUrl = paths.GetPhotoUrl(current.ReqPhotoPath);
                return View(model);
            }

            photoFileName = savedPhoto;
        }

        repository.UpdCelebrity(id, new Celebrity
        {
            FullName = model.FullName,
            Nationality = model.Nationality,
            ReqPhotoPath = photoFileName
        });

        return RedirectToAction(nameof(Human), new { id });
    }

    [HttpGet]
    public IActionResult Delete(int id)
    {
        Celebrity? celebrity = repository.GetCelebrityById(id);
        if (celebrity is null)
        {
            return NotFound();
        }

        return View(ToDetailsModel(celebrity));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(int id, string submit)
    {
        if (!string.Equals(submit, "cancel", StringComparison.OrdinalIgnoreCase))
        {
            repository.DelCelebrity(id);
        }

        return RedirectToAction(nameof(Index));
    }

    private CelebrityDetailsViewModel ToDetailsModel(Celebrity celebrity)
    {
        return new CelebrityDetailsViewModel
        {
            Celebrity = celebrity,
            PhotoUrl = paths.GetPhotoUrl(celebrity.ReqPhotoPath)
        };
    }

    private CelebrityFormModel ToFormModel(Celebrity celebrity)
    {
        return new CelebrityFormModel
        {
            Id = celebrity.Id,
            FullName = celebrity.FullName,
            Nationality = celebrity.Nationality,
            ExistingPhotoFileName = celebrity.ReqPhotoPath,
            ExistingPhotoUrl = paths.GetPhotoUrl(celebrity.ReqPhotoPath)
        };
    }

    private async Task<string?> SaveTempPhotoAsync(IFormFile upload)
    {
        string extension = Path.GetExtension(upload.FileName);
        if (!AllowedPhotoExtensions.Contains(extension))
        {
            return null;
        }

        paths.EnsurePhotosFolder();
        string fileName = $"_pending_{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
        await using FileStream stream = System.IO.File.Create(paths.GetPhotoPath(fileName));
        await upload.CopyToAsync(stream);
        return fileName;
    }

    private async Task<string?> SaveFinalPhotoAsync(string fullName, IFormFile upload)
    {
        string extension = Path.GetExtension(upload.FileName);
        if (!AllowedPhotoExtensions.Contains(extension))
        {
            return null;
        }

        paths.EnsurePhotosFolder();
        string fileName = CreateFinalPhotoFileName(fullName, extension);
        await using FileStream stream = System.IO.File.Create(paths.GetPhotoPath(fileName));
        await upload.CopyToAsync(stream);
        return fileName;
    }

    private string CreateFinalPhotoFileName(string fullName, string extension)
    {
        string stem = Slugify(fullName);
        if (string.IsNullOrWhiteSpace(stem))
        {
            stem = "celebrity";
        }

        string normalizedExtension = extension.ToLowerInvariant();
        string fileName = $"{stem}{normalizedExtension}";
        if (!System.IO.File.Exists(paths.GetPhotoPath(fileName)))
        {
            return fileName;
        }

        return $"{stem}-{Guid.NewGuid():N}{normalizedExtension}";
    }

    private void DeletePhotoIfTemporary(string? fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName) || !fileName.StartsWith("_pending_", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        string path = paths.GetPhotoPath(fileName);
        if (System.IO.File.Exists(path))
        {
            System.IO.File.Delete(path);
        }
    }

    private static void NormalizeModel(CelebrityFormModel model)
    {
        model.FullName = model.FullName.Trim();
        model.Nationality = model.Nationality.Trim().ToUpperInvariant();
    }

    private static string Slugify(string value)
    {
        StringBuilder builder = new();
        foreach (char character in value)
        {
            if (char.IsLetterOrDigit(character))
            {
                builder.Append(char.ToLowerInvariant(character));
            }
            else if (builder.Length > 0 && builder[^1] != '-')
            {
                builder.Append('-');
            }
        }

        return builder.ToString().Trim('-');
    }
}
