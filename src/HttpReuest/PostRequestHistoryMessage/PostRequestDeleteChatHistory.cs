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
    public class ModelUserChathisotyDelete()
    {
        public string User { get; set; }
        public string UserName { get; set; }
    }
    public class PostRequestDeleteChatHistory
    {
        private readonly ILogger<PostRequestDeleteChatHistory> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ExceptionDelegate _exceptionDelegate;
        private readonly HttpExceptionDelegate _httpExceptionDelegate;
        private readonly JsonExceptionDelegate _jsonExceptionDelegate;
        private readonly TaskCanccelException _taskCanccelException;

        public PostRequestDeleteChatHistory(ILogger<PostRequestDeleteChatHistory> logger, IHttpClientFactory httpClientFactory, ExceptionDelegate exceptionDelegate,
            HttpExceptionDelegate httpExceptionDelegate, JsonExceptionDelegate jsonExceptionDelegate, TaskCanccelException taskCanccelException)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _exceptionDelegate = exceptionDelegate;
            _httpExceptionDelegate = httpExceptionDelegate;
            _jsonExceptionDelegate = jsonExceptionDelegate;
            _taskCanccelException = taskCanccelException;
        }

        public async Task<bool> PostDeleteHistory(string user, string username)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("Client1Http2.0");

                var options = new HttpRequestMessage(HttpMethod.Post, "https://localhost:7167/api/ControllerDeleteHistory/deletehistory")
                {
                    Version = HttpVersion.Version20,
                    VersionPolicy = HttpVersionPolicy.RequestVersionOrHigher
                };

                var data = new ModelUserChathisotyDelete
                {
                    User = user,
                    UserName = username
                };

                var jsonser = JsonSerializer.Serialize(data, new JsonSerializerOptions
                { 
                    PropertyNameCaseInsensitive = true
                });

                var content = new StringContent(jsonser, Encoding.UTF8,"application/json");

                options.Content = content;

                HttpResponseMessage responce = await client.SendAsync(options).ConfigureAwait(false);
                if (responce.IsSuccessStatusCode)
                {
                    var contentresult = await responce.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var jsondoc = JsonDocument.Parse(contentresult);
                    var root = jsondoc.RootElement;

                    var propmessage = root.GetProperty("message").ToString() ?? string.Empty;
                    var propstate = root.GetProperty("state").ToString() ?? string.Empty;

                    if (propstate == "true")
                    {
                        return true;
                    }
                    else
                    {
                        _logger.LogError($"{propmessage}, {propstate}");
                        MessageBox.Show($"{propmessage}, {propstate}");
                        return false;
                    }
                }
                else
                {
                    _logger.LogError("Запрос закончился ошибкой. посткод:" + responce.StatusCode);
                    MessageBox.Show("Запрос закончился ошибкой. посткод:" + responce.StatusCode);
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
