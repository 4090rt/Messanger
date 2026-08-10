using System.Diagnostics;

namespace Messangers.Diagnostic
{
    public class ConsoleListener: IDisposable
    {
        private readonly ActivityListener _activityListener;
        public ConsoleListener()
        { 
            _activityListener = new ActivityListener
            { 
                ShouldListenTo = (source) => source.Name == "Messangers",
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
            ActivitySource.AddActivityListener(_activityListener);
            Debug.WriteLine("✅ PerfMonitor включен");
        }
        public void Dispose() => _activityListener?.Dispose();
    }
}
