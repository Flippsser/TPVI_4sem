using DAL003;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDirectoryBrowser();

var app = builder.Build();

string celebritiesPath = Path.Combine(builder.Environment.ContentRootPath, "Celebrities");
var celebritiesFileProvider = new PhysicalFileProvider(celebritiesPath);

Repository.JSONFileName = "Celebrities.json";
using IRepository repository = Repository.Create("Celebrities");

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = celebritiesFileProvider,
    RequestPath = "/Photo"
});

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = celebritiesFileProvider,
    RequestPath = "/Celebrities/download",
    OnPrepareResponse = context =>
    {
        context.Context.Response.Headers.Append(
            "Content-Disposition",
            $"attachment; filename=\"{context.File.Name}\"");
    }
});

app.UseDirectoryBrowser(new DirectoryBrowserOptions
{
    FileProvider = celebritiesFileProvider,
    RequestPath = "/Celebrities/download"
});

app.MapGet("/Celebrities", () => repository.getAllCelebrities());
app.MapGet("/Celebrities/{id:int}", (int id) => repository.getCelebrityById(id));
app.MapGet("/Celebrities/BySurname/{surname}", (string surname) => repository.getCelebritiesBySurname(surname));
app.MapGet("/Celebrities/PhotoPathById/{id:int}", (int id) => repository.getPhotoPathById(id));
app.MapGet("/", () => Results.Redirect("/Celebrities"));

app.Run();
