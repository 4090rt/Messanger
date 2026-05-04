using DirectoryStatistic.Http.ModelData;
using Messangers.DeserializeRequestHttp;
using MessangersUI.Delegate;
using Microsoft.Extensions.Hosting;
using System.Net;
using System.Net.NetworkInformation;

namespace Messangers.EthernetRequest
{
    public class PingRequest
    {
        private readonly ILogger<PingRequest> _loggger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly Deserialize _deserialize;

        public PingRequest(ILogger<PingRequest> logger, IHttpClientFactory httpClientFactory, Deserialize deserialize)
        {
            _loggger = logger;
            _httpClientFactory = httpClientFactory;
            _deserialize = deserialize;
        }

        public async Task<List<DataPing>> Request(string host)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("ClientServerPost1");

                var options = new HttpRequestMessage(HttpMethod.Get, "https://www.google.com/generate_204")
                {
                    Version = HttpVersion.Version20,
                    VersionPolicy = HttpVersionPolicy.RequestVersionOrHigher
                };

                var timer = System.Diagnostics.Stopwatch.StartNew();
                HttpResponseMessage rec = await client.SendAsync(options).ConfigureAwait(false);
                timer.Stop();
                if (rec.IsSuccessStatusCode)
                {
                    var ping = timer.ElapsedMilliseconds / 2;
                    Console.WriteLine("пИНГ ПОЛУЧЕН!!1");
                    return new List<DataPing>
                    {
                        new DataPing()
                        {
                        Host = host,
                        PingMs = ping,
                        Status = "success",
                        Error = null
                        }
                    };
                }
                else
                {
                    _loggger.LogError($"Возникла ошибка запрос. Статус код" + rec.StatusCode);
                    return new List<DataPing>
                    {
                        new DataPing()
                        {
                        Host = host,
                        PingMs = 0,
                        Status = "success",
                        Error = "No"
                        }
                    };
                }
            }
            catch (TaskCanceledException ex)
            {
                _loggger.LogError("Операция отменена" + ex.Message + ex.StackTrace + ex.InnerException);
                return new List<DataPing>
                    {
                        new DataPing()
                        {
                        Host = host,
                        PingMs = 0,
                        Status = "Error",
                        Error = ex.Message
                        }
                    };
            }
            catch (HttpRequestException ex)
            {
                _loggger.LogError("Возникло необработанное HTTP исключение" + ex.Message + ex.StackTrace + ex.InnerException);
                return new List<DataPing>
                    {
                        new DataPing()
                        {
                        Host = host,
                        PingMs = 0,
                        Status = "Error",
                        Error = ex.Message
                        }
                    };
            }
            catch (Exception ex)
            {
                _loggger.LogError("Возникло необработанное исключение" + ex.Message + ex.StackTrace + ex.InnerException);
                return new List<DataPing>
                    {
                        new DataPing()
                        {
                        Host = host,
                        PingMs = 0,
                        Status = "Error",
                        Error = ex.Message
                        }
                    };
            }
        }
    }
}
