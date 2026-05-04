using Messangers.DeserializeRequestHttp;
using Messangers.ModelData;
using MessangersUI.Delegate;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Polly;
using System;
using System.IO;
using System.Net;
using System.Net.Http;

namespace Messangers.EthernetRequest
{
    public class RequesetInfoProviders
    {
        private readonly ILogger<RequesetInfoProviders> _loggger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);
        private readonly IMemoryCache _memoryCache;
        private readonly ExceptionDelegate _exceptionDelegate;
        private readonly HttpExceptionDelegate _httpExceptionDelegate;
        private readonly JsonExceptionDelegate _jsonExceptionDelegate;
        private readonly TaskCanccelException _taskCanccelException;
        public RequesetInfoProviders(ILogger<RequesetInfoProviders> loggger,
            IHttpClientFactory httpClientFactory,
            IMemoryCache memoryCache,
            ExceptionDelegate exceptionDelegate,
            HttpExceptionDelegate _httpExceptionDelegate,
            JsonExceptionDelegate _jsonExceptionDelegate,
            TaskCanccelException _taskCanccelException
            )
        {
            _loggger = loggger;
            _httpClientFactory = httpClientFactory;
            _memoryCache = memoryCache;
        }

        public async Task<byte[]> CacheReqquest() 
        {
            string cachekey = "providercache_key";
            string stalecachekey = $"stale{cachekey}";
            byte[] oldcache = null;
            if (_memoryCache.TryGetValue(cachekey, out byte[] cached))
            { 
                oldcache = cached;
                return cached;
            }

            await _semaphoreSlim.WaitAsync();

            try
            {
                if (_memoryCache.TryGetValue(cachekey, out byte[] cached2))
                {
                    return cached2;
                }

                var fallback = Policy<byte[]>
                    .Handle<Exception>()
                    .OrResult(r => r == null)
                    .FallbackAsync(
                    fallbackAction: async (outcome, context, ctx) =>
                    {
                        var Exception = outcome.Exception;
                        var isEmpty = outcome.Result == null;

                        if (Exception != null)
                        {
                            _loggger.LogWarning($"⚠️ Fallback by exception: {Exception.Message}");
                        }
                        if (isEmpty)
                        {
                            _loggger.LogWarning($"⚠️ Fallback by empty result");
                        }
                        if (oldcache != null)
                        {
                            _loggger.LogInformation("✅ Fallback: возвращаю старые данные из кэша");
                            return oldcache;
                        }
                        if (_memoryCache.TryGetValue(stalecachekey, out byte[] stalecached))
                        {
                            _loggger.LogInformation($"✅ Returning stale copy");
                            return stalecached;
                        }
                        else
                        {
                            _loggger.LogWarning("⚠️ Fallback: кэш пуст, возвращаю default");
                            return default;
                        }
                    }, onFallbackAsync: async (outcome, ctx) =>
                    {
                        _loggger.LogError($"🆘 Fallback сработал: {outcome.Exception?.Message}");
                        await Task.CompletedTask;
                    });

                var fallbackresult = await fallback.ExecuteAsync(async () =>
                {
                    var result = await Request().ConfigureAwait(false);
                    if (result != null)
                    {
                        var options = new MemoryCacheEntryOptions()
                        .SetAbsoluteExpiration(TimeSpan.FromMinutes(15))
                        .SetSlidingExpiration(TimeSpan.FromMinutes(10));

                        _memoryCache.Set(cachekey, result, options);

                        var staleoptions = new MemoryCacheEntryOptions()
                        .SetAbsoluteExpiration(TimeSpan.FromMinutes(15));

                        _memoryCache.Set(stalecachekey, result, staleoptions);

                        return result;
                    }
                    else
                    {
                        _loggger.LogWarning("Результат пуст");
                        return default;
                    }
                });

                return fallbackresult;
            }
            catch (Exception ex)
            {
                await _exceptionDelegate.RunDelegate(_exceptionDelegate.DelegateException, ex);
                return default;
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }

        public async Task<byte[]> Request()
        {
            try
            {
                System.Windows.MessageBox.Show("нАЧИНАЮ ЗАПРОС");
                var client = _httpClientFactory.CreateClient("Client1Http2.0");

                var options = new HttpRequestMessage(HttpMethod.Get, "https://ipinfo.io/json")
                {
                    Version = HttpVersion.Version20,
                    VersionPolicy = HttpVersionPolicy.RequestVersionOrHigher
                };

                HttpResponseMessage response = await client.SendAsync(options).ConfigureAwait(false);
                if (response.IsSuccessStatusCode)
                {
                    byte[] bytesesult = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);

                    if (bytesesult != null)
                    {
                        return bytesesult;
                    }
                    else
                    {
                        _loggger.LogError("Массив байтов пуст");
                        return default;
                    }
                }
                else
                {
                    _loggger.LogError("Возникла ошибка при запросе провайдера. Статус код:" + response.StatusCode);
                    return default;
                }
            }
            catch (TaskCanceledException ex)
            {
                await _taskCanccelException.RunDelegate(_taskCanccelException.Delegate, ex);
                return  default;
            }
            catch (HttpRequestException ex)
            {
                await _httpExceptionDelegate.RunDelegate(_httpExceptionDelegate.Delegate, ex);
                return default;
            }
            catch (Exception ex)
            {
                await _exceptionDelegate.RunDelegate(_exceptionDelegate.DelegateException, ex);
                return default;
            }
        }
    }
}
