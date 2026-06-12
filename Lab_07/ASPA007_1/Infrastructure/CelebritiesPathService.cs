using Microsoft.Extensions.Options;

namespace ASPA007_1.Infrastructure;

public sealed class CelebritiesPathService
{
    private readonly IWebHostEnvironment environment;
    private readonly CelebritiesOptions options;

    public CelebritiesPathService(IWebHostEnvironment environment, IOptions<CelebritiesOptions> options)
    {
        this.environment = environment;
        this.options = options.Value;
    }

    public string RequestPath => NormalizeRequestPath(options.PhotosRequestPath);

    public string PhotosFolder
    {
        get
        {
            string folder = options.PhotosFolder;
            return Path.IsPathRooted(folder)
                ? folder
                : Path.Combine(environment.ContentRootPath, folder);
        }
    }

    public string GetPhotoUrl(string? fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return string.Empty;
        }

        return $"{RequestPath}/{Uri.EscapeDataString(Path.GetFileName(fileName))}";
    }

    public string GetPhotoPath(string fileName) => Path.Combine(PhotosFolder, Path.GetFileName(fileName));

    public void EnsurePhotosFolder() => Directory.CreateDirectory(PhotosFolder);

    private static string NormalizeRequestPath(string requestPath)
    {
        if (string.IsNullOrWhiteSpace(requestPath))
        {
            return "/Photos";
        }

        return requestPath.StartsWith('/')
            ? requestPath.TrimEnd('/')
            : "/" + requestPath.TrimEnd('/');
    }
}
