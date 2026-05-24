using Messangers.ModelData;
using Messangers.SQLite.HistroyMessage;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Messangers.Controllers.ControllerHistroyMessage
{
    public class UsersListHistory
    { 
        public string LoginUser1 { get; set; }
        public string LoginUser2 { get; set; }
    }

    [ApiController]
    [Route("api/[controller]")]
    public class ControllerDowloadHistroyMessage: ControllerBase
    {
        public ILogger<ControllerDowloadHistroyMessage> _logger;
        UserSearchHistoryDowload _userSearchHistoryDowload;

        public ControllerDowloadHistroyMessage(ILogger<ControllerDowloadHistroyMessage> logger, UserSearchHistoryDowload userSearchHistoryDowload)
        { 
            _logger = logger;
            _userSearchHistoryDowload = userSearchHistoryDowload;
        }

        [HttpPost("dowloadhistory")]
        public async Task<IActionResult> PostController([FromBody] UsersListHistory usersListHistory)
        {
            try
            {
                if (usersListHistory != null && !string.IsNullOrEmpty(usersListHistory.LoginUser1) && !string.IsNullOrEmpty(usersListHistory.LoginUser2))
                {
                    var result = await _userSearchHistoryDowload.SelectRequest(usersListHistory.LoginUser1, usersListHistory.LoginUser2);

                    if (result != null)
                    {
                        var jsonString = JsonSerializer.Serialize(result);
                        return Ok(jsonString);
                    }
                    else
                    {
                        return BadRequest(new { message = "Список не найден", state = "false" });
                    }
                }
                else
                {
                    _logger.LogError("Лист данный пуст");
                    return BadRequest(new { message = "Данные пусты", state = "false" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Возникло исключение" + ex.Message + ex.StackTrace + ex.InnerException);
                return BadRequest(new { message = "Возникло исключение" + ex.Message, state = "false" });
            }
        }
    }
}
