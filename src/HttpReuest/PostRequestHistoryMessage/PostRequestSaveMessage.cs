using MessangersUI.Delegate;
using MessangersUI.HttpReuest.PostRequestEthernetStat;
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
    public class SaveHistoryRequestData
    {
        public string User1 { get; set; }
        public string User2 { get; set; }
        public string Message { get; set; }
        public string Date { get; set; }

        public string State { get; set; }
    }
    public class PostRequestSaveMessage
    {
        private readonly ILogger<PostRequestSaveMessage> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ExceptionDelegate _exceptionDelegate;
        private readonly HttpExceptionDelegate _httpExceptionDelegate;
        private readonly JsonExceptionDelegate _jsonExceptionDelegate;
        private readonly TaskCanccelException _taskCanccelException;

        public PostRequestSaveMessage(ILogger<PostRequestSaveMessage> logger, IHttpClientFactory httpClientFactory, ExceptionDelegate exceptionDelegate,
            HttpExceptionDelegate httpExceptionDelegate, JsonExceptionDelegate jsonExceptionDelegate, TaskCanccelException taskCanccelException)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _exceptionDelegate = exceptionDelegate;
            _httpExceptionDelegate = httpExceptionDelegate;
            _jsonExceptionDelegate = jsonExceptionDelegate;
            _taskCanccelException = taskCanccelException;
        }

        public async Task<(string message, string state, int id)> PostRequest(string user1, string user2, string message, string date, string state)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("Client1Http2.0");

                var options = new HttpRequestMessage(HttpMethod.Post, "https://localhost:7167/api/ControllerHistorySave/savehistory")
                {
                    Version = HttpVersion.Version20,
                    VersionPolicy = HttpVersionPolicy.RequestVersionOrHigher
                };

                var data = new SaveHistoryRequestData
                {
                    User1 = user1,
                    User2 = user2,
                    Message = message,
                    Date = date,
                    State = state
                };

                var serializee = JsonSerializer.Serialize(data, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                var content = new StringContent(serializee, Encoding.UTF8, "application/json");

                options.Content = content;

                HttpResponseMessage responcemessage = await client.SendAsync(options).ConfigureAwait(false);
                if (responcemessage.IsSuccessStatusCode)
                {
                    var result = await responcemessage.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var doc = JsonDocument.Parse(result);
                    var root = doc.RootElement;

                    var propertymessage = root.GetProperty("message").ToString() ?? string.Empty;
                    var propertystate = root.GetProperty("state").ToString() ?? string.Empty;
                    int  propertyid = root.GetProperty("id").GetInt32();
                    return (propertymessage, propertystate, propertyid);
                }
                else
                {
                    var result = await responcemessage.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var doc = JsonDocument.Parse(result);
                    var root = doc.RootElement;

                    var propertymessage = root.GetProperty("message").ToString() ?? string.Empty;
                    var propertystate = root.GetProperty("state").ToString() ?? string.Empty;
                    _logger.LogError($"При запрое сохранения истории сообщений возникла ошибка: посткод: {responcemessage.StatusCode} " + propertymessage);
                    System.Windows.MessageBox.Show($"При запрое сохранения истории сообщений возникла ошибка: посткод: {responcemessage.StatusCode}");
                    return ($"{responcemessage.StatusCode}", "false", 0);
                }
            }
            catch (TaskCanceledException ex)
            {
                await _taskCanccelException.RunDelegate(_taskCanccelException.Delegate, ex);
                return ($"{ex.Message}", "false", 0);
            }
            catch (JsonException ex)
            {
                await _jsonExceptionDelegate.RunDelegate(_jsonExceptionDelegate.Delegate, ex);
                return ($"{ex.Message}", "false", 0);
            }
            catch (HttpRequestException ex)
            {
                await _httpExceptionDelegate.RunDelegate (_httpExceptionDelegate.Delegate, ex);
                return ($"{ex.Message}", "false", 0);
            }
            catch (Exception ex)
            {
                await _exceptionDelegate.RunDelegate(_exceptionDelegate.DelegateException, ex);
                return ($"{ex.Message}", "false", 0);
            }
        }
    }
}
