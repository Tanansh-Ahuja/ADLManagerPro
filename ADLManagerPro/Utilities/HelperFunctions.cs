using ADLManager;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using tt_net_sdk;

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
            try
            {
                List<string> templateNames = templateList.Select(t => t.TemplateName).ToList();
                return templateNames;

            }
            catch
            {
                MessageBox.Show("Error occured while getting template name. Shutting down.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ShutEverythingDown();
                return null;
            }
        }

        public bool TabExists(string serial, TabControl MainTab)
        {
            try
            {
                return MainTab.TabPages.Cast<TabPage>().Any(tab => tab.Text == serial);

            }
            catch
            {
                MessageBox.Show("Error occured whilechecking if tab exists. Shutting down.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ShutEverythingDown();
                return false;
            }
        }

        public void PopulateDropdownTemplateNames(string index, List<Template> templateList)
        {
            try
            {
                List<string> templateNames = templateList.Select(t => t.TemplateName).ToList();

            }
            catch
            {
                MessageBox.Show("Error occured while populating template. Shutting down.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ShutEverythingDown();
            }
        }

        public void UpdateTemplate(Template existingTemplate,string templateName, DataGridView paramGrid, string adlName, List<Template> templates)
        {
            try
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
                                else
                                {
                                    existingTemplate.ParamNameWithValue.Add(paramName, paramValue);
                                }
                            }
                        }

                        // Update dictionary
                        if(Globals.algoNameWithTemplateList.ContainsKey(adlName))
                        {
                            Globals.algoNameWithTemplateList.Remove(adlName);
                            Globals.algoNameWithTemplateList.Add(adlName, templates);
                        }
                        else
                        {
                            Globals.algoNameWithTemplateList.Add(adlName, templates);
                        }
                        

                        // Save to file
                        _fileHandlers.SaveTemplateDictionaryToFile(Globals.algoNameWithTemplateList);

                        MessageBox.Show("Template updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }

                    return;
                }

            }
            catch
            {
                MessageBox.Show("Error occured while updating template. Shutting down.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ShutEverythingDown();
            }
        }

        public void PopulateEveryComboBoxInTabs(TabControl MainTab, string adlName, string newTemplateName, string tabName)
        {
            try
            {
                //Update the combobox of all other tabs where the same Algo is there
                foreach (TabPage tabPage in MainTab.TabPages)
                {
                    //if (tabPage.Text == tabName)
                    //    continue;

                    if(Globals.tabNameWithTabInfo.ContainsKey(tabPage.Text) && Globals.tabNameWithTabInfo[tabPage.Text]._adlName == adlName )
                    {
                        Control[] foundControlTextBox = tabPage.Controls.Find("TemplateTextBox", true);
                        Control[] foundControlComboBox = tabPage.Controls.Find("TemplateComboBox", true);
                        if (foundControlTextBox.Length > 0 && foundControlTextBox[0] is TextBox textBox)
                        {
                            if (foundControlComboBox.Length > 0 && foundControlComboBox[0] is ComboBox comboBox)
                            {
                                string templateName = string.Empty;
                                if (comboBox.SelectedItem != null)
                                {
                                    templateName = comboBox.SelectedItem.ToString();

                                }
                                comboBox.Items.Clear();
                                if (Globals.algoNameWithTemplateList.ContainsKey(adlName))
                                {
                                    comboBox.Items.AddRange(GetTemplateNames(Globals.algoNameWithTemplateList[adlName]).ToArray());
                                }
                                // Select the desired item
                                if (templateName != string.Empty && comboBox.Items.Contains(newTemplateName))
                                {
                                    if(tabPage.Text == tabName)
                                    {
                                        comboBox.SelectedItem = newTemplateName;
                                    }
                                    else
                                    {

                                        comboBox.SelectedItem = templateName;
                                    }
                                }
                            }  
                        }
                    }
                
                }


            }
            catch
            {
                MessageBox.Show("Error occured while populating every combobox in other tabs. Shutting down.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ShutEverythingDown();
            }
        }

        public void PopulateParamGridWithOrderProfileParameters(DataGridView paramGrid,string adlValue)
        {
            try
            {
                if(Globals.algoNameWithParameters.ContainsKey(adlValue))
                foreach (var (paramName, paramType) in Globals.algoNameWithParameters[adlValue]._adlOrderProfileParametersWithType)
                {
                    int rowIndex = paramGrid.Rows.Add(paramName, paramType);
                    if(paramName.Contains("Account"))
                    {
                        var combocell = new DataGridViewComboBoxCell();
                        combocell.Items.AddRange(Globals._accounts.ToArray());
                        paramGrid.Rows[rowIndex].Cells["Value"] = combocell;
                        continue;
                    }

                    if (paramName.Contains("Instrument") && !paramName.Contains("Account"))
                    {
                        var combocell = new DataGridViewComboBoxCell();
                        if(Globals.instrumentNameWithInstrument.Keys.Count > 0)
                        {
                            combocell.Items.AddRange(Globals.instrumentNameWithInstrument.Keys.ToArray());
                            paramGrid.Rows[rowIndex].Cells["Value"] = combocell;
                        }
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
            catch
            {
                MessageBox.Show("Error occured while populating paramGrid with order profile parameters. Shutting down.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ShutEverythingDown();
            }

        }

        public void PopulateParamGridWithUserParameters(DataGridView paramGrid, string adlValue)
        {
            try
            {
                int rowIndex;
                if(Globals.algoNameWithParameters.ContainsKey(adlValue))
                {
                    var combocell = new DataGridViewComboBoxCell();
                    foreach (var (paramName, paramType) in Globals.algoNameWithParameters[adlValue]._adlUserParametersWithType)
                    {
                        rowIndex = paramGrid.Rows.Add(paramName, paramType);

                        if (paramName.Contains("Account"))
                        {
                            combocell = new DataGridViewComboBoxCell();
                            combocell.Items.AddRange(Globals._accounts.ToArray());
                            paramGrid.Rows[rowIndex].Cells["Value"] = combocell;
                            continue;
                        }

                        if (paramName.Contains("Instrument") && !paramName.Contains("Account"))
                        {
                            combocell = new DataGridViewComboBoxCell();
                            if(Globals.instrumentNameWithInstrument.Keys.Count > 0)
                            {
                                combocell.Items.AddRange(Globals.instrumentNameWithInstrument.Keys.ToArray());
                                paramGrid.Rows[rowIndex].Cells["Value"] = combocell;

                            }
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
                    //TODO : HARD CODE THE TWO PARAMETERS WE NEED
                    rowIndex = paramGrid.Rows.Add("CoLocation", ParameterType.String);
                    var coLocation = new DataGridViewComboBoxCell
                    {
                        DataSource = Enum.GetNames(typeof(MarketId)) // pure strings
                    };
                    paramGrid.Rows[rowIndex].Cells["Value"] = coLocation;
                    paramGrid.Rows[rowIndex].Cells["Value"].Value = Enum.GetNames(typeof(MarketId))[0];

                    rowIndex = paramGrid.Rows.Add("User Disconnection Action", ParameterType.String);
                    var orderType = new DataGridViewComboBoxCell
                    {
                        DataSource = Enum.GetNames(typeof(UserDisconnectAction)) // pure strings
                    };
                    paramGrid.Rows[rowIndex].Cells["Value"] = orderType;
                    paramGrid.Rows[rowIndex].Cells["Value"].Value = Enum.GetNames(typeof(UserDisconnectAction))[0];





                }


            }
            catch
            {
                MessageBox.Show("Error occured while populating paramGrid with user parameters. Shutting down.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ShutEverythingDown();
            }
        }

        public Template GenerateANewTemplate(string adlName, DataGridView paramGrid, string templateName)
        {
            try
            {
                if(!Globals.algoNameWithParameters.ContainsKey(adlName))
                {
                    return null;
                }
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

                    if (!string.IsNullOrEmpty(paramName))
                    {
                        newTemplate.ParamNameWithValue[paramName] = paramValue ?? "";
                    }
                }
                return newTemplate;

            }
            catch
            {
                MessageBox.Show("Error occured while generating a new template. Shutting down.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ShutEverythingDown();
                return null;
            }
        }

        public void ParamgridCellValueValidate(object sender, DataGridViewCellValidatingEventArgs e, DataGridView paramGrid,string adlValue)
        {
            try
            {
                // Check if editing the "Value" column
                if (paramGrid.Columns[e.ColumnIndex].Name != "Value") return;

                string newValue = e.FormattedValue?.ToString() ?? "";
                if (newValue == "")
                    return;

                // Get corresponding ParamName from the same row
                string paramName = paramGrid.Rows[e.RowIndex].Cells["ParamName"].Value?.ToString() ?? "";
                if (paramName.Contains("Instrument"))
                    return;
                //if (Globals.SkipParamNames.Contains(paramName))
                //    return;
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
                        MessageBox.Show("Only whole numbers (0–9) allowed for this parameter.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        e.Cancel = true;
                    }
                }
                else if (paramType == ParameterType.Float)
                {
                    if (!System.Text.RegularExpressions.Regex.IsMatch(newValue, @"^\d+(\.\d{1,})?$"))
                    {
                        MessageBox.Show("Only numeric values with at most one decimal point allowed.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        e.Cancel = true;
                    }
                }

            }
            catch
            {
                MessageBox.Show("Error occured while validating cell value. Shutting down.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ShutEverythingDown();
            }
        }

        public static void OnFromTTAlgoOrderDeletion(string AlgoName, string orderKey)
        {
            try
            {
                if (!Globals.siteOrderKeyWithTabName.ContainsKey(orderKey) || !Globals.tabNameWithTabInfo.ContainsKey(Globals.siteOrderKeyWithTabName[orderKey]))
                {
                    return;
                }


                string currentTabName = Globals.siteOrderKeyWithTabName[orderKey];
                TabPage currentTab = Globals.tabNameWithTabInfo[currentTabName]._currentTab;
                if (Globals.tabNameWithSiteOrderKey.ContainsKey(currentTabName) && Globals.algoNameWithTradeSubscription.ContainsKey(AlgoName))
                {
                    Globals.tabNameWithSiteOrderKey[currentTabName] = Globals.algoNameWithTradeSubscription[AlgoName].DeleteAlgoOrder(orderKey);
                    Globals.tabNameWithSiteOrderKey.Remove(currentTabName);

                    if (Globals.siteOrderKeyWithTabName.ContainsKey(orderKey))
                    {
                        Globals.siteOrderKeyWithTabName.Remove(orderKey);
                    }
                    Control[] foundParamGrid = currentTab.Controls.Find("ParamGrid", true);
                    Control[] foundComboBox = currentTab.Controls.Find("TemplateComboBox", true);
                    Control[] foundTextBox = currentTab.Controls.Find("TemplateTextBox", true);
                    Control[] foundTemplateButton = currentTab.Controls.Find("SaveTemplateButton", true);
                    Control[] foundStatusLabel = currentTab.Controls.Find("OrderStatusValueLabel", true);
                    Control[] foundDeleteAlgoButton = currentTab.Controls.Find("DeleteAlgoButton", true);
                    Control[] foundStartAlgoButton = currentTab.Controls.Find("StartAlgoButton", true);
                    if (foundParamGrid.Length > 0 && foundParamGrid[0] is DataGridView dgv)
                    {
                        dgv.Columns["Value"].ReadOnly = false;
                    }
                    if (foundComboBox.Length > 0)
                    {
                        foundComboBox[0].Show();
                    }
                    if (foundTextBox.Length > 0)
                    {
                        foundTextBox[0].Show();
                    }
                    if (foundTemplateButton.Length > 0)
                    {
                        foundTemplateButton[0].Show();
                    }
                    if (foundStatusLabel.Length > 0)
                    {
                        foundStatusLabel[0].Text = "DEACTIVATED";
                    }
                    if (foundDeleteAlgoButton.Length > 0)
                    {
                        foundDeleteAlgoButton[0].Hide();
                    }
                    if (foundStartAlgoButton.Length > 0)
                    {
                        foundStartAlgoButton[0].Show();
                    }
                    //mainGrid
                    string currentTabIndex = currentTabName.Split('-')[0];
                    int rowIndex = Convert.ToInt32(currentTabIndex) - 1;
                    Form1.mainGrid.Rows[rowIndex].Cells[Globals.columnZeroName].ReadOnly = false;
                    Form1.mainGrid.Rows[rowIndex].Cells[Globals.columnFourName].ReadOnly = false;
                    Form1.mainGrid.Rows[rowIndex].Cells[Globals.columnFiveName].Value = "DEACTIVATED";
                    var row = Form1.mainGrid.Rows[rowIndex];
                    foreach (DataGridViewCell cell in row.Cells)
                    {
                        cell.Style.BackColor = Color.Empty;
                        cell.Style.ForeColor = Color.Empty;
                    }


                }
            }
            catch
            {
                MessageBox.Show("Error occured while TT order algo deletion. Shutting down.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ShutEverythingDown();
            }

        }

        public static void ShutEverythingDown()
        {
            MessageBox.Show("Inside Shut Everything down", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

        }


    }
}
