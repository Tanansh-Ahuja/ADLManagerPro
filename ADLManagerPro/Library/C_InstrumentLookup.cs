using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tt_net_sdk;

namespace ADLManagerPro
{
    internal class C_InstrumentLookup
    {
        private InstrumentLookup m_instrLookupRequest = null;
        private bool m_isDisposed = false;
        private object m_Lock = new object();
        private Dispatcher m_dispatcher = null;
        public C_InstrumentLookup(Dispatcher dispatcher,string m_market, string m_prodType, string m_product, string m_alias) 
        {
            m_dispatcher = dispatcher;
            MarketId marketKey = Market.GetMarketIdFromName(m_market);
            ProductType productType = Product.GetProductTypeFromName(m_prodType);
            // lookup an instrument
            m_instrLookupRequest = new InstrumentLookup(m_dispatcher,
                        marketKey, productType, m_product, m_alias);

            m_instrLookupRequest.OnData += m_instrLookupRequest_OnData;
            m_instrLookupRequest.GetAsync();

        }
        void m_instrLookupRequest_OnData(object sender, InstrumentLookupEventArgs e)
        {
            if (e.Event == ProductDataEvent.Found)
            {
                // Instrument was found
                Instrument instrument = e.InstrumentLookup.Instrument;
                if (!Form1.instruments.Contains(instrument))
                {
                    Form1.instruments.Add(instrument);
                    if(!Form1.instrumentNameWithInstrument.ContainsKey(instrument.InstrumentDetails.Alias))
                        Form1.instrumentNameWithInstrument.Add(instrument.InstrumentDetails.Alias,instrument);
                    
                    Console.WriteLine("Found: {0}", instrument);
                    Form1.ShowMainGrid();
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

        private void Dispose()
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
    }
}
