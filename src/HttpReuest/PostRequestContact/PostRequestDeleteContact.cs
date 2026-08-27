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
    public class RequestDelete
    {
        public string user { get; set; }
        public string login { get; set; }
    }
    public class PostRequestDeleteContact
    {
        private readonly ILogger<PostRequestDeleteContact> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ExceptionDelegate _exceptionDelegate;
        private readonly HttpExceptionDelegate _httpExceptionDelegate;
        private readonly JsonExceptionDelegate _jsonExceptionDelegate;
        private readonly TaskCanccelException _taskCanccelException;

        public PostRequestDeleteContact(ILogger<PostRequestDeleteContact> logger, IHttpClientFactory httpClientFactory, ExceptionDelegate exceptionDelegate,
            HttpExceptionDelegate httpExceptionDelegate, JsonExceptionDelegate jsonExceptionDelegate, TaskCanccelException taskCanccelException)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _exceptionDelegate = exceptionDelegate;
            _httpExceptionDelegate = httpExceptionDelegate;
            _jsonExceptionDelegate = jsonExceptionDelegate;
            _taskCanccelException = taskCanccelException;
        }

        public async Task<bool> Request(string username, string loginvontact)
        {
            try
            {
                var connection = _httpClientFactory.CreateClient("Client1Http2.0");

                var options = new HttpRequestMessage(HttpMethod.Post, "https://localhost:7167/api/ControllerPostDeleteContact/contactDelete")
                {
                    Version = HttpVersion.Version20,
                    VersionPolicy = HttpVersionPolicy.RequestVersionOrHigher
                };

                var data = new RequestDelete
                {
                    user = username,
                    login = loginvontact
                };

                var json = JsonSerializer.Serialize(data,new JsonSerializerOptions
                { 
                    PropertyNameCaseInsensitive = true
                });

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                options.Content = content;

                HttpResponseMessage message = await connection.SendAsync(options).ConfigureAwait(false);
                if (message.IsSuccessStatusCode)
                {
                    var contetnrescpon = await message.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var jsondoc = JsonDocument.Parse(contetnrescpon);
                    var root = jsondoc.RootElement;
                    var propertymessage = root.GetProperty("message").ToString();
                    var propertystate = root.GetProperty("state").ToString();

                    if (propertystate == "true")
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
            catch (Exception ex)
            {
                await _exceptionDelegate.RunDelegate(_exceptionDelegate.DelegateException, ex);
                return false;
            }
        }
    }
}
