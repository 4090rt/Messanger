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
    public class UserModel
    {
        public string username { get; set; }
    }
    public class PostRequestCount
    {
        private readonly ILogger<PostRequestCount> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ExceptionDelegate _exceptionDelegate;
        private readonly HttpExceptionDelegate _httpExceptionDelegate;
        private readonly JsonExceptionDelegate _jsonExceptionDelegate;
        private readonly TaskCanccelException _taskCanccelException;

        public PostRequestCount(ILogger<PostRequestCount> logger, IHttpClientFactory httpClientFactory, ExceptionDelegate exceptionDelegate,
            HttpExceptionDelegate httpExceptionDelegate, JsonExceptionDelegate jsonExceptionDelegate, TaskCanccelException taskCanccelException)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _exceptionDelegate = exceptionDelegate;
            _httpExceptionDelegate = httpExceptionDelegate;
            _jsonExceptionDelegate = jsonExceptionDelegate;
            _taskCanccelException = taskCanccelException;
        }

        public async Task<string> RequestPost(string username)
        {
            try
            {
                var connection = _httpClientFactory.CreateClient("Client1Http2.0");

                var options = new HttpRequestMessage(HttpMethod.Post, "https://localhost:7167/api/ControllerCountUserContacts/countcontactsvidget")
                {
                    Version = HttpVersion.Version20,
                    VersionPolicy = HttpVersionPolicy.RequestVersionOrHigher
                };
                UserModel model = new UserModel()
                { 
                    username = username,
                };

                var json = JsonSerializer.Serialize(model, new JsonSerializerOptions
                { 
                    PropertyNameCaseInsensitive = true
                });
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                options.Content = content;
                HttpResponseMessage response = await connection.SendAsync(options).ConfigureAwait(false);

                if (response.IsSuccessStatusCode)
                {
                    var contentresponce = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var jsonDOC = JsonDocument.Parse(contentresponce);
                    var root = jsonDOC.RootElement;

                    var propertiesState = root.GetProperty("state").GetRawText() ?? string.Empty;
                    var properiesCount = root.GetProperty("count").GetRawText() ?? string.Empty;

                    string trimprop = propertiesState.Trim('"');
                    string trimpropcount = properiesCount.Trim('"');    
                    if (trimprop == "true")
                    {
                        return trimpropcount;
                    }
                    else
                    {
                        return "Увас нет контактов\nИли они не найдены";
                    }
                }
                else
                {
                    System.Windows.MessageBox.Show("2");
                    var contentresponce = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var jsonDOC = JsonDocument.Parse(contentresponce);
                    var root = jsonDOC.RootElement;
                    var properiesmessage = root.GetProperty("message").GetRawText() ?? string.Empty;
                    _logger.LogError($"Запрос Виджета колва контактов завершисля ошибкой {properiesmessage} статус код: {response.StatusCode}");
                    return "Увас нет контактов\nИли они не найдены";
                }
            }
            catch (TaskCanceledException ex)
            {
                await _taskCanccelException.RunDelegate(_taskCanccelException.Delegate, ex);
                return "0";
            }
            catch (JsonException ex)
            {
                await _jsonExceptionDelegate.RunDelegate(_jsonExceptionDelegate.Delegate, ex);
                return "0";
            }
            catch (HttpRequestException ex)
            {
                await _httpExceptionDelegate.RunDelegate(_httpExceptionDelegate.Delegate, ex);
                return "0";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + ex.InnerException);
                await _exceptionDelegate.RunDelegate(_exceptionDelegate.DelegateException, ex);
                return "0";
            }
        }
    }
}
