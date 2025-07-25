using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ADLManager;
using tt_net_sdk;

namespace ADLManagerPro
{
    public class ButtonEvents
    {
        private static FileHandlers _fileHandlers = null;
        private static HelperFunctions _helperFunctions = null;
        public ButtonEvents()
        {
            _fileHandlers = new FileHandlers();
            _helperFunctions = new HelperFunctions();
        }
        public void AddRowInMainGrid(DataGridView mainGrid)
        {
            int serialNumber = mainGrid.Rows.Count + 1;
            mainGrid.Rows.Add(false, serialNumber);
            mainGrid.Rows[serialNumber - 1].Cells[Globals.columnFiveName].Value = "DEACTIVATED";
        }

        public void DeleteRowsInMainGrid(object sender, EventArgs e, DataGridView mainGrid, TabControl MainTab)
        {
            if (Globals.selectedRowIndexList.Count == 0) return;

            Globals.selectedRowIndexList.Sort();

            for (int i = 0; i < Globals.selectedRowIndexList.Count; i++)
            {
                int index_to_delete = Globals.selectedRowIndexList[i];
                DataGridViewRow rowToRemove = mainGrid.Rows[index_to_delete - i];
                mainGrid.Rows[index_to_delete - i].Cells[Globals.columnFourName].Value = false;
                mainGrid.Rows.Remove(rowToRemove);
            }
            Globals.selectedRowIndexList.Clear();
           

            Dictionary<string, string> map = new Dictionary<string, string>();
            //         old,new

            for (int i = mainGrid.Rows.Count - 1; i >= 0; i--)
            {
                int x = i + 1;
                map[mainGrid.Rows[i].Cells[Globals.columnOneName].Value.ToString()] = x.ToString();
                mainGrid.Rows[i].Cells[Globals.columnOneName].Value = x.ToString();
            }


            Dictionary<string, TabInfo> temp_tabIndexWithTabInfo = new Dictionary<string, TabInfo>();
            foreach (KeyValuePair<string, TabInfo> entry in Globals.tabIndexWithTabInfo)
            {
                string old_key = entry.Key;
                if (map.ContainsKey(old_key))
                {
                    string new_key = map[old_key];
                    temp_tabIndexWithTabInfo[new_key] = entry.Value;
                }

            }
            Globals.tabIndexWithTabInfo.Clear();
            Globals.tabIndexWithTabInfo = temp_tabIndexWithTabInfo.ToDictionary(
                                entry => entry.Key,
                                entry => entry.Value // still shallow copy of value
                            );
            temp_tabIndexWithTabInfo.Clear();

            Dictionary<string, string> temp_tabIndexWithSiteOrderKey = new Dictionary<string, string>();
            foreach (KeyValuePair<string, string> entry in Globals.tabIndexWithSiteOrderKey)
            {
                string old_key = entry.Key;
                if (map.ContainsKey(old_key))
                {
                    string new_key = map[old_key];
                    temp_tabIndexWithSiteOrderKey[new_key] = entry.Value;
                }

            }
            Globals.tabIndexWithSiteOrderKey.Clear();
            Globals.tabIndexWithSiteOrderKey = temp_tabIndexWithSiteOrderKey.ToDictionary(
                                entry => entry.Key,
                                entry => entry.Value // still shallow copy of value
                            );
            temp_tabIndexWithSiteOrderKey.Clear();
            
            
            Globals.siteOrderKeyWithTabIndex.Clear();
            Globals.siteOrderKeyWithTabIndex = Globals.tabIndexWithSiteOrderKey.ToDictionary(
                                entry => entry.Value,
                                entry => entry.Key);




            string curr_index;
            for (int i = MainTab.TabPages.Count - 1; i > 0; i--)
            {
                curr_index = MainTab.TabPages[i].Text;
                if (map.ContainsKey(curr_index))
                {
                    MainTab.TabPages[i].Text = map[curr_index].ToString();
                }
            }

        }

