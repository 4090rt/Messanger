using Messangers.ModelData;
using MessangersUI.DataModel;
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
    public class HttpGetRequestProvider
    {
        private readonly ILogger<HttpGetRequestProvider> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ExceptionDelegate _exceptionDelegate;
        private readonly HttpExceptionDelegate _httpExceptionDelegate;
        private readonly JsonExceptionDelegate _jsonExceptionDelegate;
        private readonly TaskCanccelException _taskCanccelException;

        public HttpGetRequestProvider(ILogger<HttpGetRequestProvider> logger, IHttpClientFactory httpClientFactory, ExceptionDelegate exceptionDelegate,
            HttpExceptionDelegate httpExceptionDelegate, JsonExceptionDelegate jsonExceptionDelegate, TaskCanccelException taskCanccelException)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _exceptionDelegate = exceptionDelegate;
            _httpExceptionDelegate = httpExceptionDelegate;
            _jsonExceptionDelegate = jsonExceptionDelegate;
            _taskCanccelException = taskCanccelException;
        }

        public async Task<List<IpIfo>> Request()
        {
            try
            {
                var client = _httpClientFactory.CreateClient("Client1Http2.0");

                var options = new HttpRequestMessage(HttpMethod.Get, "https://localhost:7167/api/ControllerGetRequestProvider/provider")
                {
                    Version = HttpVersion.Version20,
                    VersionPolicy = HttpVersionPolicy.RequestVersionOrHigher
                };
                HttpResponseMessage recpon = await client.SendAsync(options).ConfigureAwait(false);
                if (recpon.IsSuccessStatusCode)
                {
                    var result = await recpon.Content.ReadAsStringAsync().ConfigureAwait(false);
                    if (result != null)
                    {
                        using var json = JsonDocument.Parse(result);
                        var properties = json.RootElement.GetProperty("message");
                        var deserialize = properties.Deserialize<List<IpIfo>>() ?? new List<IpIfo>();
                        return deserialize;
                    }
                    else
                    {
                        MessageBox.Show("пустой ответ о провайдере от сервера");
                        return new List<IpIfo>();
                    }
                }
                else
                {
                    var result = await recpon.Content.ReadAsStringAsync().ConfigureAwait(false);

                    if (result != null)
                    {
                        using var doc = JsonDocument.Parse(result);

                        var propertys1 = doc.RootElement.GetProperty("state");
                        var propertys2 = doc.RootElement.GetProperty("error");
                        var deserializeline1 = propertys1.Deserialize<ErrorBodyData>()
                            ?? new ErrorBodyData();
                        var deserealizeline2 = propertys2.Deserialize<ErrorBodyData>()
                            ?? new ErrorBodyData();

                        ErrorResponse errorResponse = new ErrorResponse();

                        var state = errorResponse.State = deserealizeline2;
                        var error = errorResponse.Error = deserializeline1;

                        return new List<IpIfo>();
                    }
                    else
                    {
                        MessageBox.Show("пустой ответ о ошибке провайдере от сервера");
                        return new List<IpIfo>();
                    }
                }
            }
            catch (TaskCanceledException ex)
            {
                await _taskCanccelException.RunDelegate(_taskCanccelException.Delegate, ex);
                return new List<IpIfo>();
            }
            catch (HttpRequestException ex)
            {
                await _httpExceptionDelegate.RunDelegate(_httpExceptionDelegate.Delegate, ex);
                return new List<IpIfo>();
            }
            catch (Exception ex)
            {
                await _exceptionDelegate.RunDelegate(_exceptionDelegate.DelegateException, ex);
                return new List<IpIfo>();
            }
        }
    }
}
