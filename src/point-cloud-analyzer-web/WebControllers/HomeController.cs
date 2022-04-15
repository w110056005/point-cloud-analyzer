using CliWrap;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace point_cloud_analyzer_web.WebControllers
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

        [RequestFormLimits(MultipartBodyLengthLimit= Int32.MaxValue)]
        [DisableRequestSizeLimit]
        public async Task<IActionResult> BaseView(IFormFile file)
        {
            var root = System.IO.Directory.GetCurrentDirectory();
            var upload = Path.Combine(root, "upload", file.FileName);
            var fileName = file.FileName.Split('.')[0];

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

            var redirect = "..\\output\\" + fileName + ".html";

            return Redirect(redirect);
        }

        public IActionResult Registration()
        {
            return View("Registration");
        }
        
        public IActionResult Stitch()
        {
            return View("Stitch");
        }

        [DisableRequestSizeLimit]
        public async Task<IActionResult> DoRegistration(List<IFormFile> files)
        {
            if (files.Count == 0 || files.Count != 2) {
                return BadRequest("Please upload two point cloud datasets.");
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
            var script = Path.Combine(root, "Scripts", "open3d_ICP_ori_v2.py");

            await Cli.Wrap("python3")
                .WithArguments($"{script} {file1} {file2} {mergedFile}")
                .ExecuteAsync();


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
               .WithArguments($"{converterPath} {file2} -o {outputPathFile2} --output-format LAZ --overwrite")
               .ExecuteAsync();

            await Cli.Wrap("wine")
                .WithArguments($"{converterPath} {mergedFile} -o {outputPathResult} --output-format LAZ --overwrite")
                .ExecuteAsync();


            htmlText = htmlText.Replace("[OutputFilePath1]", $"{file1Name}" + "/cloud.js");
            htmlText = htmlText.Replace("[OutputFilePath2]", $"{file2Name}" + "/cloud.js");
            htmlText = htmlText.Replace("[OutputFilePath3]", $"{file1Name}_{file2Name}" + "/cloud.js");
            System.IO.File.WriteAllText(outputPathResult + ".html", htmlText);

            var redirect = "..\\output\\" + $"{file1Name}_{file2Name}.html";

            return Redirect(redirect);
        }


        [DisableRequestSizeLimit]
        public async Task<IActionResult> DoStitch(IFormFile file1, IFormFile file2, IFormFile file3, IFormFile file4, IFormFile file5)
        {
            string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");

            if (file1 == null|| file2 == null|| file3 == null|| file4 == null|| file5 == null) {
                return BadRequest("Please upload five point cloud datasets.");
            }

            var root = Directory.GetCurrentDirectory();
            List<IFormFile> files = new List<IFormFile>() { file1, file2, file3, file4, file5 };

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

            // Convert all
            var converterPath = Path.Combine(root, "PotreeConverter", "Windows", "PotreeConverter.exe");
            await Cli.Wrap("wine")
               .WithArguments($"{converterPath} {Path.Combine(root, "upload", file1.FileName)} -o {Path.Combine(root, "wwwroot", "output", $"{file1.FileName.Split('.')[0]}")} --output-format LAZ --overwrite")
               .ExecuteAsync();

            await Cli.Wrap("wine")
               .WithArguments($"{converterPath} {Path.Combine(root, "upload", file2.FileName)} -o {Path.Combine(root, "wwwroot", "output", $"{file2.FileName.Split('.')[0]}")} --output-format LAZ --overwrite")
               .ExecuteAsync();

            await Cli.Wrap("wine")
               .WithArguments($"{converterPath} {Path.Combine(root, "upload", file3.FileName)} -o {Path.Combine(root, "wwwroot", "output", $"{file3.FileName.Split('.')[0]}")} --output-format LAZ --overwrite")
               .ExecuteAsync();

            await Cli.Wrap("wine")
               .WithArguments($"{converterPath} {Path.Combine(root, "upload", file4.FileName)} -o {Path.Combine(root, "wwwroot", "output", $"{file4.FileName.Split('.')[0]}")} --output-format LAZ --overwrite")
               .ExecuteAsync();

            await Cli.Wrap("wine")
               .WithArguments($"{converterPath} {Path.Combine(root, "upload", file5.FileName)} -o {Path.Combine(root, "wwwroot", "output", $"{file5.FileName.Split('.')[0]}")} --output-format LAZ --overwrite")
               .ExecuteAsync();

            string htmlText = System.IO.File.ReadAllText(Path.Combine(root, "PotreeConverter", "stitch_template.html"));

            htmlText = htmlText.Replace("[OutputFilePath1]", $"{file1.FileName.Split('.')[0]}" + "/cloud.js");
            htmlText = htmlText.Replace("[OutputFilePath2]", $"{file2.FileName.Split('.')[0]}" + "/cloud.js");
            htmlText = htmlText.Replace("[OutputFilePath3]", $"{file3.FileName.Split('.')[0]}" + "/cloud.js");
            htmlText = htmlText.Replace("[OutputFilePath4]", $"{file4.FileName.Split('.')[0]}" + "/cloud.js");
            htmlText = htmlText.Replace("[OutputFilePath5]", $"{file5.FileName.Split('.')[0]}" + "/cloud.js");
            htmlText = htmlText.Replace("[WindowOpenUrl]", $"{timestamp}_after_stitch.html");
            System.IO.File.WriteAllText(Path.Combine(root, "wwwroot", "output", $"{timestamp}_before_stitch") + ".html", htmlText);


            // execute stitching 
            var f1_path = Path.Combine(root, "upload", file1.FileName);
            var f2_path = Path.Combine(root, "upload", file2.FileName);
            var f3_path = Path.Combine(root, "upload", file3.FileName);
            var f4_path = Path.Combine(root, "upload", file4.FileName);
            var f5_path = Path.Combine(root, "upload", file5.FileName);
            var mergedFile =
                Path.Combine(root, "upload", $"{timestamp}_stitch.ply");
            var script = Path.Combine(root, "Scripts", "stitching", "main.py");

            await Cli.Wrap("python3")
                .WithArguments($"{script} {f1_path} {f2_path} {f3_path} {f4_path} {f5_path} {mergedFile}")
                .ExecuteAsync();

            var outputPathResult =
                Path.Combine(root, "wwwroot", "output", $"{timestamp}_stitch");
            await Cli.Wrap("wine")
                .WithArguments($"{converterPath} {mergedFile} -o {outputPathResult} --output-format LAZ --overwrite")
                .ExecuteAsync();

            string text = System.IO.File.ReadAllText(Path.Combine(root, "PotreeConverter", "template.html"));
            text = text.Replace("[OutputFilePath]", $"{timestamp}_stitch/cloud.js");
            System.IO.File.WriteAllText(Path.Combine(root, "wwwroot", "output", $"{timestamp}_after_stitch") + ".html", text);

            var redirect = "..\\output\\" + $"{timestamp}_before_stitch.html";

            return Redirect(redirect);
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