        public void OnStartbtnClick(DataGridView paramGrid, string AlgoName, string currentTabIndex, TabPage currentTab)
        {
            
            if (Globals.tabIndexWithSiteOrderKey.ContainsKey(currentTabIndex))
            {
                MessageBox.Show("Order already placed!");
                return;
            }

            
            Dictionary<string, object> algo_userparams = new Dictionary<string, object>();
            Dictionary<string, object> algo_orderprofileparams = new Dictionary<string, object>();
            string instrumentName = string.Empty;
            string instrumentId = string.Empty;
            int accountNumber = -1;
            foreach (DataGridViewRow row in paramGrid.Rows)
            {
                if (row.IsNewRow) continue; // skip any placeholder row

                string paramName = row.Cells["ParamName"].Value?.ToString()?.Trim();
                var valueCell = row.Cells["Value"];

                if (valueCell != null)
                {

                    if (string.IsNullOrEmpty(paramName)) continue;

                    object value = null;

                    // Handle cell type: TextBox, ComboBox, CheckBox
                    if (valueCell is DataGridViewTextBoxCell || valueCell is DataGridViewComboBoxCell)
                    {
                        value = valueCell.Value;
                        if (value == null || (value is string && string.IsNullOrWhiteSpace((string)value)))
                        {
                            MessageBox.Show("Please enter all the parameters before starting the algo.");
                            return;
                        }
                    }
                    else if (valueCell is DataGridViewCheckBoxCell && valueCell.Value != null)
                    {
                        value = Convert.ToBoolean(valueCell.Value);
                    }


                    if (Globals.algoNameWithParameters.ContainsKey(AlgoName))
                    {
                        if (paramName == "Quoting Instrument Account")
                        {
                            accountNumber = Globals._accounts.IndexOf(value.ToString());
                            if (accountNumber > -1)
                                algo_userparams[paramName] = accountNumber;
                            else
                                MessageBox.Show("Error fetching account index from account name");
                                
                        }
                        if (paramName.Contains("Instrument") && !paramName.Contains("Account"))
                        {
                            instrumentName = value.ToString();
                            if(Globals.instrumentNameWithInstrument.ContainsKey(instrumentName))
                            {
                                value = Globals.instrumentNameWithInstrument[instrumentName].InstrumentDetails.Id.ToString();
                            }

                        }

                        if (Globals.algoNameWithParameters[AlgoName]._adlUserParameters.Contains(paramName))
                        {
                            if(paramName.Contains("Account"))
                            {
                                value = Globals.m_accounts.ElementAt(Globals._accounts.IndexOf(value.ToString())).AccountId;
                            }
                            algo_userparams[paramName] = value;
                        }
                        else if (Globals.algoNameWithParameters[AlgoName]._adlOrderProfileParameters.Contains(paramName))
                        {
                            algo_orderprofileparams[paramName] = value;
                        }
                        else
                        {
                            Console.WriteLine("ERROR: unknown param: " + paramName + " value: " + value);
                        }
                    }


                }
                else
                {
                    MessageBox.Show("Please enter all the parameters before starting the algo.");
                    return;
                }
            }

            

            if (accountNumber >= 0 &&
                Globals.algoNameWithTradeSubscription.ContainsKey(AlgoName) &&
                Globals.instrumentNameWithInstrument.ContainsKey(instrumentName))
            {
                string orderKey = Globals.algoNameWithTradeSubscription[AlgoName].StartAlgo(accountNumber,
                        Globals.instrumentNameWithInstrument[instrumentName],
                        algo_userparams,
                        algo_orderprofileparams);

                if (!Globals.tabIndexWithSiteOrderKey.ContainsKey(currentTabIndex))
                {
                    Globals.tabIndexWithSiteOrderKey.Add(currentTabIndex, orderKey);
                }
                else
                {
                    Globals.tabIndexWithSiteOrderKey[currentTabIndex] = orderKey;
                }

                if (!Globals.siteOrderKeyWithTabIndex.ContainsKey(orderKey))
                {
                    Globals.siteOrderKeyWithTabIndex.Add(orderKey, currentTabIndex);
                }
                else
                {
                    Globals.siteOrderKeyWithTabIndex[orderKey] = currentTabIndex;
                }
                paramGrid.Columns["Value"].ReadOnly = true;
                Control[] foundComboBox = currentTab.Controls.Find("TemplateComboBox", true);
                Control[] foundTextBox = currentTab.Controls.Find("TemplateTextBox", true);
                Control[] foundTemplateButton = currentTab.Controls.Find("SaveTemplateButton", true);
                Control[] foundStatusLabel = currentTab.Controls.Find("OrderStatusValueLabel", true);
                Control[] foundDeleteAlgoButton = currentTab.Controls.Find("DeleteAlgoButton", true);
                Control[] foundStartAlgoButton = currentTab.Controls.Find("StartAlgoButton", true);
                if (foundComboBox.Length > 0)
                {
                    foundComboBox[0].Hide();
                }
                if (foundTextBox.Length > 0)
                {
                    foundTextBox[0].Hide();
                }
                if (foundTemplateButton.Length > 0)
                {
                    foundTemplateButton[0].Hide();
                }
                if (foundStatusLabel.Length > 0)
                {
                    foundStatusLabel[0].Text = "ACTIVATED";
                }
                if (foundDeleteAlgoButton.Length > 0)
                {
                    foundDeleteAlgoButton[0].Show();
                }
                if (foundStartAlgoButton.Length > 0)
                {
                    foundStartAlgoButton[0].Hide();
                }



                //mainGrid
                int rowIndex = Convert.ToInt32(currentTabIndex)-1;
                Form1.mainGrid.Rows[rowIndex].Cells[Globals.columnZeroName].ReadOnly = true;
                Form1.mainGrid.Rows[rowIndex].Cells[Globals.columnFourName].ReadOnly = true;
                Form1.mainGrid.Rows[rowIndex].Cells[Globals.columnFiveName].Value = "ACTIVATED" ;
                var row = Form1.mainGrid.Rows[rowIndex];
                foreach (DataGridViewCell cell in row.Cells)
                {
                    cell.Style.BackColor = Color.Gray;
                    cell.Style.ForeColor = Color.White;
                }


            }
            else
            {
                MessageBox.Show("Please check the parameters.");
                return;
            }

        }


