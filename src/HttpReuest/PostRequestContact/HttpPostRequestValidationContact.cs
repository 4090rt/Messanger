using MessangersUI.Delegate;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MessangersUI.HttpReuest.PostRequestContact
{
    public class RequesValidateContact
    {
        public string user { get; set; }
        public string login { get; set; }
    }

    public class HttpPostRequestValidationContact
    {
        private readonly ILogger<HttpPostRequestValidationContact> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ExceptionDelegate _exceptionDelegate;
        private readonly HttpExceptionDelegate _httpExceptionDelegate;
        private readonly JsonExceptionDelegate _jsonExceptionDelegate;
        private readonly TaskCanccelException _taskCanccelException;

        public HttpPostRequestValidationContact(ILogger<HttpPostRequestValidationContact> logger, IHttpClientFactory httpClientFactory, ExceptionDelegate exceptionDelegate,
            HttpExceptionDelegate httpExceptionDelegate, JsonExceptionDelegate jsonExceptionDelegate, TaskCanccelException taskCanccelException)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _exceptionDelegate = exceptionDelegate;
            _httpExceptionDelegate = httpExceptionDelegate;
            _jsonExceptionDelegate = jsonExceptionDelegate;
            _taskCanccelException = taskCanccelException;
        }

        public async Task<bool> RequestMethod(string user, string logincontact)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("Client1Http2.0");

                var options = new HttpRequestMessage(HttpMethod.Post, "https://localhost:7167/api/ControllerValidateContact/validatecontact")
                {
                    Version = HttpVersion.Version20,
                    VersionPolicy = HttpVersionPolicy.RequestVersionOrHigher
                };

                var data = new RequesValidateContact
                {
                    user = user,
                    login = logincontact
                };

                var jsonser = JsonSerializer.Serialize(data, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                var content = new StringContent(jsonser, Encoding.UTF8, "application/json");

                options.Content = content;

                HttpResponseMessage responseMessage = await client.SendAsync(options).ConfigureAwait(false);

                if (responseMessage.IsSuccessStatusCode)
                {
                    var resultcontent = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var json = JsonDocument.Parse(resultcontent);
                    var root = json.RootElement;

                    var propertiesstate = root.GetProperty("state").GetRawText() ?? string.Empty;

                    if (propertiesstate == "true")
                    {
                        return true;
                    }
                    return true;
                }
                else
                {
                    var resultcontent = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var json = JsonDocument.Parse(resultcontent);
                    var root = json.RootElement;

                    var ptopertiesmessage = root.GetProperty("message").GetRawText() ?? string.Empty;
                    var propertiesstate = root.GetProperty("state").GetRawText() ?? string.Empty;

                    _logger.LogWarning("Ошибка запроса");
                    return false;
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
