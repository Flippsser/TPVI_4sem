using Microsoft.AspNetCore.Diagnostics;

internal class Program
{
    private static void Main(string[] args)
    {
    
        var builder = WebApplication.CreateBuilder(args);

        // Отключаем логирование для категории "Microsoft.AspNetCore.Diagnostics",
        // чтобы не засорять консоль деталями обработки ошибок.
        builder.Logging.AddFilter("Microsoft.AspNetCore.Diagnostics", LogLevel.None);

        var app = builder.Build();

        app.UseExceptionHandler("/error");

        // Конечная точка без ошибки – возвращает строку "Start "
        app.MapGet("/", () => "Start ");

        // Конечная точка /test1 – явно выбрасывает исключение с сообщением
        app.MapGet("/test1", () =>
        {
            throw new Exception("-- Exception Test --");
        });

        // Конечная точка /test2 – вызывает исключение DivideByZeroException
        app.MapGet("/test2", () =>
        {
            int x = 0, y = 5, z = 0;
            z = y / x;   // деление на ноль – исключение
            return "test2";
        });

        // Конечная точка /test3 – вызывает исключение IndexOutOfRangeException
        app.MapGet("/test3", () =>
        {
            int[] x = new int[3] { 1, 2, 3 };
            int y = x[3];  // индекс вне границ массива
            return "test3";
        });

        // Обработчик ошибок по пути "/error"
        // Принимает ILogger и HttpContext, чтобы вывести сообщение об ошибке
        app.Map("/error", async (ILogger<Program> logger, HttpContext context) =>
        {
            // Извлекаем информацию о возникшем исключении из фич контекста
            IExceptionHandlerFeature? exobj = context.Features.Get<IExceptionHandlerFeature>();
            // Отправляем клиенту HTML-сообщение "Oops!"
            await context.Response.WriteAsync($"<h1>Oops!</h1>");
            // Логируем ошибку в консоль (или другой логгер) с пометкой "ExceptionHandler"
            logger.LogError(exobj?.Error, "ExceptionHandler");
        });

        app.Run();
    }
}