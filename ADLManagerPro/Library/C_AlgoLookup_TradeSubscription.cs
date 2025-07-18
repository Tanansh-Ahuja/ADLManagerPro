﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ADLManager;
using tt_net_sdk;

namespace ADLManagerPro
{
    public class C_AlgoLookup_TradeSubscription
    {
        private Algo m_algo = null;
        private bool m_isDisposed = false;
        private object m_Lock = new object();
        private AlgoTradeSubscription m_algoTradeSubscription = null;
        private static bool orderBookDownloadRequested = false;
        private ManualResetEvent mre = new ManualResetEvent(false);
        private AlgoLookupSubscription m_algoLookupSubscription = null;
        private Dispatcher m_dispatcher = null;
        private bool orderAdded = false;
        
        public C_AlgoLookup_TradeSubscription(Dispatcher dispatcher, string algoName) 
        {
            m_dispatcher = dispatcher;
            m_algoLookupSubscription = new AlgoLookupSubscription(m_dispatcher, algoName);
            m_algoLookupSubscription.OnData += AlgoLookupSubscription_OnData;
            m_algoLookupSubscription.GetAsync();

        }
        private void AlgoLookupSubscription_OnData(object sender, AlgoLookupEventArgs e)
        {
            if (e.Event == ProductDataEvent.Found)
            {
                m_algo = e.AlgoLookup.Algo;
                if(!Form1.algos.Contains(m_algo))
                {
                    Form1.algos.Add(m_algo);
                    Console.WriteLine("Algo Instrument Found: {0}", e.AlgoLookup.Algo.Alias);
                    Form1.ShowMainGrid();

                    string algoName = e.AlgoLookup.Algo.Alias;

                    #region Populate dictionaries
                    var userparamListWithType = new List<(string paramName, ParameterType)>();
                    var orderProfileListWithType = new List<(string paramName, ParameterType)>();

                    var userparamList = new List<string>();
                    var orderProfileList = new List<string>();


                    foreach (var item in e.AlgoLookup.Algo.AlgoParameters)
                    {
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

                    AdlParameters adlParameters = new AdlParameters(userparamListWithType, orderProfileListWithType, userparamList, orderProfileList);

                    if(!Form1.algoNameWithParameters.ContainsKey(algoName))
                    {
                        Form1.algoNameWithParameters.Add(algoName, adlParameters);
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
        }

        public string StartAlgo(int accountIndex,Instrument m_instrument,Dictionary<string, object> algo_userparams, Dictionary<string, object> algo_orderprofileparams)
        {
            while (m_algo == null)
                mre.WaitOne();

            foreach (KeyValuePair<string, object> kvp in algo_userparams)
            {
                Console.WriteLine($"{kvp.Key} : {kvp.Value}");
            }

            OrderProfile algo_op = m_algo.GetOrderProfile(m_instrument);
            algo_op.Account = Form1.m_accounts.ElementAt(accountIndex);
            //algo_op.OrderQuantity = Quantity.FromString(m_instrument, "5");
            //algo_op.Side = OrderSide.Buy;
            //algo_op.OrderType = OrderType.Limit;
            algo_op.UserParameters = algo_userparams;
            algo_op.TimeInForce = TimeInForce.GoodTillCancel;
            algo_op.LimitPrice = Price.FromDecimal(m_instrument, Convert.ToDecimal(algo_orderprofileparams["Pricing_Feed"]));
            m_algoTradeSubscription.SendOrder(algo_op);
            return algo_op.SiteOrderKey;


        }

        public string DeleteAlgoOrder(string siteOrderKey)
        {
            if(siteOrderKey != null && m_algoTradeSubscription.Orders.ContainsKey(siteOrderKey))
            {
                OrderProfile op = m_algoTradeSubscription.Orders[siteOrderKey].GetOrderProfile();
                op.Action = OrderAction.Delete;
                m_algoTradeSubscription.SendOrder(op);
            }
            return string.Empty;
        }

        #region ADL events

        void m_algoTradeSubscription_OrderBookDownload(object sender, OrderBookDownloadEventArgs e)
        {
            Console.WriteLine("Orderbook downloaded...");
            Form1.m_isOrderBookDownloaded = true;
            Form1.ShowMainGrid();

        }

        void m_algoTradeSubscription_OrderRejected(object sender, OrderRejectedEventArgs e)
        {
            Console.WriteLine("\nOrderRejected for : [{0}]", e.Order.SiteOrderKey);
        }

        void m_algoTradeSubscription_OrderFilled(object sender, OrderFilledEventArgs e)
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

        void m_algoTradeSubscription_OrderDeleted(object sender, OrderDeletedEventArgs e)
        {
            Console.WriteLine("\nOrderDeleted [{0}] , Message : {1}", e.OldOrder.SiteOrderKey, e.Message);
        }

        void m_algoTradeSubscription_OrderAdded(object sender, OrderAddedEventArgs e)
        {
            if (e.Order.IsSynthetic)
                Console.WriteLine("\nPARENT Algo OrderAdded [{0}] for Algo : {1} with Synthetic Status : {2} ", e.Order.SiteOrderKey, e.Order.Algo.Alias, e.Order.SyntheticStatus.ToString());
            else
                Console.WriteLine("\nCHILD OrderAdded [{0}] {1}: {2}", e.Order.SiteOrderKey, e.Order.BuySell, e.Order.ToString());
        }

        void m_algoTradeSubscription_OrderUpdated(object sender, OrderUpdatedEventArgs e)
        {
            if (e.NewOrder.ExecutionType == ExecType.Restated)
                Console.WriteLine("\nAlgo Order Restated [{0}] for Algo : {1} with Synthetic Status : {2} ", e.NewOrder.SiteOrderKey, e.NewOrder.Algo.Alias, e.NewOrder.SyntheticStatus.ToString());
            else
                Console.WriteLine("\nOrderUpdated [{0}] {1}: {2}", e.NewOrder.SiteOrderKey, e.NewOrder.BuySell, e.NewOrder.ToString());
        }

        #endregion

        private void Dispose()
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
    }
}
