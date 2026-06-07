using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace Messangers.Controllers.ControllerEthernetStat
{
    [ApiController]
    [Route("api/[controller]")]
    public class ControllerPostPingToServer: ControllerBase
    {
        private readonly ILogger<ControllerPostPingToServer> _logger;
        public ControllerPostPingToServer(ILogger<ControllerPostPingToServer> logger)
        {

            _logger = logger;
        }
        [HttpPost("ping")]
        public async Task<IActionResult> Conreoller()
        {
            try
            {
                string message = await new StreamReader(Request.Body).ReadToEndAsync().ConfigureAwait(false);

                message = message.Trim('"');
                byte[] data = Convert.FromBase64String(message);
                string decodedMessage = Encoding.UTF8.GetString(data);
                if (decodedMessage == "ping")
                {
                    _logger.LogError("ВВАВАВАВА");
                    return Ok(new { message = "Ok" });
                }
                else
                {
                    return BadRequest(new {message = $"Error, message - null {message}"});
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Error - {ex.Message}, message - null" });
            }
        }
    }
}
