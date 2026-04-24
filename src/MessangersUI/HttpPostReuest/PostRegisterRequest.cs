using MessangersUI.DataModel;
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
using System.Windows.Threading;

namespace MessangersUI.HttpPostReuest
{
    public class PostRegisterRequest
    {
        private readonly ILogger<PostRegisterRequest> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ExceptionDelegate _exceptionDelegate;
        private readonly HttpExceptionDelegate _httpExceptionDelegate;
        private readonly JsonExceptionDelegate _jsonExceptionDelegate;
        private readonly TaskCanccelException _taskCanccelException;

        public PostRegisterRequest(ILogger<PostRegisterRequest> logger, IHttpClientFactory httpClientFactory, ExceptionDelegate exceptionDelegate, 
            HttpExceptionDelegate httpExceptionDelegate, JsonExceptionDelegate jsonExceptionDelegate, TaskCanccelException taskCanccelException)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _exceptionDelegate = exceptionDelegate;
            _httpExceptionDelegate = httpExceptionDelegate;
            _jsonExceptionDelegate = jsonExceptionDelegate;
            _taskCanccelException = taskCanccelException;
        }

        public async Task<(bool succes, string ErrorMessage)> RequestMETHOD(List<DataRegistr> list, CancellationToken cancellation = default)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("Client1Http2.0");

                 var registrData = list.FirstOrDefault();
                if (registrData == null)
                {
                    return (false, "Нет данных для отправки");
                }

                var modelToSend = new
                {
                    login = registrData.Login,
                    password = registrData.cachePassword,
                    datetime = DateTime.Now
                };
                var options = new HttpRequestMessage(HttpMethod.Post, "https://localhost:7167/api/PostControllerRegister/register")
                {
                    Version = HttpVersion.Version20,
                    VersionPolicy = HttpVersionPolicy.RequestVersionOrHigher
                };


                var json = JsonSerializer.Serialize(modelToSend, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                options.Content = content;

                HttpResponseMessage recpon = await client.SendAsync(options).ConfigureAwait(false);
                if (recpon.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Успешно отправлено");
                    return (true, "");
                }
                else
                {
                    string errorBody = await recpon.Content.ReadAsStringAsync();

                    _logger.LogError($"❌ Статус: {recpon.StatusCode}");
                    _logger.LogError($"❌ Тело ошибки: {errorBody}");
                    _logger.LogError($"❌ Заголовки: {string.Join(", ", recpon.Headers.Select(h => $"{h.Key}: {string.Join(",", h.Value)}"))}");

                   
                        System.Windows.MessageBox.Show($"Ошибка {recpon.StatusCode}: {errorBody}");

                    return (false, errorBody);
                }

            }
            catch (TaskCanceledException ex)
            {
                await _taskCanccelException.RunDelegate(_taskCanccelException.Delegate, ex);
                return (false, ex.Message);
            }
            catch (HttpRequestException ex)
            {
                await _httpExceptionDelegate.RunDelegate(_httpExceptionDelegate.Delegate, ex);
                return (false, ex.Message);
            }
            catch (JsonException ex)
            {
                await _jsonExceptionDelegate.RunDelegate(_jsonExceptionDelegate.Delegate, ex);
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
