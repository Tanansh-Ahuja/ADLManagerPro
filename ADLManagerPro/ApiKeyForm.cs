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
        private readonly string keyFilePath = "key.txt";
        private FileHandlers _fileHandlers = null;

        public ApiKeyForm()
        {
            InitializeComponent();
            Load += ApiKeyForm_Load;
            _fileHandlers = new FileHandlers();
        }

        private void ApiKeyForm_Load(object sender, EventArgs e)
        {
            string existingKey = _fileHandlers.FetchApiKey(keyFilePath);
            if (existingKey != null)
                txtKey.Text = existingKey;
        }
        private void btnSubmit_Click(object sender, EventArgs e)
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
    }
}
