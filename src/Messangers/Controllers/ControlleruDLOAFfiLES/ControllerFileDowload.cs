using Messangers.SQLite.HistroyMessage.HistoryAttachment;
using MessangersUI.Delegate;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mail;

namespace Messangers.Controllers.ControlleruDLOAFfiLES
{
    [ApiController]
    [Route("api/[controller]")]
    public class ControllerFileDowload:ControllerBase
    {
        private readonly ILogger<ControllerFileDowload> _logger;
        private readonly ExceptionDelegate _exceptionDelegate;
        private readonly IWebHostEnvironment _env;
        private readonly SelectAllPathAttachment _selectAllPathAttachment;

        public ControllerFileDowload(IWebHostEnvironment env, ILogger<ControllerFileDowload> logger, ExceptionDelegate exceptionDelegate, SelectAllPathAttachment selectAllPathAttachment)
        { 
            _env = env;
            _logger = logger;
            _exceptionDelegate = exceptionDelegate;
            _selectAllPathAttachment = selectAllPathAttachment;
        }

        [HttpGet("download/{id}")]
        public async Task<IActionResult> DowloadCOntroller(int id)
        {
            try
            {
                _logger.LogError($"{id}");
                string Fullpath = "";
                if (id > 0)
                {
                    Fullpath = await _selectAllPathAttachment.FullpathGive(id);
                    if (!System.IO.File.Exists(Fullpath))
                    {
                        _logger.LogError($"Файл не найден: {Fullpath}");
                        return BadRequest(new { message = "Не удалось найти файл" });
                    }

                    byte[] bytes = System.IO.File.ReadAllBytes(Fullpath);
                    _logger.LogError("Полный путь найден!!!!!!");
                    return Ok(new { message = bytes });
                }
                else
                {
                    _logger.LogError($"айди");
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
