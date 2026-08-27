using Messangers.ModelData;
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

namespace MessangersUI.HttpReuest.PostRequestContact
{
    public class PostRequestOnlineUsersValidate
    {
        private readonly ILogger<PostRequestOnlineUsersValidate> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ExceptionDelegate _exceptionDelegate;
        private readonly HttpExceptionDelegate _httpExceptionDelegate;
        private readonly JsonExceptionDelegate _jsonExceptionDelegate;
        private readonly TaskCanccelException _taskCanccelException;

        public PostRequestOnlineUsersValidate(ILogger<PostRequestOnlineUsersValidate> logger, IHttpClientFactory httpClientFactory, ExceptionDelegate exceptionDelegate,
            HttpExceptionDelegate httpExceptionDelegate, JsonExceptionDelegate jsonExceptionDelegate, TaskCanccelException taskCanccelException)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _exceptionDelegate = exceptionDelegate;
            _httpExceptionDelegate = httpExceptionDelegate;
            _jsonExceptionDelegate = jsonExceptionDelegate;
            _taskCanccelException = taskCanccelException;
        }

        public async Task<bool> RequestPost(string user)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("Client1Http2.0");

                var options = new HttpRequestMessage(HttpMethod.Post, "https://localhost:7167/api/ControllerOnlineUserValidate/OnlineUsersValidate")
                {
                    Version = HttpVersion.Version20,
                    VersionPolicy = HttpVersionPolicy.RequestVersionOrHigher
                };

                var jsonser = JsonSerializer.Serialize(user, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                var contnetstring = new StringContent(jsonser, Encoding.UTF8, "application/json");

                options.Content = contnetstring;
                HttpResponseMessage responseMessage = await client.SendAsync(options).ConfigureAwait(false);
                if (responseMessage.IsSuccessStatusCode)
                {
                    var result = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
                    using var json = JsonDocument.Parse(result);
                    var root = json.RootElement;

                    var propState = root.GetProperty("state").ToString() ?? string.Empty;

                    if (propState == "true")
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
                    _logger.LogError("Возникла ошибка при запросе" + responseMessage.StatusCode);
                    return false;
                }
            }
            catch (TaskCanceledException ex)
            {
                await _taskCanccelException.RunDelegate(_taskCanccelException.Delegate, ex);
                return false;
            }
            catch (HttpRequestException ex)
            {
                await _httpExceptionDelegate.RunDelegate(_httpExceptionDelegate.Delegate, ex);
                return false;
            }
            catch (JsonException ex)
            {
                await _jsonExceptionDelegate.RunDelegate(_jsonExceptionDelegate.Delegate, ex);
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
