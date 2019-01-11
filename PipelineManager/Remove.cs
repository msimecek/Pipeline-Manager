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
using Microsoft.WindowsAzure.Storage.Blob;

namespace PipelineManager
{
    public static class Remove
    {
        [FunctionName("Remove")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            [Blob("logs", Connection = "LogStorageConnectionString")] CloudBlobContainer logsContainer,
            ILogger log)
        {
            const string containerName = "pipeline";
            var pars = await req.ReadAsStringAsync();
            var parsObj = JObject.Parse(pars);

            if (!parsObj.ContainsKey("ProcessName"))
            {
                return new BadRequestObjectResult("ProcessName missing.");
            }

            log.LogInformation(pars);

            var containerGroupName = parsObj["ProcessName"].Value<string>();
            var resourceGroupName = Environment.GetEnvironmentVariable("ResourceGroupName");

            var azure = Utils.CreateAzureClient();

            // Check if this container group exists.
            var g = await azure.ContainerGroups.GetByResourceGroupAsync(resourceGroupName, containerGroupName);
            if (g == null)
            {
                return new NotFoundObjectResult($"Container with the name {containerGroupName} was not found.");
            }

            // Store logs before removing the container.
            var logContent = await azure.ContainerGroups.GetLogContentAsync(resourceGroupName, containerGroupName, containerName);
            var blob = logsContainer.GetBlockBlobReference($"{containerGroupName}-{DateTime.UtcNow.ToString("yyyy-MM-dd_HH-mm-ssZ")}.txt");
            blob.Properties.ContentType = "text/plain";
            await blob.UploadTextAsync(logContent);

            // Remove container.
            await azure.ContainerGroups.DeleteByResourceGroupAsync(resourceGroupName, containerGroupName);

            return new OkObjectResult($"Deleted {containerGroupName} in {resourceGroupName}.");
        }
    }
}
