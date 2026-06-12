using DAL_Celebrity_MSSQL;
using System.Text;


string connectionString =
    args.FirstOrDefault() ??
    Environment.GetEnvironmentVariable("CELEBRITIES_CONNECTION") ??
    Init.DefaultConnectionString;

new Init(connectionString);
Init.Execute(delete: true, create: true);

Func<Celebrity, string> printC = celebrity =>
    $"Id = {celebrity.Id}, FullName = {celebrity.FullName}, Nationality = {celebrity.Nationality}, ReqPhotoPath = {celebrity.ReqPhotoPath}";
Func<Lifeevent, string> printL = lifeevent =>
    $"Id = {lifeevent.Id}, CelebrityId = {lifeevent.CelebrityId}, Date = {lifeevent.Date}, Description = {lifeevent.Description}, ReqPhotoPath = {lifeevent.ReqPhotoPath}";
Func<string, string> puri = fileName => fileName;

using IRepository repo = Repository.Create(connectionString);

Console.WriteLine("------ GetAllCelebrities() ------------- ");
repo.GetAllCelebrities().ForEach(celebrity => Console.WriteLine(printC(celebrity)));

Console.WriteLine("------ GetAllLifeevents() ------------- ");
repo.GetAllLifeevents().ForEach(lifeevent => Console.WriteLine(printL(lifeevent)));

Console.WriteLine("------ AddCelebrity() --------------- ");
Celebrity einstein = new() { FullName = "Albert Einstein", Nationality = "DE", ReqPhotoPath = puri("Einstein.jpg") };
Console.WriteLine(repo.AddCelebrity(einstein)
    ? $"OK: AddCelebrity: {printC(einstein)}"
    : $"ERROR:AddCelebrity: {printC(einstein)}");

Console.WriteLine("------ AddCelebrity() --------------- ");
Celebrity huntington = new() { FullName = "Samuel Huntington", Nationality = "US", ReqPhotoPath = puri("Huntington.jpg") };
Console.WriteLine(repo.AddCelebrity(huntington)
    ? $"OK: AddCelebrity: {printC(huntington)}"
    : $"ERROR:AddCelebrity: {printC(huntington)}");

Console.WriteLine("------ DelCelebrity() --------------- ");
int id = repo.GetCelebrityIdByName("Einstein");
if (id > 0)
{
    Celebrity? celebrity = repo.GetCelebrityById(id);
    if (celebrity is not null)
    {
        Console.WriteLine(printC(celebrity));
        Console.WriteLine(repo.DelCelebrity(id)
            ? $"OK: DelCelebrity: {id}"
            : $"ERROR: DelCelebrity: {id}");
    }
    else
    {
        Console.WriteLine($"ERROR: GetCelebrityById: {id}");
    }
}
else
{
    Console.WriteLine("ERROR: GetCelebrityIdByName");
}

Console.WriteLine("------ UpdCelebrity() --------------- ");
id = repo.GetCelebrityIdByName("Huntington");
if (id > 0)
{
    Celebrity? celebrity = repo.GetCelebrityById(id);
    if (celebrity is not null)
    {
        Console.WriteLine(printC(celebrity));
        celebrity.FullName = "Samuel Phillips Huntington";
        if (!repo.UpdCelebrity(id, celebrity))
        {
            Console.WriteLine($"ERROR: UpdCelebrity: {id}");
        }
        else
        {
            Console.WriteLine($"OK: UpdCelebrity:{id}, {printC(celebrity)}");
            Celebrity? updated = repo.GetCelebrityById(id);
            Console.WriteLine(updated is null
                ? $"ERROR: GetCelebrityById {id}"
                : $"OK: GetCelebrityById, {printC(updated)}");
        }
    }
    else
    {
        Console.WriteLine($"ERROR: GetCelebrityById: {id}");
    }
}
else
{
    Console.WriteLine("ERROR: GetCelebrityIdByName");
}

Console.WriteLine("------ AddLifeevent() --------------- ");
id = repo.GetCelebrityIdByName("Huntington");
if (id > 0)
{
    Celebrity? celebrity = repo.GetCelebrityById(id);
    if (celebrity is not null)
    {
        Console.WriteLine(printC(celebrity));
        Lifeevent firstBirth = new() { CelebrityId = id, Date = new DateTime(1927, 4, 18), Description = "Дата рождения" };
        Console.WriteLine(repo.AddLifeevent(firstBirth)
            ? $"OK: AddLifeevent, {printL(firstBirth)}"
            : $"ERROR: AddLifeevent, {printL(firstBirth)}");

        Lifeevent secondBirth = new() { CelebrityId = id, Date = new DateTime(1927, 4, 18), Description = "Дата рождения" };
        Console.WriteLine(repo.AddLifeevent(secondBirth)
            ? $"OK: AddLifeevent, {printL(secondBirth)}"
            : $"ERROR: AddLifeevent, {printL(secondBirth)}");

        Lifeevent death = new() { CelebrityId = id, Date = new DateTime(2008, 12, 24), Description = "Дата рождения" };
        Console.WriteLine(repo.AddLifeevent(death)
            ? $"OK: AddLifeevent, {printL(death)}"
            : $"ERROR: AddLifeevent, {printL(death)}");
    }
    else
    {
        Console.WriteLine($"ERROR: GetCelebrityById: {id}");
    }
}
else
{
    Console.WriteLine("ERROR: GetCelebrityIdByName");
}

Console.WriteLine("------ DelLifeevent() --------------- ");
id = 22;
Console.WriteLine(repo.DelLifeevent(id)
    ? $"OK: DelLifeevent: {id}"
    : $"ERROR: DelLifeevent: {id}");

Console.WriteLine("------ UpdLifeevent() --------------- ");
id = 23;
Lifeevent? lifeevent = repo.GetLifeevetById(id);
if (lifeevent is not null)
{
    lifeevent.Description = "Дата смерти";
    Console.WriteLine(repo.UpdLifeevent(id, lifeevent)
        ? $"OK:UpdLifeevent {id}, {printL(lifeevent)}"
        : $"ERROR:UpdLifeevent {id}, {printL(lifeevent)}");
}

Console.WriteLine("------ GetLifeeventsByCelebrityId ------------- ");
id = repo.GetCelebrityIdByName("Huntington");
if (id > 0)
{
    Celebrity? celebrity = repo.GetCelebrityById(id);
    if (celebrity is not null)
    {
        repo.GetLifeeventsByCelebrityId(celebrity.Id)
            .ForEach(life => Console.WriteLine($"OK: GetLifeeventsByCelebrityId, {id}, {printL(life)}"));
    }
    else
    {
        Console.WriteLine($"ERROR: GetLifeeventsByCelebrityId: {id}");
    }
}
else
{
    Console.WriteLine("ERROR: GetCelebrityIdByName");
}

Console.WriteLine("------ GetCelebrityByLifeeventId ------------- ");
id = 23;
Celebrity? result = repo.GetCelebrityByLifeeventId(id);
Console.WriteLine(result is not null
    ? $"OK:{printC(result)}"
    : $"ERROR: GetCelebrityByLifeeventId, {id}");

Console.WriteLine("------------>");
