using ASPA008_1.CountryCodes;
using ASPA008_1.Services;
using DAL_Celebrity_MSSQL;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;

namespace ASPA008_1.Infrastructure;

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
        builder.Services.AddSingleton<CountryCodesService>();
        builder.Services.AddScoped<WikiInfoCelebrity>();
        builder.Services.AddScoped<IRepository>(serviceProvider =>
        {
            CelebritiesOptions options = serviceProvider.GetRequiredService<IOptions<CelebritiesOptions>>().Value;
            return Repository.Create(options.ConnectionString);
        });
        builder.Services.AddHttpClient("Wikipedia", client =>
        {
            client.BaseAddress = new Uri("https://ru.wikipedia.org/");
            client.Timeout = TimeSpan.FromSeconds(3);
            client.DefaultRequestHeaders.UserAgent.ParseAdd("ASPA008_1/1.0");
        });

        return builder;
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
}
