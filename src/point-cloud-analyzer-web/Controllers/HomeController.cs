using CliWrap;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using point_cloud_analyzer_web.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace point_cloud_analyzer_web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost("FileUpload")]
        [DisableRequestSizeLimit]
        public async Task<IActionResult> FileUpload(IFormFile file)
        {
            var root = System.IO.Directory.GetCurrentDirectory();
            var upload = Path.Combine(root, "upload", file.FileName);
            var fileName = file.FileName.Split('.')[0];
            var redirect = "output\\" + fileName + ".html";

            Directory.CreateDirectory(Path.Combine(root, "upload"));
            using (Stream fileStream = new FileStream(upload, FileMode.Create))
            {
                file.CopyTo(fileStream);
            }

            var converterPath = Path.Combine(root, "PotreeConverter", "Linux", "PotreeConverter");
            var filePath = Path.Combine(root, "upload", file.FileName);
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

            //var proc = new Process
            //{
            //    StartInfo = new ProcessStartInfo
            //    {
            //        FileName = "wine",
            //        Arguments = $"{converterPath} {filePath} -o {outputPath} --output-format LAZ",
            //        UseShellExecute = false,
            //        RedirectStandardOutput = true,
            //        CreateNoWindow = true
            //    },
            //};

            //proc.Start();
            //proc.WaitForExit();

            await Cli.Wrap(converterPath)
                .WithArguments($"{filePath} -o {outputPath} --output-format LAZ")
                .ExecuteAsync();


            string text = System.IO.File.ReadAllText(Path.Combine(root, "PotreeConverter", "template.html"));
            text = text.Replace("[OutputFilePath]", fileName + "/cloud.js");
            System.IO.File.WriteAllText(outputPath + ".html", text);

            System.IO.File.Delete(upload);

            return Redirect(redirect);
        }

        public IActionResult Registration()
        {
            return View("Registration");
        }

        [HttpPost]
        [DisableRequestSizeLimit]
        public async Task<IActionResult> Registration(List<IFormFile> files)
        {
          
            //await Cli.Wrap(converterPath)
            //    .WithArguments($"{filePath} -o {outputPath} --output-format LAZ")
            //    .ExecuteAsync();


            //string text = System.IO.File.ReadAllText(Path.Combine(root, "PotreeConverter", "template.html"));
            //text = text.Replace("[OutputFilePath]", fileName + "/cloud.js");
            //System.IO.File.WriteAllText(outputPath + ".html", text);

            //System.IO.File.Delete(upload);

            //return Redirect(redirect);

            return Ok();
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
