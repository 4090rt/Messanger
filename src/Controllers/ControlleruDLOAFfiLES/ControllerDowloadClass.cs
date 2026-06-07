using Messangers.SQLite.HistroyMessage.HistoryAttachment;
using Microsoft.AspNetCore.Mvc;

namespace Messangers.Controllers.ControlleruDLOAFfiLES
{
    public class Fullpath()
    { 
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public string MimeType { get; set; }
    }
    [ApiController]
    [Route("api/[controller]")]
    public class ControllerDowloadClass: ControllerBase
    {
        public ILogger<ControllerDowloadClass> _logger;
        public SearchFullPathFile _pathFile;

        public ControllerDowloadClass(ILogger<ControllerDowloadClass> logger, SearchFullPathFile pathFile)
        { 
            _logger = logger;
            _pathFile = pathFile;
        }

        [HttpGet("download/{id}")]
        public async Task<IActionResult> Controller(int id)
        {
            try
            {
                if (id != 0)
                {
                    var result = await _pathFile.SearchRequest(id).ConfigureAwait(false);

                    Fullpath path = new Fullpath();

                    foreach (var item in result)
                    { 
                        path .FileName= item.FileName;
                        path .FilePath = item.FilePath;
                        path .MimeType = item.MimeType;
                    }

                    var resultbytes = await System.IO.File.ReadAllBytesAsync(path.FilePath).ConfigureAwait(false);

                    return File(resultbytes, path.FileName, path.MimeType);
                }
                else
                {
                    return BadRequest(new { message = "id = 0", state = false });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Возникло исключение" + ex.Message + ex.StackTrace + ex.InnerException);
                return BadRequest(new { message = ex.Message, state = false });
            }
        }
    }
}
