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

        [DisableRequestSizeLimit]
        public async Task<IActionResult> BaseView(IFormFile file)
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

            var converterPath = Path.Combine(root, "PotreeConverter", "Windows", "PotreeConverter.exe");
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

            await Cli.Wrap("wine")
                .WithArguments($"{converterPath} {filePath} -o {outputPath} --output-format LAZ")
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

        [DisableRequestSizeLimit]
        public async Task<IActionResult> DoRegistration(List<IFormFile> files)
        {
            if (files.Count == 0 || files.Count != 2) {
                return NotFound();
            }

            var root = Directory.GetCurrentDirectory();
            string htmlText = System.IO.File.ReadAllText(Path.Combine(root, "PotreeConverter", "registration_template.html"));


            foreach (var file in files)
            {
                var upload = Path.Combine(root, "upload", file.FileName);
                var fileName = file.FileName.Split('.')[0];

                Directory.CreateDirectory(Path.Combine(root, "upload"));
                using (Stream fileStream = new FileStream(upload, FileMode.Create))
                {
                    file.CopyTo(fileStream);
                }
            }
           
            var file1 = Path.Combine(root, "upload", files[0].FileName);
            var file2 = Path.Combine(root, "upload", files[1].FileName);
            var file1Name = files[0].FileName.Split('.')[0];
            var file2Name = files[1].FileName.Split('.')[0];
            var mergedFile = 
                Path.Combine(root, "upload", $"{file1Name}_{file2Name}.ply");
            var script = Path.Combine(root, "Scripts", "open3d_ICP_ori.py");

            await Cli.Wrap("python3")
                .WithArguments($"{script} {file1} {file2} {mergedFile}")
                .ExecuteAsync();

            var redirect = "output\\" + $"{file1Name}_{file2Name}.html";

            var converterPath = Path.Combine(root, "PotreeConverter", "Windows", "PotreeConverter.exe");
            var outputPathFile1 = 
                Path.Combine(root, "wwwroot", "output", $"{file1Name}");
            var outputPathFile2 = 
                Path.Combine(root, "wwwroot", "output", $"{file2Name}");    
            var outputPathResult = 
                Path.Combine(root, "wwwroot", "output", $"{file1Name}_{file2Name}");

            Exec($"chmod +x {converterPath}");
            Exec($"chmod +x {mergedFile}");

            Console.WriteLine(converterPath);
            Console.WriteLine(mergedFile);
            Console.WriteLine(outputPathResult);

            if (System.IO.File.Exists(converterPath))
            {
                Console.WriteLine("Converter Exists");
            }

            if (System.IO.File.Exists(mergedFile))
            {
                Console.WriteLine("upload Exists");
            }

            await Cli.Wrap("wine")
               .WithArguments($"{converterPath} {file1} -o {outputPathFile1} --output-format LAZ --overwrite")
               .ExecuteAsync();

            await Cli.Wrap("wine")
               .WithArguments($"{converterPath} {mergedFile} -o {outputPathFile2} --output-format LAZ --overwrite")
               .ExecuteAsync();

            await Cli.Wrap("wine")
                .WithArguments($"{converterPath} {mergedFile} -o {outputPathResult} --output-format LAZ --overwrite")
                .ExecuteAsync();


            htmlText = htmlText.Replace("[OutputFilePath1]", $"{file1Name}" + "/cloud.js");
            htmlText = htmlText.Replace("[OutputFilePath2]", $"{file2Name}" + "/cloud.js");
            htmlText = htmlText.Replace("[OutputFilePath3]", $"{file1Name}_{file2Name}" + "/cloud.js");
            System.IO.File.WriteAllText(outputPathResult + ".html", htmlText);

            return Redirect(redirect);
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private static void Exec(string cmd)
        {
            var escapedArgs = cmd.Replace("\"", "\\\"");

            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    FileName = "/bin/bash",
                    Arguments = $"-c \"{escapedArgs}\""
                }
            };

            process.Start();
            process.WaitForExit();
        }
    }
}
