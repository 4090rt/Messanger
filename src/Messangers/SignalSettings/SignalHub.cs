using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Messangers.SignalSettings.Hubs
{
    public class SignalHub: Hub
    {
        private readonly ILogger<SignalHub> _logger;

        public SignalHub(ILogger<SignalHub> logger)
        {
            _logger = logger;
        }
        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
            _logger.LogInformation("Получено сообщение от {User}: {Message}", user, message);
        }
    }
}
