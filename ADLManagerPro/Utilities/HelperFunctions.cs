using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ADLManagerPro
{
    public class HelperFunctions
    {
        public HelperFunctions() { }

        public List<string> GetTemplateNames(List<Template> templateList)
        {
            List<string> templateNames = templateList.Select(t => t.TemplateName).ToList();
            return templateNames;
        }

        public bool TabExists(string serial, TabControl MainTab)
        {
            return MainTab.TabPages.Cast<TabPage>().Any(tab => tab.Text == serial);
        }

        public void PopulateDropdownTemplateNames(string index, List<Template> templateList)
        {
            List<string> templateNames = templateList.Select(t => t.TemplateName).ToList();
        }
    }
}
