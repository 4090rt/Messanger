using MessangersUI.Delegate;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MessangersUI.HttpReuest.PostRequestEthernetStat
{
    public class HttpRequestPostCientProvider
    {
        private readonly ILogger<HttpRequestPostCientProvider> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ExceptionDelegate _exceptionDelegate;
        private readonly HttpExceptionDelegate _httpExceptionDelegate;
        private readonly JsonExceptionDelegate _jsonExceptionDelegate;
        private readonly TaskCanccelException _taskCanccelException;

        public HttpRequestPostCientProvider(ILogger<HttpRequestPostCientProvider> logger, IHttpClientFactory httpClientFactory, ExceptionDelegate exceptionDelegate,
            HttpExceptionDelegate httpExceptionDelegate, JsonExceptionDelegate jsonExceptionDelegate, TaskCanccelException taskCanccelException)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _exceptionDelegate = exceptionDelegate;
            _httpExceptionDelegate = httpExceptionDelegate;
            _jsonExceptionDelegate = jsonExceptionDelegate;
            _taskCanccelException = taskCanccelException;
        }

        public async Task<(string succes, string message)> Request(Stream stream)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("Client1Http2.0");

                var options = new HttpRequestMessage(HttpMethod.Post, "https://localhost:7167/api/ControllerPostProvider/provider")
                {
                    Version = HttpVersion.Version20,
                    VersionPolicy = HttpVersionPolicy.RequestVersionOrHigher
                };

                byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(stream);
                var string64 = Convert.ToBase64String(bytes);

                var json = JsonSerializer.Serialize(string64, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                });

                using var content = new StringContent(json, Encoding.UTF8,"application/json");

                options.Content  = content;

                HttpResponseMessage response = await client.SendAsync(options).ConfigureAwait(false);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    if (result != null)
                    {
                        var jsonobject = JsonDocument.Parse(result);
                            string propeties1 = jsonobject.RootElement.GetProperty("Message").GetString() ?? string.Empty;
                            string properties2 = jsonobject.RootElement.GetProperty("State").GetString() ?? string.Empty;

                            return (propeties1, properties2);
                    }
                    else return ("Данные не получены", "error");
                }
                else
                { 
                    var result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var jsonobject = JsonDocument.Parse(result);
                    if (jsonobject != null)
                    {
                        string properties1 = jsonobject.RootElement.GetProperty("Message").GetString() ?? string.Empty;
                        string properties2 = jsonobject.RootElement.GetProperty("State").GetString() ?? string.Empty;
                    }
                    else return ("Данные не получены", "error");
                    return ("Данные не получены", "error");
                }
            }
            catch (TaskCanceledException ex)
            {
                await _taskCanccelException.RunDelegate(_taskCanccelException.Delegate, ex);
                return ("Данные не получены", "error");
            }
            catch (HttpRequestException ex)
            {
                await _httpExceptionDelegate.RunDelegate(_httpExceptionDelegate.Delegate, ex);
                return ("Данные не получены", "error");
            }
            catch (Exception ex)
            { 
                await _exceptionDelegate.RunDelegate(_exceptionDelegate.DelegateException, ex);
                return ("Данные не получены", "error");
            }
        }
    }
}
