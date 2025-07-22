using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ADLManager;

namespace ADLManagerPro
{
    public class HelperFunctions
    {
        FileHandlers _fileHandlers = null;
        public HelperFunctions() 
        {
            _fileHandlers = new FileHandlers();
        }

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

        public void UpdateTemplate(Template existingTemplate,string templateName, DataGridView paramGrid, string adlName, List<Template> templates)
        {
            if (existingTemplate != null)
            {
                // Template exists, confirm update
                DialogResult result = MessageBox.Show(
                    $"Do you want to update the template '{templateName}'?",
                    "Confirm Update",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );

                if (result == DialogResult.Yes)
                {
                    // Update values only, not param types
                    foreach (DataGridViewRow row in paramGrid.Rows)
                    {
                        if (row.IsNewRow) continue;

                        string paramName = row.Cells["ParamName"].Value?.ToString();
                        string paramValue = row.Cells["Value"].Value?.ToString();


                        if (!string.IsNullOrEmpty(paramName))
                        {
                            if (existingTemplate.ParamNameWithValue.ContainsKey(paramName))
                            {
                                var old = existingTemplate.ParamNameWithValue[paramName];
                                existingTemplate.ParamNameWithValue[paramName] = paramValue;
                            }
                        }
                    }

                    // Update dictionary
                    Globals.algoNameWithTemplateList.Remove(adlName);
                    Globals.algoNameWithTemplateList.Add(adlName, templates);

                    // Save to file
                    _fileHandlers.SaveTemplateDictionaryToFile(Globals.algoNameWithTemplateList);

                    MessageBox.Show("Template updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                return;
            }
        }

        public void PopulateEveryComboBoxInTabs(TabControl MainTab, string adlName, string adlValue)
        {
            //Update the combobox of all other tabs where the same Algo is there
            foreach (TabPage tabPage in MainTab.TabPages)
            {
                bool moveForward = false;
                foreach (Control control in tabPage.Controls)
                {
                    if (control is Label lb && lb.Name == "Adl Value" && lb.Text == adlName)
                    {
                        moveForward = true;
                        break;
                    }
                }
                if (!moveForward)
                    continue;

                //TODO : Update the combobox for each tab 
                // Try to find the ComboBox inside the tab (assuming there's one per tab)
                foreach (Control control in tabPage.Controls)
                {
                    if (control is ComboBox comboBox)
                    {
                        // Optional: refresh or repopulate items here
                        object currentValueObj = comboBox.SelectedItem;
                        string currentValue;
                        if (currentValueObj == null)
                        {
                            currentValue = "";
                        }
                        else
                        {
                            currentValue = currentValueObj.ToString();
                        }

                        comboBox.Items.Clear();
                        comboBox.Items.AddRange(GetTemplateNames(Globals.algoNameWithTemplateList[adlValue]).ToArray());

                        // Select the desired item
                        if (comboBox.Items.Contains(currentValue))
                        {
                            comboBox.SelectedItem = currentValue;
                        }
                    }
                }
            }
        }

        public void PopulateParamGridWithOrderProfileParameters(DataGridView paramGrid,string adlValue)
        {
            foreach (var (paramName, paramType) in Globals.algoNameWithParameters[adlValue]._adlOrderProfileParametersWithType)
            {
                int rowIndex = paramGrid.Rows.Add(paramName, paramType);

                if (paramName.Equals("Quoting Instrument Account", StringComparison.OrdinalIgnoreCase) ||
                    paramName.Equals("Fast Mkt Inst Account", StringComparison.OrdinalIgnoreCase) ||
                    paramName.Equals("Hedge Instrument Account", StringComparison.OrdinalIgnoreCase))
                {
                    var combocell = new DataGridViewComboBoxCell();
                    combocell.Items.AddRange(Globals._accounts.ToArray());
                    paramGrid.Rows[rowIndex].Cells["Value"] = combocell;
                    continue;
                }

                if (paramName.Equals("Quoting Instrument", StringComparison.OrdinalIgnoreCase) ||
                    paramName.Equals("Fast Mkt Inst", StringComparison.OrdinalIgnoreCase) ||
                    paramName.Equals("Hedge Instrument", StringComparison.OrdinalIgnoreCase))
                {
                    var combocell = new DataGridViewComboBoxCell();
                    combocell.Items.AddRange(Globals.instrumentNameWithInstrument.Keys.ToArray());
                    paramGrid.Rows[rowIndex].Cells["Value"] = combocell;
                    continue;
                }

                if (paramType == ParameterType.BuySell)
                {
                    var comboCell = new DataGridViewComboBoxCell();
                    comboCell.Items.AddRange("Buy", "Sell");
                    paramGrid.Rows[rowIndex].Cells["Value"] = comboCell;
                }
                else if (paramType == ParameterType.Bool)
                {
                    var comboCell = new DataGridViewComboBoxCell();
                    comboCell.Items.AddRange("True", "False");
                    paramGrid.Rows[rowIndex].Cells["Value"] = comboCell;
                }
                else
                {
                    Console.WriteLine("OrderProfile paramName: " + paramName + " paramType: " + paramType);
                    var textCell = new DataGridViewTextBoxCell();
                    paramGrid.Rows[rowIndex].Cells["Value"] = textCell;
                }
            }
        }

        public void PopulateParamGridWithUserParameters(DataGridView paramGrid, string adlValue)
        {
            foreach (var (paramName, paramType) in Globals.algoNameWithParameters[adlValue]._adlUserParametersWithType)
            {
                int rowIndex = paramGrid.Rows.Add(paramName, paramType);

                if (paramName.Equals("Quoting Instrument Account", StringComparison.OrdinalIgnoreCase) ||
                    paramName.Equals("Fast Mkt Inst Account", StringComparison.OrdinalIgnoreCase) ||
                    paramName.Equals("Hedge Instrument Account", StringComparison.OrdinalIgnoreCase))
                {
                    var combocell = new DataGridViewComboBoxCell();
                    combocell.Items.AddRange(Globals._accounts.ToArray());
                    paramGrid.Rows[rowIndex].Cells["Value"] = combocell;
                    continue;
                }

                if (paramName.Equals("Quoting Instrument", StringComparison.OrdinalIgnoreCase) ||
                    paramName.Equals("Fast Mkt Inst", StringComparison.OrdinalIgnoreCase) ||
                    paramName.Equals("Hedge Instrument", StringComparison.OrdinalIgnoreCase))
                {
                    var combocell = new DataGridViewComboBoxCell();
                    combocell.Items.AddRange(Globals.instrumentNameWithInstrument.Keys.ToArray());
                    paramGrid.Rows[rowIndex].Cells["Value"] = combocell;
                    continue;
                }

                if (paramType == ParameterType.BuySell)
                {
                    var comboCell = new DataGridViewComboBoxCell();
                    comboCell.Items.AddRange("Buy", "Sell");
                    paramGrid.Rows[rowIndex].Cells["Value"] = comboCell;
                }
                else if (paramType == ParameterType.Bool)
                {
                    var comboCell = new DataGridViewComboBoxCell();
                    comboCell.Items.AddRange("True", "False");
                    paramGrid.Rows[rowIndex].Cells["Value"] = comboCell;
                }
                else
                {
                    Console.WriteLine("UserParams paramName: " + paramName + " paramType: " + paramType);
                    var textCell = new DataGridViewTextBoxCell();
                    paramGrid.Rows[rowIndex].Cells["Value"] = textCell;
                }
            }
        }
    }
}
