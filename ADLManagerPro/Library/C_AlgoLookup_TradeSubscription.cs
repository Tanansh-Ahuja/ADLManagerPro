using ADLManager;
using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using tt_net_sdk;

namespace ADLManagerPro
{
    public class C_AlgoLookup_TradeSubscription
    {
        private Algo m_algo = null;
        
        private bool m_isDisposed = false;
        private object m_Lock = new object();
        private object m_PriceUpdateLock = new object();
        private AlgoTradeSubscription m_algoTradeSubscription = null;
        private static bool orderBookDownloadRequested = false;
        private ManualResetEvent mre = new ManualResetEvent(false);
        private AlgoLookupSubscription m_algoLookupSubscription = null;
        private Dispatcher m_dispatcher = null;
     
        
        private string _algoName = string.Empty;
        //private bool orderSent = false;
        
        public C_AlgoLookup_TradeSubscription(Dispatcher dispatcher, string algoName) 
        {
            try
            {
                m_dispatcher = dispatcher;
                _algoName = algoName;
                m_algoLookupSubscription = new AlgoLookupSubscription(m_dispatcher, algoName);
                m_algoLookupSubscription.OnData += AlgoLookupSubscription_OnData;
                m_algoLookupSubscription.GetAsync();

            }
            catch
            {
                MessageBox.Show($"Error occured while initialising algo: {algoName}. Shutting down.");
                HelperFunctions.ShutEverythingDown();
            }


        }
        private void AlgoLookupSubscription_OnData(object sender, AlgoLookupEventArgs e)
        {
            try
            {
                Globals.ADLsLookedUp++;
                if (e.Event == ProductDataEvent.Found)
                {
                    if (!Globals.algoFound.Contains(e.AlgoLookup.Algo.Alias))
                        Globals.algoFound.Add(e.AlgoLookup.Algo.Alias); //only populate those algos that have been found

                    m_algo = e.AlgoLookup.Algo;
                    if(!Globals.algos.Contains(m_algo))
                    {
                        Globals.algos.Add(m_algo);
                        Console.WriteLine("Algo Instrument Found: {0}", e.AlgoLookup.Algo.Alias);
                        Globals.loadingLabel.Text = "Status: Algo Instrument Found: " + e.AlgoLookup.Algo.Alias;


                    

                        string algoName = e.AlgoLookup.Algo.Alias;

                        #region Populate dictionaries
                        var userparamListWithType = new List<(string paramName, ParameterType)>();
                        var orderProfileListWithType = new List<(string paramName, ParameterType)>();

                        var userparamList = new List<string>();
                        var orderProfileList = new List<string>();

                        Dictionary<string,ParameterType> paramNameWithType = new Dictionary<string,ParameterType>();
                        foreach (var item in e.AlgoLookup.Algo.AlgoParameters)
                        {

                            Console.WriteLine($"ParameterName: {item.Name}  type: {item.Type} isRequired: {item.IsRequired} field: {item.FieldLocation}");
                            ParameterType type;

                            if (item.Type == "Int_t")
                                type = ParameterType.Int;
                            else if (item.Type == "Float_t")
                                type = ParameterType.Float;
                            else if (item.Type == "String_t")
                                type = ParameterType.String;
                            else if (item.Type == "Boolean_t")
                                type = ParameterType.Bool;
                            else
                            {
                                if (item.EnumClass == "tt_net_sdk.OrderSide")
                                {
                                    type = ParameterType.BuySell;
                                }
                                else
                                {
                                    Console.WriteLine("Converted " + item.Type + " to string.");
                                    type = ParameterType.String;
                                }
                            }

                            if(!Globals.algoWithParamNameWithParamType.ContainsKey(algoName))
                            {
                                //Algo not there, with all params, needs population
                                paramNameWithType.Add(item.Name, type);
                            }

                            if (item.IsRequired == "true")
                            {
                                if (item.FieldLocation.ToString() == "UserParameters")
                                {
                                    userparamListWithType.Add((item.Name, type));
                                    userparamList.Add(item.Name);

                                }
                                else if (item.FieldLocation.ToString() == "OrderProfile")
                                {
                                    orderProfileListWithType.Add((item.Name, type));
                                    orderProfileList.Add(item.Name);
                                }
                            }
                        }
                        if (!Globals.algoWithParamNameWithParamType.ContainsKey(algoName))
                        {
                            //Algo not there, with all params, needs population
                            Globals.algoWithParamNameWithParamType.Add(algoName, paramNameWithType);
                        }

                        AdlParameters adlParameters = new AdlParameters(userparamListWithType, orderProfileListWithType, userparamList, orderProfileList);

                        if(!Globals.algoNameWithParameters.ContainsKey(algoName))
                        {
                            Globals.algoNameWithParameters.Add(algoName, adlParameters);
                        }

                        #endregion

                        // Create an Algo TradeSubscription to listen for order / fill events only for orders submitted through it
                        m_algoTradeSubscription = new AlgoTradeSubscription(m_dispatcher, m_algo);

                        m_algoTradeSubscription.OrderUpdated += new EventHandler<OrderUpdatedEventArgs>(m_algoTradeSubscription_OrderUpdated);
                        m_algoTradeSubscription.OrderAdded += new EventHandler<OrderAddedEventArgs>(m_algoTradeSubscription_OrderAdded);
                        m_algoTradeSubscription.OrderDeleted += new EventHandler<OrderDeletedEventArgs>(m_algoTradeSubscription_OrderDeleted);
                        m_algoTradeSubscription.OrderFilled += new EventHandler<OrderFilledEventArgs>(m_algoTradeSubscription_OrderFilled);
                        m_algoTradeSubscription.OrderRejected += new EventHandler<OrderRejectedEventArgs>(m_algoTradeSubscription_OrderRejected);
                
                        if(!orderBookDownloadRequested)
                        {
                            m_algoTradeSubscription.OrderBookDownload += new EventHandler<OrderBookDownloadEventArgs>(m_algoTradeSubscription_OrderBookDownload);
                            orderBookDownloadRequested = true;
                        }
                
                        m_algoTradeSubscription.Start();
                    

                    }
                    mre.Set();

                }
                else if (e.Event == ProductDataEvent.NotAllowed)
                {
                    Console.WriteLine("Not Allowed : Please check your Token access");
                }
                else
                {
                    // Algo Instrument was not found and TT API has given up looking for it
                    Console.WriteLine("Cannot find Algo instrument: {0}", e.Message);
                    Dispose();
                }
                Form1.ShowMainGrid();

            }
            catch
            {
                MessageBox.Show($"Error occured while getting data for algo. Shutting down.");
                HelperFunctions.ShutEverythingDown();
            }
        }

