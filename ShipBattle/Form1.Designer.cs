namespace ShipBattle
{
    partial class FrmLaunchGame
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmLaunchGame));
            this.lblUserName = new System.Windows.Forms.Label();
            this.txtbName = new System.Windows.Forms.TextBox();
            this.btnHost = new System.Windows.Forms.Button();
            this.btnJoin = new System.Windows.Forms.Button();
            this.pboxTitle = new System.Windows.Forms.PictureBox();
            this.lblIP = new System.Windows.Forms.Label();
            this.lnlPort = new System.Windows.Forms.Label();
            this.txtbIP = new System.Windows.Forms.TextBox();
            this.txtbPort = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.pboxTitle)).BeginInit();
            this.SuspendLayout();
            // 
            // lblUserName
            // 
            this.lblUserName.AutoSize = true;
            this.lblUserName.Location = new System.Drawing.Point(138, 243);
            this.lblUserName.Name = "lblUserName";
            this.lblUserName.Size = new System.Drawing.Size(73, 13);
            this.lblUserName.TabIndex = 1;
            this.lblUserName.Text = "Player Name :";
            // 
            // txtbName
            // 
            this.txtbName.Location = new System.Drawing.Point(217, 240);
            this.txtbName.Name = "txtbName";
            this.txtbName.Size = new System.Drawing.Size(97, 20);
            this.txtbName.TabIndex = 0;
            // 
            // btnHost
            // 
            this.btnHost.Location = new System.Drawing.Point(91, 200);
            this.btnHost.Name = "btnHost";
            this.btnHost.Size = new System.Drawing.Size(120, 26);
            this.btnHost.TabIndex = 3;
            this.btnHost.Text = "Host A Game";
            this.btnHost.UseVisualStyleBackColor = true;
            this.btnHost.Click += new System.EventHandler(this.btnHost_Click);
            // 
            // btnJoin
            // 
            this.btnJoin.Location = new System.Drawing.Point(217, 200);
            this.btnJoin.Name = "btnJoin";
            this.btnJoin.Size = new System.Drawing.Size(120, 26);
            this.btnJoin.TabIndex = 4;
            this.btnJoin.Text = "Join A Game";
            this.btnJoin.UseVisualStyleBackColor = true;
            this.btnJoin.Click += new System.EventHandler(this.btnJoin_Click);
            // 
            // pboxTitle
            // 
            this.pboxTitle.Image = global::ShipBattle.Properties.Resources.titlePicSmall;
            this.pboxTitle.InitialImage = global::ShipBattle.Properties.Resources.titlePicSmall;
            this.pboxTitle.Location = new System.Drawing.Point(66, 6);
            this.pboxTitle.Name = "pboxTitle";
            this.pboxTitle.Size = new System.Drawing.Size(299, 188);
            this.pboxTitle.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.pboxTitle.TabIndex = 0;
            this.pboxTitle.TabStop = false;
            // 
            // lblIP
            // 
            this.lblIP.AutoSize = true;
            this.lblIP.Location = new System.Drawing.Point(148, 274);
            this.lblIP.Name = "lblIP";
            this.lblIP.Size = new System.Drawing.Size(63, 13);
            this.lblIP.TabIndex = 5;
            this.lblIP.Text = "Ip Address :";
            // 
            // lnlPort
            // 
            this.lnlPort.AutoSize = true;
            this.lnlPort.Location = new System.Drawing.Point(179, 304);
            this.lnlPort.Name = "lnlPort";
            this.lnlPort.Size = new System.Drawing.Size(32, 13);
            this.lnlPort.TabIndex = 6;
            this.lnlPort.Text = "Port :";
            // 
            // txtbIP
            // 
            this.txtbIP.Location = new System.Drawing.Point(217, 274);
            this.txtbIP.Name = "txtbIP";
            this.txtbIP.Size = new System.Drawing.Size(77, 20);
            this.txtbIP.TabIndex = 1;
            // 
            // txtbPort
            // 
            this.txtbPort.Location = new System.Drawing.Point(217, 301);
            this.txtbPort.Name = "txtbPort";
            this.txtbPort.Size = new System.Drawing.Size(56, 20);
            this.txtbPort.TabIndex = 2;
            // 
            // FrmLaunchGame
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.BackgroundImage = global::ShipBattle.Properties.Resources.tileBack50;
            this.ClientSize = new System.Drawing.Size(437, 336);
            this.Controls.Add(this.txtbPort);
            this.Controls.Add(this.txtbIP);
            this.Controls.Add(this.lnlPort);
            this.Controls.Add(this.lblIP);
            this.Controls.Add(this.btnJoin);
            this.Controls.Add(this.btnHost);
            this.Controls.Add(this.txtbName);
            this.Controls.Add(this.lblUserName);
            this.Controls.Add(this.pboxTitle);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(453, 374);
            this.Name = "FrmLaunchGame";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "ShipBattle";
            ((System.ComponentModel.ISupportInitialize)(this.pboxTitle)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pboxTitle;
        private System.Windows.Forms.Label lblUserName;
        private System.Windows.Forms.TextBox txtbName;
        private System.Windows.Forms.Button btnHost;
        private System.Windows.Forms.Button btnJoin;
        private System.Windows.Forms.Label lblIP;
        private System.Windows.Forms.Label lnlPort;
        private System.Windows.Forms.TextBox txtbIP;
        private System.Windows.Forms.TextBox txtbPort;
    }
}

