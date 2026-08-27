using MessangersUI.DataModel;
using MessangersUI.Delegate;
using MessangersUI.HttpReuest.PostRequestContact;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MessangersUI.HttpPutR.UpdatePassword
{
    public class UPDATEPassword
    {
        private readonly ILogger<UPDATEPassword> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ExceptionDelegate _exceptionDelegate;
        private readonly HttpExceptionDelegate _httpExceptionDelegate;
        private readonly JsonExceptionDelegate _jsonExceptionDelegate;
        private readonly TaskCanccelException _taskCanccelException;

        private string _url = "https://localhost:7167/api/ControllerUpdatePass/ControllerPassUpdate";

        public UPDATEPassword(ILogger<UPDATEPassword> logger, IHttpClientFactory httpClientFactory,
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

        public async Task<bool> UpdatePasswrodMethod(string passwordhash, string username)
        {
            try
            {
                if (string.IsNullOrEmpty(passwordhash) && string.IsNullOrEmpty(username))
                    return false;

                var client = _httpClientFactory.CreateClient("Client1Http2.0");

                var data = new PasswordUpdateStruct
                {
                    Password = passwordhash,
                    UserName = username
                };

                var jsonser = JsonSerializer.Serialize(data, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                var content = new StringContent(jsonser, Encoding.UTF8, "application/json");

                using var cts = new CancellationTokenSource();

                HttpResponseMessage httpResponseMessage = await client.PutAsync(_url, content, cts.Token).ConfigureAwait(false);
                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    byte[] bytes = await httpResponseMessage.Content.ReadAsByteArrayAsync().ConfigureAwait(false);

                    ReadOnlyMemory<byte> readOnlyMemory = bytes.AsMemory();

                    if (readOnlyMemory.Length == 0)
                        return false;

                    var json = JsonDocument.Parse(bytes);
                    var root = json.RootElement;

                    if (root.TryGetProperty("Status", out var status) && root.TryGetProperty("Error", out var error))
                    {
                        var resultStatus = status.GetString() ?? string.Empty;
                        var resultError = error.GetString() ?? string.Empty;

                        if (resultStatus == "OK")
                            return true;
                        else
                            return false;
                    }
                    else
                    { 
                        var result = Encoding.UTF8.GetString(readOnlyMemory.ToArray());
                        Debug.WriteLine($"Неожиданный ответ от серве в RequestAvatarUsing\n" +
                           $"{result}");
                        return false;
                    }
                }
                else
                {
                    byte[] bytes = await httpResponseMessage.Content.ReadAsByteArrayAsync().ConfigureAwait(false);

                    ReadOnlyMemory<byte> readOnlyMemory = bytes.AsMemory();

                    if (readOnlyMemory.Length == 0)
                        return false;

                    var json = JsonDocument.Parse(bytes);
                    var root = json.RootElement;

                    if (root.TryGetProperty("Status", out var status) && root.TryGetProperty("Error", out var error))
                    {
                        var result = status.GetString() ?? string.Empty;
                        var resultError = error.GetString() ?? string.Empty;

                        if (result == "OK")
                            return true;
                        else
                            return false;
                    }
                    else
                    {
                        var result = Encoding.UTF8.GetString(readOnlyMemory.ToArray());
                        Debug.WriteLine($"Неожиданный ответ от серве в RequestAvatarUsing\n" +
                           $"{result}");
                        return false;
                    }
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
