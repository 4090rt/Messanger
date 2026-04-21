using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MessangersUI.Delegate
{
    public delegate Task HttpExcepDelegate(HttpRequestException ex);
    public class HttpExceptionDelegate
    {
        public readonly ILogger<HttpExceptionDelegate> _logger;

        public HttpExceptionDelegate(ILogger<HttpExceptionDelegate> logger)
        {
            _logger = logger;
        }

        public async Task RunDelegate(HttpExcepDelegate httpExcepDelegate, HttpRequestException ex)
        { 
                await httpExcepDelegate(ex);
        }

        public async Task Delegate(HttpRequestException ex)
        {
            _logger.LogError("Возникло необработанное HTTP исключение " + ex.Message, "место: " + ex.StackTrace, "полное исключение " + ex.InnerException);
        }
    }
}
