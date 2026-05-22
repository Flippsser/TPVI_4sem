using System.Text.Json;

namespace DAL003;

public interface IRepository : IDisposable
{
    string BasePath { get; }
    Celebrity[] getAllCelebrities();
    Celebrity? getCelebrityById(int id);
    Celebrity[] getCelebritiesBySurname(string surname);
    string? getPhotoPathById(int id);
}

public record Celebrity(int Id, string Firstname, string Surname, string PhotoPath);

public sealed class Repository : IRepository
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private Celebrity[] celebrities;
    private bool disposed;

    public static string JSONFileName { get; set; } = "Celebrities.json";

    public Repository(string directoryName)
    {
        BasePath = ResolveBasePath(directoryName);
        celebrities = LoadCelebrities();
    }

    public string BasePath { get; }

    public static IRepository Create(string directoryName) => new Repository(directoryName);

    public Celebrity[] getAllCelebrities()
    {
        ThrowIfDisposed();
        return celebrities.ToArray();
    }

    public Celebrity? getCelebrityById(int id)
    {
        ThrowIfDisposed();
        return celebrities.FirstOrDefault(celebrity => celebrity.Id == id);
    }

    public Celebrity[] getCelebritiesBySurname(string surname)
    {
        ThrowIfDisposed();

        return celebrities
            .Where(celebrity => string.Equals(celebrity.Surname, surname, StringComparison.OrdinalIgnoreCase))
            .ToArray();
    }

    public string? getPhotoPathById(int id)
    {
        ThrowIfDisposed();
        return getCelebrityById(id)?.PhotoPath;
    }

    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        celebrities = [];
        disposed = true;
    }

    private Celebrity[] LoadCelebrities()
    {
        string jsonPath = Path.Combine(BasePath, JSONFileName);

        if (!File.Exists(jsonPath))
        {
            throw new FileNotFoundException($"JSON file was not found: {jsonPath}", jsonPath);
        }

        string json = File.ReadAllText(jsonPath);
        return JsonSerializer.Deserialize<Celebrity[]>(json, SerializerOptions) ?? [];
    }

    private static string ResolveBasePath(string directoryName)
    {
        if (Path.IsPathRooted(directoryName))
        {
            return directoryName;
        }

        string currentDirectoryPath = Path.Combine(Directory.GetCurrentDirectory(), directoryName);
        if (Directory.Exists(currentDirectoryPath))
        {
            return currentDirectoryPath;
        }

        string appDirectoryPath = Path.Combine(AppContext.BaseDirectory, directoryName);
        if (Directory.Exists(appDirectoryPath))
        {
            return appDirectoryPath;
        }

        return currentDirectoryPath;
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(disposed, this);
    }
}
