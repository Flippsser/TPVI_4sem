using System.Text.Json;

namespace DAL004;

public interface IRepository : IDisposable
{
    string BasePath { get; }
    Celebrity[] getAllCelebrities();
    Celebrity? getCelebrityById(int id);
    Celebrity[] getCelebritiesBySurname(string surname);
    string? getPhotoPathById(int id);
    int? addCelebrity(Celebrity celebrity);
    bool delCelebrityById(int id);
    bool updCelebrityById(int id, Celebrity celebrity);
    int SaveChanges();
}

public record Celebrity(int Id, string Firstname, string Surname, string PhotoPath);

public sealed class Repository : IRepository
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    private List<Celebrity> celebrities;
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

    public int? addCelebrity(Celebrity celebrity)
    {
        ThrowIfDisposed();

        if (string.IsNullOrWhiteSpace(celebrity.Firstname) ||
            string.IsNullOrWhiteSpace(celebrity.Surname) ||
            string.IsNullOrWhiteSpace(celebrity.PhotoPath))
        {
            throw new AddCelebrityException("Firstname, surname and photoPath must be specified");
        }

        int id = celebrities.Count == 0 ? 1 : celebrities.Max(item => item.Id) + 1;
        celebrities.Add(celebrity with { Id = id });
        return id;
    }

    public bool delCelebrityById(int id)
    {
        ThrowIfDisposed();

        Celebrity? celebrity = getCelebrityById(id);
        return celebrity != null && celebrities.Remove(celebrity);
    }

    public bool updCelebrityById(int id, Celebrity celebrity)
    {
        ThrowIfDisposed();

        int index = celebrities.FindIndex(item => item.Id == id);
        if (index < 0)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(celebrity.Firstname) ||
            string.IsNullOrWhiteSpace(celebrity.Surname) ||
            string.IsNullOrWhiteSpace(celebrity.PhotoPath))
        {
            throw new UpdateCelebrityException($"PUT /Celebrities error, Id = {id}");
        }

        celebrities[index] = celebrity with { Id = id };
        return true;
    }

    public int SaveChanges()
    {
        ThrowIfDisposed();

        string jsonPath = Path.Combine(BasePath, JSONFileName);

        try
        {
            string json = JsonSerializer.Serialize(celebrities, SerializerOptions);
            File.WriteAllText(jsonPath, json);
            return celebrities.Count;
        }
        catch (Exception ex)
        {
            throw new SaveException(ex.Message);
        }
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

    private List<Celebrity> LoadCelebrities()
    {
        string jsonPath = Path.Combine(BasePath, JSONFileName);

        if (!File.Exists(jsonPath))
        {
            throw new FileNotFoundException($"Could not find file '{jsonPath}'.", jsonPath);
        }

        string json = File.ReadAllText(jsonPath);
        return JsonSerializer.Deserialize<List<Celebrity>>(json, SerializerOptions) ?? [];
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

public class FoundByIdException : Exception
{
    public FoundByIdException(string message)
        : base($"Found by Id: {message}")
    {
    }
}

public class DeleteCelebrityException : Exception
{
    public DeleteCelebrityException(string message)
        : base($"Delete by Id:{message}")
    {
    }
}

public class UpdateCelebrityException : Exception
{
    public UpdateCelebrityException(string message)
        : base($"UpdateCelebrityException error:{message}")
    {
    }
}

public class SaveException : Exception
{
    public SaveException(string message)
        : base($"SaveChanges error:{message}")
    {
    }
}

public class AddCelebrityException : Exception
{
    public AddCelebrityException(string message)
        : base($"AddCelebrityException error:{message}")
    {
    }
}
