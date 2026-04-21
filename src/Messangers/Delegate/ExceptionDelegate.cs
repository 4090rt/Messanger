using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessangersUI.Delegate
{
    public delegate Task DelegateExecption(Exception ex);
    public class ExceptionDelegate
    {
        private readonly ILogger<ExceptionDelegate> _logger;

        public ExceptionDelegate(ILogger<ExceptionDelegate> logger)
        {
            _logger = logger;
        }

        public async Task RunDelegate(DelegateExecption execption, Exception ex)
        { 
             await execption(ex);
        }

        public async Task DelegateException(Exception ex)
        {
            _logger.LogError("Возникло необработанное исключение " + ex.Message, "место: " + ex.StackTrace, "полное исключение " +ex.InnerException);
        }
    }
}
