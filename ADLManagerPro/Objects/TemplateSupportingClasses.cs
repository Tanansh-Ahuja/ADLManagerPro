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
        public Dictionary<string, ParameterData> TemplateParameters { get; set; }
    }
    public class ParameterData
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }
    }
    

    public class Template
    {
        public string TemplateName { get; set; }

        public Dictionary<string, (string Type, string Value)> ParamNameWithTypeAndValue { get; set; }
    }

}
