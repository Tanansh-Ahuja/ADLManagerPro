using System;
using System.Collections.Generic;
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
            foreach (KeyValuePair<string, TabInfo> entry in temp_tabIndexWithTabInfo)
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

        public void OnStartbtnClick(DataGridView paramGrid, string AlgoName, string currentTab)
        {

            if (Globals.tabIndexWithSiteOrderKey.ContainsKey(currentTab))
            {
                MessageBox.Show("Order already placed!");
                return;
            }

            
            Dictionary<string, object> algo_userparams = new Dictionary<string, object>();
            Dictionary<string, object> algo_orderprofileparams = new Dictionary<string, object>();
            string instrumentName = null;
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
                        if (paramName == "Quoting Instrument")
                        {
                            instrumentName = value.ToString();

                        }

                        if (Globals.algoNameWithParameters[AlgoName]._adlUserParameters.Contains(paramName))
                        {
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

            DialogResult result = MessageBox.Show(
                "Are you sure you want to place this order?",
                "Confirm sending order",
                MessageBoxButtons.OKCancel,
                MessageBoxIcon.Warning
            );

            if (result != DialogResult.OK)
            {
                // User clicked Cancel or closed the dialog — do nothing
                return;
            }

            if (accountNumber >= 0 &&
                Globals.algoNameWithTradeSubscription.ContainsKey(AlgoName) &&
                Globals.instrumentNameWithInstrument.ContainsKey(instrumentName))
            {
                string orderKey = Globals.algoNameWithTradeSubscription[AlgoName].StartAlgo(accountNumber,
                        Globals.instrumentNameWithInstrument[instrumentName],
                        algo_userparams,
                        algo_orderprofileparams);
                if (!Globals.tabIndexWithSiteOrderKey.ContainsKey(currentTab))
                {
                    Globals.tabIndexWithSiteOrderKey.Add(currentTab, orderKey);
                }
                else
                {
                    Globals.tabIndexWithSiteOrderKey[currentTab] = orderKey;
                }


            }
            else
            {
                MessageBox.Show("Please check the parameters.");
                return;
            }

        }


        public void OnDeletebtnClick(DataGridView paramGrid, string AlgoName, string currentTab)
        {
            if (!Globals.tabIndexWithSiteOrderKey.ContainsKey(currentTab)
                || (Globals.tabIndexWithSiteOrderKey.ContainsKey(currentTab) && Globals.tabIndexWithSiteOrderKey[currentTab] == string.Empty))
            {
                MessageBox.Show("Order not found.");
                return;
            }
            DialogResult result = MessageBox.Show(
                "Are you sure you want to delete this order?",
                "Confirm Deletion",
                MessageBoxButtons.OKCancel,
                MessageBoxIcon.Warning
            );

            if (result != DialogResult.OK)
            {
                // User clicked Cancel or closed the dialog — do nothing
                return;
            }
            // TODO: Complete implementation

            //foreach (DataGridViewRow row in mainGrid.Rows)
            //{
            //    var cellValue = row.Cells[Globals.columnOneName].Value;

            //}
            if (Globals.tabIndexWithSiteOrderKey.ContainsKey(currentTab) && Globals.algoNameWithTradeSubscription.ContainsKey(AlgoName))
            {
                string orderKey = Globals.tabIndexWithSiteOrderKey[currentTab];

                Globals.algoNameWithTradeSubscription[AlgoName].DeleteAlgoOrder(orderKey);
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
            

            List<Template> templates = Globals.algoNameWithTemplateList[adlName];

            Template existingTemplate = templates.FirstOrDefault(t => t.TemplateName == templateName); //try to find template with same name

            if (existingTemplate != null)
            {
                //same name template exists
                _helperFunctions.UpdateTemplate(existingTemplate, templateName,paramGrid,adlName,templates);
                return;
            }

            #region Adding new Template
            
            // Get the parameter definitions for this ADL
            if (Globals.algoNameWithParameters.ContainsKey(adlName))
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
                    return;
                }


                // New Template
                Template newTemplate = new Template
                {
                    TemplateName = templateName,
                    ParamNameWithTypeAndValue = new Dictionary<string, (string Type, string Value)>()
                };
                // Iterate over paramGrid rows to populate parameters
                foreach (DataGridViewRow row in paramGrid.Rows)
                {
                    if (row.IsNewRow) continue;

                    string paramName = row.Cells["ParamName"].Value?.ToString();
                    string paramValue = row.Cells["Value"].Value?.ToString();

                    if (!string.IsNullOrEmpty(paramName) && paramTypes.ContainsKey(paramName))
                    {
                        //TODO
                        newTemplate.ParamNameWithTypeAndValue[paramName] = (row.Cells["Value"].ValueType.Name, paramValue ?? "");
                    }
                }
                // Add or update the template in dictionary
                if (Globals.algoNameWithTemplateList.ContainsKey(adlName))
                {
                    var existingList = Globals.algoNameWithTemplateList[adlName];
                    existingList.Add(newTemplate);
                    Globals.algoNameWithTemplateList.Remove(adlName);
                    Globals.algoNameWithTemplateList.Add(adlName, existingList);
                }
                _fileHandlers.SaveTemplateDictionaryToFile(Globals.algoNameWithTemplateList);
                //Update the combo box in the tab we are in
                if (Globals.algoNameWithTemplateList.ContainsKey(adlValue))
                {
                    savedTemplates.Items.Clear();
                    savedTemplates.Items.AddRange(_helperFunctions.GetTemplateNames(Globals.algoNameWithTemplateList[adlValue]).ToArray());
                }
                
                savedTemplates.Refresh();
                if (savedTemplates.Items.Contains(newTemplate.TemplateName))
                {
                    savedTemplates.SelectedItem = newTemplate.TemplateName;
                }   

                txtTemplateName.Text = newTemplate.TemplateName;
                _helperFunctions.PopulateEveryComboBoxInTabs(MainTab, adlName, adlValue);
                MessageBox.Show("Template saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);


            }
            else
            {
                // need to make a new json for this specific adl
            }
            #endregion

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
                    selectedTemplate.ParamNameWithTypeAndValue.TryGetValue(paramName, out var paramData))
                {
                    row.Cells["Value"].Value = paramData.Value;  // Only set Value column
                }
            }
        }
    
    
    }
}
