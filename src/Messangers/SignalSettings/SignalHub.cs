using Messangers.ModelData;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

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

           var username = Context.User?.FindFirst(ClaimTypes.Name)?.Value;


            if (!string.IsNullOrEmpty(username))
            {
                _onlineUsers[username] = Context.ConnectionId;
                _logger.LogInformation($"✅ {username} подключился. ConnectionId: {Context.ConnectionId}. Онлайн: {_onlineUsers.Count}");
                Console.WriteLine($"✅ {username} подключился. ConnectionId: {Context.ConnectionId}. Онлайн: {_onlineUsers.Count}");

                await Clients.All.SendAsync("UserConnect", username);
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
                await Clients.All.SendAsync("UserDisconnect", user);
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task<bool> UserOnlineValidate(string username)
        {
            try
            {
                if (_onlineUsers.TryGetValue(username, out var result))
                {
                    return true;
                }
                else
                { 
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Возникло исключение" + ex.Message + ex.InnerException + ex.StackTrace);
                return false;
            }
        }

        public async Task<List<DataUsersList>> UserOnline(List<DataUsersList> list)
        {
            try
            {
                List<DataUsersList> listresult = new List<DataUsersList>();
                foreach(var item in list)
                {
                    if (_onlineUsers.TryGetValue(item.User, out var resultat))
                    {
                        var data = new DataUsersList
                        {
                            User = item.User
                        };
                        listresult.Add(data);
                    }
                }
                foreach (var item in listresult)
                {
                    _logger.LogError("Вернул юзера2" + item.User);
                }
                return listresult;
            }
            catch (Exception ex)
            {
                _logger.LogError("Возникло исключение" + ex.Message + ex.InnerException + ex.StackTrace);
                return new List<DataUsersList>();
            }
        }

        public async Task SendMessage(string touser, string message)
        {
            var fromuser = Context.User?.FindFirst(ClaimTypes.Name)?.Value;

            _logger.LogInformation($"Онлайн пользователи: {string.Join(", ", _onlineUsers.Keys)}");
            if (_onlineUsers.TryGetValue(touser, out var connectionId))
            {
                _logger.LogInformation($"Найден ConnectionId: {connectionId} для {touser}");

                await Clients.Client(connectionId).SendAsync("ReceiveMessage", fromuser, message);
                _logger.LogInformation("Сообщение отправлено успешно!");
            }
            else
            {
                _logger.LogInformation($"Найден ConnectionId: {connectionId} для {touser}");
                     _logger.LogInformation("Сообщение отправлено успешно!");
                // реализация если пользователь оффлайн: оффлайн - нашли в бд - созранали сообщение
                //отправили когда он появился онлайн
            }
        }
    }
}
