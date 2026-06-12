using DAL004;

Repository.JSONFileName = "Celebrities.json";

using IRepository repository = Repository.Create("Celebrities");

void PrintAll(string label)
{
    Console.WriteLine($"--- {label} ----------------");
    foreach (Celebrity celebrity in repository.getAllCelebrities())
    {
        Console.WriteLine(
            $"Id = {celebrity.Id}, Firstname = {celebrity.Firstname}, " +
            $"Surname = {celebrity.Surname}, PhotoPath = {celebrity.PhotoPath} ");
    }
}

PrintAll("start");

int? testdel1 = repository.addCelebrity(new Celebrity(0, "TestDel1", "TestDel1", "Photo/TestDel1.jpg"));
int? testdel2 = repository.addCelebrity(new Celebrity(0, "TestDel2", "TestDel2", "Photo/TestDel2.jpg"));
int? testupd1 = repository.addCelebrity(new Celebrity(0, "TestUpd1", "TestUpd1", "Photo/TestUpd1.jpg"));
int? testupd2 = repository.addCelebrity(new Celebrity(0, "TestUpd2", "TestUpd2", "Photo/TestUpd2.jpg"));
repository.SaveChanges();
PrintAll("add 4");

if (testdel1 != null)
{
    Console.WriteLine(repository.delCelebrityById((int)testdel1)
        ? $" delete {testdel1} "
        : $"delete {testdel1} error");
}

if (testdel2 != null)
{
    Console.WriteLine(repository.delCelebrityById((int)testdel2)
        ? $" delete {testdel2} "
        : $"delete {testdel2} error");
}

Console.WriteLine(repository.delCelebrityById(1000)
    ? " delete {1000} "
    : "delete {1000} error");
repository.SaveChanges();
PrintAll("del 2");

if (testupd1 != null)
{
    Console.WriteLine(repository.updCelebrityById((int)testupd1, new Celebrity(0, "Updated1", "Updated1", "Photo/Updated1.jpg"))
        ? $" update {testupd1} "
        : $"update {testupd1} error");
}

if (testupd2 != null)
{
    Console.WriteLine(repository.updCelebrityById((int)testupd2, new Celebrity(0, "Updated2", "Updated2", "Photo/Updated2.jpg"))
        ? $" update {testupd2} "
        : $"update {testupd2} error");
}

Console.WriteLine(repository.updCelebrityById(1000, new Celebrity(0, "Updated1000", "Updated1000", "Photo/Updated1000.jpg"))
    ? " update {1000} "
    : "update {1000} error");
repository.SaveChanges();
PrintAll("upd 2");
