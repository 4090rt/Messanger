using MessangersUI.DataModel;
using MessangersUI.Delegate;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MessangersUI.HttpPostReuest
{
    public class PostRequestContacts
    {
        private readonly ILogger<PostRequestContacts> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ExceptionDelegate _exceptionDelegate;
        private readonly HttpExceptionDelegate _httpExceptionDelegate;
        private readonly JsonExceptionDelegate _jsonExceptionDelegate;
        private readonly TaskCanccelException _taskCanccelException;

        public PostRequestContacts(ILogger<PostRequestContacts> logger, IHttpClientFactory httpClientFactory, ExceptionDelegate exceptionDelegate,
            HttpExceptionDelegate httpExceptionDelegate, JsonExceptionDelegate jsonExceptionDelegate, TaskCanccelException taskCanccelException)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _exceptionDelegate = exceptionDelegate;
            _httpExceptionDelegate = httpExceptionDelegate;
            _jsonExceptionDelegate = jsonExceptionDelegate;
            _taskCanccelException = taskCanccelException;
        }

        public async Task<(bool, string)> Request(List<UserContact> list)
        {
            try
            {
                var options = new HttpRequestMessage(HttpMethod.Post, "https://localhost:7167/api/ControllerUserContacts/contact")
                {
                    Version = HttpVersion.Version20,
                    VersionPolicy = HttpVersionPolicy.RequestVersionOrHigher
                };

                var client = _httpClientFactory.CreateClient("Client1Http2.0");

                var contactData = list.FirstOrDefault();
                if (contactData == null)
                {
                    return (false, "Нет данных для отправки");
                }

                var modeltosend = new
                {
                    UserName = contactData.Name,
                    Usercontact = contactData.Username,
                    Pohto = contactData.photo
                };

                var jsonserialiser = JsonSerializer.Serialize(modeltosend, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                var content = new StringContent(jsonserialiser, Encoding.UTF8, "application/json");

                options.Content = content;

                HttpResponseMessage response = await client.SendAsync(options).ConfigureAwait(false);
                if (response.IsSuccessStatusCode)
                {
                    string result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var json = JsonDocument.Parse(result);
                    var properties1 = json.RootElement.GetProperty("message").ToString() ?? string.Empty;
                    var properties2 = json.RootElement.GetProperty("state").ToString() ?? string.Empty;
                    return (true, properties1);
                }
                else
                {
                    string result = await response.Content.ReadAsStringAsync();
                    var json = JsonDocument.Parse(result);
                    var properties1 = json.RootElement.GetProperty("message").ToString() ?? string.Empty;
                    System.Windows.MessageBox.Show($"❌ Ошибка {response.StatusCode}: {properties1}");
                    _logger.LogError($"❌ Ошибка {response.StatusCode}: {properties1}");
                    return (false, properties1);
                }
            }
            catch (TaskCanceledException ex)
            {
                await _taskCanccelException.RunDelegate(_taskCanccelException.Delegate, ex);
                return (false, ex.Message);
            }
            catch (JsonException ex)
            {
                await _jsonExceptionDelegate.RunDelegate(_jsonExceptionDelegate.Delegate, ex);
                return (false, ex.Message);
            }
            catch (HttpRequestException ex)
            {
                await _httpExceptionDelegate.RunDelegate(_httpExceptionDelegate.Delegate, ex);
                return (false, ex.Message);
            }
            catch (Exception ex)
            {
                await _exceptionDelegate.RunDelegate(_exceptionDelegate.DelegateException, ex);
                return (false, ex.Message);
            }
        }
    }
}
