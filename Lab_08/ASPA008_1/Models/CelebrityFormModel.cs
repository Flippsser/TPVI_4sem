using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace ASPA008_1.Models;

public sealed class CelebrityFormModel
{
    public int Id { get; set; }

    [Required]
    [Display(Name = "Celebrities Name")]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [StringLength(2, MinimumLength = 2)]
    public string Nationality { get; set; } = string.Empty;

    [BindProperty]
    public IFormFile? Upload { get; set; }

    public string? TempPhotoFileName { get; set; }
    public string? OriginalPhotoName { get; set; }
    public string? ExistingPhotoFileName { get; set; }
    public string? TempPhotoUrl { get; set; }
    public string? ExistingPhotoUrl { get; set; }
}
