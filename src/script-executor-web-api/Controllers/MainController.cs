using Microsoft.AspNetCore.Mvc;
using CliWrap;
using CliWrap.Buffered;
using System.Threading.Tasks;

namespace script_executor_web_api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MainController : ControllerBase
    {

        [HttpPost("ExecuteScript")]
        public async Task<IActionResult> ExecuteScript()
        {
            var pathToPy = await Cli.Wrap("python3")
                  .WithArguments(new[] { "open3d_ICP_ori.py" })
                  .ExecuteBufferedAsync();

            return Ok();
        }
    }
}
