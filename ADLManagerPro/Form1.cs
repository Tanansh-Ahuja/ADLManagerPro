using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ADLManager;
using Newtonsoft.Json;
using tt_net_sdk;
using System.IO;
using System.Text.Json;


namespace ADLManagerPro
{
    public partial class Form1 : Form
    {

        #region Variables

        // Column Names in UI in main Data Grid
        string columnZeroName = "Select";
        string columnOneName = "Sno";
        string columnTwoName = "feed";
        string columnThreeName = "adl";
        string columnFourName = "createTab";

        private string _appSecretKey;
        private static Label loadingLabel;
        private string currentTab = null;


        private FileHandlers _fileHandlers = null;
        private HelperFunctions _helperFunctions = null;
        private ButtonEvents _buttonEvents = null;
        private UI _uI = null;


        // Declare the API objects
        private TTAPI m_api = null;
        private tt_net_sdk.WorkerDispatcher m_disp = null;
        private Dispatcher m_dispatcher = null;
        public static IReadOnlyCollection<Account> m_accounts = null;
        public static bool m_isOrderBookDownloaded = false;
        //private string m_orderKey = "";
        private object m_Lock = new object();
        private bool m_isDisposed = false;


        // Lists
        public static List<Instrument> instruments = new List<Instrument>();
        public static List<Algo> algos = new List<Algo>();
        private List<int> selectedRowIndexList = new List<int>();
        private List<string> _accounts = new List<string>();

        //Dictionaries
        public static Dictionary<string, AdlParameters> algoNameWithParameters = new Dictionary<string, AdlParameters>();
        public static Dictionary<string,Instrument> instrumentNameWithInstrument = new Dictionary<string,Instrument>();
        public static Dictionary<string, C_AlgoLookup_TradeSubscription> algoNameWithTradeSubscription = new Dictionary<string, C_AlgoLookup_TradeSubscription>();
        public static Dictionary<string,string> tabIndexWithSiteOrderKey = new Dictionary<string, string>();
        public Dictionary<string,TabInfo> tabIndexWithTabInfo = new Dictionary<string,TabInfo>();
        private Dictionary<string, List<Template>> _algoNameWithTemplateList = new Dictionary<string, List<Template>>();
        public static List<string> dummy_algos = new List<string> { "MET_ScalarBias_2_0" };
        public static List<string> dummy_instruments = new List<string> { "BRN Sep25", "BRN Dec25"};

        #endregion


        public Form1(string appSecretKey)
        {
            _appSecretKey = appSecretKey;
            _fileHandlers = new FileHandlers();
            _helperFunctions = new HelperFunctions();
            _buttonEvents = new ButtonEvents();
            _uI = new UI();
            InitializeComponent();
           
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            InitialiseLoadingLabel("Loading");
            mainGrid.Columns[columnOneName].ReadOnly = true;
            UpdateAdlDropdownSource();
            mainGrid.DefaultCellStyle.SelectionBackColor = mainGrid.DefaultCellStyle.BackColor;
            mainGrid.DefaultCellStyle.SelectionForeColor = mainGrid.DefaultCellStyle.ForeColor;
            MainTab.SelectedIndexChanged += MainTab_SelectedIndexChanged;
            _algoNameWithTemplateList = _fileHandlers.FetchJsonFromFile();
        }

        #region TT API

        public void Start(tt_net_sdk.TTAPIOptions apiConfig)
        {
            m_disp = tt_net_sdk.Dispatcher.AttachWorkerDispatcher();
            m_disp.DispatchAction(() =>
            {
                Init(apiConfig);
            });

            m_disp.Run();
        }

        
        public void Init(tt_net_sdk.TTAPIOptions apiConfig)
        {
            ApiInitializeHandler apiInitializeHandler = new ApiInitializeHandler(ttNetApiInitHandler);
            TTAPI.ShutdownCompleted += TTAPI_ShutdownCompleted;
            TTAPI.CreateTTAPI(tt_net_sdk.Dispatcher.Current, apiConfig, apiInitializeHandler);
        }

