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




        // Save CSV
        public void SaveCsv(string path, List<string[]> rows)
        {
            using (StreamWriter writer = new StreamWriter(path))
            {
                foreach (var row in rows)
                {
                    writer.WriteLine(string.Join(",", row));
                }
            }
        }

        // Read CSV
        public List<string[]> ReadCsv(string path)
        {
            var result = new List<string[]>();
            if (File.Exists(path))
            {
                var lines = File.ReadAllLines(path);
                result = lines.Select(line => line.Split(',')).ToList();
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
