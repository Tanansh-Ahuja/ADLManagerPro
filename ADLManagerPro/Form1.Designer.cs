﻿using System.Windows.Forms;

namespace ADLManagerPro
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            mainGrid = new System.Windows.Forms.DataGridView();
            this.Select = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.Sno = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.feed = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.adl = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.createTab = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.OrderStatus = new System.Windows.Forms.DataGridViewTextBoxColumn();
            MainTab = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.FeedStatus = new System.Windows.Forms.Label();
            this.del_btn = new System.Windows.Forms.Button();
            this.add_btn = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(mainGrid)).BeginInit();
            MainTab.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.SuspendLayout();
            // 
            // mainGrid
            // 
            mainGrid.AllowUserToAddRows = false;
            mainGrid.AllowUserToResizeColumns = false;
            mainGrid.AllowUserToResizeRows = false;
            mainGrid.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            mainGrid.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            mainGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            mainGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Select,
            this.Sno,
            this.feed,
            this.adl,
            this.createTab,
            this.OrderStatus});
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            mainGrid.DefaultCellStyle = dataGridViewCellStyle2;
            mainGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            mainGrid.Location = new System.Drawing.Point(3, 2);
            mainGrid.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            mainGrid.Name = "mainGrid";
            mainGrid.RowHeadersVisible = false;
            mainGrid.RowHeadersWidth = 51;
            mainGrid.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            mainGrid.Size = new System.Drawing.Size(1029, 765);
            mainGrid.TabIndex = 0;
            mainGrid.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.mainGrid_CellValueChanged);
            mainGrid.CurrentCellDirtyStateChanged += new System.EventHandler(this.mainGrid_CurrentCellDirtyStateChanged);
            // 
            // Select
            // 
            this.Select.Frozen = true;
            this.Select.HeaderText = "";
            this.Select.MinimumWidth = 6;
            this.Select.Name = "Select";
            this.Select.Width = 40;
            // 
            // Sno
            // 
            this.Sno.Frozen = true;
            this.Sno.HeaderText = "S No";
            this.Sno.MinimumWidth = 6;
            this.Sno.Name = "Sno";
            this.Sno.ReadOnly = true;
            this.Sno.Width = 75;
            // 
            // feed
            // 
            this.feed.Frozen = true;
            this.feed.HeaderText = "Feed";
            this.feed.Items.AddRange(new object[] {
            "EUR/USD",
            "EUR/GBP",
            "EUR/CHF",
            "EUR/JPY",
            "GBP/USD",
            "GBP/CHF",
            "USD/CHF"});
            this.feed.MinimumWidth = 6;
            this.feed.Name = "feed";
            this.feed.Width = 125;
            // 
            // adl
            // 
            this.adl.Frozen = true;
            this.adl.HeaderText = "ADL";
            this.adl.Items.AddRange(new object[] {
            "connecting..."});
            this.adl.MinimumWidth = 6;
            this.adl.Name = "adl";
            this.adl.Width = 150;
            // 
            // createTab
            // 
            this.createTab.Frozen = true;
            this.createTab.HeaderText = "Create Tab";
            this.createTab.MinimumWidth = 6;
            this.createTab.Name = "createTab";
            this.createTab.Width = 125;
            // 
            // OrderStatus
            // 
            this.OrderStatus.DataPropertyName = "DEACTIVATED";
            this.OrderStatus.Frozen = true;
            this.OrderStatus.HeaderText = "Order Status";
            this.OrderStatus.MinimumWidth = 6;
            this.OrderStatus.Name = "OrderStatus";
            this.OrderStatus.ReadOnly = true;
            this.OrderStatus.Width = 125;
            // 
            // MainTab
            // 
            MainTab.Controls.Add(this.tabPage1);
            MainTab.Dock = System.Windows.Forms.DockStyle.Fill;
            MainTab.Location = new System.Drawing.Point(0, 0);
            MainTab.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            MainTab.Name = "MainTab";
            MainTab.SelectedIndex = 0;
            MainTab.Size = new System.Drawing.Size(1043, 798);
            MainTab.TabIndex = 1;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.FeedStatus);
            this.tabPage1.Controls.Add(this.del_btn);
            this.tabPage1.Controls.Add(this.add_btn);
            this.tabPage1.Controls.Add(mainGrid);
            this.tabPage1.Location = new System.Drawing.Point(4, 25);
            this.tabPage1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tabPage1.Size = new System.Drawing.Size(1035, 769);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Main";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // FeedStatus
            // 
            this.FeedStatus.AutoSize = true;
            this.FeedStatus.BackColor = System.Drawing.Color.DarkRed;
            this.FeedStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FeedStatus.ForeColor = System.Drawing.SystemColors.Control;
            this.FeedStatus.Location = new System.Drawing.Point(888, 22);
            this.FeedStatus.MaximumSize = new System.Drawing.Size(200, 199);
            this.FeedStatus.MinimumSize = new System.Drawing.Size(125, 30);
            this.FeedStatus.Name = "FeedStatus";
            this.FeedStatus.Size = new System.Drawing.Size(130, 30);
            this.FeedStatus.TabIndex = 4;
            this.FeedStatus.Text = "Feed Disconnected";
            this.FeedStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // del_btn
            // 
            this.del_btn.Location = new System.Drawing.Point(889, 92);
            this.del_btn.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.del_btn.Name = "del_btn";
            this.del_btn.Size = new System.Drawing.Size(124, 34);
            this.del_btn.TabIndex = 2;
            this.del_btn.Text = "Delete Row";
            this.del_btn.UseVisualStyleBackColor = true;
            this.del_btn.Click += new System.EventHandler(this.del_btn_Click);
            // 
            // add_btn
            // 
            this.add_btn.Location = new System.Drawing.Point(889, 57);
            this.add_btn.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.add_btn.Name = "add_btn";
            this.add_btn.Size = new System.Drawing.Size(124, 31);
            this.add_btn.TabIndex = 1;
            this.add_btn.Text = "Add Row";
            this.add_btn.UseVisualStyleBackColor = true;
            this.add_btn.Click += new System.EventHandler(this.add_btn_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1043, 798);
            this.Controls.Add(MainTab);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.MaximizeBox = false;
            this.MinimumSize = new System.Drawing.Size(749, 467);
            this.Name = "Form1";
            this.Text = "ADL Manager";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(mainGrid)).EndInit();
            MainTab.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private TabPage tabPage1;
        private Button add_btn;
        private Button del_btn;
        private DataGridViewCheckBoxColumn Select;
        private DataGridViewTextBoxColumn Sno;
        private DataGridViewComboBoxColumn feed;
        private DataGridViewComboBoxColumn adl;
        private DataGridViewCheckBoxColumn createTab;
        private DataGridViewTextBoxColumn OrderStatus;
        private Label FeedStatus;
        public static DataGridView mainGrid;
        public static TabControl MainTab;
    }
}

