using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PipelineManager
{
    public static class Remove
    {
        [FunctionName("Remove")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            var pars = await req.ReadAsStringAsync();
            var parsObj = JObject.Parse(pars);

            if (!parsObj.ContainsKey("ProcessName"))
            {
                return new BadRequestObjectResult("ProcessName missing.");
            }

            log.LogInformation(pars);

            var azure = Utils.CreateAzureClient();

            await azure.ContainerGroups.DeleteByResourceGroupAsync(
                Environment.GetEnvironmentVariable("ResourceGroupName"), 
                parsObj["ProcessName"].Value<string>());

            return new OkObjectResult($"Deleted {parsObj["ProcessName"]} in {Environment.GetEnvironmentVariable("ResourceGroupName")}");
        }
    }
}
