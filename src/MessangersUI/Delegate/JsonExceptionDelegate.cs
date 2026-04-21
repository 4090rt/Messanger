using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace MessangersUI.Delegate
{
    public delegate Task JsonExcDelegate(JsonException ex);
    public class JsonExceptionDelegate
    {

        public async Task RunDelegate(JsonExcDelegate jsonExcDelegate, JsonException ex)
        { 
             await jsonExcDelegate(ex);
        }

        public async Task Delegate(JsonException ex)
        {
            MessageBox.Show("Возникло необработанное JSON исключение " + ex.Message, "место: " + ex.StackTrace + ex.InnerException);
        }
    }
}
