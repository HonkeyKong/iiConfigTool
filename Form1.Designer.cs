namespace iiConfig
{
    partial class frmMain
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            lbDevices = new ListBox();
            label1 = new Label();
            btnSelectCFG = new Button();
            btnPushCFG = new Button();
            openFileDialog1 = new OpenFileDialog();
            lblCFGFile = new Label();
            btnReboot = new Button();
            label2 = new Label();
            cbLightGun = new CheckBox();
            btnFixClock = new Button();
            SuspendLayout();
            // 
            // lbDevices
            // 
            lbDevices.FormattingEnabled = true;
            lbDevices.ItemHeight = 15;
            lbDevices.Location = new Point(12, 26);
            lbDevices.Name = "lbDevices";
            lbDevices.Size = new Size(300, 94);
            lbDevices.TabIndex = 1;
            lbDevices.SelectedIndexChanged += listBox1_SelectedIndexChanged;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 132);
            label1.Name = "label1";
            label1.Size = new Size(0, 15);
            label1.TabIndex = 2;
            label1.TextAlign = ContentAlignment.TopRight;
            // 
            // btnSelectCFG
            // 
            btnSelectCFG.Location = new Point(318, 26);
            btnSelectCFG.Name = "btnSelectCFG";
            btnSelectCFG.Size = new Size(75, 23);
            btnSelectCFG.TabIndex = 3;
            btnSelectCFG.Text = "Select CFG";
            btnSelectCFG.UseVisualStyleBackColor = true;
            btnSelectCFG.Click += btnSelectCFG_Click;
            // 
            // btnPushCFG
            // 
            btnPushCFG.Location = new Point(318, 55);
            btnPushCFG.Name = "btnPushCFG";
            btnPushCFG.Size = new Size(75, 23);
            btnPushCFG.TabIndex = 4;
            btnPushCFG.Text = "Push CFG";
            btnPushCFG.UseVisualStyleBackColor = true;
            btnPushCFG.Click += btnPushCFG_Click;
            // 
            // openFileDialog1
            // 
            openFileDialog1.FileName = "openFileDialog1";
            // 
            // lblCFGFile
            // 
            lblCFGFile.AutoSize = true;
            lblCFGFile.Location = new Point(12, 169);
            lblCFGFile.Name = "lblCFGFile";
            lblCFGFile.Size = new Size(325, 15);
            lblCFGFile.TabIndex = 5;
            lblCFGFile.Text = "Select a CFG file with the button above, or drag && drop here.";
            // 
            // btnReboot
            // 
            btnReboot.Location = new Point(318, 84);
            btnReboot.Name = "btnReboot";
            btnReboot.Size = new Size(75, 23);
            btnReboot.TabIndex = 6;
            btnReboot.Text = "Reboot";
            btnReboot.UseVisualStyleBackColor = true;
            btnReboot.Click += btnReboot_Click;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(12, 3);
            label2.Name = "label2";
            label2.Size = new Size(105, 15);
            label2.TabIndex = 7;
            label2.Text = "Select your device:";
            // 
            // cbLightGun
            // 
            cbLightGun.AutoSize = true;
            cbLightGun.Location = new Point(318, 141);
            cbLightGun.Name = "cbLightGun";
            cbLightGun.Size = new Size(78, 19);
            cbLightGun.TabIndex = 8;
            cbLightGun.Text = "Light Gun";
            cbLightGun.UseVisualStyleBackColor = true;
            // 
            // btnFixClock
            // 
            btnFixClock.Location = new Point(320, 112);
            btnFixClock.Name = "btnFixClock";
            btnFixClock.Size = new Size(75, 23);
            btnFixClock.TabIndex = 9;
            btnFixClock.Text = "Fix Clock";
            btnFixClock.UseVisualStyleBackColor = true;
            btnFixClock.Click += btnFixClock_Click;
            // 
            // frmMain
            // 
            AllowDrop = true;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            AutoSize = true;
            ClientSize = new Size(402, 193);
            Controls.Add(btnFixClock);
            Controls.Add(cbLightGun);
            Controls.Add(label2);
            Controls.Add(btnReboot);
            Controls.Add(lblCFGFile);
            Controls.Add(btnPushCFG);
            Controls.Add(btnSelectCFG);
            Controls.Add(label1);
            Controls.Add(lbDevices);
            FormBorderStyle = FormBorderStyle.Fixed3D;
            Name = "frmMain";
            Text = "iiConfigTool v0.6 by HonkeyKong";
            Load += frmMain_Load;
            DragDrop += frmMain_DragDrop;
            DragEnter += frmMain_DragEnter;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private ListBox lbDevices;
        private Label label1;
        private Button btnSelectCFG;
        private Button btnPushCFG;
        private OpenFileDialog openFileDialog1;
        private Label lblCFGFile;
        private Button btnReboot;
        private Label label2;
        private CheckBox cbLightGun;
        private Button btnFixClock;
    }
}