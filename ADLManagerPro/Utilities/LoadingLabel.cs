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

        
        public void InitialiseLoadingLabel(string showThis, Form1 form1, TabControl MainTab)
        {
            try
            {
                Globals.loadingLabel = new Label()
                {
                    Text = "Status: " + showThis + " ...",
                    AutoSize = true,
                    Font = new Font("Arial", 11),
                    Location = new Point(5, 5)
                };
                form1.Controls.Add(Globals.loadingLabel);
                Globals.loadingLabel.BringToFront();

                // Hide the main tab (you can add more components here)
                MainTab.Hide();

            }
            catch
            {
                MessageBox.Show("Error occured while creating label. Shutting down.");
                HelperFunctions.ShutEverythingDown();
            }
        }
        public void ChangeLoadingLabelText(string showThis)
        {
            try
            {
                Globals.loadingLabel.Text = "Status: " + showThis + " ...";

            }
            catch
            {
                MessageBox.Show("Error occured while updating label value. Shutting down.");
                HelperFunctions.ShutEverythingDown();
            }
        }
        
    }
}
