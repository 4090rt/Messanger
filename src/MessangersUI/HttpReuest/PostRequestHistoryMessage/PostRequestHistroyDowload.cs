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

namespace MessangersUI.HttpReuest.PostRequestHistoryMessage
{
    public class UsersListHistory
    {
        public string LoginUser1 { get; set; }
        public string LoginUser2 { get; set; }
    }

    public class PostRequestHistroyDowload
    {
        private readonly ILogger<PostRequestHistroyDowload> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ExceptionDelegate _exceptionDelegate;
        private readonly HttpExceptionDelegate _httpExceptionDelegate;
        private readonly JsonExceptionDelegate _jsonExceptionDelegate;
        private readonly TaskCanccelException _taskCanccelException;

        public PostRequestHistroyDowload(ILogger<PostRequestHistroyDowload> logger, IHttpClientFactory httpClientFactory, ExceptionDelegate exceptionDelegate,
            HttpExceptionDelegate httpExceptionDelegate, JsonExceptionDelegate jsonExceptionDelegate, TaskCanccelException taskCanccelException)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _exceptionDelegate = exceptionDelegate;
            _httpExceptionDelegate = httpExceptionDelegate;
            _jsonExceptionDelegate = jsonExceptionDelegate;
            _taskCanccelException = taskCanccelException;
        }

        public async Task<List<MessageData>> PostRequest(string userLogin1, string userLogin2)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("Client1Http2.0");

                var options = new HttpRequestMessage(HttpMethod.Post, "https://localhost:7167/api/ControllerDowloadHistroyMessage/dowloadhistory")
                {
                    Version = HttpVersion.Version20,
                    VersionPolicy = HttpVersionPolicy.RequestVersionOrHigher
                };

                var data = new UsersListHistory
                {
                    LoginUser1 = userLogin1,
                    LoginUser2 = userLogin2
                };

                var jsonsser = JsonSerializer.Serialize(data,new JsonSerializerOptions
                { 
                    PropertyNameCaseInsensitive = true
                });

                var content = new StringContent(jsonsser, Encoding.UTF8, "application/json");

                options.Content = content;

                HttpResponseMessage httpResponseMessage = await client.SendAsync(options).ConfigureAwait(false);
                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    var result = await httpResponseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var listresult = JsonSerializer.Deserialize<List<MessageData>>(result);
                    return listresult;
                }
                else
                {
                    _logger.LogError("Возникла ошибка запроса" + httpResponseMessage.StatusCode);
                    return new List<MessageData>();
                }
            }
            catch (TaskCanceledException ex)
            {
                await _taskCanccelException.RunDelegate(_taskCanccelException.Delegate, ex);
                return new List<MessageData>();
            }
            catch (JsonException ex)
            {
                await _jsonExceptionDelegate.RunDelegate(_jsonExceptionDelegate.Delegate, ex);
                return new List<MessageData>();
            }
            catch (HttpRequestException ex)
            {
                await _httpExceptionDelegate.RunDelegate(_httpExceptionDelegate.Delegate, ex);
                return new List<MessageData>();
            }
            catch (Exception ex)
            {
                await _exceptionDelegate.RunDelegate(_exceptionDelegate.DelegateException, ex);
                return new List<MessageData>(); ;
            }
        }
    }
}
