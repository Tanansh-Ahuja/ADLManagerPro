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
        string instrumentFilePath = "InstrumentsToBeFetched.csv";
        string ADLsFilePath = "ADLsToBeFetched.csv";
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
            try
            {
                if (File.Exists(keyFilePath))
                {
                    return File.ReadAllText(keyFilePath);
                }
                return null;

            }
            catch
            {
                MessageBox.Show("Error occured while fetching API key. Shutting down.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return String.Empty;
            }

        }


        public Dictionary<string, List<Template>> FetchJsonFromFile()
        {
            try
            {
                if (!File.Exists(jsonPath))
                {
                    return null;
                }

                string jsonContent = File.ReadAllText(jsonPath);
                var algoTemplateRoots = JsonConvert.DeserializeObject<List<AlgoTemplateRoot>>(jsonContent);

                
                Dictionary<string, List<Template>> algoWithTemplate = new Dictionary<string, List<Template>>();
                if(algoTemplateRoots == null)
                {
                    //no data fetched from template
                    return null;
                }

                foreach (var algo in algoTemplateRoots)
                {
                    List<Template> templateList = new List<Template>();

                    foreach (var t in algo.Templates)
                    {
                        var template = new Template
                        {
                            TemplateName = t.TemplateName,
                            ParamNameWithValue = t.TemplateParameters // Already Dictionary<string, string>
                        };

                        templateList.Add(template);
                    }

                    algoWithTemplate[algo.AlgoName] = templateList;
                }

                return algoWithTemplate;
            }
            catch(Exception ex)
            {
                MessageBox.Show("An Unexpected error occured."+ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }

        }



        public void SaveTemplateDictionaryToFile(Dictionary<string, List<Template>> algoWithTemplate)
        {
            try
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
                            TemplateParameters = t.ParamNameWithValue != null
                                ? new Dictionary<string, string>(t.ParamNameWithValue)
                                : new Dictionary<string, string>()
                        }).ToList()
                    };

                    algoTemplateRoots.Add(algoTemplateRoot);
                }

                string jsonContent = JsonConvert.SerializeObject(algoTemplateRoots, Formatting.Indented);
                File.WriteAllText(jsonPath, jsonContent);

            }
            catch
            {
                MessageBox.Show("Error occured while saving templates to the file. Shutting down.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                HelperFunctions.ShutEverythingDown();
            }
        }


        public List<string> GetInstrumentAliasList()
        {
            try
            {
                var result = new List<string>();

                using (var reader = new StreamReader(instrumentFilePath))
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
            catch (FileNotFoundException)
            {
                MessageBox.Show($"Error: File {instrumentFilePath} not found", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw;
            }
            catch
            {
                MessageBox.Show("Error occured while fetching instruments names from file. Shutting down.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw;
            }
        }
        public List<InstrumentInfo> GetInstrumentInfoList()
        {
            try
            {
                var result = new List<InstrumentInfo>();

                using (var reader = new StreamReader(instrumentFilePath))
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
                            if (string.IsNullOrWhiteSpace(columns[0]) ||
                                string.IsNullOrWhiteSpace(columns[1]) ||
                                string.IsNullOrWhiteSpace(columns[2]) ||
                                string.IsNullOrWhiteSpace(columns[3]))
                                {
                               
                                    return null;
                                    //throw new InvalidDataException($"Line {lineNumber}: One or more required columns are empty or null.");
                                }
                            InstrumentInfo instrumentInfo = new InstrumentInfo(columns[0], columns[1], columns[2], columns[3]);
                            result.Add(instrumentInfo); // 0-based index
                        }
                    }
                }

                return result;

            }
            catch (FileNotFoundException)
            {
                MessageBox.Show($"Error: File {instrumentFilePath} not found", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw;
            }
            catch
            {
                MessageBox.Show("Error occured while fetching instruments list from file. Shutting down.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw;
                
            }
        }

        public List<string> GetADLNameList()
        {
            try
            {
                var result = new List<string>();

                using (var reader = new StreamReader(ADLsFilePath))
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
                        if (columns.Length >= 1)
                        {
                            if (string.IsNullOrWhiteSpace(columns[0]))
                            {

                                return null;
                                //throw new InvalidDataException($"Line {lineNumber}: One or more required columns are empty or null.");
                            }

                            result.Add(columns[0]); // 0-based index
                        }
                    }
                }

                return result;

            }
            catch (FileNotFoundException)
            {
                MessageBox.Show($"Error: File {ADLsFilePath} not found", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw;
            }
            catch
            {
                MessageBox.Show("Error occured while fetching ADLs names from file. Shutting down.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw;
            }
        }

        
    }
}
