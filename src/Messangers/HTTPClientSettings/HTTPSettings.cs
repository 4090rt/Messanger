using Polly;
using System.Net;

namespace Messangers.HTTPClientSettings
{
    public class HTTPSettings
    {
        private readonly ILogger<HTTPSettings> _logger;
        public HTTPSettings(ILogger<HTTPSettings> logger)
        {
            _logger = logger;
        }

        public async Task ClientSettings(IServiceCollection cleint)
        {
            cleint.AddHttpClient("ClientServerPost1", clienthttp =>
            {
                clienthttp.DefaultRequestHeaders.Accept.ParseAdd("application/json");
                clienthttp.DefaultRequestHeaders.AcceptEncoding.ParseAdd("zip, deflate, br");
                clienthttp.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");

                clienthttp.DefaultRequestVersion = HttpVersion.Version20;
                clienthttp.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrHigher;
            }).AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(
                TimeSpan.FromSeconds(30),
                Polly.Timeout.TimeoutStrategy.Pessimistic,
                onTimeoutAsync: (context, timespan, task) =>
                {
                    Console.WriteLine($"⏰ Request timed out after {timespan}");
                    return Task.CompletedTask;
                })).AddTransientHttpErrorPolicy(polly => polly.CircuitBreakerAsync(
                    handledEventsAllowedBeforeBreaking: 5,
                    durationOfBreak: TimeSpan.FromMinutes(1),
                    onBreak: (outcome, timespan) =>
                    {
                        Console.WriteLine($"🔌 Circuit opened for {timespan}");
                    },
                    onHalfOpen: () =>
                    {
                        Console.WriteLine("⚠️ Circuit half-open");
                    },
                    onReset: () =>
                    {
                        Console.WriteLine("✅ Circuit reset");
                    })).AddTransientHttpErrorPolicy(policy => policy.WaitAndRetryAsync(3, retrycount =>
                    TimeSpan.FromSeconds(Math.Pow(2, retrycount)) + TimeSpan.FromMilliseconds(Random.Shared.Next(0, 100)),
                    onRetryAsync: (outcome, timespan, retrycount, task) =>
                    {
                        Console.WriteLine($"⏰ Request timed out after {timespan}");
                        return Task.CompletedTask;
                    })).ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler()
                    {
                        EnableMultipleHttp2Connections = true,

                        PooledConnectionLifetime = TimeSpan.FromMinutes(15),
                        PooledConnectionIdleTimeout = TimeSpan.FromMinutes(20),

                        AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.Brotli | DecompressionMethods.GZip,

                        MaxConnectionsPerServer = 10,
                        MaxAutomaticRedirections = 15,
                        UseCookies = false,
                        AllowAutoRedirect = true,
                    });
        }
    }
}