        public void ttNetApiInitHandler(TTAPI api, ApiCreationException ex)
        {
            if (ex == null)
            {
                Console.WriteLine("TT.NET SDK INITIALIZED");
                ChangeLoadingLabelText("TT.NET SDK INITIALIZED");
                _fileHandlers.SaveApiKey("Key.txt", _appSecretKey);

                // Authenticate your credentials
                m_api = api;
                m_api.TTAPIStatusUpdate += new EventHandler<TTAPIStatusUpdateEventArgs>(m_api_TTAPIStatusUpdate);
                m_api.Start();
            }
            else if (ex.IsRecoverable)
            {
                MessageBox.Show("TT.NET SDK Initialization Failed");
                DisposeEverything();
            }
            else
            {
                Console.WriteLine("TT.NET SDK Initialization Failed: {0}", ex.Message);
                MessageBox.Show(ex.Message);
                DisposeEverything();
            }
        }

        public void m_api_TTAPIStatusUpdate(object sender, TTAPIStatusUpdateEventArgs e)
        {
            Console.WriteLine("TTAPIStatusUpdate: {0}", e);
            string loadingLabelText = "TTAPIStatusUpdate: " + e.ToString();
            ChangeLoadingLabelText(loadingLabelText);
            if (e.IsReady == false)
            {
                // TODO: Do any connection lost processing here
                return;
            }
            m_dispatcher = tt_net_sdk.Dispatcher.Current;
            
            C_InstrumentLookup c_InstrumentLookup = new C_InstrumentLookup(m_dispatcher,
                                                                           "ICE",
                                                                           "Future", "BRN", "BRN Sep25");
            C_InstrumentLookup c_InstrumentLookup1 = new C_InstrumentLookup(m_dispatcher,
                                                                           "ICE",
                                                                           "Future", "BRN", "BRN Dec25");


            algoLookup("MET_ScalarBias_2_0");
            // Get the accounts
            m_accounts = m_api.Accounts;
            foreach(var account in m_accounts)
            {
                _accounts.Add(account.AccountName);
            }
            
            

        }

        void algoLookup(string algoName)
        {
            C_AlgoLookup_TradeSubscription c_AlgoLookup_TradeSubscription = new C_AlgoLookup_TradeSubscription(m_dispatcher, algoName);

            if (!algoNameWithTradeSubscription.ContainsKey(algoName))
            {
                algoNameWithTradeSubscription.Add(algoName, c_AlgoLookup_TradeSubscription);

            }

        }

        #endregion


        public static void ShowMainGrid()
        {
            if(m_isOrderBookDownloaded && instruments.Count() == dummy_instruments.Count() && algos.Count() == dummy_algos.Count())
            {
                ShowMainTab();  
            }

        }



        #region UI

        #region Loading

        public void InitialiseLoadingLabel(string showThis)
        {
            loadingLabel = new Label()
            {
                Text = showThis,
                AutoSize = true,
                Font = new Font("Arial", 8, FontStyle.Bold),
                Location = new Point(50, (this.Height / 2) - 10)
            };
            this.Controls.Add(loadingLabel);
            loadingLabel.BringToFront();

            // Hide the main tab (you can add more components here)
            MainTab.Hide();
        }
        private void ChangeLoadingLabelText(string showThis)
        {
            loadingLabel.Text = showThis;
        }
        private static void ShowMainTab()
        {
            loadingLabel.Hide();
            MainTab.Show();
        }
        

        #endregion

        private void UpdateAdlDropdownSource()
        {
            if (mainGrid.InvokeRequired)
            {
                mainGrid.Invoke(new Action(UpdateAdlDropdownSource));
                return;
            }

            var adlColumn = mainGrid.Columns[columnThreeName] as DataGridViewComboBoxColumn;
            if (adlColumn != null)
            {
                adlColumn.Items.Clear();
                foreach(var algo in dummy_algos)
                { 
                    adlColumn.Items.Add(algo);
                }
            }
            mainGrid.Columns[columnThreeName].ReadOnly = false;
        }

        //private void UpdateInstrumentDropdownSource()
        //{
        //    if (mainGrid.InvokeRequired)
        //    {
        //        mainGrid.Invoke(new Action(UpdateAdlDropdownSource));
        //        return;
        //    }

