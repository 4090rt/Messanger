using MessangersUI.DataModel;
using MessangersUI.Delegate;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MessangersUI.HttpReuest.PostRequestAvatar
{
    public class RequestAvatarUsing
    {
        private readonly ILogger<RequestAvatarUsing> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        private readonly ExceptionDelegate _exceptionDelegate;
        private readonly HttpExceptionDelegate _httpExceptionDelegate;
        private readonly JsonExceptionDelegate _jsonExceptionDelegate;
        private readonly TaskCanccelException _taskCanccelException;
        private AvatarStructure _avatarStructure;

        private readonly string Url = "https://localhost:7167/api/ControllerAvatarGive/controllergiveAv";

        public RequestAvatarUsing (ILogger<RequestAvatarUsing> logger, IHttpClientFactory httpClientFactory, 
            ExceptionDelegate exceptionDelegate, HttpExceptionDelegate httpExceptionDelegate, JsonExceptionDelegate jsonExceptionDelegate,
            TaskCanccelException taskCanccelException)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _exceptionDelegate = exceptionDelegate;
            _httpExceptionDelegate = httpExceptionDelegate;
            _jsonExceptionDelegate = jsonExceptionDelegate;
            _taskCanccelException = taskCanccelException;
        }

        public async Task<AvatarStructure> Request(string username)
        {
            try
            {
                if (string.IsNullOrEmpty(username))
                    return new AvatarStructure();

                var client = _httpClientFactory.CreateClient("Client1Http2.0");

                var serialise = JsonSerializer.Serialize(username, new JsonSerializerOptions
                { 
                    PropertyNameCaseInsensitive = true
                });

                var content = new StringContent(serialise,Encoding.UTF8, "application/json");

                using var cts = new CancellationTokenSource();
                HttpResponseMessage responseMessage = await client.PostAsync(Url, content, cts.Token).ConfigureAwait(false);
                if (responseMessage.IsSuccessStatusCode)
                {
                    var bytes = await responseMessage.Content.ReadAsByteArrayAsync().ConfigureAwait(false);

                    ReadOnlyMemory<byte> readOnlyMemoryBytes = bytes.AsMemory();

                    if (readOnlyMemoryBytes.Length <= 0)
                        return new AvatarStructure();

                    var json = JsonDocument.Parse(readOnlyMemoryBytes);
                    var root = json.RootElement;

                    if (root.TryGetProperty("data", out var databytes) &&
                        root.TryGetProperty("state", out var Statedata))
                    {
                        var propByte1 = databytes.GetString() ?? string.Empty;
                        var propState2 = Statedata.GetString() ?? string.Empty;

                        byte[] bytes1 = Convert.FromBase64String(propByte1);

                        _avatarStructure = new AvatarStructure
                        {
                            Data = bytes1.AsMemory(),
                            State = propState2,
                        };

                        return _avatarStructure;
                    }
                    else
                    {
                        var resultot = System.Text.Encoding.UTF8.GetString(bytes);
                        Debug.WriteLine($"Неожиданный ответ от серве в RequestAvatarUsing\n" +
                            $"{resultot}");
                        MessageBox.Show($"Неожиданный ответ от серве в RequestAvatarUsing\n" +
                            $"{resultot}");
                        return new AvatarStructure();
                    }
                }
                else
                {
                    var bytes = await responseMessage.Content.ReadAsByteArrayAsync().ConfigureAwait(false);

                    ReadOnlyMemory<byte> readOnlyMemoryBytes = bytes.AsMemory();

                    if (readOnlyMemoryBytes.Length <= 0)
                        return new AvatarStructure();

                    var json = JsonDocument.Parse(readOnlyMemoryBytes);
                    var root = json.RootElement;

                    if (root.TryGetProperty("data", out var databytes) &&
                            root.TryGetProperty("state", out var Statedata))
                    {
                        var propByte1 = databytes.GetString() ?? string.Empty;
                        var propState2 = Statedata.GetString() ?? string.Empty;

                        byte[] bytes1 = Convert.FromBase64String(propByte1);

                        _avatarStructure = new AvatarStructure
                        {
                            Data = bytes1.AsMemory(),
                            State = propState2,
                        };

                        return _avatarStructure;
                    }
                    else
                    {
                        var resultot = System.Text.Encoding.UTF8.GetString(bytes);
                        Debug.WriteLine($"Неожиданный ответ от серве в RequestAvatarUsing\n" +
                            $"{resultot}");
                        MessageBox.Show($"Неожиданный ответ от серве в RequestAvatarUsing\n" +
           $"{resultot}");
                        return new AvatarStructure();
                    }
                }
            }
            catch (TaskCanceledException ex)
            {
                MessageBox.Show("1");
                await _taskCanccelException.RunDelegate(_taskCanccelException.Delegate, ex);
                return new AvatarStructure();
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show("2");
                await _httpExceptionDelegate.RunDelegate(_httpExceptionDelegate.Delegate, ex);
                return new AvatarStructure();
            }
            catch (JsonException ex)
            {
                MessageBox.Show("3");
                await _jsonExceptionDelegate.RunDelegate(_jsonExceptionDelegate.Delegate, ex);
                return new AvatarStructure();
            }
            catch (Exception ex)
            {
                MessageBox.Show("4");
                await _exceptionDelegate.RunDelegate(_exceptionDelegate.DelegateException, ex);
                return new AvatarStructure();
            }
        }
    }
}
