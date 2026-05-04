using Messangers.EthernetRequest;
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

namespace MessangersUI.HttpPostReuest
{
    public class PostProviderClient
    {
        private readonly ILogger<PostProviderClient> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ExceptionDelegate _exceptionDelegate;
        private readonly HttpExceptionDelegate _httpExceptionDelegate;
        private readonly TaskCanccelException _taskCanccelExceptionDelegate;

        public PostProviderClient(ILogger<PostProviderClient> logger, IHttpClientFactory httpClientFactory, ExceptionDelegate exceptionDelegate,
            HttpExceptionDelegate httpExceptionDelegate, TaskCanccelException taskCanccelExceptionDelegate)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _exceptionDelegate = exceptionDelegate;
            _httpExceptionDelegate = httpExceptionDelegate;
            _taskCanccelExceptionDelegate = taskCanccelExceptionDelegate;
        }

        public async Task<(string message, string state)> PostRequest(byte[] data)
        {
            System.Windows.MessageBox.Show("нАЧИНАЮ ОТПРАВКУ");
            try
            {
                var client = _httpClientFactory.CreateClient("Client1Http2.0");

                var options = new HttpRequestMessage(HttpMethod.Post, "https://localhost:7167/api/ControllerClientProviderPost/provider")
                {
                    Version = HttpVersion.Version20,
                    VersionPolicy = HttpVersionPolicy.RequestVersionOrHigher
                };

                string tostring64 = Convert.ToBase64String(data);

                var content = new StringContent(tostring64, Encoding.UTF8, "application/json");

                options.Content = content;

                HttpResponseMessage recpon = await client.SendAsync(options).ConfigureAwait(false);
                if (recpon.IsSuccessStatusCode)
                {
                    var servermessage = await recpon.Content.ReadAsStringAsync().ConfigureAwait(false);
                    using var json = JsonDocument.Parse(servermessage);
                    string properties = json.RootElement.GetProperty("message").ToString() ?? string.Empty;
                    string properties2 = json.RootElement.GetProperty("state").ToString() ?? string.Empty;

                    MessageBox.Show(properties, properties2);
                    return (properties, properties2);
                }
                else
                {
                    var servermessage = await recpon.Content.ReadAsStringAsync().ConfigureAwait(false);
                    using var json = JsonDocument.Parse(servermessage);
                    string properties = json.RootElement.GetProperty("message").ToString() ?? string.Empty;
                    string properties2 = json.RootElement.GetProperty("state").ToString() ?? string.Empty;
                    System.Windows.MessageBox.Show($"Статус: {recpon.StatusCode}\nОтвет:{properties}");

                    MessageBox.Show(properties, properties2);
                    return (properties, properties2);
                }
            }
            catch (TaskCanceledException ex)
            {
                await _taskCanccelExceptionDelegate.RunDelegate(_taskCanccelExceptionDelegate.Delegate, ex);
                return (ex.Message, "error");
            }
            catch (HttpRequestException ex)
            {
                await _httpExceptionDelegate.RunDelegate(_httpExceptionDelegate.Delegate, ex);
                return (ex.Message, "error");
            }
            catch (Exception ex)
            {
                await _exceptionDelegate.RunDelegate(_exceptionDelegate.DelegateException, ex);
                return (ex.Message, "error");
            }
        }
    }
}
