using Messangers.Controllers.ControllerHistroyMessage;
using Messangers.SQLite.HistroyMessage.HistoryAttachment;
using Microsoft.AspNetCore.Mvc;

namespace Messangers.Controllers.ControlleruDLOAFfiLES
{
    [ApiController]
    [Route("api/[controller]")]
    public class ControllerFilesHistory:ControllerBase
    {
        public ILogger<ControllerFilesHistory> _logger;
        public FileHistory _fileHistory;

        public ControllerFilesHistory(ILogger<ControllerFilesHistory> logger, FileHistory fileHistory)
        {
            _logger = logger;
            _fileHistory = fileHistory;
        }

        [HttpPost("ControllerHistoryFiles")]
        public async Task<IActionResult> RequestHistoryFiles([FromBody] UsersListHistory usersListHistory)
        {
            try
            {
                if (usersListHistory != null && !string.IsNullOrEmpty(usersListHistory.LoginUser1) && !string.IsNullOrEmpty(usersListHistory.LoginUser2))
                {
                    var result = await _fileHistory.Request(usersListHistory.LoginUser1, usersListHistory.LoginUser2);
                    return Ok(new { message = result, state = "true"});
                }
                else
                {
                    return BadRequest(new { message = "данные пусты", state = "false" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Возникло исключение" + ex.Message + ex.StackTrace + ex.InnerException);
                return BadRequest(new { message = ex.Message, state = "false" });
            }
        }
    }
}
