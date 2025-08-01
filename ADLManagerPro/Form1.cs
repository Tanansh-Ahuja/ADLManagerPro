using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using tt_net_sdk;


namespace ADLManagerPro
{
    public partial class Form1 : Form
    {

        #region Variables

        private string _appSecretKey;
        //private string currentTab = null;
        private Image gridBackgroundImage;


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
            try
            {
                _appSecretKey = appSecretKey;
                _fileHandlers = new FileHandlers();
                _buttonEvents = new ButtonEvents();
                _uI = new UI();
                _loadingLabel = new LoadingLabel();
            
                InitializeComponent();

            }
            catch
            {
                Dispose();
            }


        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                
                this.Icon = new Icon("Logo/FinalLogo.ico");
                string imagePath = @"Logo/myLogo.png";
                if (File.Exists(imagePath))
                {
                    gridBackgroundImage = Image.FromFile(imagePath);
                    mainGrid.Invalidate(); // force paint
                }
                else
                {
                    MessageBox.Show("Image not found at path: " + imagePath);
                }
                
                _loadingLabel.InitialiseLoadingLabel("Initialising TT", this, MainTab);
                Globals.userAlgos = _fileHandlers.GetADLNameList();
                Globals.instrumentInfoList = _fileHandlers.GetInstrumentInfoList();

