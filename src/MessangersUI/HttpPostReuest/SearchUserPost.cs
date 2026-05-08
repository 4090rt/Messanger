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
    public partial class SearchUserPost
    {
        private readonly ILogger<SearchUserPost> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ExceptionDelegate _exceptionDelegate;
        private readonly HttpExceptionDelegate _httpExceptionDelegate;
        private readonly TaskCanccelException _taskCanccelExceptionDelegate;

        public SearchUserPost(ILogger<SearchUserPost> logger, IHttpClientFactory httpClientFactory, ExceptionDelegate exceptionDelegate,
            HttpExceptionDelegate httpExceptionDelegate, TaskCanccelException taskCanccelExceptionDelegate)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _exceptionDelegate = exceptionDelegate;
            _httpExceptionDelegate = httpExceptionDelegate;
            _taskCanccelExceptionDelegate = taskCanccelExceptionDelegate;
        }

        public async Task<bool> Request(string username)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("Client1Http2.0");

                var options = new HttpRequestMessage(HttpMethod.Post, "https://localhost:7167/api/CntrollerSearchUser/search")
                {
                    Version = HttpVersion.Version20,
                    VersionPolicy = HttpVersionPolicy.RequestVersionOrHigher
                };

                var json = JsonSerializer.Serialize(username, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                var stringcontent = new StringContent(json, Encoding.UTF8, "application/json");

                options.Content = stringcontent;

                HttpResponseMessage recpon = await client.SendAsync(options).ConfigureAwait(false);
                if (recpon.IsSuccessStatusCode)
                {
                    var read = await recpon.Content.ReadAsStringAsync().ConfigureAwait(false);
                    using var jsondoc = JsonDocument.Parse(read);
                    string properties1 = jsondoc.RootElement.GetProperty("result").ToString() ?? string.Empty;
                    string properties2 = jsondoc.RootElement.GetProperty("message").ToString() ?? string.Empty;

                    if (properties1 == "true")
                    {
                        MessageBox.Show("Вернул тру");
                        return true;
                    }
                    else
                    {
                        MessageBox.Show(properties2);
                        MessageBox.Show("Вернул фолс");
                        return false;
                    }
                }
                else
                {
                    var read = await recpon.Content.ReadAsStringAsync().ConfigureAwait(false);
                    using var jsondoc = JsonDocument.Parse(read);
                    string properties1 = jsondoc.RootElement.GetProperty("result").ToString() ?? string.Empty;
                    string properties2 = jsondoc.RootElement.GetProperty("message").ToString() ?? string.Empty;

                    return false;
                }
            }
            catch (TaskCanceledException ex)
            {
                await _taskCanccelExceptionDelegate.RunDelegate(_taskCanccelExceptionDelegate.Delegate, ex);
                return default;
            }
            catch (HttpRequestException ex)
            {
                await _httpExceptionDelegate.RunDelegate(_httpExceptionDelegate.Delegate, ex);
                return default;
            }
            catch (Exception ex)
            {
                await _exceptionDelegate.RunDelegate(_exceptionDelegate.DelegateException, ex);
                return default;
            }
        }
    }
}