        //    var adlColumn = mainGrid.Columns[columnThreeName] as DataGridViewComboBoxColumn;
        //    if (adlColumn != null)
        //    {
        //        adlColumn.Items.Clear();
        //        foreach (var algo in dummy_algos)
        //        {
        //            adlColumn.Items.Add(algo);
        //        }
        //    }
        //    mainGrid.Columns[columnThreeName].ReadOnly = false;
        //    add_btn.Enabled = true;
        //    add_btn.Text = "Add Row";
        //}

        private void NeonFeedButton_Click(object sender, EventArgs e)
        {
            //TODO : Connect Neon Feed
        }


        private void del_btn_Click(object sender, EventArgs e)
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

        private void MainTab_SelectedIndexChanged(object sender, EventArgs e)
        {
            var selectedTab = MainTab.SelectedTab;
            if (selectedTab != null)
            {
                string tabName = selectedTab.Text;

                if (tabName == "main")
                {
                    currentTab = null;
                }
                else if (tabIndexWithTabInfo.ContainsKey(tabName))
                {
                    currentTab = tabName;
                }
                else
                {
                    currentTab = null;
                }
            }
        }

        private void mainGrid_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            bool createParamGrid = _uI.CellValueChanged(sender, e,mainGrid,columnZeroName,columnOneName,columnTwoName,columnThreeName,columnFourName,selectedRowIndexList,MainTab);
            if(createParamGrid)
            {
                var row = mainGrid.Rows[e.RowIndex];
                string serial = row.Cells[columnOneName].Value.ToString();
                var feedValue = row.Cells[columnTwoName].Value?.ToString();
                var adlName = row.Cells[columnThreeName].Value?.ToString();
                _uI.CreateTabWithLabels(serial, feedValue, adlName,algoNameWithParameters,_accounts,instrumentNameWithInstrument,
                    _algoNameWithTemplateList,tabIndexWithSiteOrderKey,currentTab,algoNameWithTradeSubscription,tabIndexWithTabInfo,MainTab);
            }
            //if (e.RowIndex >= 0 && mainGrid.Columns[e.ColumnIndex].Name == columnZeroName)
            //{
            //    //e.RowIndex
            //    var row = mainGrid.Rows[e.RowIndex];
            //    bool isChecked = Convert.ToBoolean(row.Cells[columnZeroName].Value);

            //    if (isChecked)
            //    {
            //        if (!selectedRowIndexList.Contains(e.RowIndex))
            //            selectedRowIndexList.Add(e.RowIndex);
            //    }
            //    else
            //    {
            //        selectedRowIndexList.Remove(e.RowIndex);
            //    }
            //}

            //if (e.RowIndex >= 0 && mainGrid.Columns[e.ColumnIndex].Name == columnFourName) // "createTab"
            //{
            //    var row = mainGrid.Rows[e.RowIndex];
            //    var activateCell = row.Cells[columnFourName];
            //    bool isChecked = Convert.ToBoolean(activateCell.Value);
            //    int sno = Convert.ToInt32(mainGrid.Rows[e.RowIndex].Cells[columnOneName].Value);

            //    // Only validate if user is trying to activate
            //    if (isChecked)
            //    {
            //        var feedValue = row.Cells[columnTwoName].Value?.ToString();
            //        var adlValue = row.Cells[columnThreeName].Value?.ToString();

            //        if (string.IsNullOrWhiteSpace(feedValue) || string.IsNullOrWhiteSpace(adlValue))
            //        {
            //            // Reset the checkbox (temporarily disable event to avoid loop)
            //            mainGrid.CellValueChanged -= mainGrid_CellValueChanged;
            //            row.Cells[columnFourName].Value = false;
            //            mainGrid.CellValueChanged += mainGrid_CellValueChanged;
            //            MessageBox.Show("Please select both Feed and ADL before activating.", "Validation Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //            return;
            //        }

            //        string serial = row.Cells[columnOneName].Value.ToString();
            //        if (!TabExists(serial))
            //        {
            //            CreateTabWithLabels(serial, feedValue, adlValue);

