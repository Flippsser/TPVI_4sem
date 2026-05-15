using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// Добавляем сервисы, необходимые для работы Razor Pages (.cshtml страницы)
builder.Services.AddRazorPages();

var app = builder.Build();

// Создаём объект параметров для DefaultFilesMiddleware
DefaultFilesOptions options = new DefaultFilesOptions();
options.DefaultFileNames.Clear();
options.DefaultFileNames.Add("Neumann.html");
// Подключаем middleware, который ищет файл Neumann.html в wwwroot
app.UseDefaultFiles(options);
app.UseStaticFiles("/static");

app.UseStaticFiles(new StaticFileOptions()
{
    FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), @"Picture")),
    RequestPath = new PathString("/staticPicture")
});

if (!app.Environment.IsDevelopment())
{
    // Глобальный обработчик исключений: при ошибке перенаправлять на страницу /Error
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();