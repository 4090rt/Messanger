using Messangers.ModelData;
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

namespace MessangersUI.HttpReuest.PostRequestHistoryMessage.PostFiles
{
    public class AttachmentsResponse
    {
        public List<AttachmentMetadata> Attachments { get; set; }
        public string State { get; set; }
    }
    public class PostHistoryFiles
    {
        private readonly ILogger<PostHistoryFiles> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ExceptionDelegate _exceptionDelegate;
        private readonly HttpExceptionDelegate _httpExceptionDelegate;
        private readonly JsonExceptionDelegate _jsonExceptionDelegate;
        private readonly TaskCanccelException _taskCanccelException;

        public PostHistoryFiles(ILogger<PostHistoryFiles> logger, IHttpClientFactory httpClientFactory, ExceptionDelegate exceptionDelegate,
            HttpExceptionDelegate httpExceptionDelegate, JsonExceptionDelegate jsonExceptionDelegate, TaskCanccelException taskCanccelException)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _exceptionDelegate = exceptionDelegate;
            _httpExceptionDelegate = httpExceptionDelegate;
            _jsonExceptionDelegate = jsonExceptionDelegate;
            _taskCanccelException = taskCanccelException;
        }

        public async Task<List<AttachmentMetadata>> Request(string User1, string User2)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("ClientHttp2.0");

                var options = new HttpRequestMessage(HttpMethod.Post, "https://localhost:7167/api/ControllerFilesHistory/ControllerHistoryFiles")
                {
                    Version = HttpVersion.Version20,
                    VersionPolicy = HttpVersionPolicy.RequestVersionOrHigher
                };

                var data = new UsersListHistory
                {
                    LoginUser1 = User1,
                    LoginUser2 = User2,
                };

                var jsonSer = JsonSerializer.Serialize(data, new JsonSerializerOptions
                { 
                    PropertyNameCaseInsensitive = true
                });

                var stringcontent = new StringContent(jsonSer, Encoding.UTF8, "application/json");

                options.Content = stringcontent; 

                HttpResponseMessage responseMessage = await client.SendAsync(options).ConfigureAwait(false);
                if (responseMessage.IsSuccessStatusCode)
                {
                    var result = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var jsondoc = JsonDocument.Parse(result);
                    var root = jsondoc.RootElement;

                    var deserialize = JsonSerializer.Deserialize<AttachmentsResponse>(result);

                    return deserialize.Attachments;
                }
                else
                {
                    var result = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var jsondoc = JsonDocument.Parse(result);
                    var root = jsondoc.RootElement;

                    var propmessage = root.GetProperty("message").ToString() ?? string.Empty;

                    _logger.LogError("Возникла ошибка запрос" + responseMessage.StatusCode + propmessage);
                    MessageBox.Show("Возникла ошибка запрос" + responseMessage.StatusCode + propmessage);
                    return new List<AttachmentMetadata>();
                }
            }
            catch (TaskCanceledException ex)
            {
                await _taskCanccelException.RunDelegate(_taskCanccelException.Delegate, ex);
                return new List<AttachmentMetadata>();
            }
            catch (JsonException ex)
            {
                await _jsonExceptionDelegate.RunDelegate(_jsonExceptionDelegate.Delegate, ex);
                return new List<AttachmentMetadata>();
            }
            catch (HttpRequestException ex)
            {
                await _httpExceptionDelegate.RunDelegate(_httpExceptionDelegate.Delegate,ex);
                return new List<AttachmentMetadata>();
            }
            catch (Exception ex)
            {
                await _exceptionDelegate.RunDelegate(_exceptionDelegate.DelegateException, ex);
                return new List<AttachmentMetadata>();
            }
        }
    }
}
