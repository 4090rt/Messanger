using Messangers.ModelData;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.Contracts;

namespace Messangers.ControllerPost
{
    [ApiController]
    [Route("api/[controller]")]
    public class ControllerPostProvider: ControllerBase
    {
        private readonly ILogger<ControllerPostProvider> _logger;
        public ControllerPostProvider(ILogger<ControllerPostProvider> logger)
        {
            _logger = logger;
        }

        [HttpPost("provider")]
        public async Task<IActionResult> Request([FromBody] IpIfo ipIfo)
        {
            if (ipIfo == null) return BadRequest(new {message = "Пусто", state = "error"});

            return Ok(new {message = "успешно", state = "succes"});
        }
    } 
}
