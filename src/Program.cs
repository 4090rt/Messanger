using Messangers.Delegate;
using Messangers.DeserializeRequestHttp;
using Messangers.EthernetRequest;
using Messangers.JWToken;
using Messangers.ModelData;
using Messangers.SignalSettings.Hubs;
using Messangers.SQLite.ContactBse;
using Messangers.SQLite.ContactBse.CountOfUserVidget;
using Messangers.SQLite.ContactBse.DeleteContact;
using Messangers.SQLite.ContactBse.UserSave;
using Messangers.SQLite.ContactBse.UserSearchContact;
using Messangers.SQLite.ContactBse.UserSerach;
using Messangers.SQLite.DataBaseCreatesTables.CreateDataBases;
using Messangers.SQLite.DataBaseCreatesTables.InithilizateDataBaseCreate;
using Messangers.SQLite.HistroyMessage;
using Messangers.SQLite.HistroyMessage.HistoryAttachment;
using Messangers.SQLite.PoolSQLiteConnection;
using Messangers.SQLite.UserProviderInsert;
using Messangers.SQLite.ValidationAndRegistrationUserRequest.RequestRegisterAndLogin;
using Messangers.SQLite.ValidationAndRegistrationUserRequest.UserLoginCheck;
using MessangersUI.Delegate;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;
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
builder.Services.AddHttpClient();
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
                Console.WriteLine($"🔑 Получен токен: {accessToken}");
                Console.WriteLine($"📍 Путь: {path}");
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/chatHub"))
                {
                    context.Token = accessToken;
                    Console.WriteLine("✅ Токен установлен в контекст");

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
builder.Services.AddScoped<PingRequest>();
builder.Services.AddScoped<RequesetInfoProviders>();
builder.Services.AddScoped<Deserialize>();
builder.Services.AddScoped<InsertProvider>();
builder.Services.AddScoped<Search>();
builder.Services.AddScoped<SaveClass>();
builder.Services.AddScoped<UserSearchContacts>();
builder.Services.AddScoped<DeleteContact>();
builder.Services.AddScoped<ValidateContact>();
builder.Services.AddScoped<CountUser>();
builder.Services.AddScoped<SignalHub>();
builder.Services.AddScoped<SaveHistoryMessage>();
builder.Services.AddScoped<UserSearchHistoryDowload>();
builder.Services.AddScoped<DeleteHistory>();
builder.Services.AddScoped<DeleteConcrectMessage>();
builder.Services.AddScoped<AttachmentSave>();
builder.Services.AddScoped<AttachmentIdUpdate>();
builder.Services.AddScoped<FileHistory>();

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


_ = Task.Run(async () =>
{
    while (true)
    {
        string command = Console.ReadLine();

        if (command == "provider")
        {
            using (var scope = app.Services.CreateScope())
            {
                var request = scope.ServiceProvider.GetRequiredService<RequesetInfoProviders>();
                var result = await request.CacheRequest();
                foreach (var item in result)
                {
                    Console.WriteLine($"IP: {item.IP}");
                    Console.WriteLine($"Город: {item.City}");
                    Console.WriteLine($"Провайдер: {item.Org}");
                    Console.WriteLine($"Регион: {item.Region}");
                    Console.WriteLine($"Страна: {item.Country}");
                    Console.WriteLine($"Почтовый индекс: {item.Postal}");
                }
            }
        }

        if (command == "ping")
        {
            using (var scope = app.Services.CreateScope())
            {
                string host = "www.google.com";
                var requesrt = scope.ServiceProvider.GetService<PingRequest>();
                var result = await requesrt.Request(host);

                foreach (var item in result)
                {
                    Console.WriteLine($"PingMS: {item.PingMs}");
                    Console.WriteLine($"Status: {item.Status}");
                    Console.WriteLine($"HOST: {item.Host}");
                    Console.WriteLine($"Errors: {item.Error}");
                }
            }
        }
    }
});

// 6. Запуск (здесь сервер начинает слушать запросы)
Console.WriteLine("Сервер успешно запущен!");
app.Run();  // ? блокирует выполнение, сервер работает


