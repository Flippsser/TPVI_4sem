using ASPA008_1.Infrastructure;
using Microsoft.AspNetCore.DataProtection;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

string dataProtectionKeys = Path.Combine(builder.Environment.ContentRootPath, "App_Data", "DataProtection-Keys");
Directory.CreateDirectory(dataProtectionKeys);
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionKeys))
    .SetApplicationName("ASPA008_1");

builder.AddCelebritiesConfiguration();
builder.AddCelebritiesServices();
builder.Services.AddControllersWithViews();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCelebritiesPhotos();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "new-celebrity",
    pattern: "0",
    defaults: new { controller = "Celebrities", action = "NewHumanForm" });

app.MapControllerRoute(
    name: "celebrity",
    pattern: "{id:int:min(1)}",
    defaults: new { controller = "Celebrities", action = "Human" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Celebrities}/{action=Index}/{id?}");

app.Run();
