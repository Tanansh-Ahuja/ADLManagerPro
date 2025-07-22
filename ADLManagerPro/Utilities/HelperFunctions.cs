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

        public void PopulateEveryComboBoxInTabs(TabControl MainTab, string adlName, string newTemplateName)
        {
            //Update the combobox of all other tabs where the same Algo is there
            foreach (TabPage tabPage in MainTab.TabPages)
            {
                if(Globals.tabIndexWithTabInfo.ContainsKey(tabPage.Name) && Globals.tabIndexWithTabInfo[tabPage.Name]._adlName == adlName )
                {
                    Control[] foundControl = tabPage.Controls.Find("TemplateComboBox", true);
                    if (foundControl.Length > 0 && foundControl[0] is ComboBox comboBox)
                    {
                        comboBox.Items.Clear();
                        comboBox.Items.AddRange(GetTemplateNames(Globals.algoNameWithTemplateList[adlName]).ToArray());

                        // Select the desired item
                        if (comboBox.Items.Contains(newTemplateName))
                        {
                            comboBox.SelectedItem = newTemplateName;
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

        public Template GenerateANewTemplate(string adlName, DataGridView paramGrid, string templateName)
        {
            //We have the adl name already, so we will add to the given json
            AdlParameters adlParams = Globals.algoNameWithParameters[adlName];
            Dictionary<string, ParameterType> paramTypes = null;
            if (Globals.algoWithParamNameWithParamType.ContainsKey(adlName))
            {
                paramTypes = Globals.algoWithParamNameWithParamType[adlName];
            }
            else
            {
                MessageBox.Show("ADL Parameters type not able to fetch", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }


            // New Template
            Template newTemplate = new Template
            {
                TemplateName = templateName,
                ParamNameWithValue = new Dictionary<string, string>()
            };
            // Iterate over paramGrid rows to populate parameters
            foreach (DataGridViewRow row in paramGrid.Rows)
            {
                if (row.IsNewRow) continue;

                string paramName = row.Cells["ParamName"].Value?.ToString();
                string paramValue = row.Cells["Value"].Value?.ToString();

                if (!string.IsNullOrEmpty(paramName) && paramTypes.ContainsKey(paramName))
                {
                    newTemplate.ParamNameWithValue[paramName] = paramValue ?? "";
                }
            }
            return newTemplate;
        }

        public void ParamgridCellValueValidate(object sender, DataGridViewCellValidatingEventArgs e, DataGridView paramGrid,string adlValue)
        {
            // Check if editing the "Value" column
            if (paramGrid.Columns[e.ColumnIndex].Name != "Value") return;

            string newValue = e.FormattedValue?.ToString() ?? "";
            if (newValue == "")
                return;

            // Get corresponding ParamName from the same row
            string paramName = paramGrid.Rows[e.RowIndex].Cells["ParamName"].Value?.ToString() ?? "";
            if (Globals.SkipParamNames.Contains(paramName))
                return;
            // Skip if paramName is not found
            if (string.IsNullOrWhiteSpace(paramName)) return;

            // Replace this with your actual algoName
            string algoName = adlValue;

            if (!Globals.algoWithParamNameWithParamType.TryGetValue(algoName, out var paramMap)) return;
            if (!paramMap.TryGetValue(paramName, out var paramType)) return;

            // Now validate based on paramType
            if (paramType == ParameterType.Int)
            {
                if (!System.Text.RegularExpressions.Regex.IsMatch(newValue, @"^\d+$"))
                {
                    MessageBox.Show("Only whole numbers (0–9) allowed for this parameter.");
                    e.Cancel = true;
                }
            }
            else if (paramType == ParameterType.Float)
            {
                if (!System.Text.RegularExpressions.Regex.IsMatch(newValue, @"^\d+(\.\d{1,})?$"))
                {
                    MessageBox.Show("Only numeric values with at most one decimal point allowed.");
                    e.Cancel = true;
                }
            }
        }

        
    }
}
