using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace point_cloud_analyzer_web.Models
{
    public class PayloadModel
    {
        public string Url { get; set; }
        public List<IFormFile> Files { get; set; }
        public string Command { get; set; }
    }
}
