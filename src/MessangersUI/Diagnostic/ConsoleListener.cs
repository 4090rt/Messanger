using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessangersUI.Diagnostic
{
    public class ConsoleListener: IDisposable
    {
        private readonly ActivityListener _listener;

        public ConsoleListener()
        {
            _listener = new ActivityListener
            {
                ShouldListenTo = (source) => source.Name == "PasswordMenedger",
                Sample = (ref ActivityCreationOptions<ActivityContext> options) => ActivitySamplingResult.AllData,

                ActivityStopped = (activity) =>
                {
                    var duration = activity.Duration.TotalMilliseconds;

                    var indent = new string(' ', activity.ParentId != null ? 2 : 0);

                    Debug.WriteLine($"{indent}⏱ {activity.OperationName}: {duration:F0}мс");

                    foreach (var tag in activity.Tags)
                    {
                        Debug.WriteLine($"{indent}   📌 {tag.Key} = {tag.Value}");
                    }

                    if (duration > 100)
                    {
                        Debug.WriteLine($"{indent}   ⚠️ МЕДЛЕННО (>100мс)");
                    }
                }
            };
            ActivitySource.AddActivityListener(_listener);
            Debug.WriteLine("✅ PerfMonitor включен");
        }
        public void Dispose() => _listener?.Dispose();
    }
}
