using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using point_cloud_analyzer_web.Models;

namespace point_cloud_analyzer_web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ModuleDataController : BaseController
    {
        [HttpGet]
        public IActionResult Get()
        {
            var path = Path.Combine(Environment.CurrentDirectory, $"Files/module.json");
            FileInfo file = new FileInfo(path);
            file.Directory.Create(); // If the directory already exists, this method does nothing.

            StreamReader r = new StreamReader(path);
            string jsonString = r.ReadToEnd();
            var customModules = JsonConvert.DeserializeObject<List<CustomModule>>(jsonString) ?? new List<CustomModule>();

            return Ok(customModules);
        }

        [HttpPost]
        public IActionResult Post(CustomModule newCustomModules)
        {
            var path = Path.Combine(Environment.CurrentDirectory, $"Files/module.json");
            FileInfo file = new FileInfo(path);
            file.Directory.Create(); // If the directory already exists, this method does nothing.

            StreamReader r = new StreamReader(path);
            string jsonString = r.ReadToEnd();
            var customModules = JsonConvert.DeserializeObject<List<CustomModule>>(jsonString) ?? new List<CustomModule>();

            var newModule = new CustomModule
            {
                Id = customModules.Count,
                Name = newCustomModules.Name,
                Url = newCustomModules.Url,
                NeedUploadFile = newCustomModules.NeedUploadFile,
                NeedCommand = newCustomModules.NeedCommand,
            };
            customModules.Add(newModule);

            var jsonOutput = JsonConvert.SerializeObject(customModules);
            System.IO.File.WriteAllText(file.FullName, jsonOutput);

            return Ok();
        }

        [HttpPut]
        public IActionResult Put(CustomModule newCustomModules)
        {
            var path = Path.Combine(Environment.CurrentDirectory, $"Files/module.json");
            FileInfo file = new FileInfo(path);
            file.Directory.Create(); // If the directory already exists, this method does nothing.

            StreamReader r = new StreamReader(path);
            string jsonString = r.ReadToEnd();
            var customModules = JsonConvert.DeserializeObject<List<CustomModule>>(jsonString) ?? new List<CustomModule>();

            if (customModules.Find(m => m.Id == newCustomModules.Id) != null)
            {
                var updateModule = customModules.Find(m => m.Id == newCustomModules.Id);
                updateModule.NeedUploadFile = newCustomModules.NeedUploadFile;
                updateModule.NeedCommand = newCustomModules.NeedCommand;
            }
            else
            {
                var newModule = new CustomModule
                {
                    Id = customModules.Count,
                    Name = newCustomModules.Name,
                    Url = newCustomModules.Url,
                    NeedUploadFile = newCustomModules.NeedUploadFile,
                    NeedCommand = newCustomModules.NeedCommand,
                };
                customModules.Add(newModule);
            }

            var jsonOutput = JsonConvert.SerializeObject(customModules);
            System.IO.File.WriteAllText(file.FullName, jsonOutput);

            return Ok();
        }
    }
}
