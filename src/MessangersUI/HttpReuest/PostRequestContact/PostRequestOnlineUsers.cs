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
    public class PostRequestOnlineUsers
    {
        private readonly ILogger<PostRequestOnlineUsers> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ExceptionDelegate _exceptionDelegate;
        private readonly HttpExceptionDelegate _httpExceptionDelegate;
        private readonly JsonExceptionDelegate _jsonExceptionDelegate;
        private readonly TaskCanccelException _taskCanccelException;

        public PostRequestOnlineUsers(ILogger<PostRequestOnlineUsers> logger, IHttpClientFactory httpClientFactory, ExceptionDelegate exceptionDelegate,
            HttpExceptionDelegate httpExceptionDelegate, JsonExceptionDelegate jsonExceptionDelegate, TaskCanccelException taskCanccelException)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _exceptionDelegate = exceptionDelegate;
            _httpExceptionDelegate = httpExceptionDelegate;
            _jsonExceptionDelegate = jsonExceptionDelegate;
            _taskCanccelException = taskCanccelException;
        }

        public async Task<List<DataUsersList>> RequestPost(List<DataUsersList> list)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("Client1Http2.0");

                var options = new HttpRequestMessage(HttpMethod.Post, "https://localhost:7167/api/ControllerOnlineUser/onlinetusers")
                {
                    Version = HttpVersion.Version20,
                    VersionPolicy = HttpVersionPolicy.RequestVersionOrHigher
                };

                if (list != null)
                {
                    var users = list.FirstOrDefault();

                    var modeltosend = new
                    {
                        User = users?.User
                    };

                    var jsonser = JsonSerializer.Serialize(modeltosend, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    var stringcontent = new StringContent(jsonser, Encoding.UTF8, "application/json");

                    options.Content = stringcontent;

                    HttpResponseMessage response = await client.SendAsync(options).ConfigureAwait(false);
                    if (response.IsSuccessStatusCode)
                    {
                        var resultresponce = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                        var jsondoc = JsonDocument.Parse(resultresponce);
                        var root = jsondoc.RootElement;

                        var listresult = JsonSerializer.Deserialize<List<DataUsersList>>(root.GetProperty("message"));

                        return listresult;
                    }
                    else
                    {
                        _logger.LogError("Запрос неудача" + response.StatusCode);
                        return new List<DataUsersList>();
                    }
                }
                else
                {
                    return new List<DataUsersList>();
                }
            }
            catch (TaskCanceledException ex)
            {
                await _taskCanccelException.RunDelegate(_taskCanccelException.Delegate, ex);
                return new List<DataUsersList>();
            }
            catch (HttpRequestException ex)
            {
                await _httpExceptionDelegate.RunDelegate(_httpExceptionDelegate.Delegate, ex);
                return new List<DataUsersList>();
            }
            catch (JsonException ex)
            {
                await _jsonExceptionDelegate.RunDelegate(_jsonExceptionDelegate.Delegate, ex);
                return new List<DataUsersList>();
            }
            catch (Exception ex)
            {
                await _exceptionDelegate.RunDelegate(_exceptionDelegate.DelegateException, ex);
                return new List<DataUsersList>();
            }
        }
    }
}
