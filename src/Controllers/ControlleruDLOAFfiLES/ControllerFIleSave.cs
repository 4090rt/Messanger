using Messangers.ModelData;
using Messangers.SQLite.HistroyMessage.HistoryAttachment;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Messangers.Controllers.ControlleruDLOAFfiLES
{
    public class FileUploadRequest
    {
        public IFormFile File { get; set; }
        public string User { get; set; }
        public string Username { get; set; }
    }

    [ApiController]
    [Route("api/[controller]")]
    public class ControllerFIleSave: ControllerBase
    {
        public ILogger<ControllerFIleSave> _logger;
        private readonly IWebHostEnvironment _env;
        private readonly AttachmentSave _attachmentSave;

        public ControllerFIleSave (IWebHostEnvironment env,ILogger<ControllerFIleSave> logger, AttachmentSave attachmentSave)
        {
            _logger = logger;
            _attachmentSave = attachmentSave;
            _env = env;
        }

        [HttpPost("updloadfiles")]
        public async Task<IActionResult> Controller([FromForm] FileUploadRequest fileUploadRequest)
        {
            try
            {
                if (fileUploadRequest.File == null || fileUploadRequest.File.Length == 0)
                {
                    return BadRequest(new { message = "Файл не выбран", state = false });
                }

                var filextension = Path.GetExtension(fileUploadRequest.File.FileName);
                var fuiduniqueqFUle = $"{Guid.NewGuid()}{filextension}";

                var directoryname = Path.Combine(_env.ContentRootPath ?? "wwwroot", "uploads", DateTime.Now.ToString("yyyy/MM"));
                if (!Directory.Exists(directoryname))
                {
                    Directory.CreateDirectory(directoryname);
                }

                var filepath = Path.Combine(directoryname, fuiduniqueqFUle);
                var userpath = $"uploads/{DateTime.Now.ToString("yyyy/MM")} / {fuiduniqueqFUle}";

                using (FileStream fs = new FileStream(filepath, FileMode.Create))
                {
                    await fileUploadRequest.File.CopyToAsync(fs);
                }

                var at = new AttachmentMetadata
                {
                    FileName = fileUploadRequest.File.FileName,
                    FilePath = userpath,
                    FileSize = (int)fileUploadRequest.File.Length,
                    MimeType = fileUploadRequest.File.ContentType,
                    CreatedAt = DateTime.UtcNow.ToString("yyyy/MM.dd")
                };
                _logger.LogError(at.FilePath, at.FileSize);
                Int64 result = await _attachmentSave.SaveRequest(at).ConfigureAwait(false);

                var at2 = new AttachmentMetadata
                {
                    Id = result,
                    FileName = fileUploadRequest.File.FileName,
                    FilePath = userpath,
                    FileSize = (int)fileUploadRequest.File.Length,
                    MimeType = fileUploadRequest.File.ContentType,
                    CreatedAt = DateTime.UtcNow.ToString("yyyy/MM.dd")
                };
                if (result != 0)
                {
                    _logger.LogError("метаданные сохранены" + $"{at2.Id}" + $"{at2.FileName}");
                    return Ok(new { message = "метаданные файла", state = "true", attachment = at2 });
                }
                return BadRequest(new { message = "Не удалось сохранить данные о файле", state = "false", attachment = "" });
            }
            catch (Exception ex)
            {
                _logger.LogError("Возникло исключение" + ex.Message + ex.StackTrace + ex.InnerException);
                return BadRequest(new {message = "Возникло исключение на сервере", state = "false", attachment = "" });
            }
        }
    }
}
