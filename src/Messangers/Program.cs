using Messangers.Delegate;
using Messangers.JWToken;
using Messangers.SignalSettings.Hubs;
using Messangers.SQLite.CreateDataBases;
using Messangers.SQLite.InithilizateDataBaseCreate;
using Messangers.SQLite.PoolSQLiteConnection;
using Messangers.SQLite.RequestRegisterAndLogin;
using Messangers.SQLite.UserLoginCheck;
using MessangersUI.Delegate;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Text.Json;
var builder = WebApplication.CreateBuilder(args);


string hsondoc = File.ReadAllText("appsettings.json");
using JsonDocument doc = JsonDocument.Parse(hsondoc);

string secretkey = doc.RootElement
    .GetProperty("SecretKey")
    .GetProperty("key")
    .GetString();

// 1. Регистрация сервисов
builder.Services.AddSignalR();
builder.Services.AddRazorPages();
builder.Services.AddMemoryCache();
builder.Services.AddControllers();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(OPTIONS =>
    {
        OPTIONS.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                // Получаем токен из query string для SignalR подключений
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/chatHub"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
        OPTIONS.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = "https://localhost:7167",

            ValidateAudience = true,
            ValidAudience = "Client",

            ValidateLifetime = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretkey)),

            NameClaimType = JwtRegisteredClaimNames.UniqueName
        };
    });

builder.Services.AddScoped<Inithializate>();
builder.Services.AddScoped<PoolSQLite>();
builder.Services.AddScoped<ExceptionDelegate>();
builder.Services.AddScoped<SQLiteExceptionDelegate>();
builder.Services.AddScoped<CreateRegisterBase>();
builder.Services.AddScoped<SaveRequestInBdRegister>();
builder.Services.AddScoped<CheckLogin>();
builder.Services.AddScoped<CheckHashPasswordFromBD>();
builder.Services.AddScoped<CheckUserInBD>();
builder.Services.AddScoped<JWTokenSettings>();


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
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

using (var scope = app.Services.CreateScope())
{
    var inithializate = scope.ServiceProvider.GetRequiredService<Inithializate>();
    await inithializate.MethodCreateBase();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<SignalHub>("/chatHub");
app.MapStaticAssets();
app.MapRazorPages().WithStaticAssets();

// 6. Запуск (здесь сервер начинает слушать запросы)
Console.WriteLine("Сервер успешно запущен!");
app.Run();  // ? блокирует выполнение, сервер работает
