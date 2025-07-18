using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADLManagerPro
{
    public class AlgoTemplateRoot
    {
        public string algo_name { get; set; }
        public List<Template> templates { get; set; }
    }

    public class Template
    {
        public string template_name { get; set; }
        public Dictionary<string, Parameter> template_parameters { get; set; }
    }

    public class Parameter
    {
        public string type { get; set; }
        public string value { get; set; }
    }
}
