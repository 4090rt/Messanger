using Messangers.ModelData;
using Messangers.SignalSettings.Hubs;
using Microsoft.AspNetCore.Mvc;

namespace Messangers.Controllers.ControllerRegisterAndLogin
{
    [ApiController]
    [Route("api/[controller]")]
    public class ControllerOnlineUserValidate: ControllerBase
    {
        private readonly ILogger<ControllerOnlineUserValidate> _logger;
        private readonly SignalHub _signalhub;

        public ControllerOnlineUserValidate(ILogger<ControllerOnlineUserValidate> logger, SignalHub signalHub)
        {
            _logger = logger;
            _signalhub = signalHub;
        }

        [HttpPost("OnlineUsersValidate")]
        public async Task<IActionResult> Controller([FromBody] string username)
        {
            try
            {
                if (username != null && !string.IsNullOrEmpty(username))
                {
                    bool result = await _signalhub.UserOnlineValidate(username);

                    if (result)
                    {
                        return BadRequest(new {message = "Пользователь уже онлайн!", state = "false"});
                    }
                    else
                    {
                        return Ok(new {message = "Пользователь не онлайн", state = "true" });
                    }
                }
                return BadRequest(new {message = "тело пусто", state = "false"});
            }
            catch (Exception ex)
            {
                _logger.LogError("Возникло исключение" + ex.Message + ex.StackTrace + ex.InnerException);
                return BadRequest(new { message = "Возникло исключение при попытке получить статус авторизируемого юзера", state = "false" });
            }
        }
    }
}
