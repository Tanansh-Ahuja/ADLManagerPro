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
using Newtonsoft.Json;
using tt_net_sdk;
using System.IO;
using System.Text.Json;


namespace ADLManagerPro
{
    public partial class Form1 : Form
    {

        #region Variables

        private string _appSecretKey;
        //private string currentTab = null;


        private FileHandlers _fileHandlers = null;
        private ButtonEvents _buttonEvents = null;
        private UI _uI = null;
        private LoadingLabel _loadingLabel = null;

        // Declare the API objects
        private TTAPI m_api = null;
        private tt_net_sdk.WorkerDispatcher m_disp = null;
        private Dispatcher m_dispatcher = null;
        private object m_Lock = new object();
        private bool m_isDisposed = false;
        #endregion


        public Form1(string appSecretKey)
        {
            _appSecretKey = appSecretKey;
            _fileHandlers = new FileHandlers();
            _buttonEvents = new ButtonEvents();
            _uI = new UI();
            _loadingLabel = new LoadingLabel();
            
            InitializeComponent();
            
           
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            _loadingLabel.InitialiseLoadingLabel("Initialising TT",this,MainTab);
            Globals.userAlgos = _fileHandlers.GetADLNameList();
            Globals.instrumentInfoList = _fileHandlers.GetInstrumentInfoList();
            mainGrid.Columns[Globals.columnOneName].ReadOnly = true;
            mainGrid.DefaultCellStyle.SelectionBackColor = mainGrid.DefaultCellStyle.BackColor;
            mainGrid.DefaultCellStyle.SelectionForeColor = mainGrid.DefaultCellStyle.ForeColor;
            
            // if this is null then file was empty
            Globals.algoNameWithTemplateList = _fileHandlers.FetchJsonFromFile();
        }

        public void PopulateMarketIdAndDisconnectActionDictionary()
        {

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
                _loadingLabel.ChangeLoadingLabelText("TT.NET SDK INITIALIZED");
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
            _loadingLabel.ChangeLoadingLabelText("TTAPIStatusUpdate: " + e.ToString());
            if (e.IsReady == false)
            {
                // TODO: Do any connection lost processing here
                return;
            }
            m_dispatcher = tt_net_sdk.Dispatcher.Current;
            if(Globals.instrumentInfoList == null)
            {
                Globals.loadingLabel.Text = "Status: One or more required columns of instruments in \"InstrumentsToBeFetched.csv\" are empty or null.";
                return;
            }
            if(Globals.userAlgos.Count == 0)
            {
                Globals.loadingLabel.Text = "Status: No Algos in \"ADLsToBeFetched.txt\" found.";
                return;
            }
            foreach(var instrumentInfo in Globals.instrumentInfoList)
            {
                C_InstrumentLookup c_InstrumentLookup = new C_InstrumentLookup(m_dispatcher,instrumentInfo);
            }
            
            

            foreach(var adlName in Globals.userAlgos)
            {
                algoLookup(adlName);
            }
            // Get the accounts
            Globals.m_accounts = m_api.Accounts;
            foreach(var account in Globals.m_accounts)
            {
                Globals._accounts.Add(account.AccountName);
            }
            
            

        }

        void algoLookup(string algoName)
        {
            C_AlgoLookup_TradeSubscription c_AlgoLookup_TradeSubscription = new C_AlgoLookup_TradeSubscription(m_dispatcher, algoName);

            if (!Globals.algoNameWithTradeSubscription.ContainsKey(algoName))
            {
                Globals.algoNameWithTradeSubscription.Add(algoName, c_AlgoLookup_TradeSubscription);

            }

        }

        #endregion


        public static void ShowMainGrid()
        {
            if(Globals.m_isOrderBookDownloaded && Globals.instrumentInfoList.Count() == Globals.instrumentsLookedUp && Globals.userAlgos.Count() == Globals.ADLsLookedUp)
            {
                ShowMainTab();
                UpdateAdlDropdownSource();
            }

        }



        #region UI


        private static void ShowMainTab()
        {
            Globals.loadingLabel.Hide();
            MainTab.Show();
        }


        private static void UpdateAdlDropdownSource()
        {
            if (mainGrid.InvokeRequired)
            {
                mainGrid.Invoke(new Action(UpdateAdlDropdownSource));
                return;
            }

            var adlColumn = mainGrid.Columns[Globals.columnThreeName] as DataGridViewComboBoxColumn;
            if (adlColumn != null)
            {
                adlColumn.Items.Clear();
                foreach(var algo in Globals.algoFound)
                { 
                    adlColumn.Items.Add(algo);
                }
            }
            mainGrid.Columns[Globals.columnThreeName].ReadOnly = false;
        }

        private void NeonFeedButton_Click(object sender, EventArgs e)
        {
            //TODO : Connect Neon Feed
        }


        private void del_btn_Click(object sender, EventArgs e)
        {
            _buttonEvents.DeleteRowsInMainGrid(sender, e,mainGrid,MainTab);
            
        }

        private void add_btn_Click(object sender, EventArgs e)
        {
            _buttonEvents.AddRowInMainGrid(mainGrid);
        }

        private void mainGrid_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            bool createParamGrid = _uI.CellValueChanged(sender, e,mainGrid,MainTab);
            if(createParamGrid)
            {
                var row = mainGrid.Rows[e.RowIndex];
                string serial = row.Cells[Globals.columnOneName].Value.ToString();
                var feedValue = row.Cells[Globals.columnTwoName].Value?.ToString();
                var adlName = row.Cells[Globals.columnThreeName].Value?.ToString();
                _uI.CreateTabWithLabels(serial, feedValue, adlName,MainTab);
            }
        }

        private void mainGrid_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (mainGrid.IsCurrentCellDirty)
            {
                // Get current column
                int colIndex = mainGrid.CurrentCell.ColumnIndex;
                if (mainGrid.Columns[colIndex].Name == Globals.columnFourName || mainGrid.Columns[colIndex].Name == Globals.columnZeroName)
                {
                    mainGrid.CommitEdit(DataGridViewDataErrorContexts.Commit);
                }
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(Globals.tabIndexWithSiteOrderKey.Keys.Count>0)
            {
                MessageBox.Show("Remove all orders before closing the app.");
                e.Cancel = true;
                return;
            }
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
