using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using System;
using System.Collections.Generic;
using System.Text;

namespace PipelineManager
{
    class Utils
    {
        public static IAzure CreateAzureClient()
        {
            string appId = Environment.GetEnvironmentVariable("PrincipalAppId");
            string appSecret = Environment.GetEnvironmentVariable("PrincipalAppSecret");
            string tenantId = Environment.GetEnvironmentVariable("AzureTenantId");

            var credentials = SdkContext.AzureCredentialsFactory.FromServicePrincipal(appId, appSecret, tenantId, AzureEnvironment.AzureGlobalCloud);

            var azure = Azure
                .Configure()
                .Authenticate(credentials)
                .WithDefaultSubscription();

            return azure;
        }
    }
}
