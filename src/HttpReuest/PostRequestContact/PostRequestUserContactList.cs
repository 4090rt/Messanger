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

namespace MessangersUI.HttpReuest.PostRequestContact
{
    public class PostRequestUserContactList
    {
        private readonly ILogger<PostRequestUserContactList> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ExceptionDelegate _exceptionDelegate;
        private readonly HttpExceptionDelegate _httpExceptionDelegate;
        private readonly JsonExceptionDelegate _jsonExceptionDelegate;
        private readonly TaskCanccelException _taskCanccelException;

        public PostRequestUserContactList(ILogger<PostRequestUserContactList> logger, IHttpClientFactory httpClientFactory, ExceptionDelegate exceptionDelegate,
            HttpExceptionDelegate httpExceptionDelegate, JsonExceptionDelegate jsonExceptionDelegate, TaskCanccelException taskCanccelException)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _exceptionDelegate = exceptionDelegate;
            _httpExceptionDelegate = httpExceptionDelegate;
            _jsonExceptionDelegate = jsonExceptionDelegate;
            _taskCanccelException = taskCanccelException;
        }

        public async Task<List<UserContact>> RequestPost(string Username)
        {
            List<UserContact> list = null;
            try
            {
                var client = _httpClientFactory.CreateClient("Client1Http2.0");

                var options = new HttpRequestMessage(HttpMethod.Post, "https://localhost:7167/api/ControllerPostUserList/listcontacts")
                {
                    Version = HttpVersion.Version20,
                    VersionPolicy = HttpVersionPolicy.RequestVersionOrHigher
                };

                if (Username != null)
                {
                    var objectusers = new
                    {
                        user = Username
                    };

                    var json = JsonSerializer.Serialize(objectusers, new JsonSerializerOptions
                    { 
                        PropertyNameCaseInsensitive = true
                    });

                    var stringcontent = new StringContent(json, Encoding.UTF8, "application/json");

                    options.Content = stringcontent;
                    HttpResponseMessage response = await client.SendAsync(options).ConfigureAwait(false);
                    if (response.IsSuccessStatusCode)
                    {
                        var result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                        var contacts = JsonSerializer.Deserialize<List<UserContact>>(result);
                        return contacts ?? new List<UserContact>();
                    }
                    else
                    {
                        MessageBox.Show("Не получил твет");
                        _logger.LogError("Возникла ошибка запроса списка кнотактов" + response.StatusCode);
                        return new List<UserContact>();
                    }
                }
                else
                {
                    return new List<UserContact>();
                }
            }
            catch (TaskCanceledException ex)
            {
                MessageBox.Show("1");
                await _taskCanccelException.RunDelegate(_taskCanccelException.Delegate, ex);
                return new List<UserContact>();
            }
            catch (JsonException ex)
            {
                MessageBox.Show("2");
                await _jsonExceptionDelegate.RunDelegate(_jsonExceptionDelegate.Delegate, ex);
                return new List<UserContact>();
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show("3");
                await _httpExceptionDelegate.RunDelegate(_httpExceptionDelegate.Delegate, ex);
                return new List<UserContact>();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + ex.InnerException);
                await _exceptionDelegate.RunDelegate(_exceptionDelegate.DelegateException, ex);
                return new List<UserContact>();
            }
        }
    }
}
