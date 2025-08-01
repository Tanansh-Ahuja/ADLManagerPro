using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace ADLManagerPro
{
    public partial class ApiKeyForm : Form
    {

        public string SecretKey { get; private set; }
        public string SelectedEnvironment
        {
            get { return EnvComboBox.SelectedItem?.ToString(); }
        }
        private readonly string keyFilePath = "key.txt";
        private FileHandlers _fileHandlers = null;

        public ApiKeyForm()
        {
            try
            {
                InitializeComponent();
                Load += ApiKeyForm_Load;
                _fileHandlers = new FileHandlers();

            }
            catch
            {
                MessageBox.Show("Error occured while initialising key entering form. Shutting down.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                HelperFunctions.ShutEverythingDown();
            }
        }

        private void ApiKeyForm_Load(object sender, EventArgs e)
        {
            try
            {
                this.Icon = new Icon("Logo/FinalLogo.ico");
                EnvComboBox.SelectedItem = "UatCert";
                string existingKey = _fileHandlers.FetchApiKey(keyFilePath);
                if (existingKey != null)
                    txtKey.Text = existingKey;

            }
            catch
            {
                MessageBox.Show("Error occured while API key form load. Shutting down.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                HelperFunctions.ShutEverythingDown();
            }
        }
        private void btnSubmit_Click(object sender, EventArgs e)
        {
            try
            {
                string enteredKey = txtKey.Text.Trim();
                if (string.IsNullOrEmpty(enteredKey))
                {
                    MessageBox.Show("Please enter a valid key.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                SecretKey = enteredKey;

                DialogResult = DialogResult.OK;
                Close();

            }
            catch
            {
                MessageBox.Show("Error occured while fetching key from form. Shutting down.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                HelperFunctions.ShutEverythingDown();
            }
        }
    }
}
