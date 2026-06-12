using DAL004;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Services.AddDirectoryBrowser();

var app = builder.Build();

string celebritiesPath = Path.Combine(builder.Environment.ContentRootPath, "Celebrities");
var celebritiesFileProvider = new PhysicalFileProvider(celebritiesPath);

Repository.JSONFileName = "Celebrities.json";
using IRepository repository = Repository.Create(celebritiesPath);

app.UseExceptionHandler(new ExceptionHandlerOptions
{
    ExceptionHandlingPath = "/Celebrities/Error",
    AllowStatusCode404Response = true
});

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
        context.Context.Response.Headers["Content-Disposition"] =
            $"attachment; filename=\"{context.File.Name}\"";
    }
});

app.UseDirectoryBrowser(new DirectoryBrowserOptions
{
    FileProvider = celebritiesFileProvider,
    RequestPath = "/Celebrities/download"
});

app.Map("/Celebrities/Error", (HttpContext context) =>
{
    Exception? exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
    IResult result = Results.Problem(
        detail: "Panic",
        instance: app.Environment.EnvironmentName,
        title: "ASPA004",
        statusCode: StatusCodes.Status500InternalServerError);

    if (exception != null)
    {
        if (exception is FoundByIdException || exception is DeleteCelebrityException)
        {
            result = Results.NotFound(exception.Message);
        }
        else if (exception is Microsoft.AspNetCore.Http.BadHttpRequestException)
        {
            result = Results.BadRequest(exception.Message);
        }
        else if (exception is SaveException)
        {
            result = Results.Problem(
                title: "ASPA004/SaveChanges",
                detail: exception.Message,
                instance: app.Environment.EnvironmentName,
                statusCode: StatusCodes.Status500InternalServerError);
        }
        else if (exception is AddCelebrityException)
        {
            result = Results.Problem(
                title: "ASPA004/addCelebrity",
                detail: exception.Message,
                instance: app.Environment.EnvironmentName,
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    return result;
});

app.MapGet("/Celebrities", () => repository.getAllCelebrities());
app.MapGet("/Celebrities/{id:int}", (int id) =>
    repository.getCelebrityById(id) ??
    throw new FoundByIdException($"/Celebrities, Celebrity Id = {id}"));
app.MapGet("/Celebrities/BySurname/{surname}", (string surname) =>
    repository.getCelebritiesBySurname(surname));
app.MapGet("/Celebrities/PhotoPathById/{id:int}", (int id) =>
    repository.getPhotoPathById(id));
app.MapPost("/Celebrities", (Celebrity celebrity) =>
{
    int? id = repository.addCelebrity(celebrity);
    if (id == null)
    {
        throw new AddCelebrityException("Impossible add");
    }

    repository.SaveChanges();
    return repository.getCelebrityById((int)id);
});
app.MapDelete("/Celebrities/{id:int}", (int id) =>
{
    if (!repository.delCelebrityById(id))
    {
        throw new DeleteCelebrityException($"DELETE /Celebrities error, Id = {id}");
    }

    repository.SaveChanges();
    return $"Celebrity with Id = {id} deleted";
});
app.MapGet("/", () => Results.Redirect("/Celebrities"));

app.MapMethods("/{**path}", ["GET", "POST", "PUT", "DELETE"], (HttpContext context) =>
    Results.NotFound(new { error = $"path {context.Request.Path} not supported" }));

app.Run();
