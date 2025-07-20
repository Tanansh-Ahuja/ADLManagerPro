using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using ADLManager;
using tt_net_sdk;
using System.IO;
using Newtonsoft.Json;

namespace ADLManagerPro
{
    public class UI
    {
        HelperFunctions _helperFunctions = null;
        ButtonEvents _buttonEvents = null;
        public UI() {
            _helperFunctions = new HelperFunctions();
            _buttonEvents = new ButtonEvents();
        }

        public bool CellValueChanged(object sender, DataGridViewCellEventArgs e, DataGridView mainGrid,
            string columnZeroName,string columnOneName,string columnTwoName, string columnThreeName,string columnFourName, 
            List<int> selectedRowIndexList,TabControl MainTab, Dictionary<string, TabInfo> tabIndexWithTabInfo)
        {
            if (e.RowIndex >= 0 && mainGrid.Columns[e.ColumnIndex].Name == columnZeroName)
            {
                //e.RowIndex
                var row = mainGrid.Rows[e.RowIndex];
                bool isChecked = Convert.ToBoolean(row.Cells[columnZeroName].Value);

                if (isChecked)
                {
                    if (!selectedRowIndexList.Contains(e.RowIndex))
                        selectedRowIndexList.Add(e.RowIndex);
                }
                else
                {
                    selectedRowIndexList.Remove(e.RowIndex);
                }
            }

            if (e.RowIndex >= 0 && mainGrid.Columns[e.ColumnIndex].Name == columnFourName) // "createTab"
            {
                var row = mainGrid.Rows[e.RowIndex];
                var activateCell = row.Cells[columnFourName];
                bool isChecked = Convert.ToBoolean(activateCell.Value);
                int sno = Convert.ToInt32(mainGrid.Rows[e.RowIndex].Cells[columnOneName].Value);

                // Only validate if user is trying to activate
                if (isChecked)
                {
                    var feedValue = row.Cells[columnTwoName].Value?.ToString();
                    var adlValue = row.Cells[columnThreeName].Value?.ToString();

                    if (string.IsNullOrWhiteSpace(feedValue) || string.IsNullOrWhiteSpace(adlValue))
                    {
                        // Reset the checkbox (temporarily disable event to avoid loop)
                        //mainGrid.CellValueChanged -= Form1.mainGrid_CellValueChanged;
                        row.Cells[columnFourName].Value = false;
                        //mainGrid.CellValueChanged += Form1.mainGrid_CellValueChanged;
                        MessageBox.Show("Please select both Feed and ADL before activating.", "Validation Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return false;
                    }

                    string serial = row.Cells[columnOneName].Value.ToString();
                    if (!_helperFunctions.TabExists(serial,MainTab))
                    {
                        mainGrid.Rows[row.Index].Cells[columnTwoName].ReadOnly = true;
                        mainGrid.Rows[row.Index].Cells[columnThreeName].ReadOnly = true;
                        return true;
                    }
                }
                else
                {
                    //TODO: If tab has adl order delete it
                    string serial = row.Cells[columnOneName].Value.ToString();
                    for (int i = MainTab.TabPages.Count - 1; i > 0; i--)
                    {
                        if (MainTab.TabPages[i].Text == serial)
                        {

                            MainTab.TabPages.RemoveAt(i);

                            if(tabIndexWithTabInfo.Remove(i.ToString()))
                            {

                                tabIndexWithTabInfo.Remove(i.ToString());
                            }
                            
                            break;
                        }
                    }

                    mainGrid.Rows[row.Index].Cells[columnTwoName].ReadOnly = false;
                    mainGrid.Rows[row.Index].Cells[columnThreeName].ReadOnly = false;
                }
            }
            return false;
        }

        public void CreateTabWithLabels(string serial, string feedValue, string adlValue, Dictionary<string, AdlParameters> algoNameWithParameters,
            List<string> _accounts, Dictionary<string, Instrument> instrumentNameWithInstrument,Dictionary<string, List<Template>> _algoNameWithTemplateList,
            Dictionary<string, string> tabIndexWithSiteOrderKey,string currentTab, Dictionary<string, C_AlgoLookup_TradeSubscription> algoNameWithTradeSubscription,
            Dictionary<string, TabInfo> tabIndexWithTabInfo, TabControl MainTab)
        {
            TabPage newTab = new TabPage(serial);

            // Static label: "Feed Name"
            Label lblFeedTitle = new Label
            {
                Text = "Feed Name:",
                Left = 20,
                Top = 10,
                AutoSize = true
            };

            // Dynamic label: actual feed value
            Label lblFeedValue = new Label
            {
                Text = feedValue,
                Left = 150,
                Top = 10,
                AutoSize = true
            };

            // Static label: "Algorithm Name"
            Label lblAdlTitle = new Label
            {
                Text = "Algorithm Name:",
                Left = 20,
                Top = 30,
                AutoSize = true
            };

            // Dynamic label: actual adl value
            Label lblAdlValue = new Label
            {
                Text = adlValue,
                Left = 150,
                Top = 30,
                AutoSize = true
            };

            newTab.Controls.Add(lblFeedTitle);
            newTab.Controls.Add(lblFeedValue);
            newTab.Controls.Add(lblAdlTitle);
            newTab.Controls.Add(lblAdlValue);

            // Create DataGridView
            DataGridView paramGrid = new DataGridView
            {
                Left = 20,
                Top = 60,
                Width = 400,
                Height = 500,
                AllowUserToAddRows = false,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };


            paramGrid.DataError += (s, e) =>
            {
                e.ThrowException = false;
            };

            paramGrid.DefaultCellStyle.SelectionBackColor = paramGrid.DefaultCellStyle.BackColor;
            paramGrid.DefaultCellStyle.SelectionForeColor = paramGrid.DefaultCellStyle.ForeColor;

            paramGrid.Columns.Add("ParamName", "Parameter Name");
            paramGrid.Columns["ParamName"].ReadOnly = true; // Make ParamName column read-only
            paramGrid.Columns.Add("Value", "Value");

            if (algoNameWithParameters.ContainsKey(adlValue))
            {
                foreach (var (paramName, paramType) in algoNameWithParameters[adlValue]._adlOrderProfileParametersWithType)
                {
                    int rowIndex = paramGrid.Rows.Add(paramName, paramType);

                    if (paramName.Equals("Quoting Instrument Account", StringComparison.OrdinalIgnoreCase) ||
                        paramName.Equals("Fast Mkt Inst Account", StringComparison.OrdinalIgnoreCase) ||
                        paramName.Equals("Hedge Instrument Account", StringComparison.OrdinalIgnoreCase))
                    {
                        var combocell = new DataGridViewComboBoxCell();
                        combocell.Items.AddRange(_accounts.ToArray());
                        paramGrid.Rows[rowIndex].Cells["Value"] = combocell;
                        continue;
                    }

                    if (paramName.Equals("Quoting Instrument", StringComparison.OrdinalIgnoreCase) ||
                        paramName.Equals("Fast Mkt Inst", StringComparison.OrdinalIgnoreCase) ||
                        paramName.Equals("Hedge Instrument", StringComparison.OrdinalIgnoreCase))
                    {
                        var combocell = new DataGridViewComboBoxCell();
                        combocell.Items.AddRange(instrumentNameWithInstrument.Keys.ToArray());
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

                foreach (var (paramName, paramType) in algoNameWithParameters[adlValue]._adlUserParametersWithType)
                {
                    int rowIndex = paramGrid.Rows.Add(paramName, paramType);

                    if (paramName.Equals("Quoting Instrument Account", StringComparison.OrdinalIgnoreCase) ||
                        paramName.Equals("Fast Mkt Inst Account", StringComparison.OrdinalIgnoreCase) ||
                        paramName.Equals("Hedge Instrument Account", StringComparison.OrdinalIgnoreCase))
                    {
                        var combocell = new DataGridViewComboBoxCell();
                        combocell.Items.AddRange(_accounts.ToArray());
                        paramGrid.Rows[rowIndex].Cells["Value"] = combocell;
                        continue;
                    }

                    if (paramName.Equals("Quoting Instrument", StringComparison.OrdinalIgnoreCase) ||
                        paramName.Equals("Fast Mkt Inst", StringComparison.OrdinalIgnoreCase) ||
                        paramName.Equals("Hedge Instrument", StringComparison.OrdinalIgnoreCase))
                    {
                        var combocell = new DataGridViewComboBoxCell();
                        combocell.Items.AddRange(instrumentNameWithInstrument.Keys.ToArray());
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

            newTab.Controls.Add(paramGrid);

            //TODO: Add delete button and functionality: order remove
            Button btnDeleteAlgo = new Button
            {
                Text = "Delete Algo",
                Left = 160,
                Top = paramGrid.Bottom + 10,
                Width = 120,
                Height = 30
            };


            //TODO: Template Save Button and name entering box


            // Add "Start Algo" button
            Button btnStartAlgo = new Button
            {
                Text = "Start Algo",
                Left = 20,
                Top = paramGrid.Bottom + 10,
                Width = 120,
                Height = 30
            };

            ComboBox savedTemplates = new ComboBox
            {
                Left = 450,
                Top = 60,
                Width = 150,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            //TODO: add functionality and handle edge case
            if (_algoNameWithTemplateList.ContainsKey(adlValue))
                savedTemplates.Items.AddRange(_helperFunctions.GetTemplateNames(_algoNameWithTemplateList[adlValue]).ToArray());

            TextBox txtTemplateName = new TextBox
            {
                Left = 450,
                Top = savedTemplates.Bottom + 10,
                Width = 150,
                //TODO : placeholder text
            };

            Button btnSaveTemplate = new Button
            {
                Text = "Save Template",
                Left = 450,
                Top = txtTemplateName.Bottom + 10,
                Width = 120,
                Height = 30
            };

            #region Events for UI objects

            btnStartAlgo.Click += (s, e) =>
            {

                _buttonEvents.OnStartbtnClick(paramGrid, adlValue, _accounts, tabIndexWithSiteOrderKey, currentTab,
                    algoNameWithParameters, algoNameWithTradeSubscription, instrumentNameWithInstrument);
            };
            newTab.Controls.Add(btnStartAlgo);


            btnDeleteAlgo.Click += (s, e) =>
            {
                _buttonEvents.OnDeletebtnClick(paramGrid, adlValue, tabIndexWithSiteOrderKey, currentTab, algoNameWithTradeSubscription);
            };

            newTab.Controls.Add(btnDeleteAlgo);

            savedTemplates.SelectedIndexChanged += (s, e) =>
            {
                _buttonEvents.SavedTemplatesIndexChanged(s, e, savedTemplates, txtTemplateName, _algoNameWithTemplateList, adlValue, paramGrid);
            };

            btnSaveTemplate.Click += (s, e) =>
            {
                _buttonEvents.OnSaveOrUpdateTemplateBtnClick(s, e,txtTemplateName,adlValue,_algoNameWithTemplateList,paramGrid,algoNameWithParameters);
            };

            #endregion


            newTab.Controls.Add(savedTemplates);
            newTab.Controls.Add(txtTemplateName);
            newTab.Controls.Add(btnSaveTemplate);
            TabInfo tabInfo = new TabInfo(paramGrid, adlValue, feedValue);
            tabIndexWithTabInfo.Add(serial, tabInfo);

            // Add tab to TabControl
            MainTab.TabPages.Add(newTab);
        }


    }
}
