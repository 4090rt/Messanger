using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MessangersUI.Delegate
{
    public delegate Task TaskCancelDelegate(TaskCanceledException ex);
    public class TaskCanccelException
    {
        public async Task RunDelegate(TaskCancelDelegate taskCancelDelegate, TaskCanceledException ex)
        {
           await taskCancelDelegate(ex);
        }

        public async Task Delegate(TaskCanceledException ex)
        {
            MessageBox.Show("Операция отменена ");
        }
    }
}
