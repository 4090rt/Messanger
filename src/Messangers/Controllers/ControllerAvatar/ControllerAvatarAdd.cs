using Messangers.SQLite.AvatarAdd;
using MessangersUI.DataModel;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Buffers;

namespace Messangers.Controllers.ControllerAvatar
{
    [ApiController]
    [Route("api/[controller]")]
    public class ControllerAvatarAdd : ControllerBase
    {
        private readonly ILogger<ControllerAvatarAdd> _logger;
        private readonly AvatarUpdate _avatarUpdate;

        public ControllerAvatarAdd(ILogger<ControllerAvatarAdd> logger, AvatarUpdate avatarUpdate)
        {
            _logger = logger;
            _avatarUpdate = avatarUpdate;
        }

        [HttpPost("controlleravatar")]
        public async Task<IActionResult> ControolerAv([FromForm] string UserName, [FromForm] string expansion, IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest(new {ResultLog = "Файл не загружен", Bool = "false"});
                }

                using var memorystream = new MemoryStream();
                await file.CopyToAsync(memorystream);
                byte[] bytes = memorystream.ToArray();

                if (string.IsNullOrEmpty(UserName) || string.IsNullOrEmpty(expansion))
                {
                    return BadRequest(new {ResultLog = "Данные пусты", Bool = "false" });
                }

                if (string.IsNullOrEmpty(UserName) && string.IsNullOrEmpty(expansion))
                {
                    return BadRequest(new {ResultLog = "Данные пусты", Bool = "false" });
                }

                var data = new AvatarMetaData
                {
                    UserName = UserName,
                    File = bytes,
                    expansion = expansion
                };
                await _avatarUpdate.RequestUpdAvatar(data).ConfigureAwait(false);
                return Ok(new {ResultLog = "Успешно сохранено", Bool = "true" });
            }
            catch (Exception ex)
            {
                _logger.LogError("Возникло исключение в контроллере добавления аватара" + ex.Message);
                return BadRequest(new {ResultLog = ex.Message, Bool = "false" });
            }
        }
    }
}
