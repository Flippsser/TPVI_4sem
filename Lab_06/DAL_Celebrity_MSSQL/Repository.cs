using Microsoft.EntityFrameworkCore;

namespace DAL_Celebrity_MSSQL;

public sealed class Repository : IRepository
{
    private readonly Context context;
    private bool disposed;

    private Repository(string connectionString)
    {
        context = new Context(connectionString);
    }

    public static IRepository Create(string connectionString) => new Repository(connectionString);

    public List<Celebrity> GetAllCelebrities()
    {
        ThrowIfDisposed();
        return context.Celebrities
            .AsNoTracking()
            .OrderBy(celebrity => celebrity.Id)
            .ToList();
    }

    public Celebrity? GetCelebrityById(int id)
    {
        ThrowIfDisposed();
        return context.Celebrities
            .AsNoTracking()
            .FirstOrDefault(celebrity => celebrity.Id == id);
    }

    public bool DelCelebrity(int id)
    {
        ThrowIfDisposed();
        Celebrity? celebrity = context.Celebrities.Find(id);
        if (celebrity is null)
        {
            return false;
        }

        context.Celebrities.Remove(celebrity);
        return context.SaveChanges() > 0;
    }

    public bool AddCelebrity(Celebrity celebrity)
    {
        ThrowIfDisposed();
        ValidateCelebrity(celebrity);
        celebrity.Id = 0;
        context.Celebrities.Add(celebrity);
        return context.SaveChanges() > 0;
    }

    public bool UpdCelebrity(int id, Celebrity celebrity)
    {
        ThrowIfDisposed();
        Celebrity? current = context.Celebrities.Find(id);
        if (current is null)
        {
            return false;
        }

        current.Update(celebrity);
        context.SaveChanges();
        return true;
    }

    public int GetCelebrityIdByName(string name)
    {
        ThrowIfDisposed();
        if (string.IsNullOrWhiteSpace(name))
        {
            return 0;
        }

        string pattern = $"%{name.Trim()}%";
        return context.Celebrities
            .AsNoTracking()
            .Where(celebrity => EF.Functions.Like(celebrity.FullName, pattern))
            .Select(celebrity => celebrity.Id)
            .FirstOrDefault();
    }

    public List<Lifeevent> GetAllLifeevents()
    {
        ThrowIfDisposed();
        return context.Lifeevents
            .AsNoTracking()
            .OrderBy(lifeevent => lifeevent.Id)
            .ToList();
    }

    public Lifeevent? GetLifeevetById(int id)
    {
        ThrowIfDisposed();
        return context.Lifeevents
            .AsNoTracking()
            .FirstOrDefault(lifeevent => lifeevent.Id == id);
    }

    public bool DelLifeevent(int id)
    {
        ThrowIfDisposed();
        Lifeevent? lifeevent = context.Lifeevents.Find(id);
        if (lifeevent is null)
        {
            return false;
        }

        context.Lifeevents.Remove(lifeevent);
        return context.SaveChanges() > 0;
    }

    public bool AddLifeevent(Lifeevent lifeevent)
    {
        ThrowIfDisposed();
        ValidateLifeevent(lifeevent);
        lifeevent.Id = 0;
        context.Lifeevents.Add(lifeevent);
        return context.SaveChanges() > 0;
    }

    public bool UpdLifeevent(int id, Lifeevent lifeevent)
    {
        ThrowIfDisposed();
        Lifeevent? current = context.Lifeevents.Find(id);
        if (current is null)
        {
            return false;
        }

        current.Update(lifeevent);
        context.SaveChanges();
        return true;
    }

    public List<Lifeevent> GetLifeeventsByCelebrityId(int celebrityId)
    {
        ThrowIfDisposed();
        return context.Lifeevents
            .AsNoTracking()
            .Where(lifeevent => lifeevent.CelebrityId == celebrityId)
            .OrderBy(lifeevent => lifeevent.Date)
            .ThenBy(lifeevent => lifeevent.Id)
            .ToList();
    }

    public Celebrity? GetCelebrityByLifeeventId(int lifeeventId)
    {
        ThrowIfDisposed();
        return context.Lifeevents
            .AsNoTracking()
            .Where(lifeevent => lifeevent.Id == lifeeventId)
            .Select(lifeevent => lifeevent.Celebrity)
            .FirstOrDefault();
    }

    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        context.Dispose();
        disposed = true;
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(disposed, this);
    }

    private void ValidateCelebrity(Celebrity celebrity)
    {
        if (string.IsNullOrWhiteSpace(celebrity.FullName) ||
            string.IsNullOrWhiteSpace(celebrity.Nationality))
        {
            throw new ArgumentException("FullName and Nationality are required.", nameof(celebrity));
        }
    }

    private void ValidateLifeevent(Lifeevent lifeevent)
    {
        if (lifeevent.CelebrityId <= 0 ||
            string.IsNullOrWhiteSpace(lifeevent.Description))
        {
            throw new ArgumentException("CelebrityId and Description are required.", nameof(lifeevent));
        }

        if (!context.Celebrities.Any(celebrity => celebrity.Id == lifeevent.CelebrityId))
        {
            throw new InvalidOperationException($"Celebrity with Id = {lifeevent.CelebrityId} was not found.");
        }
    }
}
