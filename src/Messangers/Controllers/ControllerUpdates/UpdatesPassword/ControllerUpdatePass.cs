using Messangers.DataModel;
using Messangers.SQLite.Updates.UpdatesPassword;
using Microsoft.AspNetCore.Mvc;

namespace Messangers.Controllers.ControllerUpdates.UpdatesPassword
{
    [Controller]
    [Route("api/[controller]")]
    public class ControllerUpdatePass:ControllerBase
    {
        private readonly ILogger<ControllerUpdatePass> _logger;
        private readonly UpdatePassword _updatePassword;

        public ControllerUpdatePass (ILogger<ControllerUpdatePass> logger, UpdatePassword updatePassword)
        {
            _logger = logger;
            _updatePassword = updatePassword;   
        }

        [HttpPut]
        public async Task<IActionResult> ControllerPassUpdate([FromBody] PasswordUpdateStruct data)
        {
            try
            {
                if (string.IsNullOrEmpty(data.UserName) || string.IsNullOrEmpty(data.Password))
                    return BadRequest(new { Status = "false", Error = $"Паролья для изменения пуст!" });

                bool result = await _updatePassword.UpdateRequest(data).ConfigureAwait(false);
                if (result)
                    return Ok(new {Status = "true", Error = "-" });
                else
                    return BadRequest(new { Status = "false", Error = "Не удалось обновить пароль" });

            }
            catch (Exception ex)
            {
                _logger.LogError("Возникло исключение при изменении пароля" + ex.Message + ex.StackTrace);
                return BadRequest(new { Status = "false", Error = $"{ex.Message}"});
            }
        }
    }
}
