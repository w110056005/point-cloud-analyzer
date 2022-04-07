using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace point_cloud_analyzer_web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FilesController : BaseController
    {

        [HttpPost]
        [DisableRequestSizeLimit]
        public async Task<IActionResult> Post(List<IFormFile> files)
        {
            var root = Directory.GetCurrentDirectory();
            var size = files.Sum(f => f.Length);
            var folderPath = Path.Combine(root, "upload", DateTime.Now.ToString("yyyyMMdd-hhmmssfff"));

            Directory.CreateDirectory(folderPath);
            foreach (var formFile in files)
            {
                if (formFile.Length <= 0) continue;

                var upload = Path.Combine(folderPath, formFile.FileName);
                using (var stream = new FileStream(upload, FileMode.Create))
                {
                    await formFile.CopyToAsync(stream);
                }
            }

            return Ok(new { count = files.Count, size, folderPath });
        }
    }
}
