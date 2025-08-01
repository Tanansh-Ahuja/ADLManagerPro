﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ADLManagerPro
{
    public class PriceConsumer
    {
        private readonly ConcurrentDictionary<string, int> _processingFlag = new ConcurrentDictionary<string, int>(); // 0/1 per contract
        private string _feedName = string.Empty;
        private double _previousPrice = double.NaN;
        public PriceConsumer(string feedName)
        {
            _feedName = feedName;
            _ = Task.Run(() => Subscribe(feedName));
            //Subscribe(feedName);
        }
        public void Subscribe(string feedName)
        {
            PriceSimulator.PriceChanged += (id, price) =>
            {
                if (id != feedName) return;

                Globals.feedNameWithLatestPrice[id] = price;
                //price change

                //Console.WriteLine($"{feedName}-{price}");
                if(_previousPrice == double.NaN || _previousPrice != price)
                {
                    _previousPrice = price;
                    if(Globals.feedNameWithRowIndex.ContainsKey(feedName))
                    {
                        List<string> rowIndexes = Globals.feedNameWithRowIndex[feedName];

                        if(rowIndexes != null && rowIndexes.Count > 0)
                        {
                            foreach (string rowIndex in rowIndexes)
                            {


                                if(Globals.tabNameWithSiteOrderKey.ContainsKey(rowIndex) && Globals.tabNameWithTabInfo.ContainsKey(rowIndex))
                                {


                                    string siteOrderKey = Globals.tabNameWithSiteOrderKey[rowIndex];
                                    TabInfo tabInfo = Globals.tabNameWithTabInfo[rowIndex];
                                    string adlName = Globals.tabNameWithTabInfo[rowIndex]._adlName;
                                    if(!tabInfo._lag)
                                    {
                                        Console.WriteLine("No lag");
                                        if(Globals.algoNameWithTradeSubscription.ContainsKey(adlName))
                                        {
                                            Globals.algoNameWithTradeSubscription[adlName].UpdateAlgoOrderPrice(siteOrderKey,price);
                                        }
                                    }
                                    else
                                    {
                                        tabInfo._laggedPrice = price;
                                        DateTime now = DateTime.Now;
                                        string timeWithMilliseconds = now.ToString("HH:mm:ss.fff");
                                        Console.WriteLine($"lag : {tabInfo._laggedPrice} {timeWithMilliseconds}");
                                    }

                                }
                            }
                        }
                    }
                }
                


                // start a processor for this contract if not already running
                //if (_processingFlag.TryAdd(id, 1))
                //{
                //    _ = Task.Run(() => ProcessLoop(id));
                //}

            };
        }

        private async Task ProcessLoop(string feedName)
        {
            try
            {
                while (true)
                {
                    // snapshot
                    double val = Globals.feedNameWithLatestPrice[feedName];

                    await HeavyWorkAsync(feedName, val); // >10ms allowed

                    // if nobody updated during processing, we’re caught up -> exit
                    if (val == Globals.feedNameWithLatestPrice[feedName])
                        break;
                }
            }
            finally
            {
                _processingFlag.TryRemove(feedName, out _);
            }
        }

        private Task HeavyWorkAsync(string feedName, double price)
        {
            //Yaha pe karo tab ke ander param grid ko change
            
            //Console.WriteLine($"[{feedName}] Processing {price:F2}");
            return Task.Delay(50); // simulate heavy work
        }
    }
}