        public string StartAlgo(int accountIndex,Instrument m_instrument,
            Dictionary<string, object> algo_userparams, Dictionary<string, object> algo_orderprofileparams, 
            MarketId marketId, UserDisconnectAction userDisconnectAction)
        {
            try
            {
                while (m_algo == null)
                    mre.WaitOne();
                if(!Globals.algoNameWithParameters.ContainsKey(_algoName))
                {
                    return string.Empty;
                }
                foreach (var (userParameter, paramType) in Globals.algoNameWithParameters[_algoName]._adlUserParametersWithType)
                {
                
                    object value = algo_userparams[userParameter];
                    string svalue = value.ToString();
                    object result;
                    if(paramType == ParameterType.Int)
                    {
                        result = int.Parse(svalue);
      
                    }
                    else if(paramType == ParameterType.Float)
                    {
                        result = Double.Parse(svalue);
                    }
                    else if (paramType == ParameterType.Bool)
                    {
                        result = bool.Parse(svalue);
                    }
                    else
                    {
                        result = svalue;
                    }
                    algo_userparams[userParameter] = result;
                }
                OrderProfile algo_op = m_algo.GetOrderProfile();
                /*   algo_op.LimitPrice = m_price;
                   algo_op.OrderQuantity = Quantity.FromDecimal(m_instrument, 5);*/
                algo_op.Side = OrderSide.Buy;
                //TODO : put below two parameters as user input for every algo
                algo_op.UserDisconnectAction = userDisconnectAction;
                algo_op.CoLocation = marketId;
            
                algo_op.OrderType = OrderType.Limit;
                algo_op.Account = Globals.m_accounts.ElementAt(accountIndex);
                algo_op.UserParameters = algo_userparams;

                m_algoTradeSubscription.SendOrder(algo_op);
                DateTime now = DateTime.Now;
                string timeWithMilliseconds = now.ToString("HH:mm:ss.fff");
                Console.WriteLine("Req Order: " + timeWithMilliseconds);
                //orderSent = true;

                if (Globals.siteOrderKeyWithTabName.ContainsKey(algo_op.SiteOrderKey) &&
                    Globals.tabNameWithTabInfo.ContainsKey(Globals.siteOrderKeyWithTabName[algo_op.SiteOrderKey]))
                {
                    TabInfo tabInfo = Globals.tabNameWithTabInfo[Globals.siteOrderKeyWithTabName[algo_op.SiteOrderKey]];
                    //tabInfo._laggedPrice = Convert.ToDouble(algo_userparams["Pricing_Feed"]);
                    tabInfo._lag = true;
                    tabInfo._laggedPrice = double.NaN;
                }


                return algo_op.SiteOrderKey;
            

            }
            catch
            {
                MessageBox.Show("Error occured while sending order. Shutting down.");
                HelperFunctions.ShutEverythingDown();
                return null;
            }


        }

