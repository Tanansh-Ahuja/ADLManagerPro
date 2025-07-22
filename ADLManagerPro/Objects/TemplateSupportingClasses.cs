using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ADLManagerPro
{
    public class AlgoTemplateRoot
    {
        [JsonProperty("algo_name")]
        public string AlgoName { get; set; }

        [JsonProperty("templates")]
        public List<AlgoTemplate> Templates { get; set; }
    }

    public class AlgoTemplate
    {
        [JsonProperty("template_name")]
        public string TemplateName { get; set; }

        [JsonProperty("template_parameters")]
        public Dictionary<string, string> TemplateParameters { get; set; }
    }
    public class Template
    {
        public string TemplateName { get; set; }
        public Dictionary<string, string> ParamNameWithValue { get; set; }
    }
}
