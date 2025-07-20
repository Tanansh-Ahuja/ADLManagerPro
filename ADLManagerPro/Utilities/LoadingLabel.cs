using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ADLManagerPro
{
    internal class LoadingLabel
    {
        public LoadingLabel() 
        {
 
        }

        
        public Label InitialiseLoadingLabel(string showThis, Label loadingLabel, Form1 form1, TabControl MainTab)
        {
            loadingLabel = new Label()
            {
                Text = "Status: " + showThis + "...",
                AutoSize = true,
                Font = new Font("Arial", 8, FontStyle.Bold),
                Location = new Point(5, 5)
            };
            form1.Controls.Add(loadingLabel);
            loadingLabel.BringToFront();

            // Hide the main tab (you can add more components here)
            MainTab.Hide();
            return loadingLabel;
        }
        public void ChangeLoadingLabelText(string showThis,Label loadingLabel)
        {
            loadingLabel.Text = "Status: " + showThis + "...";
        }
        
    }
}