        public void UpdateAlgoOrderPrice(string siteOrderKey , double price)
        {
            
            try
            {
                if (siteOrderKey != null && m_algoTradeSubscription.Orders.ContainsKey(siteOrderKey))
                {
                    OrderProfile op = m_algoTradeSubscription.Orders[siteOrderKey].GetOrderProfile();
                    Dictionary<string, object> temp = (Dictionary<string, object>)op.UserParameters;
                    temp["Pricing_Feed"] = price;
                    op.UserParameters = (IReadOnlyDictionary<string,object>)temp;
                    op.Action = OrderAction.Change;
                    m_algoTradeSubscription.SendOrder(op);
                    DateTime now = DateTime.Now;
                    string timeWithMilliseconds = now.ToString("HH:mm:ss.fff");
                    Console.WriteLine("Req Update: " + price + " :: " + timeWithMilliseconds);
                    if (Globals.siteOrderKeyWithTabName.ContainsKey(op.SiteOrderKey) &&
                    Globals.tabNameWithTabInfo.ContainsKey(Globals.siteOrderKeyWithTabName[op.SiteOrderKey]))
                    {
                        TabInfo tabInfo = Globals.tabNameWithTabInfo[Globals.siteOrderKeyWithTabName[op.SiteOrderKey]];
                        //tabInfo._laggedPrice = Convert.ToDouble(algo_userparams["Pricing_Feed"]);
                        tabInfo._lag = true;
                        tabInfo._laggedPrice = double.NaN;
                    }
                }

            }
            catch
            {
                MessageBox.Show("Error occured while updating order. Shutting down.");
                HelperFunctions.ShutEverythingDown();
            }

        }

        public string DeleteAlgoOrder(string siteOrderKey)
        {
            try
            {
                if(siteOrderKey != null && m_algoTradeSubscription.Orders.ContainsKey(siteOrderKey))
                {
                    OrderProfile op = m_algoTradeSubscription.Orders[siteOrderKey].GetOrderProfile();
                    op.Action = OrderAction.Delete;
                    m_algoTradeSubscription.SendOrder(op);
                    //orderSent = false;

                }
                return string.Empty;

            }
            catch
            {
                MessageBox.Show("Error occured while deleting order. Shutting down.");
                HelperFunctions.ShutEverythingDown();
                return null;
            }
        }

        #region ADL events

        void m_algoTradeSubscription_OrderBookDownload(object sender, OrderBookDownloadEventArgs e)
        {
            try
            {
                Console.WriteLine("Orderbook downloaded...");
                Globals.m_isOrderBookDownloaded = true;
                Globals.loadingLabel.Text = "Status: Orderbook Downloaded...";
                Form1.ShowMainGrid();

            }
            catch
            {
                MessageBox.Show("Error occured while order book download. Shutting down.");
                HelperFunctions.ShutEverythingDown();
            }

        }

        void m_algoTradeSubscription_OrderRejected(object sender, OrderRejectedEventArgs e)
        {
            try
            {
               //TODO: ye order delete ke time aaya hai ya order added ke time
                HelperFunctions.OnFromTTAlgoOrderDeletion(_algoName, e.Order.SiteOrderKey);
            
            
                Console.WriteLine("\nOrderRejected for : [{0}]", e.Order.Message);

            }
            catch
            {
                MessageBox.Show("Error occured while rejecting order. Shutting down.");
                HelperFunctions.ShutEverythingDown();
            }
        }

        void m_algoTradeSubscription_OrderFilled(object sender, OrderFilledEventArgs e)
        {
            try
            {
                if (e.FillType == tt_net_sdk.FillType.Full)
                {
                    Console.WriteLine("\nOrderFullyFilled [{0}]: {1}@{2}", e.Fill.SiteOrderKey, e.Fill.Quantity, e.Fill.MatchPrice);
                }
                else
                {
                    Console.WriteLine("\nOrderPartiallyFilled [{0}]: {1}@{2}", e.Fill.SiteOrderKey, e.Fill.Quantity, e.Fill.MatchPrice);
                }

            }
            catch
            {
                MessageBox.Show("Error occured while order filled. Shutting down.");
                HelperFunctions.ShutEverythingDown();
            }
        }

