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

namespace MessangersUI.HttpReuest.PostRequestHistoryMessage.PostFiles
{
    public class UpdateId
    {
        public int Id { get; set; }
        public Int64 attaid { get; set; }
    }
    public class PostRequestUpdateID
    {
        private readonly ILogger<PostRequestUpdateID> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ExceptionDelegate _exceptionDelegate;
        private readonly HttpExceptionDelegate _httpExceptionDelegate;
        private readonly JsonExceptionDelegate _jsonExceptionDelegate;
        private readonly TaskCanccelException _taskCanccelException;

        public PostRequestUpdateID(ILogger<PostRequestUpdateID> logger, IHttpClientFactory httpClientFactory, ExceptionDelegate exceptionDelegate,
            HttpExceptionDelegate httpExceptionDelegate, JsonExceptionDelegate jsonExceptionDelegate, TaskCanccelException taskCanccelException)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _exceptionDelegate = exceptionDelegate;
            _httpExceptionDelegate = httpExceptionDelegate;
            _jsonExceptionDelegate = jsonExceptionDelegate;
            _taskCanccelException = taskCanccelException;
        }

        public async Task<bool> RequestUpdate(int id, Int64 attachid)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("Client1Http2.0");

                var options = new HttpRequestMessage(HttpMethod.Post, "https://localhost:7167/api/ControllerUpdateAttachmentId/controllerupdateId")
                {
                    Version = HttpVersion.Version20,
                    VersionPolicy = HttpVersionPolicy.RequestVersionOrHigher
                };
                var data = new UpdateId
                {
                    Id = id,
                    attaid = attachid
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
                    var result = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var jsondoc = JsonDocument.Parse(result);
                    var root = jsondoc.RootElement;
                    var propertystate = root.GetProperty("state").GetRawText() ?? string.Empty;

                    if (propertystate == "true")
                    {
                        _logger.LogInformation("Успешно обновлено");
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    var result = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var jsondoc = JsonDocument.Parse(result);
                    var root = jsondoc.RootElement;
                    var propertystate = root.GetProperty("message").GetRawText() ?? string.Empty;
                    MessageBox.Show(propertystate);
                    _logger.LogError("Возникла ошибка запроса. статус код:" + responseMessage.StatusCode);
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
