using MessangersUI.DataModel;
using MessangersUI.Delegate;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MessangersUI.HttpReuest.PostRequestAvatar
{
    public class PostMethodAvatar
    {
        private readonly ILogger<PostMethodAvatar> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        private readonly ExceptionDelegate _exceptionDelegate;
        private readonly HttpExceptionDelegate _httpExceptionDelegate;
        private readonly JsonExceptionDelegate _jsonExceptionDelegate;
        private readonly TaskCanccelException _taskCanccelException;

        private readonly string Url = "https://localhost:7167/api/ControllerAvatarAdd/controlleravatar";

        public PostMethodAvatar(ILogger<PostMethodAvatar> logger, IHttpClientFactory httpClientFactory, 
            ExceptionDelegate exceptionDelegate, HttpExceptionDelegate httpExceptionDelegate, 
            JsonExceptionDelegate jsonExceptionDelegate, TaskCanccelException taskCanccelException)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _exceptionDelegate = exceptionDelegate;
            _httpExceptionDelegate = httpExceptionDelegate;
            _jsonExceptionDelegate = jsonExceptionDelegate;
            _taskCanccelException = taskCanccelException;
        }

        public async Task<(string, string)> RequestMethod(AvatarMetaData avatarMetaData)
        {
            try
            {
                if (string.IsNullOrEmpty(avatarMetaData.expansion))
                {
                    return (string.Empty, string.Empty);
                }
                if (string.IsNullOrEmpty(avatarMetaData.Filepath))
                {
                    return (string.Empty, string.Empty);
                }

                byte[] bytes = await System.IO.File.ReadAllBytesAsync(avatarMetaData.Filepath)
                    .ConfigureAwait(false);

                ReadOnlyMemory<byte> readOnlyMemorybytes = bytes.AsMemory();

                if (readOnlyMemorybytes.Length == 0)
                {
                    return (string.Empty, string.Empty);
                }

                using HttpClient client = _httpClientFactory.CreateClient("Client1Http2.0");
                {
                    using (var fordata = new MultipartFormDataContent())
                    {
                        fordata.Add(new StringContent(avatarMetaData.UserName), "UserName");
                        fordata.Add(new StringContent(avatarMetaData.expansion), "expansion");
                        var bytecontnt = new ByteArrayContent(bytes);
                        fordata.Add(bytecontnt, "file", "Avatar");

                        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

                        HttpResponseMessage httpResponseMessage = await client.PostAsync(Url, fordata, cts.Token).ConfigureAwait(false);
                        if (httpResponseMessage.IsSuccessStatusCode)
                        {
                            var readbytes = await httpResponseMessage.Content.ReadAsByteArrayAsync().ConfigureAwait(false);

                            ReadOnlyMemory<byte> readOnlyMemoryreadbytesO = readbytes.AsMemory();

                            if (readOnlyMemoryreadbytesO.Length == 0)
                                return (string.Empty, string.Empty);

                            var jsondoc = JsonDocument.Parse(readOnlyMemoryreadbytesO);
                            var root = jsondoc.RootElement;

                            if (root.TryGetProperty("resultLog", out var resultLogElement) &&
                                root.TryGetProperty("bool", out var boolElement))
                            {
                                var property1 = resultLogElement.GetString() ?? string.Empty;
                                var property2 = boolElement.GetString() ?? string.Empty;

                                MessageBox.Show($"✅ Успешно: {property1}");
                                return (property1, property2);
                            }
                            else
                            {
                                var responseString = System.Text.Encoding.UTF8.GetString(readbytes);
                                MessageBox.Show($"❌ Неожиданный ответ сервера:\n{responseString}");
                                return ("Ошибка формата ответа", "false");
                            }
                        }
                        else
                        {
                            var readbytes = await httpResponseMessage.Content.ReadAsByteArrayAsync().ConfigureAwait(false);

                            ReadOnlyMemory<byte> readOnlyMemoryreadbytesO = readbytes.AsMemory();

                            if (readOnlyMemoryreadbytesO.Length == 0)
                                return (string.Empty, string.Empty);

                            var jsondoc = JsonDocument.Parse(readOnlyMemoryreadbytesO);
                            var root = jsondoc.RootElement;

                            var property1 = root.GetProperty("ResultLog").GetString() ?? string.Empty;
                            var property2 = root.GetProperty("Bool").GetString() ?? string.Empty;

                            var result = ("запрос завершился посткодом", httpResponseMessage.StatusCode);
                            return (result.ToString(), "false");
                        }
                    }
                }
            }
            catch (TaskCanceledException ex)
            {
                await _taskCanccelException.RunDelegate(_taskCanccelException.Delegate, ex);
                return (string.Empty, string.Empty);
            }
            catch (HttpRequestException ex)
            {
                await _httpExceptionDelegate.RunDelegate(_httpExceptionDelegate.Delegate, ex);
                return (string.Empty, string.Empty);
            }
            catch (JsonException ex)
            {
                await _jsonExceptionDelegate.RunDelegate(_jsonExceptionDelegate.Delegate, ex);
                return (string.Empty, string.Empty);
            }
            catch (Exception ex)
            {
                await _exceptionDelegate.RunDelegate(_exceptionDelegate.DelegateException, ex);
                return (string.Empty, string.Empty);
            }
        }
    }
}
