using Messangers.SQLite.HistroyMessage.HistoryAttachment;
using MessangersUI.Delegate;
using Microsoft.AspNetCore.Mvc;

namespace Messangers.Controllers.ControlleruDLOAFfiLES
{
    [ApiController]
    [Route("api/[controller]")]
    public class ControllerFileDowload:ControllerBase
    {
        private readonly ILogger<ControllerFileDowload> _logger;
        private readonly ExceptionDelegate _exceptionDelegate;
        private readonly SelectAllPathAttachment _selectAllPathAttachment;

        public ControllerFileDowload(ILogger<ControllerFileDowload> logger, ExceptionDelegate exceptionDelegate, SelectAllPathAttachment selectAllPathAttachment)
        { 
            _logger = logger;
            _exceptionDelegate = exceptionDelegate;
            _selectAllPathAttachment = selectAllPathAttachment;
        }

        [HttpGet("download/{id}")]
        public async Task<IActionResult> DowloadCOntroller(int id)
        {
            try
            {
                string Fullpath = "";
                if (id > 0)
                {
                    Fullpath = await _selectAllPathAttachment.FullpathGive(id);

                    if (!System.IO.File.Exists(Fullpath))
                    {
                        return BadRequest(new { message = "Не удалось найти файл" });
                    }

                    byte[] bytes = System.IO.File.ReadAllBytes(Fullpath);

                    return Ok(new { message = bytes });
                }
                else
                {
                    return BadRequest(new { message = "Некорректный айди!" });
                }
            }
            catch (Exception ex)
            {
                await _exceptionDelegate.RunDelegate(_exceptionDelegate.DelegateException, ex);
                return BadRequest(new {message = "Возникло исключение" + ex.Message});
            }
        }
    }
}
