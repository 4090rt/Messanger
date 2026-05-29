using Messangers.SQLite.HistroyMessage;
using Microsoft.AspNetCore.Mvc;

namespace Messangers.Controllers.ControllerHistroyMessage
{
    public class ModelUserChathisotyDelete()
    {
        public string User { get; set; }
        public string UserName { get; set; }
    }

    [ApiController]
    [Route("api/[controller]")]
    public class ControllerDeleteHistory:ControllerBase
    {
        public ILogger<ControllerDeleteHistory> _logger;
        public DeleteHistory _DeleteHistory;

        public ControllerDeleteHistory(ILogger<ControllerDeleteHistory> logger, DeleteHistory DeleteHistory)
        { 
            _logger = logger;
            _DeleteHistory = DeleteHistory;
        }

        [HttpPost("deletehistory")]
        public async Task<IActionResult> DeleteMthod([FromBody] ModelUserChathisotyDelete modelUserChathisotyDelete)
        {
            try
            {
                if (modelUserChathisotyDelete != null)
                {
                    await _DeleteHistory.RequestDelete(modelUserChathisotyDelete.User, modelUserChathisotyDelete.UserName);
                    return Ok(new { message = "Удалено успешно", state = "true" });
                }
                else
                {
                    return BadRequest(new { meessage = "Данные пусты", state = "false" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Возникло исключение" + ex.Message + ex.InnerException + ex.StackTrace);
                return BadRequest(new { meessage = "Возникло исключение" + ex.Message, state = "false" });
            }
        }
    }
}
