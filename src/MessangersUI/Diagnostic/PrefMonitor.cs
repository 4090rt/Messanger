using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessangersUI.Diagnostic
{
    public class PrefMonitor
    {
        private static readonly ActivitySource Source = new ActivitySource("MessangersUI");

        private static bool Enbled { get; set; } = true;

        public static IDisposable Mesearu(string operationName, params (string key, object value)[] tags)
        { 
            if (!Enbled) return NullDisposable.Instance;

            using var activity = Source.StartActivity(operationName, ActivityKind.Internal);
            
            if (activity != null)
            {
                foreach (var (key, value) in tags)
                {
                    activity.AddTag(key, value);
                }
                return new ActivityDisposable(activity);
            }
            return NullDisposable.Instance;
        }

        public static async Task<T>MesearuAsync<T>(string operationName, Func<Task<T>> func,params (string key, object value)[] tags)
        {
            using var _ = Mesearu(operationName, tags);
            return await func();
        }

        public class NullDisposable : IDisposable
        { 
            public static NullDisposable Instance = new NullDisposable();

            public void Dispose() { }
        }

        public class ActivityDisposable : IDisposable
        {
            public readonly Activity _activity;
            public ActivityDisposable(Activity activity) => _activity = activity;
            public void Dispose() => _activity?.Stop();
        }
    }
}
