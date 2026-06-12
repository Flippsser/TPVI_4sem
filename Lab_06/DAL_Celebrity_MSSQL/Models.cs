using System.Text.Json.Serialization;

namespace DAL_Celebrity_MSSQL;

public interface IRepository : DAL_Celebrity.IRepository<Celebrity, Lifeevent>
{
}

public class Celebrity
{
    public Celebrity()
    {
        FullName = string.Empty;
        Nationality = string.Empty;
    }

    public int Id { get; set; }
    public string FullName { get; set; }
    public string Nationality { get; set; }
    public string? ReqPhotoPath { get; set; }

    [JsonIgnore]
    public virtual ICollection<Lifeevent> Lifeevents { get; set; } = [];

    public virtual bool Update(Celebrity celebrity)
    {
        if (!string.IsNullOrWhiteSpace(celebrity.FullName))
        {
            FullName = celebrity.FullName;
        }

        if (!string.IsNullOrWhiteSpace(celebrity.Nationality))
        {
            Nationality = celebrity.Nationality;
        }

        if (!string.IsNullOrWhiteSpace(celebrity.ReqPhotoPath))
        {
            ReqPhotoPath = celebrity.ReqPhotoPath;
        }

        return true;
    }
}

public class Lifeevent
{
    public Lifeevent()
    {
        Description = string.Empty;
    }

    public int Id { get; set; }
    public int CelebrityId { get; set; }
    public DateTime? Date { get; set; }
    public string Description { get; set; }
    public string? ReqPhotoPath { get; set; }

    [JsonIgnore]
    public virtual Celebrity? Celebrity { get; set; }

    public virtual bool Update(Lifeevent lifeevent)
    {
        if (lifeevent.CelebrityId > 0)
        {
            CelebrityId = lifeevent.CelebrityId;
        }

        if (lifeevent.Date.HasValue)
        {
            Date = lifeevent.Date;
        }

        if (!string.IsNullOrWhiteSpace(lifeevent.Description))
        {
            Description = lifeevent.Description;
        }

        if (!string.IsNullOrWhiteSpace(lifeevent.ReqPhotoPath))
        {
            ReqPhotoPath = lifeevent.ReqPhotoPath;
        }

        return true;
    }
}
