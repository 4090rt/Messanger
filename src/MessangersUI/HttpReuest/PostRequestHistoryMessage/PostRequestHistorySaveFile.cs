using MessangersUI.Delegate;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MessangersUI.HttpReuest.PostRequestHistoryMessage
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

        public async Task<(string message, string state)> ReqoestSAVE(string filename, string user, string username)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("Client1Http2.0");

                using (var fromdata = new MultipartFormDataContent())
                {
                    var filerad = File.ReadAllBytes(filename);
                    var filebytes = new ByteArrayContent(filerad);

                    var extencion = Path.GetExtension(filename).ToLower();

                    var mimeType = extencion switch
                    {
                        ".jpg" or ".jpeg" => "image/jpeg",
                        ".png" => "image/png",
                        ".gif" => "image/gif",
                        ".pdf" => "application/pdf",
                        ".txt" => "text/plain",
                        _ => "application/octet-stream"
                    };

                    filebytes.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(mimeType);

                    fromdata.Add(filebytes, "file", Path.GetFileName(filename));
                    fromdata.Add(new StringContent(user), "user");
                    fromdata.Add(new StringContent(username), "username");

                    HttpResponseMessage responce = await client.PostAsync("", fromdata);
                    if (responce.IsSuccessStatusCode)
                    {
                        var result = await responce.Content.ReadAsStringAsync().ConfigureAwait(false);
                        var jsondoc = JsonDocument.Parse(result);
                        var root = jsondoc.RootElement;

                        var propmessage = root.GetProperty("message").ToString() ?? string.Empty;
                        var propstate = root.GetProperty("state").ToString() ?? string.Empty;

                        if (propstate == "true")
                        {
                            return (propmessage, propstate);
                        }
                        else
                        {
                            return ($"Ошибка + {propmessage}", propstate);
                        }
                    }
                    else
                    {
                        _logger.LogError("Ошибка загрузки: " + responce.StatusCode);
                        return ($"Ошибка: {responce.StatusCode}", "false");
                    }
                }
            }
            catch (TaskCanceledException ex)
            {
                await _taskCanccelException.RunDelegate(_taskCanccelException.Delegate, ex);
                return ($"{ex.Message}", "false");
            }
            catch (JsonException ex)
            {
                await _jsonExceptionDelegate.RunDelegate(_jsonExceptionDelegate.Delegate, ex);
                return ($"{ex.Message}", "false");
            }
            catch (HttpRequestException ex)
            {
                await _httpExceptionDelegate.RunDelegate(_httpExceptionDelegate.Delegate, ex);
                return ($"{ex.Message}", "false");
            }
            catch (Exception ex)
            {
                await _exceptionDelegate.RunDelegate(_exceptionDelegate.DelegateException, ex);
                return ($"{ex.Message}", "false");
            }
        }
    }
}
