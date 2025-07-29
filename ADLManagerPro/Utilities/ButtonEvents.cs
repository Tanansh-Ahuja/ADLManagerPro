using ADLManager;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.PeerToPeer;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
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
            try
            {
                int serialNumber = mainGrid.Rows.Count + 1;
                mainGrid.Rows.Add(false, serialNumber);
                mainGrid.Rows[serialNumber - 1].Cells[Globals.columnFiveName].Value = "DEACTIVATED";

            }
            catch
            {
                MessageBox.Show("Error occured while adding row in main table. Shutting down.");
                HelperFunctions.ShutEverythingDown();
            }
        }

        public void DeleteRowsInMainGrid(object sender, EventArgs e, DataGridView mainGrid, TabControl MainTab)
        {
            try
            {
                if (Globals.selectedRowIndexList.Count == 0) return;

                Globals.selectedRowIndexList.Sort();

                for (int i = 0; i < Globals.selectedRowIndexList.Count; i++)
                {
                    int index_to_delete = Globals.selectedRowIndexList[i];
                    DataGridViewRow rowToRemove = mainGrid.Rows[index_to_delete - i];
                    bool tabCreated = Convert.ToBoolean(mainGrid.Rows[index_to_delete - i].Cells[Globals.columnFourName].Value);
                    if (tabCreated)
                    {
                        mainGrid.Rows[index_to_delete - i].Cells[Globals.columnFourName].Value = false;
                    }
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

                foreach (string feedName in Globals.feedNames)
                {
                    if (Globals.feedNameWithRowIndex.ContainsKey(feedName))
                    {
                        List<string> temp = Globals.feedNameWithRowIndex[feedName];
                        List<string> new_indexes = new List<string>();
                        foreach(string old_index in temp)
                        {
                            if(map.ContainsKey(old_index))
                            {
                                string new_index = map[old_index];
                                new_indexes.Add(new_index);
                            }
                        }
                        Globals.feedNameWithRowIndex[feedName] = new_indexes;
                    }
                }

                Dictionary<string, TabInfo> temp_tabNameWithTabInfo = new Dictionary<string, TabInfo>();
                foreach (KeyValuePair<string, TabInfo> entry in Globals.tabNameWithTabInfo)
                {
                    TabInfo currentTabInfo = entry.Value;
                    string old_key = entry.Key;
                    string old_key_num = old_key.Split('-')[0];
                    string old_key_feed = old_key.Split('-')[1];

                    if (map.ContainsKey(old_key_num))
                    {
                        string new_key_num = map[old_key_num];
                        string new_tab_name = new_key_num + "-" + old_key_feed;
                        temp_tabNameWithTabInfo.Add(new_tab_name, entry.Value);
                        currentTabInfo._currentTab.Text = new_tab_name;

                    }

                }
                Globals.tabNameWithTabInfo.Clear();
                Globals.tabNameWithTabInfo = temp_tabNameWithTabInfo.ToDictionary(
                                    entry => entry.Key,
                                    entry => entry.Value // still shallow copy of value
                                );
                temp_tabNameWithTabInfo.Clear();


                Dictionary<string, string> temp_tabNameWithSiteOrderKey = new Dictionary<string, string>();
                foreach (KeyValuePair<string, string> entry in Globals.tabNameWithSiteOrderKey)
                {
                    string old_key = entry.Key;
                    string old_key_num = old_key.Split('-')[0];
                    string old_key_feed = old_key.Split('-')[1];
                    if (map.ContainsKey(old_key))
                    {
                        string new_key_num = map[old_key_num];
                        string new_tab_name = new_key_num + "-" + old_key_feed;
                        temp_tabNameWithSiteOrderKey.Add(new_tab_name, entry.Value);
                    }

                }
                Globals.tabNameWithSiteOrderKey.Clear();
                Globals.tabNameWithSiteOrderKey = temp_tabNameWithSiteOrderKey.ToDictionary(
                                    entry => entry.Key,
                                    entry => entry.Value // still shallow copy of value
                                );
                temp_tabNameWithSiteOrderKey.Clear();
            
            
                Globals.siteOrderKeyWithTabName.Clear();
                Globals.siteOrderKeyWithTabName = Globals.tabNameWithSiteOrderKey.ToDictionary(
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
            catch
            {
                MessageBox.Show("Error occured while deleting row from table. Shutting down.");
                HelperFunctions.ShutEverythingDown();
            }
        }

        public void OnStartbtnClick(DataGridView paramGrid, string AlgoName, TabPage currentTab)
        {
            

            try
            {
                string currentTabName = currentTab.Text;
                if (Globals.tabNameWithSiteOrderKey.ContainsKey(currentTabName))
                {
                    MessageBox.Show("Order already placed!");
                    return;
                }

            
                Dictionary<string, object> algo_userparams = new Dictionary<string, object>();
                Dictionary<string, object> algo_orderprofileparams = new Dictionary<string, object>();
                string instrumentName = string.Empty;
                string instrumentId = string.Empty;
                int accountNumber = -1;
                MarketId marketId = MarketId.NotSet;
                UserDisconnectAction userDisconnectAction = UserDisconnectAction.NotSet;
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

                        //Hardcoded values in all algos: CoLocation and User Disconnect Action
                        if(paramName == "CoLocation")
                        {
                            var cellVal = row.Cells["Value"].Value?.ToString();
                            marketId = (MarketId)Enum.Parse(typeof(MarketId), cellVal, ignoreCase: true);
                        }
                        else if(paramName == "User Disconnection Action")
                        {
                            var cellVal = row.Cells["Value"].Value?.ToString();
                            userDisconnectAction = (UserDisconnectAction)Enum.Parse(typeof(UserDisconnectAction), cellVal, ignoreCase: true);
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
                                if (Globals.instrumentNameWithInstrument.ContainsKey(instrumentName))
                                {
                                    value = Globals.instrumentNameWithInstrument[instrumentName].InstrumentDetails.Id.ToString();
                                }

                            }

                            if (Globals.algoNameWithParameters[AlgoName]._adlUserParameters.Contains(paramName))
                            {
                                if (paramName.Contains("Account"))
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
                                Console.WriteLine("NOTE: unknown param: " + paramName + " value: " + value);
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
                            algo_orderprofileparams,marketId,userDisconnectAction);

                    if (!Globals.tabNameWithSiteOrderKey.ContainsKey(currentTabName))
                    {
                        Globals.tabNameWithSiteOrderKey.Add(currentTabName, orderKey);
                    }
                    else
                    {
                        Globals.tabNameWithSiteOrderKey[currentTabName] = orderKey;
                    }

                    if (!Globals.siteOrderKeyWithTabName.ContainsKey(orderKey))
                    {
                        Globals.siteOrderKeyWithTabName.Add(orderKey, currentTabName);
                    }
                    else
                    {
                        Globals.siteOrderKeyWithTabName[orderKey] = currentTabName;
                    }

                    if (Globals.siteOrderKeyWithTabName.ContainsKey(orderKey) &&
                    Globals.tabNameWithTabInfo.ContainsKey(Globals.siteOrderKeyWithTabName[orderKey]))
                    {
                        TabInfo tabInfo = Globals.tabNameWithTabInfo[Globals.siteOrderKeyWithTabName[orderKey]];
                    
                        tabInfo._lag = true;
                        tabInfo._laggedPrice = double.NaN;
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
                    string currentTabIndex = currentTabName.Split('-')[0];
                    int rowIndex = Convert.ToInt32(currentTabIndex) - 1;
                    Form1.mainGrid.Rows[rowIndex].Cells[Globals.columnZeroName].ReadOnly = true;
                    Form1.mainGrid.Rows[rowIndex].Cells[Globals.columnFourName].ReadOnly = true;
                    Form1.mainGrid.Rows[rowIndex].Cells[Globals.columnFiveName].Value = "ACTIVATED";
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
            catch
            {
                MessageBox.Show("Error occured while click on start algo button. Shutting down.");
                HelperFunctions.ShutEverythingDown();
            }

        }


        public void OnDeletebtnClick(DataGridView paramGrid, string AlgoName, TabPage currentTab)
        {
            try
            {
                string currentTabName = currentTab.Text;
                if (!Globals.tabNameWithSiteOrderKey.ContainsKey(currentTabName)
                    || (Globals.tabNameWithSiteOrderKey.ContainsKey(currentTabName) && Globals.tabNameWithSiteOrderKey[currentTabName] == string.Empty))
                {
                    MessageBox.Show("Order not found.");
                    return;
                }
            
                if (Globals.tabNameWithSiteOrderKey.ContainsKey(currentTabName) && Globals.algoNameWithTradeSubscription.ContainsKey(AlgoName))
                {
                    string orderKey = Globals.tabNameWithSiteOrderKey[currentTabName];
                    Globals.tabNameWithSiteOrderKey[currentTabName] = Globals.algoNameWithTradeSubscription[AlgoName].DeleteAlgoOrder(orderKey);
                    Globals.tabNameWithSiteOrderKey.Remove(currentTabName);
                    if(Globals.siteOrderKeyWithTabName.ContainsKey(orderKey))
                    {
                        Globals.siteOrderKeyWithTabName.Remove(orderKey);
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
                MessageBox.Show("Error occured while delete button click. Shutting down.");
                HelperFunctions.ShutEverythingDown();
            }
        }

        public void OnSaveOrUpdateTemplateBtnClick(object s, EventArgs e, TextBox txtTemplateName, string adlValue,DataGridView paramGrid, ComboBox savedTemplates,TabControl MainTab)
        {
           
            try
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
            catch
            {
                MessageBox.Show("Error occured while saving template. Shutting down.");
                HelperFunctions.ShutEverythingDown();
            }
        }


        public void SavedTemplatesIndexChanged(object s, EventArgs e,ComboBox savedTemplates, TextBox txtTemplateName,string adlValue, DataGridView paramGrid)
        {
            try
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
            catch
            {
                MessageBox.Show("Error occured while changing template. Shutting down.");
                HelperFunctions.ShutEverythingDown();
            }
        }
    
    
    }
}
