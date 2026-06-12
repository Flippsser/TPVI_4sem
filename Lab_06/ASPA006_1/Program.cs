using DAL_Celebrity_MSSQL;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("Celebrities.config.json", optional: false, reloadOnChange: true);
builder.Services.Configure<CelebritiesConfig>(builder.Configuration.GetSection(CelebritiesConfig.SectionName));
builder.Services.AddScoped<IRepository>(serviceProvider =>
{
    CelebritiesConfig config = serviceProvider.GetRequiredService<IOptions<CelebritiesConfig>>().Value;
    return Repository.Create(config.ConnectionString);
});

var app = builder.Build();

app.UseMiddleware<ApiExceptionMiddleware>();
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/", () => Results.Redirect("/index.html"));

RouteGroupBuilder celebrities = app.MapGroup("/api/Celebrities");
celebrities.MapGet("/", (IRepository repository) => Results.Ok(repository.GetAllCelebrities()));
celebrities.MapGet("/{id:int:min(1)}", (int id, IRepository repository) =>
    repository.GetCelebrityById(id) is { } celebrity
        ? Results.Ok(celebrity)
        : Results.NotFound(new { message = $"Celebrity with Id = {id} was not found." }));
celebrities.MapGet("/Lifeevents/{id:int:min(1)}", (int id, IRepository repository) =>
    repository.GetCelebrityByLifeeventId(id) is { } celebrity
        ? Results.Ok(celebrity)
        : Results.NotFound(new { message = $"Celebrity for Lifeevent Id = {id} was not found." }));
celebrities.MapDelete("/{id:int:min(1)}", (int id, IRepository repository) =>
    repository.DelCelebrity(id)
        ? Results.Ok(new { message = $"Celebrity with Id = {id} deleted." })
        : Results.NotFound(new { message = $"Celebrity with Id = {id} was not found." }));
celebrities.MapPost("/", (Celebrity celebrity, IRepository repository) =>
{
    if (!repository.AddCelebrity(celebrity))
    {
        return Results.BadRequest(new { message = "Celebrity was not added." });
    }

    return Results.Created($"/api/Celebrities/{celebrity.Id}", celebrity);
});
celebrities.MapPut("/{id:int:min(1)}", (int id, Celebrity celebrity, IRepository repository) =>
    repository.UpdCelebrity(id, celebrity)
        ? Results.Ok(repository.GetCelebrityById(id))
        : Results.NotFound(new { message = $"Celebrity with Id = {id} was not found." }));
celebrities.MapGet("/photo/{fileName}", (
    string fileName,
    IWebHostEnvironment environment,
    IOptions<CelebritiesConfig> options) =>
{
    string safeFileName = Path.GetFileName(fileName);
    string photosDirectory = ResolvePhotosDirectory(environment, options.Value);
    string photoPath = Path.Combine(photosDirectory, safeFileName);

    return File.Exists(photoPath)
        ? Results.File(photoPath, GetContentType(photoPath))
        : Results.NotFound(new { message = $"Photo {safeFileName} was not found." });
});

RouteGroupBuilder lifeevents = app.MapGroup("/api/Lifeevents");
lifeevents.MapGet("/", (IRepository repository) => Results.Ok(repository.GetAllLifeevents()));
lifeevents.MapGet("/{id:int:min(1)}", (int id, IRepository repository) =>
    repository.GetLifeevetById(id) is { } lifeevent
        ? Results.Ok(lifeevent)
        : Results.NotFound(new { message = $"Lifeevent with Id = {id} was not found." }));
lifeevents.MapGet("/Celebrities/{id:int:min(1)}", (int id, IRepository repository) =>
    Results.Ok(repository.GetLifeeventsByCelebrityId(id)));
lifeevents.MapDelete("/{id:int:min(1)}", (int id, IRepository repository) =>
    repository.DelLifeevent(id)
        ? Results.Ok(new { message = $"Lifeevent with Id = {id} deleted." })
        : Results.NotFound(new { message = $"Lifeevent with Id = {id} was not found." }));
lifeevents.MapPost("/", (Lifeevent lifeevent, IRepository repository) =>
{
    if (!repository.AddLifeevent(lifeevent))
    {
        return Results.BadRequest(new { message = "Lifeevent was not added." });
    }

    return Results.Created($"/api/Lifeevents/{lifeevent.Id}", lifeevent);
});
lifeevents.MapPut("/{id:int:min(1)}", (int id, Lifeevent lifeevent, IRepository repository) =>
    repository.UpdLifeevent(id, lifeevent)
        ? Results.Ok(repository.GetLifeevetById(id))
        : Results.NotFound(new { message = $"Lifeevent with Id = {id} was not found." }));

app.MapFallback((HttpContext context) =>
    Results.NotFound(new { message = $"Path {context.Request.Path} is not supported." }));

app.Run();

static string ResolvePhotosDirectory(IWebHostEnvironment environment, CelebritiesConfig config)
{
    return Path.IsPathRooted(config.PhotosDirectory)
        ? config.PhotosDirectory
        : Path.Combine(environment.ContentRootPath, config.PhotosDirectory);
}

static string GetContentType(string filePath)
{
    return Path.GetExtension(filePath).ToLowerInvariant() switch
    {
        ".jpg" or ".jpeg" => "image/jpeg",
        ".png" => "image/png",
        ".gif" => "image/gif",
        ".webp" => "image/webp",
        _ => "application/octet-stream"
    };
}

public sealed class CelebritiesConfig
{
    public const string SectionName = "Celebrities";

    public string ConnectionString { get; set; } = Init.DefaultConnectionString;
    public string PhotosDirectory { get; set; } = "Celebrities";
}

public sealed class ApiExceptionMiddleware
{
    private readonly RequestDelegate next;
    private readonly ILogger<ApiExceptionMiddleware> logger;

    public ApiExceptionMiddleware(RequestDelegate next, ILogger<ApiExceptionMiddleware> logger)
    {
        this.next = next;
        this.logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unhandled API exception");

            if (context.Response.HasStarted)
            {
                throw;
            }

            int statusCode = exception switch
            {
                BadHttpRequestException => StatusCodes.Status400BadRequest,
                ArgumentException => StatusCodes.Status400BadRequest,
                InvalidOperationException => StatusCodes.Status400BadRequest,
                _ => StatusCodes.Status500InternalServerError
            };

            IResult result = Results.Problem(
                title: "ASPA006_1",
                detail: exception.Message,
                statusCode: statusCode,
                instance: context.Request.Path);
            await result.ExecuteAsync(context);
        }
    }
}
