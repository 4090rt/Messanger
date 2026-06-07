using Messangers.SQLite.HistroyMessage.HistoryAttachment;
using Microsoft.AspNetCore.Mvc;

namespace Messangers.Controllers.ControlleruDLOAFfiLES
{
    public class UpdateId
    {
        public int Id { get; set; }
        public Int64 attaid { get; set; }
    }
    [ApiController]
    [Route("api/[controller]")]
    public class ControllerUpdateAttachmentId:ControllerBase
    {
        public ILogger<ControllerUpdateAttachmentId> _logger { get; set; }
        public AttachmentIdUpdate _AttachmentIdUpdate;

        public ControllerUpdateAttachmentId(ILogger<ControllerUpdateAttachmentId> logger, AttachmentIdUpdate attachmentIdUpdate)
        {
            _logger = logger;
            _AttachmentIdUpdate = attachmentIdUpdate;
        }

        [HttpPost("controllerupdateId")]
        public async Task<IActionResult> ControllerUpdate([FromBody] UpdateId updateId)
        {
            try
            {
                if (updateId != null && updateId.Id > 0)
                {
                    bool result = await _AttachmentIdUpdate.RequestUpdate(updateId.Id, updateId.attaid);
                    if (result)
                    {
                        _logger.LogError("j,yjdktyjyjyjtjjgbtgit");
                        return Ok(new { message = "Успешно обновлено", state = "true" });
                    }
                    else
                    {
                        return BadRequest(new { message = "Ошибка обновления", state = "false" });
                    }
                }
                else
                {
                    return BadRequest(new { message = "Айди не найден", state = "false" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Возникло исключение" + ex.Message + ex.InnerException + ex.StackTrace);
                return BadRequest(new {message = "Возникло исключение" + ex.Message, state = "false" });
            }
        }
    }
}