        public void OnDeletebtnClick(DataGridView paramGrid, string AlgoName, string currentTabIndex, TabPage currentTab)
        {
            if (!Globals.tabIndexWithSiteOrderKey.ContainsKey(currentTabIndex)
                || (Globals.tabIndexWithSiteOrderKey.ContainsKey(currentTabIndex) && Globals.tabIndexWithSiteOrderKey[currentTabIndex] == string.Empty))
            {
                MessageBox.Show("Order not found.");
                return;
            }
            
            if (Globals.tabIndexWithSiteOrderKey.ContainsKey(currentTabIndex) && Globals.algoNameWithTradeSubscription.ContainsKey(AlgoName))
            {
                string orderKey = Globals.tabIndexWithSiteOrderKey[currentTabIndex];
                Globals.tabIndexWithSiteOrderKey[currentTabIndex] = Globals.algoNameWithTradeSubscription[AlgoName].DeleteAlgoOrder(orderKey);
                Globals.tabIndexWithSiteOrderKey.Remove(currentTabIndex);
                if(Globals.siteOrderKeyWithTabIndex.ContainsKey(orderKey))
                {
                    Globals.siteOrderKeyWithTabIndex.Remove(orderKey);
                }
                
                paramGrid.Columns["Value"].ReadOnly = false;
                Control[] foundComboBox = currentTab.Controls.Find("TemplateComboBox", true);
                Control[] foundTextBox = currentTab.Controls.Find("TemplateTextBox", true);
                Control[] foundTemplateButton = currentTab.Controls.Find("SaveTemplateButton", true);
                Control[] foundStatusLabel = currentTab.Controls.Find("OrderStatusValueLabel", true);
                Control[] foundDeleteAlgoButton = currentTab.Controls.Find("DeleteAlgoButton", true);
                Control[] foundStartAlgoButton = currentTab.Controls.Find("StartAlgoButton", true);
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





        public void OnSaveOrUpdateTemplateBtnClick(object s, EventArgs e, TextBox txtTemplateName, string adlValue,DataGridView paramGrid, ComboBox savedTemplates,TabControl MainTab)
        {
           
            //check template name is not null
            string templateName = txtTemplateName.Text.Trim();
            if (string.IsNullOrWhiteSpace(templateName))
            {
                MessageBox.Show("Template name cannot be empty.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }


            string adlName = adlValue;
            // Get selected ADL name and check if adl name is not null
            if (string.IsNullOrEmpty(adlName))
            {
                MessageBox.Show("Invalid ADL selection.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if(Globals.algoNameWithTemplateList == null)
            {
                // Create new template
                Template newTemplate = _helperFunctions.GenerateANewTemplate(adlName, paramGrid, templateName);

                // Create new list and add to the dictionary
                var newList = new List<Template> { newTemplate };
                Globals.algoNameWithTemplateList = new Dictionary<string, List<Template>>();
                Globals.algoNameWithTemplateList.Add(adlName, newList);

                // Save to file
                _fileHandlers.SaveTemplateDictionaryToFile(Globals.algoNameWithTemplateList);
                txtTemplateName.Text = newTemplate.TemplateName;
                _helperFunctions.PopulateEveryComboBoxInTabs(MainTab, adlName, txtTemplateName.Text);

                MessageBox.Show("Template created and saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;

            }
            if(Globals.algoNameWithTemplateList.ContainsKey(adlName))
            {
                List<Template> templates = Globals.algoNameWithTemplateList[adlName];
                Template existingTemplate = templates.FirstOrDefault(t => t.TemplateName == templateName); //try to find template with same name
                if (existingTemplate != null)
                {
                    //same name template exists
                    _helperFunctions.UpdateTemplate(existingTemplate, templateName, paramGrid, adlName, templates);
                    return;
                }

                // Get the parameter definitions for this ADL
                
                Template newTemplate = _helperFunctions.GenerateANewTemplate(adlName, paramGrid, templateName);
                // Add or update the template in dictionary
                if (Globals.algoNameWithTemplateList.ContainsKey(adlName))
                {
                    var existingList = Globals.algoNameWithTemplateList[adlName];
                    existingList.Add(newTemplate);
                    Globals.algoNameWithTemplateList.Remove(adlName);
                    Globals.algoNameWithTemplateList.Add(adlName, existingList);
                }
                _fileHandlers.SaveTemplateDictionaryToFile(Globals.algoNameWithTemplateList);
                txtTemplateName.Text = newTemplate.TemplateName;
                _helperFunctions.PopulateEveryComboBoxInTabs(MainTab, adlName, txtTemplateName.Text);
                MessageBox.Show("Template saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                

            }
            else
            {
                // Create new template
                Template newTemplate = _helperFunctions.GenerateANewTemplate(adlName, paramGrid, templateName);

                // Create new list and add to the dictionary
                var newList = new List<Template> { newTemplate };
                Globals.algoNameWithTemplateList.Add(adlName, newList);

                // Save to file
                _fileHandlers.SaveTemplateDictionaryToFile(Globals.algoNameWithTemplateList);
                txtTemplateName.Text = newTemplate.TemplateName;
                _helperFunctions.PopulateEveryComboBoxInTabs(MainTab, adlName, txtTemplateName.Text);

                MessageBox.Show("Template created and saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;

            }

        }


        public void SavedTemplatesIndexChanged(object s, EventArgs e,ComboBox savedTemplates, TextBox txtTemplateName,string adlValue, DataGridView paramGrid)
        {
            string selectedTemplateName = savedTemplates.SelectedItem?.ToString();
            txtTemplateName.Text = selectedTemplateName;

            if (string.IsNullOrEmpty(selectedTemplateName) || string.IsNullOrEmpty(adlValue))
                return;

            // Step 1: Fetch the correct Template object
            if (!Globals.algoNameWithTemplateList.TryGetValue(adlValue, out List<Template> templatesForAlgo))
                return;

            var selectedTemplate = templatesForAlgo.FirstOrDefault(t => t.TemplateName == selectedTemplateName);
            if (selectedTemplate == null)
                return;

            // Step 2: Populate paramGrid with values from the template
            foreach (DataGridViewRow row in paramGrid.Rows)
            {
                if (row.IsNewRow) continue;

                string paramName = row.Cells["ParamName"].Value?.ToString();

                if (!string.IsNullOrEmpty(paramName) &&
                    selectedTemplate.ParamNameWithValue.TryGetValue(paramName, out var value))
                {
                    row.Cells["Value"].Value = value;  // Only set Value column
                }
            }
        }
    
    
    }
}
