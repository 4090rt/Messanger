using Messangers.ModelData;
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

namespace MessangersUI.HttpGetRequest.DowloadQFIle
{
    public class PostAttachmentDowloadToServer
    {
        private readonly ILogger<PostAttachmentDowloadToServer> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ExceptionDelegate _exceptionDelegate;
        private readonly HttpExceptionDelegate _httpExceptionDelegate;
        private readonly JsonExceptionDelegate _jsonExceptionDelegate;
        private readonly TaskCanccelException _taskCanccelException;

        public PostAttachmentDowloadToServer(ILogger<PostAttachmentDowloadToServer> logger, IHttpClientFactory httpClientFactory, ExceptionDelegate exceptionDelegate,
            HttpExceptionDelegate httpExceptionDelegate, JsonExceptionDelegate jsonExceptionDelegate, TaskCanccelException taskCanccelException)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _exceptionDelegate = exceptionDelegate;
            _httpExceptionDelegate = httpExceptionDelegate;
            _jsonExceptionDelegate = jsonExceptionDelegate;
            _taskCanccelException = taskCanccelException;
        }

        public async Task GetFile(int IdAttachment, string filename)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("ClientHttp2.0");

                var options = new HttpRequestMessage(HttpMethod.Get, $"https://localhost:7167/api/ControllerDowloadClass/download/{IdAttachment}")
                {
                    Version = HttpVersion.Version20,
                    VersionPolicy = HttpVersionPolicy.RequestVersionOrHigher
                };

                HttpResponseMessage response = await client.SendAsync(options).ConfigureAwait(false);
                if (response.IsSuccessStatusCode)
                {
                    byte[] bytes = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
                    if (bytes != null)
                    {
                        var savefilefialog = new SaveFileDialog
                        {
                            FileName = filename,
                            Filter = "All files (*.*)|*.*"
                        };

                        if (savefilefialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        {
                            await System.IO.File.WriteAllBytesAsync(savefilefialog.FileName, bytes);
                            MessageBox.Show("Файл сохранен по пути" + filename);

                            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                            {
                                FileName = savefilefialog.FileName,
                                UseShellExecute = true
                            });
                        }
                    }
                }
                else
                {
                    _logger.LogError("\"Ошибка при попытке скачаь файл\" + response.IsSuccessStatusCode");
                    MessageBox.Show("Ошибка при попытке скачаь файл" + response.IsSuccessStatusCode);
                }
            }
            catch (TaskCanceledException ex)
            {
                await _taskCanccelException.RunDelegate(_taskCanccelException.Delegate, ex);
            }
            catch (HttpRequestException ex)
            {
                await _httpExceptionDelegate.RunDelegate(_httpExceptionDelegate.Delegate, ex);
            }
            catch (Exception ex)
            {
                await _exceptionDelegate.RunDelegate(_exceptionDelegate.DelegateException, ex);
            }
        }
    }
}
