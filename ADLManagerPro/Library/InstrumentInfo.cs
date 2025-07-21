using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADLManagerPro
{
    public class InstrumentInfo
    {
        public string _m_market = null;
        public string _m_prodType = null;
        public string _m_product = null;
        public string _m_alias = null;
        public InstrumentInfo(string m_market, string m_prodType, string m_product, string m_alias) 
        {
            _m_market=m_market;
            _m_prodType = m_prodType;
            _m_product=m_product;
            _m_alias=m_alias;

        }
    }
}
