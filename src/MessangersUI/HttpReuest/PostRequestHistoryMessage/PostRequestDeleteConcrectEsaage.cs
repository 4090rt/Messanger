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

namespace MessangersUI.HttpReuest.PostRequestHistoryMessage
{
    public class ModelConcrectMessage
    {
        public int Id { get; set; }
    }
    public class PostRequestDeleteConcrectEsaage
    {
        private readonly ILogger<PostRequestDeleteConcrectEsaage> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ExceptionDelegate _exceptionDelegate;
        private readonly HttpExceptionDelegate _httpExceptionDelegate;
        private readonly JsonExceptionDelegate _jsonExceptionDelegate;
        private readonly TaskCanccelException _taskCanccelException;

        public PostRequestDeleteConcrectEsaage(ILogger<PostRequestDeleteConcrectEsaage> logger, IHttpClientFactory httpClientFactory, ExceptionDelegate exceptionDelegate,
            HttpExceptionDelegate httpExceptionDelegate, JsonExceptionDelegate jsonExceptionDelegate, TaskCanccelException taskCanccelException)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _exceptionDelegate = exceptionDelegate;
            _httpExceptionDelegate = httpExceptionDelegate;
            _jsonExceptionDelegate = jsonExceptionDelegate;
            _taskCanccelException = taskCanccelException;
        }

        public async Task<bool> RequestDeleteConcret(int id)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("Client1Http2.0");

                var options = new HttpRequestMessage(HttpMethod.Post, "https://localhost:7167/api/ControllerDeleteContcrectMessage/deleteconcrect")
                {
                    Version = HttpVersion.Version20,
                    VersionPolicy = HttpVersionPolicy.RequestVersionOrHigher
                };

                var data = new ModelConcrectMessage
                {
                    Id = id
                };

                var jsonser = JsonSerializer.Serialize(data, new JsonSerializerOptions
                { 
                    PropertyNameCaseInsensitive = true
                });

                var content = new StringContent(jsonser, Encoding.UTF8, "application/json");

                options.Content = content;
                HttpResponseMessage httpResponseMessage = await client.SendAsync(options).ConfigureAwait(false);
                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    var result = await httpResponseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var jsondoc = JsonDocument.Parse(result);
                    var root = jsondoc.RootElement;

                    var propertiesstate = root.GetProperty("state").ToString() ?? string.Empty;
                    var ptoperiesmessage = root.GetProperty("message").ToString() ?? string.Empty;

                    if (propertiesstate == "true")
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    _logger.LogError("Запрос завершился ошибкой" + httpResponseMessage.StatusCode);
                    var result = await httpResponseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var jsondoc = JsonDocument.Parse(result);
                    var root = jsondoc.RootElement;

                    var propertiesstate = root.GetProperty("state").ToString() ?? string.Empty;
                    var ptoperiesmessage = root.GetProperty("message").ToString() ?? string.Empty;

                    if (propertiesstate == "false")
                    {
                        _logger.LogError("Возникла ошибкс" + propertiesstate + ptoperiesmessage);
                        return false;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (TaskCanceledException ex)
            {
                await _taskCanccelException.RunDelegate(_taskCanccelException.Delegate, ex);
                return false;
            }
            catch (JsonException ex)
            {
                await _jsonExceptionDelegate.RunDelegate(_jsonExceptionDelegate.Delegate, ex);
                return false;
            }
            catch (HttpRequestException ex)
            {
                await _httpExceptionDelegate.RunDelegate(_httpExceptionDelegate.Delegate, ex);
                return false;
            }
            catch (Exception ex)
            {
                await _exceptionDelegate.RunDelegate(_exceptionDelegate.DelegateException, ex);
                return false;
            }
        }
    }
}
