using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MessangersUI.Delegate
{
    public delegate Task JsonExcDelegate(JsonException ex);
    public class JsonExceptionDelegate
    {
        private readonly ILogger<JsonExceptionDelegate> _logger;

        public JsonExceptionDelegate(ILogger<JsonExceptionDelegate> logger)
        {
            _logger = logger;
        }

        public async Task RunDelegate(JsonExcDelegate jsonExcDelegate, JsonException ex)
        { 
             await jsonExcDelegate(ex);
        }

        public async Task Delegate(JsonException ex)
        {
            _logger.LogError("Возникло необработанное JSON исключение " + ex.Message, "место: " + ex.StackTrace, "полное исключение " + ex.InnerException);
        }
    }
}
