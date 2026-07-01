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

        public ControllerFileDowload(ILogger<ControllerFileDowload> logger, ExceptionDelegate exceptionDelegate)
        { 
            _logger = logger;
            _exceptionDelegate = exceptionDelegate;
        }

        [HttpGet("download/{id}")]
        public async Task<IActionResult> DowloadCOntroller(int id)
        {
            try
            {
                string Fullpath = "";
                if (id > 0)
                {
                    // запрос в бд для поиска по id

                    if (!System.IO.File.Exists(Fullpath))
                    {
                        return BadRequest(new {message = "Не удалось найти файл"});
                    }

                    byte[] bytes = System.IO.File.ReadAllBytes(Fullpath);

                    return Ok(new {message = bytes});   
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
