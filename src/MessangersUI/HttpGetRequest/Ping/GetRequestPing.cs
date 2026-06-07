using DirectoryStatistic.Http.ModelData;
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

namespace MessangersUI.HttpGetRequest.Ping
{
    public class GetRequestPing
    {
        private readonly ILogger<GetRequestPing> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ExceptionDelegate _exceptionDelegate;
        private readonly HttpExceptionDelegate _httpExceptionDelegate;
        private readonly JsonExceptionDelegate _jsonExceptionDelegate;
        private readonly TaskCanccelException _taskCanccelException;

        public GetRequestPing(ILogger<GetRequestPing> logger, IHttpClientFactory httpClientFactory, ExceptionDelegate exceptionDelegate,
            HttpExceptionDelegate httpExceptionDelegate, JsonExceptionDelegate jsonExceptionDelegate, TaskCanccelException taskCanccelException)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _exceptionDelegate = exceptionDelegate;
            _httpExceptionDelegate = httpExceptionDelegate;
            _jsonExceptionDelegate = jsonExceptionDelegate;
            _taskCanccelException = taskCanccelException;
        }

        public async Task<List<DataPing>> Request()
        {
            try
            {
                var client = _httpClientFactory.CreateClient("Client1Http2.0");

                var options = new HttpRequestMessage(HttpMethod.Get, "https://localhost:7167/api/ControllerGET/ping")
                {
                    Version = HttpVersion.Version20,
                    VersionPolicy = HttpVersionPolicy.RequestVersionOrHigher
                };
                HttpResponseMessage recponce = await client.SendAsync(options).ConfigureAwait(false);
                if (recponce.IsSuccessStatusCode)
                {
                    var content = await recponce.Content.ReadAsStringAsync().ConfigureAwait(false);
                    using var json = JsonDocument.Parse(content);
                    var result = json.RootElement.GetProperty("message");
                    var deserealize = JsonSerializer.Deserialize<List<DataPing>>(result.GetRawText()) ?? new List<DataPing>();
                    return deserealize;
                }
                else
                {
                    _logger.LogError("BadRequest:", recponce.StatusCode);
                    return new List<DataPing>();
                }
            }
            catch (TaskCanceledException ex)
            {
                await _taskCanccelException.RunDelegate(_taskCanccelException.Delegate, ex);
                return new List<DataPing>();
            }
            catch (HttpRequestException ex)
            {
                await _httpExceptionDelegate.RunDelegate(_httpExceptionDelegate.Delegate, ex);
                return new List<DataPing>();
            }
            catch (Exception ex)
            {
                await _exceptionDelegate.RunDelegate(_exceptionDelegate.DelegateException, ex);
                return new List<DataPing>();
            }
        }
    }
}
