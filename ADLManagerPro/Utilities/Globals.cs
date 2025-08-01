﻿using ADLManager;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using tt_net_sdk;

namespace ADLManagerPro
{
    public class Globals
    {
        public static readonly string columnZeroName = "Select";
        public static readonly string columnOneName = "Sno";
        public static readonly string columnTwoName = "feed";
        public static readonly string columnThreeName = "adl";
        public static readonly string columnFourName = "createTab";
        public static readonly string columnFiveName = "OrderStatus";
        
        public static int instrumentsLookedUp = 0;
        public static int ADLsLookedUp = 0;

        public static Label loadingLabel;

        public static IReadOnlyCollection<Account> m_accounts = null;
        public static bool m_isOrderBookDownloaded = false;

        public static List<Instrument> instruments = new List<Instrument>();
        public static List<Algo> algos = new List<Algo>();
        public static List<int> selectedRowIndexList = new List<int>(); 
        public static List<string> _accounts = new List<string>();
        public static List<string> userAlgos = null;
        public static List<string> algoFound = new List<string>();
        public static List<InstrumentInfo> instrumentInfoList = null;
        public static List<string> instrumentsPriceSubscribed = new List<string>();
        public static List<string> feedNames = new List<string>();


        public static Dictionary<string, AdlParameters> algoNameWithParameters = new Dictionary<string, AdlParameters>();
        public static Dictionary<string, Instrument> instrumentNameWithInstrument = new Dictionary<string, Instrument>();
        public static Dictionary<string, C_AlgoLookup_TradeSubscription> algoNameWithTradeSubscription = new Dictionary<string, C_AlgoLookup_TradeSubscription>();
        public static Dictionary<string, string> tabNameWithSiteOrderKey = new Dictionary<string, string>();
        public static Dictionary<string, string> siteOrderKeyWithTabName = new Dictionary<string, string>();
        public static Dictionary<string, TabInfo> tabNameWithTabInfo = new Dictionary<string, TabInfo>();
        public static Dictionary<string, List<Template>> algoNameWithTemplateList = new Dictionary<string, List<Template>>();
        public static Dictionary<string,Dictionary<string,ParameterType>> algoWithParamNameWithParamType = new Dictionary<string, Dictionary<string,ParameterType>>();
        public static readonly HashSet<string> SkipParamNames = new HashSet<string> { "Quoting Instrument Account", "Hedge Instrument Account", "Fast Mkt Instrument", "Fast Mkt Instrument Account" };
        //public static Dictionary<string, PriceConsumer> feedNameWithPriceConsumer = new Dictionary<string, PriceConsumer>();
        public static ConcurrentDictionary<string, double> feedNameWithLatestPrice = new ConcurrentDictionary<string, double>();
        public static Dictionary<string, List<string>> feedNameWithRowIndex = new Dictionary<string, List<string>>();
    }
}
