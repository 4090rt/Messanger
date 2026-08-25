using MessangersUI.DataModel;
using MessangersUI.Delegate;
using MessangersUI.HttpReuest.PostRequestAvatar;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Polly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection.Metadata;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MessangersUI.HttpReuest.PostRequestNumberEmail
{
    public class GiveMailNumber
    {
        private readonly ILogger<GiveMailNumber> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        private readonly ExceptionDelegate _exceptionDelegate;
        private readonly HttpExceptionDelegate _httpExceptionDelegate;
        private readonly JsonExceptionDelegate _jsonExceptionDelegate;
        private readonly TaskCanccelException _taskCanccelException;
        private readonly IMemoryCache _memoryCache;

        private readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);

        private readonly string Url = "https://localhost:7167/api/ControllerGiveMailPhone/giveMailPhoneControoller";

        public GiveMailNumber(ILogger<GiveMailNumber> logger, IHttpClientFactory httpClientFactory,
            ExceptionDelegate exceptionDelegate, HttpExceptionDelegate httpExceptionDelegate, JsonExceptionDelegate jsonExceptionDelegate,
            TaskCanccelException taskCanccelException, IMemoryCache memoryCache)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _exceptionDelegate = exceptionDelegate;
            _httpExceptionDelegate = httpExceptionDelegate;
            _jsonExceptionDelegate = jsonExceptionDelegate;
            _taskCanccelException = taskCanccelException;
            _memoryCache = memoryCache;
        }

        public async Task<MailNumberStrcuct> CacheRequestGive(string username)
        {
            string cache_key = username + "cache_key_giveMailNumber";
            string stalecache_key = cache_key + "stale";
            MailNumberStrcuct oldcache = new MailNumberStrcuct();

            if (_memoryCache.TryGetValue(cache_key, out MailNumberStrcuct cached))
            {
                oldcache = cached;
                return cached;
            }

            await _semaphoreSlim.WaitAsync().ConfigureAwait(false);

            try
            {
                if (_memoryCache.TryGetValue(cache_key, out MailNumberStrcuct cached2))
                {
                    oldcache = cached;
                    return cached;
                }

                var fallback = Policy<MailNumberStrcuct>
                    .Handle<Exception>()
                    .OrResult(r => string.IsNullOrEmpty(r.Phone) || string.IsNullOrEmpty(r.Phone))
                    .FallbackAsync(
                    fallbackAction: async (outcome, context, ctx) =>
                    {
                        var exception = outcome.Exception;

                        if (exception != null)
                        {
                            _logger.LogWarning($"⚠️ Fallback by exception: {exception.Message}");
                            return default;
                        }
                        else if (!string.IsNullOrEmpty(oldcache.Phone) || !string.IsNullOrEmpty(oldcache.Mail))
                        {
                            _logger.LogInformation("✅ Fallback: возвращаю старые данные из кэша");
                            return oldcache;
                        }
                        else if (_memoryCache.TryGetValue(stalecache_key, out MailNumberStrcuct stalecached))
                        {
                            _logger.LogInformation($"✅ Returning stale copy for {stalecached}");
                            return stalecached;
                        }
                        else
                        {
                            _logger.LogWarning("⚠️ Fallback: кэш пуст, возвращаю default");
                            return default;
                        }
                    },
                    onFallbackAsync: async (outcome, ctx) =>
                    {
                        await Task.CompletedTask;
                    });

                var resultfallback = await fallback.ExecuteAsync(async () =>
                {
                    MailNumberStrcuct mailNumberStrcuct = await RequestGive(username).ConfigureAwait(false);

                    if (string.IsNullOrEmpty(oldcache.Phone) && string.IsNullOrEmpty(oldcache.Mail))
                        return default;

                    var memoryoptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(15))
                    .SetSlidingExpiration(TimeSpan.FromMinutes(10));

                    _memoryCache.Set(cache_key, mailNumberStrcuct, memoryoptions);

                    var stalememoryoptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(20))
                    .SetSlidingExpiration(TimeSpan.FromMinutes(15));

                    _memoryCache.Set(stalecache_key, mailNumberStrcuct, stalememoryoptions);

                    return mailNumberStrcuct;
                });

                return resultfallback;
            }
            catch (Exception ex)
            {
                await _exceptionDelegate.RunDelegate(_exceptionDelegate.DelegateException, ex);
                return new MailNumberStrcuct();
            }
            finally
            { 
                _semaphoreSlim.Release();
            }
        }

        public async Task<MailNumberStrcuct> RequestGive(string username)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("Client1Http2.0");

                var jsonser = JsonSerializer.Serialize(username, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                var content = new StringContent(jsonser,Encoding.UTF8,"application/json");

                using var cts = new CancellationTokenSource();

                HttpResponseMessage httpResponseMessage = await client.PostAsync(Url, content, cts.Token).ConfigureAwait(false);
                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    byte[] result = await httpResponseMessage.Content.ReadAsByteArrayAsync().ConfigureAwait(false);

                    ReadOnlyMemory<byte> readOnlyMemorybytes = result.AsMemory();
                    if (readOnlyMemorybytes.Length != 0)
                    {
                        var jsondoc = JsonDocument.Parse(readOnlyMemorybytes);
                        var root = jsondoc.RootElement;

                        if (root.TryGetProperty("Mail", out var mail) && root.TryGetProperty("Phone", out var number))
                        {
                            var mailadress = mail.GetString() ?? string.Empty;
                            var phonenumber = number.GetString() ?? string.Empty;

                            MailNumberStrcuct mailNumberStrcuct = new MailNumberStrcuct
                            {
                                Mail = mailadress,
                                Phone = phonenumber
                            };

                            return mailNumberStrcuct;
                        }
                        else
                        { 
                            var resultf = System.Text.Encoding.UTF8.GetString(readOnlyMemorybytes.ToArray());
                            MessageBox.Show($"Неожиданный ответ от серве в RequestGive\n" +
                             $"{resultf}");
                            return new MailNumberStrcuct();
                        }
                    }
                    else
                    {
                        return new MailNumberStrcuct();
                    }
                }
                else
                {
                    byte[] result = await httpResponseMessage.Content.ReadAsByteArrayAsync().ConfigureAwait(false);

                    ReadOnlyMemory<byte> readOnlyMemorybytes = result.AsMemory();

                    if (readOnlyMemorybytes.Length != 0)
                    {
                        var jsondoc = JsonDocument.Parse(readOnlyMemorybytes);
                        var root = jsondoc.RootElement;

                        if (root.TryGetProperty("ErrorBody", out var body) && root.TryGetProperty("Status", out var status))
                        {
                            var errobody = body.GetString() ?? string.Empty;
                            var statuscode = status.GetString() ?? string.Empty;

                            MessageBox.Show($"Ошибка запроса в RequestGive\n" +
                            $"{errobody}");
                            return new MailNumberStrcuct();
                        }
                        else
                        {
                            var resultf = System.Text.Encoding.UTF8.GetString(readOnlyMemorybytes.ToArray());
                            MessageBox.Show($"Неожиданный ответ от серве в RequestGive\n" +
                             $"{resultf}");
                            return new MailNumberStrcuct();
                        }
                    }
                    else
                    {
                        return new MailNumberStrcuct();
                    }
                }
            }
            catch (TaskCanceledException ex)
            {
                await _taskCanccelException.RunDelegate(_taskCanccelException.Delegate, ex);
                return new MailNumberStrcuct();
            }
            catch (HttpRequestException ex)
            {
                await _httpExceptionDelegate.RunDelegate(_httpExceptionDelegate.Delegate, ex);
                return new MailNumberStrcuct();
            }
            catch (JsonException ex)
            {
                await _jsonExceptionDelegate.RunDelegate(_jsonExceptionDelegate.Delegate, ex);
                return new MailNumberStrcuct();
            }
            catch (Exception ex)
            {
                await _exceptionDelegate.RunDelegate(_exceptionDelegate.DelegateException, ex);
                return new MailNumberStrcuct();
            }
        }
    }
}
