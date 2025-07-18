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

        private DataGridView _dataGridView = null;
        private string _adlName = null;
        private string _feedName = null;
        public TabInfo(DataGridView dataGridView, string adlName , string feedName) 
        {
            _dataGridView = dataGridView;
            _adlName = adlName;
            _feedName = feedName;
        }
    }
}
