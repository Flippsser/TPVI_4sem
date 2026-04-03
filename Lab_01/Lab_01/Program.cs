// Подключаем необходимые пространства имен для создания веб-приложения.
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;

// Объявляем пространство имен, в котором находится наш класс Program.
namespace ASPA001;

// Объявление класса Program, который содержит точку входа в приложение.
public class Program
{
    // Метод Main — это точка входа в приложение.
    public static void Main(string[] args)
    {
        // Создаем объект WebApplicationBuilder. Он настраивает сервисы, конфигурацию и хост приложения.
        var builder = WebApplication.CreateBuilder(args);

        // ДОБАВИТЬ: Регистрируем сервис HTTPLogging в контейнере внедрения зависимостей
        builder.Services.AddHttpLogging(o => { }); // внутри можно настроить детали логирования

        // Строим приложение (объект WebApplication) из подготовленного builder'а.
        var app = builder.Build();

        // ДОБАВИТЬ: Добавляем для HTTP-логирования в конвейер обработки запросов
        app.UseHttpLogging();

        // Определяем конечную точку (endpoint). При GET-запросе к корню "/" будет возвращаться строка "Hello ASPA!".
        app.MapGet("/", () => "Hello ASPA!");

        // Запускаем приложение, которое начинает прослушивать входящие HTTP-запросы.
        app.Run();
    }
}