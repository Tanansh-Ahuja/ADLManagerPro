using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace ADLManagerPro
{
    public class FileHandlers
    {
        string jsonPath = "algo_templates.json";
        string csvPath = "InstrumentsToBeFetched.csv";
        string txtPath = "ADLsToBeFetched.txt";
        Dictionary<string, List<Template>> algoWithTemplate = new Dictionary<string, List<Template>>();

        public void SaveApiKey(string path, string key)
        {
            try
            {
                File.WriteAllText(path, key);
            }
            catch (Exception fileEx)
            {
                MessageBox.Show($"Connected, but failed to save key: {fileEx.Message}", "File Save Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        public string FetchApiKey(string keyFilePath)
        {
            if (File.Exists(keyFilePath))
            {
                return File.ReadAllText(keyFilePath);
            }
            return null;

        }


        public Dictionary<string, List<Template>> FetchJsonFromFile()
        {
            string jsonContent = File.ReadAllText(jsonPath);
            var algoTemplateRoots = JsonConvert.DeserializeObject<List<AlgoTemplateRoot>>(jsonContent);
            foreach (var algo in algoTemplateRoots)
            {
                List<Template> templateList = new List<Template>();

                foreach (var t in algo.Templates)
                {
                    var template = new Template
                    {
                        TemplateName = t.TemplateName,
                        ParamNameWithTypeAndValue = t.TemplateParameters.ToDictionary(
                            kvp => kvp.Key,
                            kvp => (kvp.Value.Type, kvp.Value.Value))
                    };

                    templateList.Add(template);
                }

                algoWithTemplate[algo.AlgoName] = templateList;
            }
            return algoWithTemplate;
        }


        public void SaveTemplateDictionaryToFile(Dictionary<string, List<Template>> algoWithTemplate)
        {
            var algoTemplateRoots = new List<AlgoTemplateRoot>();

            foreach (var kvp in algoWithTemplate)
            {
                var algoName = kvp.Key;
                var templates = kvp.Value;

                var algoTemplateRoot = new AlgoTemplateRoot
                {
                    AlgoName = algoName,
                    Templates = templates.Select(t => new AlgoTemplate
                    {
                        TemplateName = t.TemplateName,
                        TemplateParameters = t.ParamNameWithTypeAndValue.ToDictionary(
                            param => param.Key,
                            param => new ParameterData
                            {
                                Type = param.Value.Type,
                                Value = param.Value.Value
                            })
                    }).ToList()
                };

                algoTemplateRoots.Add(algoTemplateRoot);
            }

            string jsonContent = JsonConvert.SerializeObject(algoTemplateRoots, Formatting.Indented);
            File.WriteAllText(jsonPath, jsonContent);
        }

        public List<string> GetInstrumentAliasList()
        {
            var result = new List<string>();

            using (var reader = new StreamReader(csvPath))
            {
                int lineNumber = 0;

                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    lineNumber++;

                    // Skip header
                    if (lineNumber == 1)
                        continue;

                    var columns = line.Split(',');

                    // Ensure there are at least 4 columns
                    if (columns.Length >= 4)
                    {
                        result.Add(columns[3]); // 0-based index
                    }
                }
            }

            return result;
        }

        public List<string> GetADLNameList()
        {
            var result = new List<string>();

            using (var reader = new StreamReader(txtPath))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        result.Add(line.Trim());
                    }
                }
            }

            return result;
        }

        // Load Config JSON
        public Dictionary<string, string> LoadConfig(string path)
        {
            if (!File.Exists(path))
                return new Dictionary<string, string>();

            string json = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
        }
    }
}
