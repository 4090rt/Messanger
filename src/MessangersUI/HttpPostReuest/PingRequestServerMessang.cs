using MessangersUI.Delegate;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MessangersUI.HttpPostReuest
{
    public class PingRequestServerMessang
    {
        private readonly ILogger<PingRequestServerMessang> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ExceptionDelegate _exceptionDelegate;
        private readonly HttpExceptionDelegate _httpExceptionDelegate;
        private readonly JsonExceptionDelegate _jsonExceptionDelegate;
        private readonly TaskCanccelException _taskCanccelException;

        public PingRequestServerMessang(ILogger<PingRequestServerMessang> logger, IHttpClientFactory httpClientFactory, ExceptionDelegate exceptionDelegate,
            HttpExceptionDelegate httpExceptionDelegate, JsonExceptionDelegate jsonExceptionDelegate, TaskCanccelException taskCanccelException)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _exceptionDelegate = exceptionDelegate;
            _httpExceptionDelegate = httpExceptionDelegate;
            _jsonExceptionDelegate = jsonExceptionDelegate;
            _taskCanccelException = taskCanccelException;
        }

        public async Task<double> Request(string ping = "ping")
        {
            try
            {
                var client = _httpClientFactory.CreateClient("Client1Http2.0");

                var options = new HttpRequestMessage(HttpMethod.Post, "https://localhost:7167/api/ControllerPostPingToServer/ping")
                {
                    Version = HttpVersion.Version20,
                    VersionPolicy = HttpVersionPolicy.RequestVersionOrHigher
                };

                byte[] bytes = Encoding.UTF8.GetBytes(ping);
                string to64convert = Convert.ToBase64String(bytes);

                var json = JsonSerializer.Serialize(to64convert, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                options.Content = content;

                var timer = System.Diagnostics.Stopwatch.StartNew();
                HttpResponseMessage responc = await client.SendAsync(options).ConfigureAwait(false);
                if (responc.IsSuccessStatusCode)
                {
                    var result = await responc.Content.ReadAsStringAsync().ConfigureAwait(false);
                    timer.Stop();

                    double pingresult = timer.ElapsedMilliseconds / 2;
                    return pingresult;
                }
                else
                {
                    var result = await responc.Content.ReadAsStringAsync().ConfigureAwait(false);
                    using var json2 = JsonDocument.Parse(result);
                    string properties = json2.RootElement.GetProperty("message").ToString() ?? string.Empty;
                    MessageBox.Show($"BadRequest " + responc.StatusCode + properties);
                    return default;
                }
            }
            catch (TaskCanceledException ex)
            {
                await _taskCanccelException.RunDelegate(_taskCanccelException.Delegate, ex);
                return default;
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
