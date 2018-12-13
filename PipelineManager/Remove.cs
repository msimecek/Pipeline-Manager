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

            if (!parsObj.ContainsKey("resourceGroupName") || !parsObj.ContainsKey("containerGroupName"))
            {
                return new BadRequestObjectResult("resourceGroupName and containerGroupName values are required.");
            }

            var azure = Utils.CreateAzureClient();

            await azure.ContainerGroups.DeleteByResourceGroupAsync(parsObj["resourceGroupName"].Value<string>(), parsObj["containerGroupName"].Value<string>());

            return new OkObjectResult("OK");
        }
    }
}