        void m_algoTradeSubscription_OrderDeleted(object sender, OrderDeletedEventArgs e)
        {
            
            try
            {
                HelperFunctions.OnFromTTAlgoOrderDeletion(_algoName, e.OldOrder.SiteOrderKey);

                Console.WriteLine("\nOrderDeleted [{0}] , Message : {1}", e.OldOrder.SiteOrderKey, e.Message);

            }
            catch
            {
                MessageBox.Show("Error occured when order is deleted. Shutting down.");
                HelperFunctions.ShutEverythingDown();
            }
        }

        void m_algoTradeSubscription_OrderAdded(object sender, OrderAddedEventArgs e)
        {
            try
            {
                DateTime now = DateTime.Now;
                string timeWithMilliseconds = now.ToString("HH:mm:ss.fff");
                Console.WriteLine("Order Added: " + e.Order.UserParameters["Pricing_Feed"] + timeWithMilliseconds);
                string orderKey = e.Order.SiteOrderKey;
                if (Globals.siteOrderKeyWithTabName.ContainsKey(orderKey) &&
                    Globals.tabNameWithTabInfo.ContainsKey(Globals.siteOrderKeyWithTabName[orderKey]))
                {
                    TabInfo tabInfo = Globals.tabNameWithTabInfo[Globals.siteOrderKeyWithTabName[orderKey]];
                    //tabInfo._laggedPrice = Convert.ToDouble(algo_userparams["Pricing_Feed"]);
                    if(!double.IsNaN(tabInfo._laggedPrice))
                    {
                        tabInfo._lag = true;
                        UpdateAlgoOrderPrice(orderKey, tabInfo._laggedPrice);
                        tabInfo._laggedPrice = double.NaN;
                    }
                    else
                    {
                        tabInfo._lag = false;
                    }

                }

            }
            catch
            {
                MessageBox.Show("Error occured when order is added. Shutting down.");
                HelperFunctions.ShutEverythingDown();
            }

        }

        void m_algoTradeSubscription_OrderUpdated(object sender, OrderUpdatedEventArgs e)
        {
            try
            {
                DateTime now = DateTime.Now;
                string timeWithMilliseconds = now.ToString("HH:mm:ss.fff");
                Console.WriteLine($"Price Updated: {e.NewOrder.UserParameters["Pricing_Feed"]} {timeWithMilliseconds} from server time: {e.NewOrder.ExchTransactionTime.ToString("HH:mm:ss.fff")}");

                string orderKey = e.NewOrder.SiteOrderKey;
                if (Globals.siteOrderKeyWithTabName.ContainsKey(orderKey) &&
                    Globals.tabNameWithTabInfo.ContainsKey(Globals.siteOrderKeyWithTabName[orderKey]))
                {
                    TabInfo tabInfo = Globals.tabNameWithTabInfo[Globals.siteOrderKeyWithTabName[orderKey]];
                    //tabInfo._laggedPrice = Convert.ToDouble(algo_userparams["Pricing_Feed"]);
                    if (!double.IsNaN(tabInfo._laggedPrice))
                    {
                        tabInfo._lag = true;
                        UpdateAlgoOrderPrice(orderKey, tabInfo._laggedPrice);
                        tabInfo._laggedPrice = double.NaN;
                    }
                    else
                    {
                        tabInfo._lag = false;
                    }

                }

            }
            catch
            {
                MessageBox.Show("Error occured when order is updated. Shutting down.");
                HelperFunctions.ShutEverythingDown();
            }
        }

        #endregion

        private void Dispose()
        {
            try
            {
                lock (m_Lock)
                {

                    if (m_algoLookupSubscription != null)
                    {
                        m_algoLookupSubscription.OnData -= AlgoLookupSubscription_OnData;
                        m_algoLookupSubscription.Dispose();
                        m_algoLookupSubscription = null;
                    }
                    if (m_algoTradeSubscription != null)
                    {
                        m_algoTradeSubscription.OrderUpdated -= m_algoTradeSubscription_OrderUpdated;
                        m_algoTradeSubscription.OrderAdded -= m_algoTradeSubscription_OrderAdded;
                        m_algoTradeSubscription.OrderDeleted -= m_algoTradeSubscription_OrderDeleted;
                        m_algoTradeSubscription.OrderFilled -= m_algoTradeSubscription_OrderFilled;
                        m_algoTradeSubscription.OrderRejected -= m_algoTradeSubscription_OrderRejected;
                        m_algoTradeSubscription.Dispose();
                        m_algoTradeSubscription = null;
                    }
                    m_isDisposed = true;
                

                }

            }
            catch
            {
                MessageBox.Show("Error occured while disposing order. Shutting down.");
                HelperFunctions.ShutEverythingDown();
            }
        }
    }
}
