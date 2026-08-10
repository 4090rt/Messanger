using System.Diagnostics;

namespace Messangers.Diagnostic
{
    public class PrefMonitor
    {
        private static readonly ActivitySource Source = new ActivitySource("Messangers");
        private static bool Enbled { get; set; } = true;

        public class NullDisposable : IDisposable
        { 
            public static NullDisposable Instance = new NullDisposable();
            public void Dispose() { }
        }

        public class ActivityStoppedDisposable : IDisposable
        {
            public readonly Activity _activity;
            public ActivityStoppedDisposable(Activity activity) => _activity = activity;
            public void Dispose() => _activity?.Stop();
        }

        public static IDisposable Measury(string OperationName, params (string key, object value)[] tags)
        {
            if (!Enbled) return NullDisposable.Instance;

            using var activity = Source.StartActivity(OperationName, ActivityKind.Internal);

            if (activity != null)
            {
                foreach (var (key, value) in tags)
                {
                    activity.AddTag(key,value);
                }
                return new ActivityStoppedDisposable(activity);
            }
            return NullDisposable.Instance;
        }

        public static async Task<T> AsyncMeasury<T>(string operationName, Func<Task<T>> func, params (string key, object value)[] tags)
        {
            using var _ = Measury(operationName, tags);
            return await func();
        }
    }
}
