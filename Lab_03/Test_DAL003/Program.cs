using DAL003;

Repository.JSONFileName = "Celebrities.json";

using IRepository repository = Repository.Create("Celebrities");

foreach (Celebrity celebrity in repository.getAllCelebrities())
{
    PrintCelebrity(celebrity);
}

Celebrity? celebrity1 = repository.getCelebrityById(1);
if (celebrity1 != null)
{
    PrintCelebrity(celebrity1);
}

Celebrity? celebrity3 = repository.getCelebrityById(3);
if (celebrity3 != null)
{
    PrintCelebrity(celebrity3);
}

Celebrity? celebrity7 = repository.getCelebrityById(7);
if (celebrity7 != null)
{
    PrintCelebrity(celebrity7);
}

Celebrity? celebrity222 = repository.getCelebrityById(222);
if (celebrity222 != null)
{
    PrintCelebrity(celebrity222);
}
else
{
    Console.WriteLine("Not Found 222");
}

foreach (Celebrity celebrity in repository.getCelebritiesBySurname("Chomsky"))
{
    PrintCelebrity(celebrity);
}

foreach (Celebrity celebrity in repository.getCelebritiesBySurname("Knuth"))
{
    PrintCelebrity(celebrity);
}

foreach (Celebrity celebrity in repository.getCelebritiesBySurname("XXXX"))
{
    PrintCelebrity(celebrity);
}

Console.WriteLine($"PhotoPathById = {repository.getPhotoPathById(4)}");
Console.WriteLine($"PhotoPathById = {repository.getPhotoPathById(6)}");
Console.WriteLine($"PhotoPathById = {repository.getPhotoPathById(222)}");

static void PrintCelebrity(Celebrity celebrity)
{
    Console.WriteLine(
        $"Id = {celebrity.Id}, Firstname = {celebrity.Firstname}, " +
        $"Surname = {celebrity.Surname}, PhotoPath = {celebrity.PhotoPath} ");
}
