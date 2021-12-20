using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace point_cloud_analyzer_web.Models
{
    public class Payload
    {
        public string Command { get; set; }
        public string RequestURL { get; set; }
        public List<IFormFile> Files { get; set; } 
    }
}
