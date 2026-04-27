using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;

namespace Messangers.SignalSettings.Hubs
{
    public class SignalHub: Hub
    {
        private readonly ILogger<SignalHub> _logger;
        private static readonly ConcurrentDictionary<string, string> _onlineUsers = new();
        public SignalHub(ILogger<SignalHub> logger)
        {
            _logger = logger;
        }
        public override async Task OnConnectedAsync()
        {
            var username = Context.User?.FindFirst(JwtRegisteredClaimNames.UniqueName)?.Value;

            if (!string.IsNullOrEmpty(username))
            {
                _onlineUsers[username] = Context.ConnectionId;
                Console.WriteLine($"✅ {username} подключился. Онлайн: {_onlineUsers.Count}");
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var user = _onlineUsers.FirstOrDefault(x => x.Value == Context.ConnectionId).Key;

            if (!string.IsNullOrEmpty(user))
            {
                _onlineUsers.Remove(user, out _);
                Console.WriteLine($"❌ {user} отключился. Онлайн: {_onlineUsers.Count}");
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(string touser, string message)
        {
            var fromuser = Context.User?.FindFirst(JwtRegisteredClaimNames.UniqueName);

            if (_onlineUsers.TryGetValue(touser, out var connectionId))
            {
                await Clients.Client(connectionId).SendAsync("ReceiveMessage", fromuser, message);
                _logger.LogInformation("Получено сообщение от {User}: {Message}", fromuser, message);
            }
            else
            {
                // реализация если пользователь оффлайн: оффлайн - нашли в бд - созранали сообщение
                //отправили когда он появился онлайн
            }
        }
    }
}
