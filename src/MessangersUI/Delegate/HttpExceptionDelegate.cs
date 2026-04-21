using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MessangersUI.Delegate
{
    public delegate Task HttpExcepDelegate(HttpRequestException ex);
    public class HttpExceptionDelegate
    {

        public async Task RunDelegate(HttpExcepDelegate httpExcepDelegate, HttpRequestException ex)
        { 
                await httpExcepDelegate(ex);
        }

        public async Task Delegate(HttpRequestException ex)
        {
            MessageBox.Show("Возникло необработанное HTTP исключение " + ex.Message, "место: " + ex.StackTrace + ex.InnerException);
        }
    }
}
