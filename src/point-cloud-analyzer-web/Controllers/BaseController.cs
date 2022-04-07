using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using point_cloud_analyzer_web.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace point_cloud_analyzer_web.Controllers
{
    public class BaseController : ControllerBase
    {
        protected static void Exec(string cmd)
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

        protected async Task<Stream> CallMicroService(PayloadModel payload)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    var multipartContent = new MultipartFormDataContent();
                    foreach (var file in payload.Files)
                    {
                        multipartContent.Add(new StreamContent(file.OpenReadStream()), "file", file.FileName);
                    }

                    multipartContent.Add(new StringContent(payload.Command), "command");
                    client.Timeout = TimeSpan.FromSeconds(30);
                    HttpResponseMessage response = await client.PostAsync(payload.Url, multipartContent);
                    response.EnsureSuccessStatusCode();
                    var responseBody = await response.Content.ReadAsStreamAsync();

                    return responseBody;
                }
                catch (HttpRequestException e)
                {
                    Console.WriteLine("\nException Caught!");
                    Console.WriteLine("Message :{0} ", e.Message);
                    throw;
                }
            }
        }
        protected async Task<HttpResponseMessage> CallMicroServiceThenRedirect(PayloadModel payload)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    var multipartContent = new MultipartFormDataContent();
                    foreach (var file in payload.Files)
                    {
                        multipartContent.Add(new StreamContent(file.OpenReadStream()), "file", file.FileName);
                    }

                    multipartContent.Add(new StringContent(payload.Command), "command");
                    client.Timeout = TimeSpan.FromSeconds(300);
                    HttpResponseMessage response = await client.PostAsync(payload.Url, multipartContent);
                    response.EnsureSuccessStatusCode();

                    return response;
                }
                catch (HttpRequestException e)
                {
                    Console.WriteLine("\nException Caught!");
                    Console.WriteLine("Message :{0} ", e.Message);
                    throw;
                }
            }
        }

        protected void CopyStream(Stream stream, string destPath)
        {
            using (var fileStream = new FileStream(destPath, FileMode.Create, FileAccess.Write))
            {
                stream.CopyTo(fileStream);
            }
        }
    }
}
