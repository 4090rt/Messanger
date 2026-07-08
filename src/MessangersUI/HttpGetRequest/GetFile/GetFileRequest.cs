using Messangers.ModelData;
using MessangersUI.Delegate;
using MessangersUI.HttpReuest.PostRequestHistoryMessage.PostFiles;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MessangersUI.HttpGetRequest.GetFile
{
    public class GetFileRequest
    {
        private readonly ILogger<GetFileRequest> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ExceptionDelegate _exceptionDelegate;
        private readonly HttpExceptionDelegate _httpExceptionDelegate;
        private readonly JsonExceptionDelegate _jsonExceptionDelegate;
        private readonly TaskCanccelException _taskCanccelException;

        private readonly string FilepathSave = "";
        public GetFileRequest(ILogger<GetFileRequest> logger, 
            IHttpClientFactory httpClientFactory,
            ExceptionDelegate exceptionDelegate,
            HttpExceptionDelegate httpExceptionDelegate,
            JsonExceptionDelegate jsonExceptionDelegate,
            TaskCanccelException taskCanccelException)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _exceptionDelegate = exceptionDelegate;
            _httpExceptionDelegate = httpExceptionDelegate;
            _jsonExceptionDelegate = jsonExceptionDelegate;
            _taskCanccelException = taskCanccelException;
        }

        public async Task<byte[]> GetDowloadFile(long id)
        {
            try
            {
                MessageBox.Show("Запрос на скачивание файла");
                var client = _httpClientFactory.CreateClient("ClientHttp2.0");

                var options = new HttpRequestMessage(HttpMethod.Get, $"https://localhost:7167/api/ControllerFileDowload/download/{id}")
                {
                    Version = HttpVersion.Version20,
                    VersionPolicy = HttpVersionPolicy.RequestVersionOrHigher
                };
                MessageBox.Show("запрос настроен");
                HttpResponseMessage httpResponseMessage = await client.SendAsync(options).ConfigureAwait(false);
                MessageBox.Show("Запрос сделан");
                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    MessageBox.Show("1");
                    byte[] bytes = await httpResponseMessage.Content.ReadAsByteArrayAsync().ConfigureAwait(false);

                    if (bytes != null)
                    {
                        MessageBox.Show("bytes not null");
                        return bytes;
                    }
                    else
                    {
                        MessageBox.Show("bytes null");
                        return null;
                    }
                }
                else
                {
                    MessageBox.Show("bytes null0" + httpResponseMessage.StatusCode);
                    _logger.LogError("Ошибка запроса. посткод:" + httpResponseMessage.StatusCode);
                    return null;
                }
            }
            catch (TaskCanceledException ex)
            {
                MessageBox.Show("bytes null1");
                await _taskCanccelException.RunDelegate(_taskCanccelException.Delegate, ex);
                return null;
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show("bytes null2");
                await _httpExceptionDelegate.RunDelegate(_httpExceptionDelegate.Delegate, ex);
                return null;
            }
            catch (Exception ex)
            {
                MessageBox.Show("bytes null3");
                await _exceptionDelegate.RunDelegate(_exceptionDelegate.DelegateException, ex);
                return null;
            }
        }
    }
}
