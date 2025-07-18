using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

namespace ADLManagerPro
{
    public partial class LoadingForm : Form
    {
        private System.Windows.Forms.Timer animationTimer;
        private int dotCount = 1;
        public static LoadingForm Instance { get; private set; }
        public LoadingForm()
        {
            InitializeComponent();
            Instance = this; 
            this.ControlBox = false; // Optional: disables close button
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.Width = 300;
            this.Height = 100;

            Label lbl = new Label
            {
                Text = "Loading",
                Name = "lblLoading",
                AutoSize = false,
                Dock = DockStyle.Fill,
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
                Font = new System.Drawing.Font("Arial", 14)
            };
            this.Controls.Add(lbl);

            animationTimer = new System.Windows.Forms.Timer();
            animationTimer.Interval = 500;
            animationTimer.Tick += (s, e) =>
            {
                dotCount = (dotCount % 3) + 1;
                lbl.Text = "Loading" + new string('.', dotCount);
            };
        }
        public void StartAnimation()
        {
            animationTimer.Start();
        }

        public void StopAnimation()
        {
            animationTimer.Stop();
        }

        public static void CloseLoadingForm()
        {
            if (Instance != null)
            {
                Instance.Invoke(new Action(() =>
                {
                    Instance.StopAnimation();
                    Instance.Close();
                    Instance.Dispose();
                    Instance = null;
                }));
            }
        }
    }
}