            //            mainGrid.Rows[row.Index].Cells[columnTwoName].ReadOnly = true;
            //            mainGrid.Rows[row.Index].Cells[columnThreeName].ReadOnly = true;
            //        }
            //    }
            //    else
            //    {
            //        //TODO: If tab has adl order delete it
            //        string serial = row.Cells[columnOneName].Value.ToString();
            //        for (int i = MainTab.TabPages.Count - 1; i > 0; i--)
            //        {
            //            if (MainTab.TabPages[i].Text == serial)
            //            {
                           
            //                MainTab.TabPages.RemoveAt(i);
            //                break;
            //            }
            //        }

            //        mainGrid.Rows[row.Index].Cells[columnTwoName].ReadOnly = false;
            //        mainGrid.Rows[row.Index].Cells[columnThreeName].ReadOnly = false;
            //    }
            //}
        }

        private void mainGrid_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (mainGrid.IsCurrentCellDirty)
            {
                // Get current column
                int colIndex = mainGrid.CurrentCell.ColumnIndex;
                if (mainGrid.Columns[colIndex].Name == columnFourName || mainGrid.Columns[colIndex].Name == columnZeroName)
                {
                    mainGrid.CommitEdit(DataGridViewDataErrorContexts.Commit);
                }
            }
        }

        private void CreateTabWithLabels(string serial, string feedValue, string adlValue)
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
                    
                    if(paramName.Equals("Quoting Instrument Account", StringComparison.OrdinalIgnoreCase) ||
                        paramName.Equals("Fast Mkt Inst Account", StringComparison.OrdinalIgnoreCase)||
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
            if(_algoNameWithTemplateList.ContainsKey(adlValue))
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
                
                _buttonEvents.OnStartbtnClick(paramGrid, adlValue,_accounts,tabIndexWithSiteOrderKey,currentTab,
                    algoNameWithParameters,algoNameWithTradeSubscription,instrumentNameWithInstrument);
            };
            newTab.Controls.Add(btnStartAlgo);


            btnDeleteAlgo.Click += (s, e) =>
            {
                _buttonEvents.OnDeletebtnClick(paramGrid, adlValue,tabIndexWithSiteOrderKey,currentTab,algoNameWithTradeSubscription);
            };

            newTab.Controls.Add(btnDeleteAlgo);

            savedTemplates.SelectedIndexChanged += (s, e) =>
            {
                txtTemplateName.Text = savedTemplates.SelectedItem?.ToString();
            };


            #endregion

            btnSaveTemplate.Click += (s, e) =>
            {
                string selectedAlgo = adlValue;
                string templateName = txtTemplateName.Text.Trim();

                if (string.IsNullOrEmpty(selectedAlgo) || string.IsNullOrEmpty(templateName))
                {
                    MessageBox.Show("Please select an algo and enter template name.");
                    return;
                }

                string jsonFilePath = "algo_templates.json";
                List<AlgoTemplateRoot> templatesData = new List<AlgoTemplateRoot>();

                // Load existing file
                if (File.Exists(jsonFilePath))
                {
                    string existing = File.ReadAllText(jsonFilePath);
                    templatesData = System.Text.Json.JsonSerializer.Deserialize<List<AlgoTemplateRoot>>(existing) ?? new List<AlgoTemplateRoot>();
                }

                // Find or add algo section
                //var algoEntry = templatesData.FirstOrDefault(a => a.algo_name == selectedAlgo);
                //if (algoEntry == null)
                //{
                //    algoEntry = new AlgoTemplateRoot { algo_name = selectedAlgo, templates = new List<Template>() };
                //    templatesData.Add(algoEntry);
                //}

                //var existingTemplate = algoEntry.templates.FirstOrDefault(t => t.template_name == templateName);
                //if (existingTemplate != null)
                //{
                //    var result = MessageBox.Show("Template exists. Do you want to override?", "Confirm", MessageBoxButtons.YesNo);
                //    if (result != DialogResult.Yes) return;

                //    // Update template
                //    existingTemplate.template_parameters = SaveParameterToTemplate(paramGrid); // Replace with your parameter fetch logic
                //}
                //else
                //{
                //    // Add new
                //    algoEntry.templates.Add(new Template
                //    {
                //        template_name = templateName,
                //        template_parameters = SaveParameterToTemplate(paramGrid) // Replace with real param fetch
                //    });
                //}

                // Save JSON
                File.WriteAllText(jsonFilePath, System.Text.Json.JsonSerializer.Serialize(templatesData, new JsonSerializerOptions { WriteIndented = true }));
                MessageBox.Show("Template saved.");
            };

            #endregion


            newTab.Controls.Add(savedTemplates);
            newTab.Controls.Add(txtTemplateName);
            newTab.Controls.Add(btnSaveTemplate);
            TabInfo tabInfo = new TabInfo(paramGrid, adlValue,feedValue);
            tabIndexWithTabInfo.Add(serial, tabInfo);

            // Add tab to TabControl
            MainTab.TabPages.Add(newTab);
        }

        //TODO : add functionality
        private Dictionary<string, Parameter> SaveParameterToTemplate(DataGridView paramGrid)
        {
            return new Dictionary<string, Parameter>
            {
                { "parameter1", new Parameter { type = "int", value = "10" } },
                { "parameter2", new Parameter { type = "string", value = "ABC" } }
            };



        }

        

        
        private void add_btn_Click(object sender, EventArgs e)
        {
            _buttonEvents.AddRowInMainGrid(mainGrid);
        }

        //TODO funtionality thik karo
        private void mainGrid_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            //DataGridView dgv = sender as DataGridView;


            //if (dgv.CurrentCell.ColumnIndex == 1) // Assuming "Value" column
            //{
            //    TextBox tb = e.Control as TextBox;

            //    if (tb != null)
            //    {
            //        tb.KeyPress -= NumericKeyPressHandler; // Prevent double hook
            //        tb.KeyPress += NumericKeyPressHandler;

            //        tb.Tag = GetCurrentParamType(dgv.CurrentCell.RowIndex, dgv); // Store type in Tag
            //    }
            //}
        }
        private void NumericKeyPressHandler(object sender, KeyPressEventArgs e)
        {
            TextBox tb = sender as TextBox;
            string paramType = tb.Tag?.ToString() ?? "string";

            if (paramType == "int")
            {
                // Allow digits and control keys only
                if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
                {
                    e.Handled = true;
                }
            }
            else if (paramType == "float")
            {
                // Allow digits, one '.', and control keys
                if (!char.IsControl(e.KeyChar) &&
                    !char.IsDigit(e.KeyChar) &&
                    e.KeyChar != '.')
                {
                    e.Handled = true;
                }

                // Only one dot allowed
                if (e.KeyChar == '.' && tb.Text.Contains('.'))
                {
                    e.Handled = true;
                }
            }
            else
            {
                // allow anything for string/bool
                e.Handled = false;
            }
        }
        private ParameterType GetCurrentParamType(int rowIndex, DataGridView dgv)
        {
            //// First column is parameter name, we use that to find the type from dictionary
            //string paramName = dgv.Rows[rowIndex].Cells[0].Value?.ToString();

            //// Each tab has the ADL name in its header
            //string adlName = MainTab.SelectedTab?.Text;
            //if (algoNameWithParameters.ContainsKey(adlName))
            //{
            //    foreach (var (name, type) in algoNameWithParameters[adlName]._adlUserParameters)
            //    {
            //        if (name == paramName)
            //            return type;
            //    }
            //}
            return ParameterType.None; // fallback
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            TTAPI.Shutdown();
        }





        #region Dispose and Shutdown
        public void DisposeEverything()
        {
            lock (m_Lock)
            {
                if (!m_isDisposed)
                {
                   
                    m_isDisposed = true;
                }


                TTAPI.Shutdown();
                Dispose();
            }
        }

        public void TTAPI_ShutdownCompleted(object sender, EventArgs e)
        {
            // Dispose of any other objects / resources
            Console.WriteLine("TTAPI shutdown completed");
        }

        #endregion

        
    }
}
