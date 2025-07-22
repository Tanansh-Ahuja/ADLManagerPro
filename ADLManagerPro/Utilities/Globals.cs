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
    public class Globals
    {
        public static string columnZeroName = "Select";
        public static string columnOneName = "Sno";
        public static string columnTwoName = "feed";
        public static string columnThreeName = "adl";
        public static string columnFourName = "createTab";

        public static Label loadingLabel;

        public static IReadOnlyCollection<Account> m_accounts = null;
        public static bool m_isOrderBookDownloaded = false;

        public static List<Instrument> instruments = new List<Instrument>();
        public static List<Algo> algos = new List<Algo>();
        public static List<int> selectedRowIndexList = new List<int>(); //TODO optimise later
        public static List<string> _accounts = new List<string>();
        public static List<string> userAlgos = null;
        public static List<InstrumentInfo> instrumentInfoList = null;
        public static List<string> instrumentsPriceSubscribed = new List<string>();


        public static Dictionary<string, AdlParameters> algoNameWithParameters = new Dictionary<string, AdlParameters>();
        public static Dictionary<string, Instrument> instrumentNameWithInstrument = new Dictionary<string, Instrument>();
        public static Dictionary<string, C_AlgoLookup_TradeSubscription> algoNameWithTradeSubscription = new Dictionary<string, C_AlgoLookup_TradeSubscription>();
        public static Dictionary<string, string> tabIndexWithSiteOrderKey = new Dictionary<string, string>();
        public static Dictionary<string, TabInfo> tabIndexWithTabInfo = new Dictionary<string, TabInfo>();
        public static Dictionary<string, List<Template>> algoNameWithTemplateList = new Dictionary<string, List<Template>>();
        public static Dictionary<string,Dictionary<string,ParameterType>> algoWithParamNameWithParamType = new Dictionary<string, Dictionary<string,ParameterType>>();


    }
}
