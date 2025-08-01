﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using tt_net_sdk;

namespace ADLManagerPro
{
    public class C_InstrumentLookup
    {
        private InstrumentLookup m_instrLookupRequest = null;
        private bool m_isDisposed = false;
        private object m_Lock = new object();
        private Dispatcher m_dispatcher = null;
        private PriceSubscription m_priceSubscription = null;
        public C_InstrumentLookup(Dispatcher dispatcher,InstrumentInfo instrumentInfo) 
        {
            try
            {
                m_dispatcher = dispatcher;
                MarketId marketKey = Market.GetMarketIdFromName(instrumentInfo._m_market);
                ProductType productType = Product.GetProductTypeFromName(instrumentInfo._m_prodType);
                // lookup an instrument
                m_instrLookupRequest = new InstrumentLookup(m_dispatcher,
                            marketKey, productType, instrumentInfo._m_product, instrumentInfo._m_alias);

                m_instrLookupRequest.OnData += m_instrLookupRequest_OnData;
                m_instrLookupRequest.GetAsync();

            }
            catch(Exception exception)
            {
                HelperFunctions.ShutEverythingDown($"Error occured while initialising instrument. \nMessage: {exception.Message}");
            }

        }
        void m_instrLookupRequest_OnData(object sender, InstrumentLookupEventArgs e)
        {
            try
            {
                Globals.instrumentsLookedUp++ ;
                if (e.Event == ProductDataEvent.Found)
                {
                    // Instrument was found
                    Instrument instrument = e.InstrumentLookup.Instrument;
                    // Subscribe for market Data
                    m_priceSubscription = new PriceSubscription(instrument, m_dispatcher);
                    m_priceSubscription.Settings = new PriceSubscriptionSettings(PriceSubscriptionType.MarketDepth);
                    m_priceSubscription.FieldsUpdated += m_priceSubscription_FieldsUpdated;
                    m_priceSubscription.Start();
                    if (!Globals.instruments.Contains(instrument))
                    {
                        Globals.instruments.Add(instrument);
                        if (!Globals.instrumentNameWithInstrument.ContainsKey(instrument.InstrumentDetails.Alias))
                            Globals.instrumentNameWithInstrument.Add(instrument.InstrumentDetails.Alias, instrument);

                        Console.WriteLine("Found: {0}", instrument);
                     


                    }
                


                }
                else if (e.Event == ProductDataEvent.NotAllowed)
                {
                    Console.WriteLine("Not Allowed : Please check your Token access");
                }
                else
                {
                    // Instrument was not found and TT API has given up looking for it
                    Console.WriteLine("Cannot find instrument: {0}", e.Message);
                    Dispose();
                }

            }
            catch(Exception exception)
            {
                HelperFunctions.ShutEverythingDown($"Error occured while data look up of instrument. \nMessage: {exception.Message}");
            }
        }
        void m_priceSubscription_FieldsUpdated(object sender, FieldsUpdatedEventArgs e)
        {
            try
            {
                if(e.Error == null)
                {
                    if(!Globals.instrumentsPriceSubscribed.Contains(e.Fields.Instrument.Name))
                    {
                        Console.WriteLine($"Price for {e.Fields.Instrument.InstrumentDetails.Alias}");
                        Globals.instrumentsPriceSubscribed.Add(e.Fields.Instrument.Name);
                        Globals.loadingLabel.Text = "Status: All Instruments Found...";
                        Form1.ShowMainGrid();
                    }
                }
                else
                {
                
                    Console.WriteLine("Unrecoverable price subscription error: {0}", e.Error.Message);
                
                
                }

            }
            catch(Exception exception)
            {
                HelperFunctions.ShutEverythingDown($"Error occured while updating price subscription fields. \nMessage: {exception.Message}");
            }

        }

        private void Dispose()
        {
            try
            {
                lock(m_Lock)
                {

                    if (!m_isDisposed)
                    {
                        if (m_instrLookupRequest != null)
                        {
                            m_instrLookupRequest.OnData -= m_instrLookupRequest_OnData;
                            m_instrLookupRequest.Dispose();
                            m_instrLookupRequest = null;
                        }
                        m_isDisposed = true;
                    }

                }

            }
            catch(Exception exception)
            {
                HelperFunctions.ShutEverythingDown($"Error occured while disposing instrument. \nMessage: {exception.Message}");
            }
        }
    }
}
