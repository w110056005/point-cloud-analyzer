using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CliWrap;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;


namespace point_cloud_analyzer_web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MicroServiceController : BaseController
    {

        [HttpPost]
        [DisableRequestSizeLimit]
        public async Task<IActionResult> Post(IFormCollection data)
        {
            var fullFileName = "test.ply";
            var file = CallMicroService(data);
            var root = Directory.GetCurrentDirectory();
            var upload = Path.Combine(root, "upload", fullFileName);
            var fileName = fullFileName.Split('.')[0];

            Directory.CreateDirectory(Path.Combine(root, "upload"));
            CopyStream(file.Result, upload);

            var converterPath = Path.Combine(root, "PotreeConverter", "Windows", "PotreeConverter.exe");
            var filePath = Path.Combine(root, "upload", fullFileName);
            var outputPath = Path.Combine(root, "wwwroot", "output", fileName);

            Exec($"chmod +x {converterPath}");
            Exec($"chmod +x {filePath}");

            Console.WriteLine(converterPath);
            Console.WriteLine(filePath);
            Console.WriteLine(outputPath);

            if (System.IO.File.Exists(converterPath))
            {
                Console.WriteLine("Converter Exists");
            }

            if (System.IO.File.Exists(filePath))
            {
                Console.WriteLine("upload Exists");
            }

            await Cli.Wrap("wine")
                .WithArguments($"{converterPath} {filePath} -o {outputPath} --output-format LAZ")
                .ExecuteAsync();


            string text = System.IO.File.ReadAllText(Path.Combine(root, "PotreeConverter", "template.html"));
            text = text.Replace("[OutputFilePath]", fileName + "/cloud.js");
            System.IO.File.WriteAllText(outputPath + ".html", text);

            System.IO.File.Delete(upload);

            var redirect = "..\\output\\" + fileName + ".html";

            return Redirect(redirect);
        }
    }
}
