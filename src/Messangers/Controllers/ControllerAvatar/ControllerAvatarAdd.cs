using MessangersUI.DataModel;
using Microsoft.AspNetCore.Mvc;
using System.Buffers;

namespace Messangers.Controllers.ControllerAvatar
{
    [ApiController]
    [Route("api/[controller]")]
    public class ControllerAvatarAdd : ControllerBase
    {
        private readonly ILogger<ControllerAvatarAdd> _logger;

        public ControllerAvatarAdd(ILogger<ControllerAvatarAdd> logger)
        {
            _logger = logger;
        }

        [HttpPost("controlleravatar")]
        public async Task<IActionResult> ControolerAv([FromForm] string UserName, [FromForm] string expansion, IFormFile file)
        {
            byte[] fullbytes = ArrayPool<byte>.Shared.Rent(1024);
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest("Файл не загружен");

               using var memoryStream = new MemoryStream();

                int reads = 0;

                await using var stream = await file.CopyToAsync(memoryStream);
            }
            catch (Exception ex)
            {

            }
        }
    }
}
