using System;
using System.Collections.Generic;
using System.Text;

namespace PipelineManager.Messages
{
    public class StartMessage
    {
        public string ContainerName { get; set; }
        public string Location { get; set; }
        public string ResourceGroup { get; set; }
        public string ContainerImage { get; set; }

        public Dictionary<string, string> Env { get; set; }
    }
}
