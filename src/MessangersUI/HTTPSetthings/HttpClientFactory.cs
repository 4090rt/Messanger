using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MessangersUI.HTTPSetthings
{
    public class HttpClientFactory
    {
        public void Setthings(IServiceCollection client)
        {
            client.AddHttpClient("Client1Http2.0", clienthttp =>
            {
                clienthttp.DefaultRequestHeaders.Accept.ParseAdd("application/json");
                clienthttp.DefaultRequestHeaders.AcceptEncoding.ParseAdd("gzip, deflate, br");
                clienthttp.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");

                clienthttp.DefaultRequestVersion = HttpVersion.Version20;
                clienthttp.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrHigher;
            }).AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(
                TimeSpan.FromMinutes(0.30),
                Polly.Timeout.TimeoutStrategy.Pessimistic,
                onTimeoutAsync: (context, timespan, task) =>
                {
                    Console.WriteLine($"⏰ Request timed out after {timespan}");
                    return Task.CompletedTask;
                })).AddTransientHttpErrorPolicy(polly => polly.CircuitBreakerAsync(
                    handledEventsAllowedBeforeBreaking: 5,
                    durationOfBreak: TimeSpan.FromMinutes(1),
                    onHalfOpen: () =>
                    {
                        Console.WriteLine("⚠️ Circuit half-open");
                    },
                    onBreak: (outcome, timespan) =>
                    {
                        Console.WriteLine($"🔌 Circuit opened for {timespan}");
                    },
                    onReset: () =>
                    {
                        Console.WriteLine("✅ Circuit reset");
                    })).AddTransientHttpErrorPolicy(polly => polly.WaitAndRetryAsync(3, retrycount =>
                    TimeSpan.FromSeconds(Math.Pow(2, retrycount)) +
                    TimeSpan.FromMilliseconds(Random.Shared.Next(0, 100)),
                    onRetryAsync: (outcome, timespan, retrycount, task) =>
                    {
                        Console.WriteLine($"⏰ Request timed out after {timespan}");
                        return Task.CompletedTask;
                    })).ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler()
                    {
                        EnableMultipleHttp2Connections = true,

                        PooledConnectionLifetime = TimeSpan.FromMinutes(15),
                        PooledConnectionIdleTimeout = TimeSpan.FromMinutes(20),

                        AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate | DecompressionMethods.Brotli,

                        MaxConnectionsPerServer = 10,
                        MaxAutomaticRedirections = 15,
                        UseCookies = false,
                        AllowAutoRedirect = true
                    });
        }
    }
}
