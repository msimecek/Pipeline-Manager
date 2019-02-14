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
using Newtonsoft.Json.Linq;
using PipelineManager.Messages;

namespace PipelineManager
{
    public static class Start
    {
        [FunctionName("Start")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            [Queue("starts")] ICollector<string> definition,
            ILogger log)
        {
            // Expecting start parameters as JSON object in request body.
            // Everything to be passed through to the container should start with "pipeline.".

            const string processNameKey = "pipeline.processName";
            const string containerImageKey = "containerImage";
            const string defaultContainerImage = "msimecek/speech-pipeline:latest";

            var reqStr = await req.ReadAsStringAsync();
            var reqObj = JObject.Parse(reqStr);

            if (!reqObj.ContainsKey(processNameKey) || string.IsNullOrWhiteSpace(reqObj[processNameKey].Value<string>())) {
                return new BadRequestObjectResult($"Process name is required. Provide value in the {processNameKey} parameter.");
            }

            string containerImage = (reqObj.ContainsKey(containerImageKey)) ? reqObj[containerImageKey].Value<string>() : defaultContainerImage;

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

            var message = new StartMessage()
            {
                Env = env,
                ContainerImage = containerImage,
                ContainerName = reqObj[processNameKey].Value<string>(),
                Location = Environment.GetEnvironmentVariable("Location"),
                ResourceGroup = Environment.GetEnvironmentVariable("ResourceGroupName"),
            };

            definition.Add(JsonConvert.SerializeObject(message));

            return new AcceptedResult();
        }
    }
}
