using System;
using Microsoft.Azure.Management.ContainerInstance.Fluent.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using PipelineManager.Messages;

namespace PipelineManager
{
    public static class StartWorker
    {
        [FunctionName("StartWorker")]
        public static void Run([QueueTrigger("starts")]StartMessage startmessage, ILogger log)
        {
            log.LogInformation($"Processing start request: {startmessage.ContainerName}");

            var azure = Utils.CreateAzureClient();

            var containerGroup = azure.ContainerGroups.Define(startmessage.ContainerName)
                .WithRegion(startmessage.Location)
                .WithExistingResourceGroup(startmessage.ResourceGroup)
                .WithLinux()
                .WithPublicImageRegistryOnly()
                .WithoutVolume()
                .DefineContainerInstance("pipeline")
                    .WithImage(startmessage.ContainerImage)
                    .WithoutPorts()
                    .WithCpuCoreCount(2)
                    .WithMemorySizeInGB(3.5)
                    .WithEnvironmentVariables(startmessage.Env)
                    .Attach()
                .WithRestartPolicy(ContainerGroupRestartPolicy.Never)
                .Create();
        }
    }
}
