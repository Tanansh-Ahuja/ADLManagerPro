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

        public void DeleteRowsInMainGrid(object sender, EventArgs e, List<int> selectedRowIndexList, DataGridView mainGrid, string columnOneName, string columnFourName, 
            Dictionary<string, TabInfo> tabIndexWithTabInfo, TabControl MainTab)
        {
            if (selectedRowIndexList.Count == 0) return;

            selectedRowIndexList.Sort();

            for (int i = 0; i < selectedRowIndexList.Count; i++)
            {
                int index_to_delete = selectedRowIndexList[i];
                DataGridViewRow rowToRemove = mainGrid.Rows[index_to_delete - i];
                mainGrid.Rows[index_to_delete - i].Cells[columnFourName].Value = false;
                mainGrid.Rows.Remove(rowToRemove);
            }
            selectedRowIndexList.Clear();


            Dictionary<string, string> map = new Dictionary<string, string>();
            //         old,new

            for (int i = mainGrid.Rows.Count - 1; i >= 0; i--)
            {
                int x = i + 1;
                map[mainGrid.Rows[i].Cells[columnOneName].Value.ToString()] = x.ToString();
                mainGrid.Rows[i].Cells[columnOneName].Value = x.ToString();
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
            tabIndexWithTabInfo.Clear();
            tabIndexWithTabInfo = temp_tabIndexWithTabInfo.ToDictionary(
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

        public void OnStartbtnClick(DataGridView paramGrid, string AlgoName, List<string> _accounts,
            Dictionary<string, string> tabIndexWithSiteOrderKey, string currentTab, Dictionary<string, AdlParameters> algoNameWithParameters,
             Dictionary<string, C_AlgoLookup_TradeSubscription> algoNameWithTradeSubscription, Dictionary<string, Instrument> instrumentNameWithInstrument)
        {

            if (tabIndexWithSiteOrderKey.ContainsKey(currentTab))
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


                    if (algoNameWithParameters.ContainsKey(AlgoName))
                    {
                        if (paramName == "Quoting Instrument Account")
                        {
                            accountNumber = _accounts.IndexOf(value.ToString());

                        }
                        if (paramName == "Quoting Instrument")
                        {
                            instrumentName = value.ToString();

                        }

                        if (algoNameWithParameters[AlgoName]._adlUserParameters.Contains(paramName))
                        {
                            algo_userparams[paramName] = value;
                        }
                        else if (algoNameWithParameters[AlgoName]._adlOrderProfileParameters.Contains(paramName))
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
                algoNameWithTradeSubscription.ContainsKey(AlgoName) &&
                instrumentNameWithInstrument.ContainsKey(instrumentName))
            {
                string orderKey = algoNameWithTradeSubscription[AlgoName].StartAlgo(accountNumber,
                        instrumentNameWithInstrument[instrumentName],
                        algo_userparams,
                        algo_orderprofileparams);
                if (!tabIndexWithSiteOrderKey.ContainsKey(currentTab))
                {
                    tabIndexWithSiteOrderKey.Add(currentTab, orderKey);
                }
                else
                {
                    tabIndexWithSiteOrderKey[currentTab] = orderKey;
                }


            }
            else
            {
                MessageBox.Show("Please check the parameters.");
                return;
            }

        }


        public void OnDeletebtnClick(DataGridView paramGrid, string AlgoName, Dictionary<string, string> tabIndexWithSiteOrderKey, string currentTab,
            Dictionary<string, C_AlgoLookup_TradeSubscription> algoNameWithTradeSubscription)
        {
            if (!tabIndexWithSiteOrderKey.ContainsKey(currentTab)
                || (tabIndexWithSiteOrderKey.ContainsKey(currentTab) && tabIndexWithSiteOrderKey[currentTab] == string.Empty))
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
            //    var cellValue = row.Cells[columnOneName].Value;

            //}
            if (tabIndexWithSiteOrderKey.ContainsKey(currentTab) && algoNameWithTradeSubscription.ContainsKey(AlgoName))
            {
                string orderKey = tabIndexWithSiteOrderKey[currentTab];

                algoNameWithTradeSubscription[AlgoName].DeleteAlgoOrder(orderKey);
            }
        }



        public void OnSaveOrUpdateTemplateBtnClick(object s, EventArgs e, TextBox txtTemplateName, string adlValue, Dictionary<string, List<Template>> _algoNameWithTemplateList,
            DataGridView paramGrid, Dictionary<string, AdlParameters> algoNameWithParameters, ComboBox savedTemplates,TabControl MainTab)
        {
            string templateName = txtTemplateName.Text.Trim();
            if (string.IsNullOrWhiteSpace(templateName))
            {
                MessageBox.Show("Template name cannot be empty.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string adlName = adlValue; // assuming this holds the currently selected algo name for this tab
            // Get selected ADL name
            if (string.IsNullOrEmpty(adlName))
            {
                MessageBox.Show("Invalid ADL selection.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            

            List<Template> templates = _algoNameWithTemplateList[adlName];
            Template existingTemplate = templates.FirstOrDefault(t => t.TemplateName == templateName);

            if (existingTemplate != null)
            {
                _helperFunctions.UpdateTemplate(existingTemplate, templateName,paramGrid,_algoNameWithTemplateList,adlName,templates);
                return;
            }

            #region Adding new Template
            
            // Get the parameter definitions for this ADL
            if (algoNameWithParameters.ContainsKey(adlName))
            {
                //We have the adl name already, so we will add to the given json
                AdlParameters adlParams = algoNameWithParameters[adlName];
                Dictionary<string, ParameterType> paramTypes = adlParams.GetParamNameWithTypeAll();
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
                if (_algoNameWithTemplateList.ContainsKey(adlName))
                {
                    var existingList = _algoNameWithTemplateList[adlName];
                    existingList.Add(newTemplate);
                    _algoNameWithTemplateList.Remove(adlName);
                    _algoNameWithTemplateList.Add(adlName, existingList);
                }
                _fileHandlers.SaveTemplateDictionaryToFile(_algoNameWithTemplateList);
                //Update the combo box in the tab we are in
                if (_algoNameWithTemplateList.ContainsKey(adlValue))
                {
                    savedTemplates.Items.Clear();
                    savedTemplates.Items.AddRange(_helperFunctions.GetTemplateNames(_algoNameWithTemplateList[adlValue]).ToArray());
                }
                
                savedTemplates.Refresh();
                if (savedTemplates.Items.Contains(newTemplate.TemplateName))
                {
                    savedTemplates.SelectedItem = newTemplate.TemplateName;
                }   

                txtTemplateName.Text = newTemplate.TemplateName;
                _helperFunctions.PopulateEveryComboBoxInTabs(MainTab, adlName, _algoNameWithTemplateList, adlValue);
                MessageBox.Show("Template saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);


            }
            else
            {
                // need to make a new json for this specific adl
            }
            #endregion

        }


        public void SavedTemplatesIndexChanged(object s, EventArgs e,ComboBox savedTemplates, TextBox txtTemplateName, Dictionary<string, List<Template>> _algoNameWithTemplateList,
            string adlValue, DataGridView paramGrid)
        {
            string selectedTemplateName = savedTemplates.SelectedItem?.ToString();
            txtTemplateName.Text = selectedTemplateName;

            if (string.IsNullOrEmpty(selectedTemplateName) || string.IsNullOrEmpty(adlValue))
                return;

            // Step 1: Fetch the correct Template object
            if (!_algoNameWithTemplateList.TryGetValue(adlValue, out List<Template> templatesForAlgo))
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
