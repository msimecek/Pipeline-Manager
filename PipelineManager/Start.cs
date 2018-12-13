using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using Microsoft.Azure.Management.ContainerInstance.Fluent.Models;
using System.Linq;
using System.Collections;
using PipelineManager.Requests;
using Newtonsoft.Json.Linq;

namespace PipelineManager
{
    public static class Start
    {
        [FunctionName("Start")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            // Expecting start parameters as JSON object in request body.
            // Everything to be passed through to the container should start with "pipeline.".

            const string processNameKey = "pipeline.processName";
            const string pipelineImageName = "msimecek/speech-pipeline:0.15-full";

            var reqStr = await req.ReadAsStringAsync();
            var reqObj = JObject.Parse(reqStr);

            if (!reqObj.ContainsKey("pipeline.processName") || string.IsNullOrWhiteSpace(reqObj[processNameKey].Value<string>())) {
                return new BadRequestObjectResult($"Process name is required. Provide value in the {processNameKey} parameter.");
            }

            var env = new Dictionary<string, string>();
            
            foreach (var e in reqObj)
            {
                var key = e.Key.ToString();

                if (key.StartsWith("pipeline."))
                {
                    if (!string.IsNullOrWhiteSpace(e.Value.ToString()))
                        env.Add(key.Replace("pipeline.", ""), e.Value.ToString());
                }
            }

            var azure = Utils.CreateAzureClient();

            var containerGroup = azure.ContainerGroups.Define(reqObj[processNameKey].Value<string>())
                .WithRegion(Environment.GetEnvironmentVariable("Location"))
                .WithExistingResourceGroup(Environment.GetEnvironmentVariable("ResourceGroupName"))
                .WithLinux()
                .WithPublicImageRegistryOnly()
                .WithoutVolume()
                .DefineContainerInstance("pipeline")
                    .WithImage(pipelineImageName)
                    .WithoutPorts()
                    .WithCpuCoreCount(2)
                    .WithMemorySizeInGB(3.5)
                    .WithEnvironmentVariables(env)
                    .Attach()
                .WithRestartPolicy(ContainerGroupRestartPolicy.Never)
                .Create();

            return new OkObjectResult("OK");
        }
    }
}
