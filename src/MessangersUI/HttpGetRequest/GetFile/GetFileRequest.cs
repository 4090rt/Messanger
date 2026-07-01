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

        public async Task<byte[]> GetDowloadFile(AttachmentMetadata attachmentMetadata)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("ClientHttp2.0");

                var options = new HttpRequestMessage(HttpMethod.Get, $"https://localhost:7167/api/ControllerGetFile/download/{attachmentMetadata.Id}")
                {
                    Version = HttpVersion.Version20,
                    VersionPolicy = HttpVersionPolicy.RequestVersionOrHigher
                };

                HttpResponseMessage httpResponseMessage = await client.SendAsync(options).ConfigureAwait(false);
                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    byte[] bytes = await httpResponseMessage.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
                    
                    return bytes;
                }
                else
                {
                    _logger.LogError("Ошибка запроса. посткод:" + httpResponseMessage.StatusCode);
                    return null;
                }
            }
            catch (TaskCanceledException ex)
            {
                await _taskCanccelException.RunDelegate(_taskCanccelException.Delegate, ex);
                return null;
            }
            catch (HttpRequestException ex)
            {
                await _httpExceptionDelegate.RunDelegate(_httpExceptionDelegate.Delegate, ex);
                return null;
            }
            catch (Exception ex)
            {
                await _exceptionDelegate.RunDelegate(_exceptionDelegate.DelegateException, ex);
                return null;
            }
        }
    }
}
