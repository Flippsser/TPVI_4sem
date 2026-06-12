using ASPA005_2;
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
Validation.SurnameFilter.repository = Validation.PhotoExistFilter.repository = repository;
Validation.PutFilter.repository = Validation.DeleteFilter.repository = repository;

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
        title: "ASPA005_2",
        statusCode: StatusCodes.Status500InternalServerError);

    if (exception != null)
    {
        if (exception is FoundByIdException ||
            exception is DeleteCelebrityException ||
            exception is UpdateCelebrityException)
        {
            result = Results.NotFound(exception.Message);
        }
        else if (exception is BadHttpRequestException)
        {
            result = Results.BadRequest(exception.Message);
        }
        else if (exception is SaveException)
        {
            result = Results.Problem(
                title: "ASPA005_2/SaveChanges",
                detail: exception.Message,
                instance: app.Environment.EnvironmentName,
                statusCode: StatusCodes.Status500InternalServerError);
        }
        else if (exception is AddCelebrityException)
        {
            result = Results.Problem(
                title: "ASPA005_2/addCelebrity",
                detail: exception.Message,
                instance: app.Environment.EnvironmentName,
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    return result;
});

RouteGroupBuilder api = app.MapGroup("/Celebrities");

api.MapGet("/", () => repository.getAllCelebrities());
api.MapGet("/{id:int}", (int id) =>
    repository.getCelebrityById(id) ??
    throw new FoundByIdException($"/Celebrities, Celebrity Id = {id}"));
api.MapGet("/BySurname/{surname}", (string surname) =>
    repository.getCelebritiesBySurname(surname));
api.MapGet("/PhotoPathById/{id:int}", (int id) =>
    repository.getPhotoPathById(id));
api.MapPost("/", (Celebrity celebrity) =>
{
    int? id = repository.addCelebrity(celebrity);
    if (id == null)
    {
        throw new AddCelebrityException("POST /Celebrities error, id == null");
    }

    if (repository.SaveChanges() <= 0)
    {
        throw new SaveException("/Celebrities error, SaveChanges() <= 0");
    }

    return new Celebrity((int)id, celebrity.Firstname, celebrity.Surname, celebrity.PhotoPath);
})
.AddEndpointFilter<Validation.SurnameFilter>()
.AddEndpointFilter<Validation.PhotoExistFilter>();
api.MapDelete("/{id:int}", (int id) =>
{
    if (!repository.delCelebrityById(id))
    {
        throw new DeleteCelebrityException($"DELETE /Celebrities error, Id = {id}");
    }

    repository.SaveChanges();
    return $"Celebrity with Id = {id} deleted";
})
.AddEndpointFilter<Validation.DeleteFilter>();
api.MapPut("/{id:int}", (int id, Celebrity celebrity) =>
{
    if (!repository.updCelebrityById(id, celebrity))
    {
        throw new UpdateCelebrityException($"PUT /Celebrities error, Id = {id}");
    }

    repository.SaveChanges();
    return repository.getCelebrityById(id);
})
.AddEndpointFilter<Validation.PutFilter>();

app.MapGet("/", () => Results.Redirect("/Celebrities"));

app.MapMethods("/{**path}", ["GET", "POST", "PUT", "DELETE"], (HttpContext context) =>
    Results.NotFound(new { message = $"path {context.Request.Path} not supported" }));

app.Run();
