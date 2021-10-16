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
        public async Task<IActionResult> Get(string ply1, string ply2, string output)
        {
            try
            {
                var root = System.IO.Directory.GetCurrentDirectory();
                var pathToPy = await Cli.Wrap("python3")
                  .WithArguments(new[] { Path.Combine(root, "Scripts", "open3d_ICP_ori.py"),
                      Path.Combine(root, "Files", ply1),
                      Path.Combine(root, "Files", ply2),
                      Path.Combine("Outputs", output)}
                  )
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
