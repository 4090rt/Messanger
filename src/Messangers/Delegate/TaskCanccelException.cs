using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessangersUI.Delegate
{
    public delegate Task TaskCancelDelegate(TaskCanceledException ex);
    public class TaskCanccelException
    {
        public readonly ILogger<TaskCanceledException> _logger;

        public TaskCanccelException(ILogger<TaskCanceledException> logger)
        {
            _logger = logger;
        }

        public async Task RunDelegate(TaskCancelDelegate taskCancelDelegate, TaskCanceledException ex)
        {
           await taskCancelDelegate(ex);
        }

        public async Task Delegate(TaskCanceledException ex)
        {
            _logger.LogError("Операция отменена ");
        }
    }
}
