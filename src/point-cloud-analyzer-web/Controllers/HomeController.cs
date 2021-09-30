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


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost("FileUpload")]
        [DisableRequestSizeLimit]
        public IActionResult Index(IFormFile file)
        {
            var root = System.IO.Directory.GetCurrentDirectory();
            var upload = Path.Combine(root, "upload", file.FileName);
            var fileName = file.FileName.Split('.')[0];
            var output = Path.Combine(root, "wwwroot", "output");

            Directory.CreateDirectory(Path.Combine(root, "upload"));
            Directory.CreateDirectory(Path.Combine(root, "wwwroot", "output"));
            using (Stream fileStream = new FileStream(upload, FileMode.Create))
            {
                file.CopyTo(fileStream);
            }

            //var proc = new Process
            //{
            //    StartInfo = new ProcessStartInfo
            //    {
            //        FileName = Path.Combine(root, "PotreeConverter", "PotreeConverter.exe"),
            //        Arguments = upload + " -o " + output + " --output-format LAZ",
            //        UseShellExecute = false,
            //        RedirectStandardOutput = true,
            //        CreateNoWindow = true
            //    },
            //};

            //proc.Start();
            //proc.WaitForExit();

            Exec($"PotreeConverter/PotreeConverter.exe -o ../wwwroot/output/{fileName} --output-format LAZ");

            string text = System.IO.File.ReadAllText(Path.Combine(root, "PotreeConverter", "template.html"));
            text = text.Replace("[OutputFilePath]", fileName + "/cloud.js");
            System.IO.File.WriteAllText(Path.Combine(output, fileName)  + ".html", text);

            //System.IO.File.Delete(upload);

            var redirect = "output\\" + fileName + ".html";
            return Redirect(redirect);
        }

        public static void Exec(string cmd)
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
