using System.Windows.Forms;

namespace ADLManagerPro
{
    partial class ApiKeyForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        private TextBox txtKey;
        private Button SubmitButton;
        private Label lblPrompt;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.txtKey = new System.Windows.Forms.TextBox();
            this.SubmitButton = new System.Windows.Forms.Button();
            this.lblPrompt = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // txtKey
            // 
            this.txtKey.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.txtKey.Location = new System.Drawing.Point(29, 39);
            this.txtKey.Name = "txtKey";
            this.txtKey.Size = new System.Drawing.Size(852, 34);
            this.txtKey.TabIndex = 0;
            // 
            // SubmitButton
            // 
            this.SubmitButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SubmitButton.Location = new System.Drawing.Point(416, 79);
            this.SubmitButton.Name = "SubmitButton";
            this.SubmitButton.Size = new System.Drawing.Size(98, 30);
            this.SubmitButton.TabIndex = 1;
            this.SubmitButton.Text = "Submit";
            this.SubmitButton.Click += new System.EventHandler(this.btnSubmit_Click);
            // 
            // lblPrompt
            // 
            this.lblPrompt.AutoSize = true;
            this.lblPrompt.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPrompt.Location = new System.Drawing.Point(25, 15);
            this.lblPrompt.Name = "lblPrompt";
            this.lblPrompt.Size = new System.Drawing.Size(141, 20);
            this.lblPrompt.TabIndex = 2;
            this.lblPrompt.Text = "Enter Secret Key:";
            // 
            // ApiKeyForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(938, 135);
            this.Controls.Add(this.txtKey);
            this.Controls.Add(this.SubmitButton);
            this.Controls.Add(this.lblPrompt);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ApiKeyForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "TT Secret Key";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
    }
}