using Messangers.ModelData;
using MessangersUI.Delegate;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MessangersUI.HttpReuest.PostRequestHistoryMessage.PostFiles
{
    public class PostRequestHistorySaveFile
    {
        private readonly ILogger<PostRequestHistorySaveFile> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ExceptionDelegate _exceptionDelegate;
        private readonly HttpExceptionDelegate _httpExceptionDelegate;
        private readonly JsonExceptionDelegate _jsonExceptionDelegate;
        private readonly TaskCanccelException _taskCanccelException;

        public PostRequestHistorySaveFile(ILogger<PostRequestHistorySaveFile> logger, IHttpClientFactory httpClientFactory, ExceptionDelegate exceptionDelegate,
            HttpExceptionDelegate httpExceptionDelegate, JsonExceptionDelegate jsonExceptionDelegate, TaskCanccelException taskCanccelException)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _exceptionDelegate = exceptionDelegate;
            _httpExceptionDelegate = httpExceptionDelegate;
            _jsonExceptionDelegate = jsonExceptionDelegate;
            _taskCanccelException = taskCanccelException;
        }

        public async Task<(AttachmentMetadata metadata, string message, string state)> ReqoestSAVE(string filepath, string user, string username)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("Client1Http2.0");

                var options = new HttpRequestMessage(HttpMethod.Post, "https://localhost:7167/api/ControllerFIleSave/updloadfiles")
                {
                    Version = HttpVersion.Version20,
                    VersionPolicy = HttpVersionPolicy.RequestVersionOrHigher
                };

                var formdata = new MultipartFormDataContent();

                var file = File.ReadAllBytes(filepath);
                var filebytescontnt = new ByteArrayContent(file);
                filebytescontnt.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");

                formdata.Add(filebytescontnt, "file", Path.GetFileName(filepath));
                formdata.Add(new StringContent(user), "user");
                formdata.Add(new StringContent(username), "username");

                options.Content = formdata;

                HttpResponseMessage response = await client.SendAsync(options).ConfigureAwait(false);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var JsonDoc = JsonDocument.Parse(result);
                    var root = JsonDoc.RootElement;
                        
                    var propertiesAttach = root.GetProperty("attachment").GetRawText() ?? string.Empty;
                    var ser = JsonSerializer.Deserialize<AttachmentMetadata>(propertiesAttach, new JsonSerializerOptions
                    {   PropertyNameCaseInsensitive = true,
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    });

                    if (ser != null)
                    {
                        return (ser, "Успешно возвращены метаданные после сохранения на сервере", "true");
                    }
                    else
                    {
                        return (new AttachmentMetadata(), "метаданные пусты", "false");
                    }
                }
                else
                {
                    var result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var JsonDoc = JsonDocument.Parse(result);
                    var root = JsonDoc.RootElement;

                    var propertiemessage = root.GetProperty("message").GetRawText() ?? string.Empty;
                    MessageBox.Show($"{propertiemessage}");
                    return (new AttachmentMetadata(), $"Ошибка зпроса {response.StatusCode}", "true");
                }
            }
            catch (TaskCanceledException ex)
            {
                await _taskCanccelException.RunDelegate(_taskCanccelException.Delegate, ex);
                return (new AttachmentMetadata(), $"{ex.Message}", "false");
            }
            catch (JsonException ex)
            {
                await _jsonExceptionDelegate.RunDelegate(_jsonExceptionDelegate.Delegate, ex);
                return (new AttachmentMetadata(), $"{ex.Message}", "false");
            }
            catch (HttpRequestException ex)
            {
                await _httpExceptionDelegate.RunDelegate(_httpExceptionDelegate.Delegate, ex);
                return (new AttachmentMetadata(), $"{ex.Message}", "false");
            }
            catch (Exception ex)
            {
                await _exceptionDelegate.RunDelegate(_exceptionDelegate.DelegateException, ex);
                return (new AttachmentMetadata(), $"{ex.Message}", "false");
            }
        }
    }
}