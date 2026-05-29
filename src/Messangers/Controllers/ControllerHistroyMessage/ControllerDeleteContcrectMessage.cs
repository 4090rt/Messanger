using Messangers.SQLite.HistroyMessage;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Messangers.Controllers.ControllerHistroyMessage
{
    public class ModelConcrectMessage
    { 
        public int Id { get; set; }
    }

    [ApiController]
    [Route("api/[controller]")]
    public class ControllerDeleteContcrectMessage: ControllerBase
    {
        public ILogger<ControllerDeleteContcrectMessage> _logger;
        public DeleteConcrectMessage _deleteContcrectMessage;

        public ControllerDeleteContcrectMessage(ILogger<ControllerDeleteContcrectMessage> logger, DeleteConcrectMessage deleteContcrectMessage)
        {
            _logger = logger;
            _deleteContcrectMessage = deleteContcrectMessage;
        }

        [HttpPost("deleteconcrect")]
        public async Task<IActionResult> ControllerDeleteConcrect([FromBody] ModelConcrectMessage modelConcrectMessage)
        {
            try
            {
                if (modelConcrectMessage != null)
                {
                    await _deleteContcrectMessage.RequestDelete(modelConcrectMessage.Id);
                    return Ok(new { message = "Успешно удалено", state = "true" });
                }
                else
                {
                    return BadRequest(new { message = $"Данные пустые", state = "false" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Возникло исключение" + ex.Message + ex.StackTrace + ex.InnerException);
                return BadRequest(new { message = $"Возникло исключение {ex.Message}", state = "false" });
            }
        }
    }
}
