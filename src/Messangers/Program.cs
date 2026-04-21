using Messangers.SignalSettings.Hubs;
using Microsoft.Extensions.Logging;
var builder = WebApplication.CreateBuilder(args);

// 1. Регистрация сервисов
builder.Services.AddSignalR();
builder.Services.AddRazorPages();
builder.Services.AddMemoryCache();
builder.Services.AddControllers();

// 2. Настройка конфигурации
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddEnvironmentVariables();

// 3. Настройка логирования
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Warning);

// 4. Построение приложения
var app = builder.Build();

// 5. Настройка pipeline (middleware)
app.MapControllers();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();

app.MapHub<SignalHub>("/chatHub");
app.MapStaticAssets();
app.MapRazorPages().WithStaticAssets();

// 6. Запуск (здесь сервер начинает слушать запросы)
Console.WriteLine("Сервер успешно запущен!");
app.Run();  // ? блокирует выполнение, сервер работает
