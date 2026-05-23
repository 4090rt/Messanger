using Messangers.ModelData;
using Messangers.SignalSettings.Hubs;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Messangers.Controllers.ControllerUsersContacts
{
    [ApiController]
    [Route("api/[controller]")]
    public class ControllerOnlineusers: ControllerBase
    {
        public ILogger<ControllerOnlineusers> _logger;
        public SignalHub _signalhub;

        public ControllerOnlineusers(ILogger<ControllerOnlineusers> logger, SignalHub signal)
        { 
            _logger = logger;
            _signalhub = signal;
        }

        [HttpPost("onlineuser")]
        public async Task<IActionResult> Controller([FromBody] List<DataUsersList> dataUsersLists)
        {
            try
            {
                if (dataUsersLists != null)
                {
                    var result = await _signalhub.UserOnline(dataUsersLists).ConfigureAwait(false);
                    if (result != null)
                    {
                        var jsonString = JsonSerializer.Serialize(result);
                        return Ok(jsonString);
                    }
                    else
                    {
                        return BadRequest(new {message = "Список не найден", state = "false"});
                    }
                }
                return BadRequest(new { message = "Данные пусты", state = "false" });
            }
            catch (Exception ex)
            {
                _logger.LogError("Возникло исключение" + ex.Message + ex.StackTrace + ex.InnerException);
                return BadRequest(new { message = "Возникло исключение" + ex.Message, state = "false" });
            }
        }
    }
}