                mainGrid.Columns[Globals.columnOneName].ReadOnly = true;
                mainGrid.DefaultCellStyle.SelectionBackColor = mainGrid.DefaultCellStyle.BackColor;
                mainGrid.DefaultCellStyle.SelectionForeColor = mainGrid.DefaultCellStyle.ForeColor;
                // if this is null then file was empty
                Globals.algoNameWithTemplateList = _fileHandlers.FetchJsonFromFile();
                mainGrid.Paint += DataGridView_Paint;

            }
            catch
            {
                Dispose();
                //HelperFunctions.ShutEverythingDown($"Error occured while loading Form./n Message: {exception.Message}");
            }

        }

        

        #region TT API

        public void Start(tt_net_sdk.TTAPIOptions apiConfig)
        {
            try
            {
                m_disp = tt_net_sdk.Dispatcher.AttachWorkerDispatcher();
                m_disp.DispatchAction(() =>
                {
                    Init(apiConfig);
                });

                m_disp.Run();

            }
            catch(Exception exception)
            {
                HelperFunctions.ShutEverythingDown($"Error occured while starting TT. \nMessage: {exception.Message}");
            }
        }

        
        public void Init(tt_net_sdk.TTAPIOptions apiConfig)
        {
            try
            {
                ApiInitializeHandler apiInitializeHandler = new ApiInitializeHandler(ttNetApiInitHandler);
                TTAPI.ShutdownCompleted += TTAPI_ShutdownCompleted;
                TTAPI.CreateTTAPI(tt_net_sdk.Dispatcher.Current, apiConfig, apiInitializeHandler);

            }
            catch(Exception exception)
            {
                HelperFunctions.ShutEverythingDown($"Error occured while starting TT. \nMessage: {exception.Message}");
            }
        }

        public void ttNetApiInitHandler(TTAPI api, ApiCreationException ex)
        {
            try
            {
                if (ex == null)
                {
                    _loadingLabel.ChangeLoadingLabelText("TT.NET SDK INITIALIZED");
                    _fileHandlers.SaveApiKey("Key.txt", _appSecretKey);

                    // Authenticate your credentials
                    m_api = api;
                    m_api.TTAPIStatusUpdate += new EventHandler<TTAPIStatusUpdateEventArgs>(m_api_TTAPIStatusUpdate);
                    m_api.Start();
                }
                else if (ex.IsRecoverable)
                {
                    HelperFunctions.ShutEverythingDown("TT.NET SDK Initialization Failed");
                }
                else
                {
                    HelperFunctions.ShutEverythingDown($"TT.NET SDK Initialization Failed: {ex.Message}");
                }

            }
            catch(Exception exception)
            { 
                HelperFunctions.ShutEverythingDown($"TT.NET SDK Initialization Failed: {exception.Message}");
            }
        }

        public void m_api_TTAPIStatusUpdate(object sender, TTAPIStatusUpdateEventArgs e)
        {
            try
            {
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
                    Globals.loadingLabel.Text = "Status: No Algos in \"ADLsToBeFetched.csv\" found.";
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
                    if (!Globals._accounts.Contains(account.AccountName))
                    {
                        Globals._accounts.Add(account.AccountName);
                    }
                }

            }
            catch(Exception exception)
            {
                HelperFunctions.ShutEverythingDown($"Error occured while status update of API handler. \nMessage: {exception.Message}");
            }
        }

        void algoLookup(string algoName)
        {
            try
            {
                C_AlgoLookup_TradeSubscription c_AlgoLookup_TradeSubscription = new C_AlgoLookup_TradeSubscription(m_dispatcher, algoName);

                if (!Globals.algoNameWithTradeSubscription.ContainsKey(algoName))
                {
                    Globals.algoNameWithTradeSubscription.Add(algoName, c_AlgoLookup_TradeSubscription);

                }

            }
            catch(Exception exception)
            {
                HelperFunctions.ShutEverythingDown($"Error occured while looking up algo. \nMessage: {exception.Message}");
            }

        }

        #endregion


        public static void ShowMainGrid()
        {
            try
            {
                if(Globals.m_isOrderBookDownloaded && Globals.instrumentInfoList.Count() == Globals.instrumentsLookedUp && Globals.userAlgos.Count() == Globals.ADLsLookedUp)
                {
                    Globals.loadingLabel.Hide();
                    MainTab.Show();
                    UpdateAdlDropdownSource();
                }

            }
            catch(Exception exception)
            {
                HelperFunctions.ShutEverythingDown($"Error occured while showing main grid. \nMessage: {exception.Message}");
            }

        }



        #region UI


        private static void UpdateAdlDropdownSource()
        {
            try
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
            catch(Exception exception)
            {
                HelperFunctions.ShutEverythingDown($"Error occured while updating ADL dropdown.\nMessage: {exception.Message}");
            }
        }

        private void NeonFeedButton_Click(object sender, EventArgs e)
        {
            //TODO : Connect Neon Feed
            /*PriceSimulator.Start(Globals.feedNames);
            //TODO : consuming price
            foreach(var feedName in Globals.feedNames)
            {
                PriceConsumer priceConsumer = new PriceConsumer(feedName);

                //Globals.feedNameWithPriceConsumer.Add(feedName, priceConsumer);
            }*/
        }


        private void del_btn_Click(object sender, EventArgs e)
        {
            try
            {
                _buttonEvents.DeleteRowsInMainGrid(sender, e,mainGrid,MainTab);

            }
            catch(Exception exception)
            {
                HelperFunctions.ShutEverythingDown($"Error occured while delete button click. \nMessage: {exception.Message}");
            }

        }

        private void add_btn_Click(object sender, EventArgs e)
        {
            try
            {
                _buttonEvents.AddRowInMainGrid(mainGrid);

            }
            catch(Exception exception)
            {
                HelperFunctions.ShutEverythingDown($"Error occured while add button click. \nMessage: {exception.Message}");
            }
        }

        private void mainGrid_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            try
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
            catch(Exception exception)
            {
                HelperFunctions.ShutEverythingDown($"Error occured while changing cell value. \nMessage: {exception.Message}");
            }
        }

        private void mainGrid_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            try
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
            catch(Exception exception)
            {
                HelperFunctions.ShutEverythingDown($"Error occured while checking state of the cell. \nMessage: {exception.Message}");
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                if(Globals.tabNameWithSiteOrderKey.Keys.Count>0)
                {
                    MessageBox.Show("Remove all orders before closing the app.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    e.Cancel = true;
                    return;
                }
                TTAPI.Shutdown();

            }
            catch(Exception exception)
            {
                HelperFunctions.ShutEverythingDown($"Error occured while closing form. \nMessage: {exception.Message}");
            }
        }


        #endregion


        #region Dispose and Shutdown
        public void DisposeEverything()
        {
            try
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
            catch(Exception exception)
            {
                HelperFunctions.ShutEverythingDown($"Error occured while disposing form. \nMessage: {exception.Message}");
            }
        }

        public void TTAPI_ShutdownCompleted(object sender, EventArgs e)
        {
            // Dispose of any other objects / resources
            Console.WriteLine("TTAPI shutdown completed");
        }


        #endregion

        private void DataGridView_Paint(object sender, PaintEventArgs e)
        {
            try
            {
                if (gridBackgroundImage == null) return;

                DataGridView dgv = sender as DataGridView;

                int maxWidth = dgv.ClientSize.Width;
                int maxHeight = dgv.ClientSize.Height;

                int imgOriginalWidth = gridBackgroundImage.Width;
                int imgOriginalHeight = gridBackgroundImage.Height;

                float ratioX = (float)maxWidth / imgOriginalWidth;
                float ratioY = (float)maxHeight / imgOriginalHeight;
                float ratio = Math.Min(ratioX, ratioY);

                int scaledWidth = (int)(imgOriginalWidth * ratio);
                int scaledHeight = (int)(imgOriginalHeight * ratio);

                int x = (maxWidth - scaledWidth) / 2;
                int y = (maxHeight - scaledHeight) / 2;

                // 🔥 Apply transparency via ColorMatrix
                float alpha = 0.2f; // 0 = fully transparent, 1 = fully opaque

                ColorMatrix matrix = new ColorMatrix
                {
                    Matrix33 = alpha // This sets the transparency
                };

                ImageAttributes attributes = new ImageAttributes();
                attributes.SetColorMatrix(matrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

                Rectangle drawRect = new Rectangle(x, y, scaledWidth, scaledHeight);

                e.Graphics.DrawImage(
                    gridBackgroundImage,
                    drawRect,
                    0, 0, imgOriginalWidth, imgOriginalHeight,
                    GraphicsUnit.Pixel,
                    attributes
                );

            }
            catch(Exception exception)
            {
                HelperFunctions.ShutEverythingDown($"Error occured while showing image. \nMessage: {exception.Message}");
            }
        }
        

    }
}
