using ASPA007_1.Infrastructure;
using Microsoft.AspNetCore.DataProtection;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();


string dataProtectionKeys = Path.Combine(builder.Environment.ContentRootPath, "App_Data", "DataProtection-Keys");
Directory.CreateDirectory(dataProtectionKeys);
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionKeys))
    .SetApplicationName("ASPA007_1");

builder.AddCelebritiesConfiguration();
builder.AddCelebritiesServices();

builder.Services.AddRazorPages(options =>
{
    options.Conventions.AddPageRoute("/Celebrities", "/");
    options.Conventions.AddPageRoute("/NewCelebrity", "/0");
    options.Conventions.AddPageRoute("/Celebrity", "/Celebrities/{id:int:min(1)}");
    options.Conventions.AddPageRoute("/Celebrity", "/{id:int:min(1)}");
}
);

var app = builder.Build();


app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCelebritiesPhotos();

app.UseRouting();

app.MapRazorPages();
app.MapCelebrities();
app.MapLifeevents();
app.MapPhotoCelebrities();

app.Run();
