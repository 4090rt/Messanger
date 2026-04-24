using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MessangersUI.Delegate
{
    public delegate Task DelegateExecption(Exception ex);
    public class ExceptionDelegate
    {
        public async Task RunDelegate(DelegateExecption execption, Exception ex)
        { 
             await execption(ex);
        }

        public async Task DelegateException(Exception ex)
        {
            System.Windows.MessageBox.Show("Возникло необработанное исключение " + ex.Message, "место: " + ex.StackTrace +ex.InnerException);
        }
    }
}
