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
