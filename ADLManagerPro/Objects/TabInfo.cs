using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ADLManagerPro
{
    public class TabInfo
    {
        
        public DataGridView _dataGridView = null;
        public string _adlName = null;
        public string _feedName = null;
        public TabPage _currentTab = null;
        public TabInfo(DataGridView dataGridView, string adlName , string feedName, TabPage currentTab) 
        {
            _dataGridView = dataGridView;
            _adlName = adlName;
            _feedName = feedName;
            _currentTab = currentTab;
            
        }
        
    }
}
