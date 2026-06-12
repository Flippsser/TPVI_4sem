using DAL_Celebrity_MSSQL;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;

namespace ASPA007_1.Infrastructure;

public static class CelebritiesWebApplicationExtensions
{
    public static WebApplicationBuilder AddCelebritiesConfiguration(this WebApplicationBuilder builder)
    {
        builder.Configuration.AddJsonFile("Celebrities.config.json", optional: false, reloadOnChange: true);
        builder.Services.Configure<CelebritiesOptions>(builder.Configuration.GetSection(CelebritiesOptions.SectionName));
        return builder;
    }

    public static WebApplicationBuilder AddCelebritiesServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton<CelebritiesPathService>();
        builder.Services.AddScoped<IRepository>(serviceProvider =>
        {
            CelebritiesOptions options = serviceProvider.GetRequiredService<IOptions<CelebritiesOptions>>().Value;
            return Repository.Create(options.ConnectionString);
        });

        return builder;
    }

    public static IApplicationBuilder UseANCErrorHandler(this IApplicationBuilder app, string errorCode)
    {
        return app.UseExceptionHandler(errorApp =>
        {
            errorApp.Run(async context =>
            {
                IExceptionHandlerPathFeature? feature = context.Features.Get<IExceptionHandlerPathFeature>();
                Exception? exception = feature?.Error;

                int statusCode = exception switch
                {
                    BadHttpRequestException => StatusCodes.Status400BadRequest,
                    ArgumentException => StatusCodes.Status400BadRequest,
                    InvalidOperationException => StatusCodes.Status400BadRequest,
                    FileNotFoundException => StatusCodes.Status404NotFound,
                    _ => StatusCodes.Status500InternalServerError
                };

                context.Response.StatusCode = statusCode;
                await Results.Problem(
                    title: errorCode,
                    detail: exception?.Message ?? "Unexpected server error.",
                    statusCode: statusCode,
                    instance: context.Request.Path).ExecuteAsync(context);
            });
        });
    }

    public static WebApplication UseCelebritiesPhotos(this WebApplication app)
    {
        CelebritiesPathService paths = app.Services.GetRequiredService<CelebritiesPathService>();
        paths.EnsurePhotosFolder();

        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(paths.PhotosFolder),
            RequestPath = paths.RequestPath
        });

        return app;
    }

    public static RouteGroupBuilder MapCelebrities(this WebApplication app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/Celebrities");

        group.MapGet("/", (IRepository repository) => Results.Ok(repository.GetAllCelebrities()));
        group.MapGet("/{id:int:min(1)}", (int id, IRepository repository) =>
            repository.GetCelebrityById(id) is { } celebrity
                ? Results.Ok(celebrity)
                : Results.NotFound(new { message = $"Celebrity with Id = {id} was not found." }));
        group.MapGet("/Lifeevents/{id:int:min(1)}", (int id, IRepository repository) =>
            repository.GetCelebrityByLifeeventId(id) is { } celebrity
                ? Results.Ok(celebrity)
                : Results.NotFound(new { message = $"Celebrity for Lifeevent Id = {id} was not found." }));
        group.MapPost("/", (Celebrity celebrity, IRepository repository) =>
        {
            repository.AddCelebrity(celebrity);
            return Results.Created($"/api/Celebrities/{celebrity.Id}", celebrity);
        });
        group.MapPut("/{id:int:min(1)}", (int id, Celebrity celebrity, IRepository repository) =>
            repository.UpdCelebrity(id, celebrity)
                ? Results.Ok(repository.GetCelebrityById(id))
                : Results.NotFound(new { message = $"Celebrity with Id = {id} was not found." }));
        group.MapDelete("/{id:int:min(1)}", (int id, IRepository repository) =>
            repository.DelCelebrity(id)
                ? Results.Ok(new { message = $"Celebrity with Id = {id} deleted." })
                : Results.NotFound(new { message = $"Celebrity with Id = {id} was not found." }));

        return group;
    }

    public static RouteGroupBuilder MapLifeevents(this WebApplication app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/Lifeevents");

        group.MapGet("/", (IRepository repository) => Results.Ok(repository.GetAllLifeevents()));
        group.MapGet("/{id:int:min(1)}", (int id, IRepository repository) =>
            repository.GetLifeevetById(id) is { } lifeevent
                ? Results.Ok(lifeevent)
                : Results.NotFound(new { message = $"Lifeevent with Id = {id} was not found." }));
        group.MapGet("/Celebrities/{id:int:min(1)}", (int id, IRepository repository) =>
            Results.Ok(repository.GetLifeeventsByCelebrityId(id)));
        group.MapPost("/", (Lifeevent lifeevent, IRepository repository) =>
        {
            repository.AddLifeevent(lifeevent);
            return Results.Created($"/api/Lifeevents/{lifeevent.Id}", lifeevent);
        });
        group.MapPut("/{id:int:min(1)}", (int id, Lifeevent lifeevent, IRepository repository) =>
            repository.UpdLifeevent(id, lifeevent)
                ? Results.Ok(repository.GetLifeevetById(id))
                : Results.NotFound(new { message = $"Lifeevent with Id = {id} was not found." }));
        group.MapDelete("/{id:int:min(1)}", (int id, IRepository repository) =>
            repository.DelLifeevent(id)
                ? Results.Ok(new { message = $"Lifeevent with Id = {id} deleted." })
                : Results.NotFound(new { message = $"Lifeevent with Id = {id} was not found." }));

        return group;
    }

    public static RouteGroupBuilder MapPhotoCelebrities(this WebApplication app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/PhotoCelebrities");

        group.MapGet("/{fileName}", (string fileName, CelebritiesPathService paths) =>
        {
            string safeFileName = Path.GetFileName(fileName);
            string photoPath = paths.GetPhotoPath(safeFileName);

            return File.Exists(photoPath)
                ? Results.File(photoPath, GetContentType(photoPath))
                : Results.NotFound(new { message = $"Photo {safeFileName} was not found." });
        });

        return group;
    }

    private static string GetContentType(string filePath)
    {
        return Path.GetExtension(filePath).ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            ".svg" => "image/svg+xml",
            _ => "application/octet-stream"
        };
    }
}
