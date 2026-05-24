using MessangersUI.Delegate;
using MessangersUI.HttpReuest.PostRequestHistoryMessage;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MessangersUI.HttpReuest.PostRequestContact
{
    public class modelfirstadd()
    {
        public string Useradding { get; set; }
    }

    public class PostRequestAddFirstUserContact
    {
        private readonly ILogger<PostRequestAddFirstUserContact> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ExceptionDelegate _exceptionDelegate;
        private readonly HttpExceptionDelegate _httpExceptionDelegate;
        private readonly JsonExceptionDelegate _jsonExceptionDelegate;
        private readonly TaskCanccelException _taskCanccelException;

        public PostRequestAddFirstUserContact(ILogger<PostRequestAddFirstUserContact> logger, IHttpClientFactory httpClientFactory, ExceptionDelegate exceptionDelegate,
            HttpExceptionDelegate httpExceptionDelegate, JsonExceptionDelegate jsonExceptionDelegate, TaskCanccelException taskCanccelException)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _exceptionDelegate = exceptionDelegate;
            _httpExceptionDelegate = httpExceptionDelegate;
            _jsonExceptionDelegate = jsonExceptionDelegate;
            _taskCanccelException = taskCanccelException;
        }

        public async Task<bool> PostRequest(string username)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("Client1Http2.0");

                var options = new HttpRequestMessage(HttpMethod.Post, "https://localhost:7167/api/ControllerNewUserOnlineFirstAdd/ControolerOnlineFirstAddUser")
                {
                    Version = HttpVersion.Version20,
                    VersionPolicy = HttpVersionPolicy.RequestVersionOrHigher
                };

                var modeldata = new modelfirstadd
                {
                    Useradding = username
                };

                var jsonserialized = JsonSerializer.Serialize(modeldata, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                var content = new StringContent(jsonserialized, Encoding.UTF8, "application/json");

                options.Content = content;
                HttpResponseMessage responseMessage = await client.SendAsync(options).ConfigureAwait(false);
                if (responseMessage.IsSuccessStatusCode)
                {
                    var result = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var jsondoc = JsonDocument.Parse(result);
                    var root = jsondoc.RootElement;

                    var propertiesState = root.GetProperty("state").ToString() ?? string.Empty;

                    if (propertiesState == "truecolor")
                    {
                        return true;
                    }
                    else if (propertiesState == "falsecolor")
                    {
                        return false;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    _logger.LogError("Запрос первого добавления юзера в контакты завершился ошибкой" + responseMessage.StatusCode);

                    var result = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var jsondoc = JsonDocument.Parse(result);
                    var root = jsondoc.RootElement;

                    var propertiesState = root.GetProperty("state").ToString() ?? string.Empty;
                    var propertiesmessage = root.GetProperty("message").ToString() ?? string.Empty;

                    _logger.LogError(propertiesmessage, propertiesState);
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
