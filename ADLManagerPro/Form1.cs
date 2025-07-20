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
        private LoadingLabel _loadingLabel = null;

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
            _loadingLabel = new LoadingLabel();
            InitializeComponent();
           
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            loadingLabel = _loadingLabel.InitialiseLoadingLabel("Loading",loadingLabel,this,MainTab);
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
                _loadingLabel.ChangeLoadingLabelText("TT.NET SDK INITIALIZED",loadingLabel);
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
            _loadingLabel.ChangeLoadingLabelText(loadingLabelText,loadingLabel);
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


        private static void ShowMainTab()
        {
            loadingLabel.Hide();
            MainTab.Show();
        }


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
            _buttonEvents.DeleteRowsInMainGrid(sender, e,selectedRowIndexList,mainGrid,columnOneName,columnFourName,tabIndexWithTabInfo,MainTab);
            
        }

        private void add_btn_Click(object sender, EventArgs e)
        {
            _buttonEvents.AddRowInMainGrid(mainGrid);
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
            bool createParamGrid = _uI.CellValueChanged(sender, e,mainGrid,columnZeroName,columnOneName,columnTwoName,columnThreeName,columnFourName,selectedRowIndexList,MainTab,tabIndexWithTabInfo);
            if(createParamGrid)
            {
                var row = mainGrid.Rows[e.RowIndex];
                string serial = row.Cells[columnOneName].Value.ToString();
                var feedValue = row.Cells[columnTwoName].Value?.ToString();
                var adlName = row.Cells[columnThreeName].Value?.ToString();
                _uI.CreateTabWithLabels(serial, feedValue, adlName,algoNameWithParameters,_accounts,instrumentNameWithInstrument,
                    _algoNameWithTemplateList,tabIndexWithSiteOrderKey,currentTab,algoNameWithTradeSubscription,tabIndexWithTabInfo,MainTab);
            }
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


        #endregion


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
