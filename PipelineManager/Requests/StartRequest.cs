using System;
using System.Collections.Generic;
using System.Text;

namespace PipelineManager.Requests
{
    public class StartRequest
    {
        public string audioFilesList { get; set; }
        public string transcriptFilesList { get; set; }
        public string languageModelFile { get; set; }

        public string languageModelId { get; set; }
        public string location { get; set; }
        public string processName { get; set; }
        public string removeSilence { get; set; }
    }
}
