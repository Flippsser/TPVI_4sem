using System.ComponentModel.DataAnnotations;
using System.Text;
using ASPA007_1.Infrastructure;
using DAL_Celebrity_MSSQL;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ASPA007_1.Pages;

public sealed class NewCelebrityModel : PageModel
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg",
        ".jpeg",
        ".png",
        ".gif",
        ".webp"
    };

    private readonly IRepository repository;
    private readonly CelebritiesPathService paths;

    public NewCelebrityModel(IRepository repository, CelebritiesPathService paths)
    {
        this.repository = repository;
        this.paths = paths;
    }

    [BindProperty]
    [Required]
    public string FullName { get; set; } = string.Empty;

    [BindProperty]
    [Required]
    [StringLength(2, MinimumLength = 2)]
    public string Nationality { get; set; } = string.Empty;

    [BindProperty]
    public IFormFile? Photo { get; set; }

    [BindProperty]
    public string TempPhotoFileName { get; set; } = string.Empty;

    [BindProperty]
    public string OriginalPhotoName { get; set; } = string.Empty;

    public bool IsConfirmation { get; private set; }
    public string? ErrorMessage { get; private set; }
    public string PreviewPhotoUrl => paths.GetPhotoUrl(TempPhotoFileName);

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostPreviewAsync(string action)
    {
        if (string.Equals(action, "cancel", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToPage("/Celebrities");
        }

        FullName = FullName.Trim();
        Nationality = Nationality.Trim().ToUpperInvariant();

        ModelState.Remove(nameof(Photo));
        ModelState.Remove(nameof(TempPhotoFileName));
        ModelState.Remove(nameof(OriginalPhotoName));

        if (!ModelState.IsValid || Photo is null || Photo.Length == 0)
        {
            ErrorMessage = "Fill in the name, two-letter nation code, and select a photo.";
            return Page();
        }

        string extension = Path.GetExtension(Photo.FileName);
        if (!AllowedExtensions.Contains(extension))
        {
            ErrorMessage = "Use jpg, png, gif, or webp photo.";
            return Page();
        }

        paths.EnsurePhotosFolder();
        TempPhotoFileName = $"_pending_{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
        OriginalPhotoName = Path.GetFileName(Photo.FileName);

        await using FileStream stream = System.IO.File.Create(paths.GetPhotoPath(TempPhotoFileName));
        await Photo.CopyToAsync(stream);

        IsConfirmation = true;
        ModelState.Clear();
        return Page();
    }

    public IActionResult OnPostConfirm(string action)
    {
        if (string.Equals(action, "cancel", StringComparison.OrdinalIgnoreCase))
        {
            TryDeleteTempPhoto();
            return RedirectToPage("/Celebrities");
        }

        FullName = FullName.Trim();
        Nationality = Nationality.Trim().ToUpperInvariant();

        ModelState.Remove(nameof(Photo));

        if (!ModelState.IsValid || string.IsNullOrWhiteSpace(TempPhotoFileName))
        {
            ErrorMessage = "Confirmation data is incomplete.";
            IsConfirmation = true;
            return Page();
        }

        string sourcePath = paths.GetPhotoPath(TempPhotoFileName);
        if (!System.IO.File.Exists(sourcePath))
        {
            ErrorMessage = "Uploaded photo was not found.";
            IsConfirmation = true;
            return Page();
        }

        string finalFileName = CreateFinalPhotoFileName(FullName, Path.GetExtension(sourcePath));
        string finalPath = paths.GetPhotoPath(finalFileName);
        CopyPhoto(sourcePath, finalPath);

        repository.AddCelebrity(new Celebrity
        {
            FullName = FullName,
            Nationality = Nationality,
            ReqPhotoPath = finalFileName
        });

        TryDeleteTempPhoto();
        return RedirectToPage("/Celebrities");
    }

    private string CreateFinalPhotoFileName(string fullName, string extension)
    {
        string stem = Slugify(fullName);
        if (string.IsNullOrWhiteSpace(stem))
        {
            stem = "celebrity";
        }

        string fileName = $"{stem}{extension.ToLowerInvariant()}";
        string path = paths.GetPhotoPath(fileName);

        if (!System.IO.File.Exists(path))
        {
            return fileName;
        }

        string suffix = Guid.NewGuid().ToString("N")[..6];
        return $"{stem}-{suffix}{extension.ToLowerInvariant()}";
    }

    private void CopyPhoto(string sourcePath, string finalPath)
    {
        using FileStream source = new(
            sourcePath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.ReadWrite | FileShare.Delete);
        using FileStream destination = new(finalPath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
        source.CopyTo(destination);
    }

    private void TryDeleteTempPhoto()
    {
        if (string.IsNullOrWhiteSpace(TempPhotoFileName))
        {
            return;
        }

        string tempPath = paths.GetPhotoPath(TempPhotoFileName);
        if (System.IO.File.Exists(tempPath))
        {
            try
            {
                System.IO.File.Delete(tempPath);
            }
            catch (IOException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }
        }
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
