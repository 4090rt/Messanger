using Messangers.DeserializeRequestHttp;
using Messangers.ModelData;
using MessangersUI.Delegate;
using Microsoft.Extensions.Caching.Memory;
using Polly;
using System;
using System.Net;

namespace Messangers.EthernetRequest
{
    public class RequesetInfoProvidersServer
    {
        private readonly ILogger<RequesetInfoProvidersServer> _loggger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);
        private readonly Deserialize _deserialize;
        private readonly IMemoryCache _memoryCache;
        public RequesetInfoProvidersServer(ILogger<RequesetInfoProvidersServer> loggger, IHttpClientFactory httpClientFactory, Deserialize deserialize, IMemoryCache memoryCache)
        {
            _loggger = loggger;
            _httpClientFactory = httpClientFactory;
            _deserialize = deserialize;
            _memoryCache = memoryCache;
        }

        public async Task<List<IpIfo>> CacheRequest()
        {
            string cachecode = "cache_code_ipinfo";
            string stalecache = $"{cachecode}stale";
            List<IpIfo> oldcache = null;

            if (_memoryCache.TryGetValue(cachecode, out List<IpIfo> cached) && cached != null)
            { 
                oldcache = cached;
                return cached;
            }

            await _semaphoreSlim.WaitAsync();

            try
            {
                if (_memoryCache.TryGetValue(cachecode, out List<IpIfo> cached2) && cached2 != null)
                {
                    return cached2;
                }

                var fallback = Policy<List<IpIfo>>
                    .Handle<Exception>()
                    .OrResult(r => r == null)
                    .FallbackAsync(
                    fallbackAction: async (outcome, context, ctx) =>
                    {
                        var eception = outcome.Exception;
                        var isEmpty = outcome.Result == null;

                        if (eception != null)
                        {
                            _loggger.LogWarning($"⚠️ Fallback by exception: {eception.Message}");
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
                        if (_memoryCache.TryGetValue(stalecache, out List<IpIfo> stalecached))
                        {
                            _loggger.LogInformation($"✅ Returning stale copy for {stalecached}");
                            return stalecached;
                        }
                        else
                        {
                            _loggger.LogWarning("⚠️ Fallback: кэш пуст, возвращаю default");
                            return default;
                        }
                    },
                    onFallbackAsync: async (outcome, ctx) =>
                    {
                        _loggger.LogError($"🆘 Fallback сработал: {outcome.Exception?.Message}");
                        await Task.CompletedTask;
                    });

                var fallbackresult = await fallback.ExecuteAsync(async () =>
                {
                    var result = await Request();

                    if (result != null && result.Any())
                    {
                        var options = new MemoryCacheEntryOptions()
                        .SetAbsoluteExpiration(TimeSpan.FromMinutes(15))
                        .SetSlidingExpiration(TimeSpan.FromMinutes(10));

                        _memoryCache.Set(cachecode, result, options);

                        var staleoptions = new MemoryCacheEntryOptions()
                        .SetAbsoluteExpiration(TimeSpan.FromMinutes(15));

                        _memoryCache.Set(stalecache, result, staleoptions);

                        return result;
                    }
                    else
                    {
                        _loggger.LogError("Результат запроса пуст");
                        return default;
                    }
                });
                return fallbackresult;
            }
            catch (Exception ex)
            {
                _loggger.LogError("Возникло необработанное исключение" + ex.Message + ex.StackTrace + ex.InnerException);
                return new List<IpIfo>();
            }
            finally
            { 
                _semaphoreSlim.Release();
            }
        }

        public async Task<List<IpIfo>> Request()
        {
            try
            {
                var client = _httpClientFactory.CreateClient("ClientServerPost1");

                var options = new HttpRequestMessage(HttpMethod.Get, "https://ipinfo.io/json")
                {
                    Version = HttpVersion.Version20,
                    VersionPolicy = HttpVersionPolicy.RequestVersionOrHigher
                };

                HttpResponseMessage responseMessage = await client.SendAsync(options).ConfigureAwait(false);
                if (responseMessage.IsSuccessStatusCode)
                {
                    var resultread = await responseMessage.Content.ReadAsStreamAsync().ConfigureAwait(false);
                    var pars = await _deserialize.Parsing<IpIfo>(resultread);
                    return pars;
                }
                else
                {
                    _loggger.LogError($"Возникла ошибка запрос. Статус код" + responseMessage.StatusCode);
                    return new List<IpIfo>();
                }
            }
            catch (TaskCanceledException ex)
            {
                _loggger.LogError("Операция отменена" + ex.Message + ex.StackTrace + ex.InnerException);
                return new List<IpIfo>();
            }
            catch (HttpRequestException ex)
            {
                _loggger.LogError("Возникло необработанное HTTP ислюкчение" + ex.Message + ex.StackTrace + ex.InnerException);
                return new List<IpIfo>();
            }
            catch (Exception ex)
            {
                _loggger.LogError("Возникло необработанное исключение" + ex.Message + ex.StackTrace + ex.InnerException);
                return new List<IpIfo>();
            }
        }
    }
}
