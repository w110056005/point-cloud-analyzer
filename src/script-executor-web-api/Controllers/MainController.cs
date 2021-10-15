using Microsoft.AspNetCore.Mvc;
using CliWrap;
using CliWrap.Buffered;
using System.Threading.Tasks;
using System;
using System.IO;

namespace script_executor_web_api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MainController : ControllerBase
    {

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                var root = System.IO.Directory.GetCurrentDirectory();
                var pathToPy = await Cli.Wrap("python3")
                  .WithArguments(new[] { Path.Combine(root, "Files", "open3d_ICP_ori.py" )})
                  .ExecuteBufferedAsync();
                return Ok(pathToPy);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }
    }
}
