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

namespace MessangersUI.HttpPostReuest
{
    public class PostLoginRequest
    {
        private readonly ILogger<PostLoginRequest> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ExceptionDelegate _exceptionDelegate;
        private readonly HttpExceptionDelegate _httpExceptionDelegate;
        private readonly JsonExceptionDelegate _jsonExceptionDelegate;
        private readonly TaskCanccelException _taskCanccelException;

        public PostLoginRequest(ILogger<PostLoginRequest> logger, IHttpClientFactory httpClientFactory, ExceptionDelegate exceptionDelegate,
            HttpExceptionDelegate httpExceptionDelegate, JsonExceptionDelegate jsonExceptionDelegate, TaskCanccelException taskCanccelException)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _exceptionDelegate = exceptionDelegate;
            _httpExceptionDelegate = httpExceptionDelegate;
            _jsonExceptionDelegate = jsonExceptionDelegate;
            _taskCanccelException = taskCanccelException;
        }

        public async Task<(bool Succes, string errormesseage)> Request(List<DataLogin> list)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("Client1Http2.0");

                var options = new HttpRequestMessage(HttpMethod.Post, "https://localhost:7167/api/PostControllerLogin/login")
                {
                    Version = HttpVersion.Version20,
                    VersionPolicy = HttpVersionPolicy.RequestVersionOrHigher
                };

                var logindata = list.FirstOrDefault();
                if (logindata == null)
                {
                    return (false, "Нет данных для отправки");
                }

                var loglist = new
                {
                    login = logindata.Login,
                    password = logindata.Password
                };

                byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(list);
                string tobase64 = Convert.ToBase64String(bytes);

                var json = JsonSerializer.Serialize(loglist, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                options.Content = content;

                HttpResponseMessage recpon = await client.SendAsync(options);

                if (recpon.IsSuccessStatusCode)
                {
                    System.Windows.MessageBox.Show("Успешно отправлено");
                    _logger.LogInformation("Успешно отправлено");
                    return (true, "");
                }
                else
                {
                    string errorBody = await recpon.Content.ReadAsStringAsync();
                    System.Windows.MessageBox.Show($"❌ Ошибка {recpon.StatusCode}: {errorBody}");
                    _logger.LogError($"❌ Ошибка {recpon.StatusCode}: {errorBody}");
                    return (false, errorBody);
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
