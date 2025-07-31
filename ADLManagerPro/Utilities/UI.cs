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
using System.Drawing;

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

        public bool CellValueChanged(object sender, DataGridViewCellEventArgs e, DataGridView mainGrid,TabControl MainTab)
        {
            try
            {
                if (e.RowIndex >= 0 && mainGrid.Columns[e.ColumnIndex].Name == Globals.columnZeroName)
                {
                    //e.RowIndex
                    var row = mainGrid.Rows[e.RowIndex];
                    bool isChecked = Convert.ToBoolean(row.Cells[Globals.columnZeroName].Value);

                    string serial_no = row.Cells[Globals.columnOneName].Value.ToString();
                    if (isChecked)
                    {
                        if (!Globals.selectedRowIndexList.Contains(int.Parse(serial_no) - 1))
                            Globals.selectedRowIndexList.Add(int.Parse(serial_no) - 1);
                    }
                    else
                    {
                        Globals.selectedRowIndexList.Remove(int.Parse(serial_no) - 1);
                    }
                }

                if (e.RowIndex >= 0 && mainGrid.Columns[e.ColumnIndex].Name == Globals.columnFourName) // "createTab"
                {
                    var row = mainGrid.Rows[e.RowIndex];
                    var activateCell = row.Cells[Globals.columnFourName];
                    bool isChecked = Convert.ToBoolean(activateCell.Value);
                    int sno = Convert.ToInt32(mainGrid.Rows[e.RowIndex].Cells[Globals.columnOneName].Value);

                    // Only validate if user is trying to activate
                    if (isChecked)
                    {
                        var feedValue = row.Cells[Globals.columnTwoName].Value?.ToString();
                        var adlValue = row.Cells[Globals.columnThreeName].Value?.ToString();

                        if (string.IsNullOrWhiteSpace(feedValue) || string.IsNullOrWhiteSpace(adlValue))
                        {
                            row.Cells[Globals.columnFourName].Value = false;
                            MessageBox.Show("Please select both Feed and ADL before activating.", "Validation Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return false;
                        }

                        string serial = row.Cells[Globals.columnOneName].Value.ToString();
                        string feedName = row.Cells[Globals.columnTwoName].Value.ToString();
                        string tabName = serial + "-" + feedName;
                        if (!_helperFunctions.TabExists(tabName, MainTab))
                        {
                            mainGrid.Rows[row.Index].Cells[Globals.columnTwoName].ReadOnly = true;
                            mainGrid.Rows[row.Index].Cells[Globals.columnThreeName].ReadOnly = true;
                            if (Globals.feedNameWithRowIndex.ContainsKey(feedName))
                            {
                                List<string> temp = Globals.feedNameWithRowIndex[feedName];
                                temp.Add((row.Index + 1).ToString());
                                Globals.feedNameWithRowIndex[feedName] = temp;

                            }
                            else
                            {
                                Globals.feedNameWithRowIndex.Add(feedName, new List<string> { (row.Index + 1).ToString() });
                            }
                            return true;
                        }
                    }
                    else
                    {

                        string serial = row.Cells[Globals.columnOneName].Value.ToString();
                        string feedName = row.Cells[Globals.columnTwoName].Value?.ToString();
                        if (feedName != null)
                        {
                            if (Globals.feedNameWithRowIndex.ContainsKey(feedName) && Globals.feedNameWithRowIndex[feedName].Contains(serial))
                            {
                                Globals.feedNameWithRowIndex[feedName].Remove(serial);
                            }

                            string tabName = serial + "-" + feedName;
                            for (int i = MainTab.TabPages.Count - 1; i > 0; i--)
                            {
                                if (MainTab.TabPages[i].Text == tabName)
                                {
                                    Control[] foundStatusLabel = MainTab.TabPages[i].Controls.Find("OrderStatusValueLabel", true);
                                    if (foundStatusLabel.Length > 0 && foundStatusLabel[0].Text == "ACTIVATED")
                                    {
                                        MessageBox.Show($"Please delete order before deleting tab {serial}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                        return false;
                                    }
                                    MainTab.TabPages.RemoveAt(i);
                                    if (Globals.tabNameWithTabInfo.ContainsKey(tabName))
                                    {
                                        Globals.tabNameWithTabInfo.Remove(tabName);
                                    }
                                    break;
                                }
                            }
                            mainGrid.Rows[row.Index].Cells[Globals.columnTwoName].ReadOnly = false;
                            mainGrid.Rows[row.Index].Cells[Globals.columnThreeName].ReadOnly = false;
                        }
                    }
                }
                return false;

            }
            catch
            {
                MessageBox.Show("Error occured while checking the cell value change. Shutting down.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                HelperFunctions.ShutEverythingDown();
                return false;
            }
        }

        public void CreateTabWithLabels(string serial, string feedValue, string adlValue,TabControl MainTab)
        {
            try
            {
                string tabName = serial + "-" + feedValue;
                TabPage newTab = new TabPage(tabName);
                

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
                    Name = "Adl Value",
                    Text = adlValue,
                    Left = 150,
                    Top = 30,
                    AutoSize = true
                };

                Label lblOrderStatus = new Label
                {
                    Name = "OrderStatusLabel",
                    Text = "Order Status:",
                    Left = 20,
                    Top = 50,
                    AutoSize = true
                };


                Label lblOrderStatusValue = new Label
                {
                    Name = "OrderStatusValueLabel",
                    Text = "DEACTIVATED",
                    Left = 150,
                    Top = 50,
                    AutoSize = true
                };
                lblOrderStatusValue.Font = new Font(lblOrderStatusValue.Font, FontStyle.Bold);

                newTab.Controls.Add(lblFeedTitle);
                newTab.Controls.Add(lblFeedValue);
                newTab.Controls.Add(lblAdlTitle);
                newTab.Controls.Add(lblAdlValue);
                newTab.Controls.Add(lblOrderStatus);
                newTab.Controls.Add(lblOrderStatusValue);

                // Create DataGridView
                DataGridView paramGrid = new DataGridView
                {
                    Name = "ParamGrid",
                    Left = 20,
                    Top = 80,
                    Width = 400,
                    Height = 500,
                    AllowUserToAddRows = false,
                    RowHeadersVisible = false,
                    AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                    AllowUserToResizeRows = false,
                    AllowUserToResizeColumns = false

                };


                paramGrid.DefaultCellStyle.SelectionBackColor = paramGrid.DefaultCellStyle.BackColor;
                paramGrid.DefaultCellStyle.SelectionForeColor = paramGrid.DefaultCellStyle.ForeColor;

                paramGrid.Columns.Add("ParamName", "Parameter Name");
                paramGrid.Columns["ParamName"].ReadOnly = true; // Make ParamName column read-only
                paramGrid.Columns["ParamName"].DefaultCellStyle.BackColor = Color.LightGray;
                paramGrid.Columns.Add("Value", "Value");

                paramGrid.DataError += (s, e) =>
                {
                    e.ThrowException = false;
                };


                paramGrid.CellValidating += (s, e) =>
                {
                    _helperFunctions.ParamgridCellValueValidate(s, e, paramGrid, adlValue);
                };

                if (Globals.algoNameWithParameters.ContainsKey(adlValue))
                {
                    _helperFunctions.PopulateParamGridWithOrderProfileParameters(paramGrid, adlValue);
                    _helperFunctions.PopulateParamGridWithUserParameters(paramGrid, adlValue);
                }

                newTab.Controls.Add(paramGrid);



                Button btnDeleteAlgo = new Button
                {
                    Name = "DeleteAlgoButton",
                    Text = "Delete Order",
                    Left = 160,
                    Top = paramGrid.Bottom + 10,
                    Width = 120,
                    Height = 30
                };
                btnDeleteAlgo.Hide();

                // Add "Start Algo" button
                Button btnStartAlgo = new Button
                {
                    Name = "StartAlgoButton",
                    Text = "Send Order",
                    Left = 20,
                    Top = paramGrid.Bottom + 10,
                    Width = 120,
                    Height = 30
                };

                ComboBox savedTemplates = new ComboBox
                {
                    Name = "TemplateComboBox",
                    Left = 450,
                    Top = paramGrid.Top,
                    Width = 150,
                    DropDownStyle = ComboBoxStyle.DropDownList
                };

                if (Globals.algoNameWithTemplateList != null && Globals.algoNameWithTemplateList.ContainsKey(adlValue))
                    savedTemplates.Items.AddRange(_helperFunctions.GetTemplateNames(Globals.algoNameWithTemplateList[adlValue]).ToArray());

                TextBox txtTemplateName = new TextBox
                {
                    Name = "TemplateTextBox",
                    Left = 450,
                    Top = savedTemplates.Bottom + 10,
                    Width = 150
                };

                Button btnSaveTemplate = new Button
                {
                    Name = "SaveTemplateButton",
                    Text = "Save Template",
                    Left = 450,
                    Top = txtTemplateName.Bottom + 10,
                    Width = 120,
                    Height = 30
                };
                newTab.Controls.Add(savedTemplates);
                newTab.Controls.Add(txtTemplateName);
                newTab.Controls.Add(btnSaveTemplate);


                #region Events for UI objects

                btnStartAlgo.Click += (s, e) =>
                {

                    _buttonEvents.OnStartbtnClick(paramGrid, adlValue, newTab);

                };
                newTab.Controls.Add(btnStartAlgo);


                btnDeleteAlgo.Click += (s, e) =>
                {

                    _buttonEvents.OnDeletebtnClick(paramGrid, adlValue, newTab);

                };

                newTab.Controls.Add(btnDeleteAlgo);

                savedTemplates.SelectedIndexChanged += (s, e) =>
                {
                    _buttonEvents.SavedTemplatesIndexChanged(s, e, savedTemplates, txtTemplateName, adlValue, paramGrid);
                };

                btnSaveTemplate.Click += (s, e) =>
                {
                    _buttonEvents.OnSaveOrUpdateTemplateBtnClick(s, e, txtTemplateName, adlValue, paramGrid, savedTemplates, MainTab,newTab.Text);
                };

                #endregion



                TabInfo tabInfo = new TabInfo(paramGrid, adlValue, feedValue, newTab, double.NaN, false);
                if (Globals.tabNameWithTabInfo.ContainsKey(tabName))
                {
                    Globals.tabNameWithTabInfo[tabName] = tabInfo;
                }
                else
                {

                    Globals.tabNameWithTabInfo.Add(tabName, tabInfo);
                }

                // Add tab to TabControl
                MainTab.TabPages.Add(newTab);
            }
            catch
            {
                MessageBox.Show("Error occured while creating a new Tab. Shutting down.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                HelperFunctions.ShutEverythingDown();
            }
        }



    }
}
