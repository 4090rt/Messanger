using Messangers.SQLite.HistroyMessage;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Messangers.Controllers.ControllerHistroyMessage
{
    public class SaveHistoryRequestData
    { 
        public string User1 { get; set; }
        public string User2 { get; set; }
        public string Message { get; set; }
        public string Date { get; set; }

        public string State { get; set; }
    }

    [ApiController]
    [Route("api/[controller]")]
    public class ControllerHistorySave: ControllerBase
    {
        public ILogger<ControllerHistorySave> _logger;
        public SaveHistoryMessage _historymessage;

        public ControllerHistorySave(ILogger<ControllerHistorySave> logger, SaveHistoryMessage historymessage)
        { 
            _logger = logger;
            _historymessage = historymessage;
        }

        [HttpPost("savehistory")]
        public async Task<IActionResult> SaveControoler([FromBody] SaveHistoryRequestData request)
        {
            try
            {
                if (!string.IsNullOrEmpty(request.User1) && !string.IsNullOrEmpty(request.User2) && !string.IsNullOrEmpty(request.Message) && !string.IsNullOrEmpty(request.Date) && !string.IsNullOrEmpty(request.State))
                {
                   var result =  await _historymessage.SaveMethod(request.User1, request.User2, request.Message, request.Date, request.State); 
                    return Ok(new { message = "Успешно сохранено", state = "true", id =  result});
                }
                else
                {
                    return BadRequest(new { message = $"Данные пусты", state = "false", id = 0 });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Возникло исключение" + ex.Message  + ex.InnerException + ex.StackTrace);
                return BadRequest(new {message = $"возникло исключение {ex.Message}", state = "false", id = 0 });
            }
        }
    }
}
