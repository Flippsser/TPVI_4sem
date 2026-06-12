namespace DAL_Celebrity_MSSQL;

public class Init
{
    public const string DefaultConnectionString =
        "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=LES01_Lab06;Integrated Security=True;Encrypt=True;TrustServerCertificate=True";

    private static string connectionString = DefaultConnectionString;

    public Init()
    {
    }

    public Init(string connectionString)
    {
        Init.connectionString = connectionString;
    }

    public static void Execute(bool delete = true, bool create = true)
    {
        using Context context = new(connectionString);
        if (delete)
        {
            context.Database.EnsureDeleted();
        }

        if (create)
        {
            context.Database.EnsureCreated();
        }

        Func<string, string> puri = fileName => fileName;

        AddCelebrity(context, "Noam Chomsky", "US", puri("Chomsky.jpg"),
            new Lifeevent { CelebrityId = 1, Date = new DateTime(1928, 12, 7), Description = "Дата рождения" },
            new Lifeevent { CelebrityId = 1, Date = new DateTime(1955, 1, 1), Description = "Издание книги \"Логическая структура лингвистической теории\"" });

        AddCelebrity(context, "Tim Berners-Lee", "UK", puri("Berners-Lee.jpg"),
            new Lifeevent { CelebrityId = 2, Date = new DateTime(1955, 6, 8), Description = "Дата рождения" },
            new Lifeevent { CelebrityId = 2, Date = new DateTime(1989, 6, 8), Description = "В CERN предложил \"Гиппертекстовый проект\"" });

        AddCelebrity(context, "Edgar Codd", "US", puri("Codd.jpg"),
            new Lifeevent { CelebrityId = 3, Date = new DateTime(1923, 8, 23), Description = "Дата рождения" },
            new Lifeevent { CelebrityId = 3, Date = new DateTime(2003, 4, 18), Description = "Дата смерти" });

        AddCelebrity(context, "Donald Knuth", "US", puri("Knuth.jpg"),
            new Lifeevent { CelebrityId = 4, Date = new DateTime(1938, 1, 10), Description = "Дата рождения" },
            new Lifeevent { CelebrityId = 4, Date = new DateTime(1974, 1, 1), Description = "Премия Тьюринга" });

        AddCelebrity(context, "Linus Torvalds", "US", puri("Linus.jpg"),
            new Lifeevent { CelebrityId = 5, Date = new DateTime(1969, 12, 28), Description = "Дата рождения. Финляндия." },
            new Lifeevent { CelebrityId = 5, Date = new DateTime(1991, 9, 17), Description = "Выложил исходный код OS Linux (версии 0.01)" });

        AddCelebrity(context, "John Neumann", "US", puri("Neumann.jpg"),
            new Lifeevent { CelebrityId = 6, Date = new DateTime(1903, 12, 28), Description = "Дата рождения. Венгрия" },
            new Lifeevent { CelebrityId = 6, Date = new DateTime(1957, 2, 8), Description = "Дата смерти" });

        AddCelebrity(context, "Edsger Dijkstra", "NL", puri("Dijkstra.jpg"),
            new Lifeevent { CelebrityId = 7, Date = new DateTime(1930, 12, 28), Description = "Дата рождения" },
            new Lifeevent { CelebrityId = 7, Date = new DateTime(2002, 8, 6), Description = "Дата смерти" });

        AddCelebrity(context, "Ada Lovelace", "UK", puri("Lovelace.jpg"),
            new Lifeevent { CelebrityId = 8, Date = new DateTime(1815, 12, 10), Description = "Дата рождения" },
            new Lifeevent { CelebrityId = 8, Date = new DateTime(1852, 11, 27), Description = "Дата смерти" });

        AddCelebrity(context, "Charles Babbage", "UK", puri("Babbage.jpg"),
            new Lifeevent { CelebrityId = 9, Date = new DateTime(1791, 12, 26), Description = "Дата рождения" },
            new Lifeevent { CelebrityId = 9, Date = new DateTime(1871, 10, 18), Description = "Дата смерти" });

        AddCelebrity(context, "Andrew Tanenbaum", "NL", puri("Tanenbaum.jpg"),
            new Lifeevent { CelebrityId = 10, Date = new DateTime(1944, 3, 16), Description = "Дата рождения" },
            new Lifeevent { CelebrityId = 10, Date = new DateTime(1987, 1, 1), Description = "Cоздал OS MINIX - бесплатную Unix-подобную систему" });

        context.SaveChanges();
    }

    private static void AddCelebrity(Context context, string fullName, string nationality, string reqPhotoPath, params Lifeevent[] events)
    {
        context.Celebrities.Add(new Celebrity
        {
            FullName = fullName,
            Nationality = nationality,
            ReqPhotoPath = reqPhotoPath
        });

        context.Lifeevents.AddRange(events);
    }
}
