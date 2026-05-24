using Messangers.ModelData;
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

namespace MessangersUI.HttpReuest.PostRequestContact
{
    public class PostRequestOnlineUser
    {
        private readonly ILogger<PostRequestOnlineUser> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ExceptionDelegate _exceptionDelegate;
        private readonly HttpExceptionDelegate _httpExceptionDelegate;
        private readonly TaskCanccelException _taskCanccelExceptionDelegate; 
        private readonly JsonExceptionDelegate _jsonExceptionDelegate;

        public PostRequestOnlineUser(ILogger<PostRequestOnlineUser> logger, IHttpClientFactory httpClientFactory, ExceptionDelegate exceptionDelegate,
            HttpExceptionDelegate httpExceptionDelegate, TaskCanccelException taskCanccelExceptionDelegate, JsonExceptionDelegate jsonExceptionDelegate)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _exceptionDelegate = exceptionDelegate;
            _httpExceptionDelegate = httpExceptionDelegate;
            _taskCanccelExceptionDelegate = taskCanccelExceptionDelegate;
            _jsonExceptionDelegate = jsonExceptionDelegate;
        }

        public async Task<List<DataUsersList>> RequestPost(List<DataUsersList> list)
        {
            try 
            {
                var client = _httpClientFactory.CreateClient("Client1Http2.0");

                var options = new HttpRequestMessage(HttpMethod.Post, "https://localhost:7167/api/ControllerOnlineusers/onlineuser")
                {
                    Version = HttpVersion.Version20,
                    VersionPolicy = HttpVersionPolicy.RequestVersionOrHigher
                };

                var jsonser = JsonSerializer.Serialize(list, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                var stirngcontent = new StringContent(jsonser, Encoding.UTF8,"application/json");

                options.Content = stirngcontent;

                HttpResponseMessage httpResponseMessage = await client.SendAsync(options).ConfigureAwait(false);
                if (httpResponseMessage.IsSuccessStatusCode)
                {

                    var result = await httpResponseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);

                    var listresult = JsonSerializer.Deserialize<List<DataUsersList>>(result);
                        return listresult;
                }
                else
                {
                    _logger.LogError("Возникла ошибка запроса" + httpResponseMessage.StatusCode);
                    return new List<DataUsersList>();
                }
            }
            catch (TaskCanceledException ex)
            {
                await _taskCanccelExceptionDelegate.RunDelegate(_taskCanccelExceptionDelegate.Delegate, ex);
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
