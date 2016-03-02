namespace TestForms
{
    partial class SourceForm
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
            this.label1 = new System.Windows.Forms.Label();
            this.txtIPort = new System.Windows.Forms.TextBox();
            this.btnCreate = new System.Windows.Forms.Button();
            this.btnInfo = new System.Windows.Forms.Button();
            this.btnRules = new System.Windows.Forms.Button();
            this.btnPlayers = new System.Windows.Forms.Button();
            this.txtConsole = new System.Windows.Forms.TextBox();
            this.txtQuery = new System.Windows.Forms.TextBox();
            this.btnQuery = new System.Windows.Forms.Button();
            this.btnFakeRcon = new System.Windows.Forms.Button();
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.btnPassword = new System.Windows.Forms.Button();
            this.btnConnectLog = new System.Windows.Forms.Button();
            this.btnDisconnectLog = new System.Windows.Forms.Button();
            this.txtRcon = new System.Windows.Forms.TextBox();
            this.btnRcon = new System.Windows.Forms.Button();
            this.btnDisconnectRcon = new System.Windows.Forms.Button();
            this.btnConnectRcon = new System.Windows.Forms.Button();
            this.btnPing = new System.Windows.Forms.Button();
            this.txtIPAddress = new System.Windows.Forms.TextBox();
            this.btnStatus = new System.Windows.Forms.Button();
            this.btnCvar = new System.Windows.Forms.Button();
            this.txtCvar = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(50, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "IP:PORT";
            // 
            // txtIPort
            // 
            this.txtIPort.Location = new System.Drawing.Point(68, 12);
            this.txtIPort.Name = "txtIPort";
            this.txtIPort.Size = new System.Drawing.Size(175, 20);
            this.txtIPort.TabIndex = 1;
            // 
            // btnCreate
            // 
            this.btnCreate.Location = new System.Drawing.Point(323, 10);
            this.btnCreate.Name = "btnCreate";
            this.btnCreate.Size = new System.Drawing.Size(58, 23);
            this.btnCreate.TabIndex = 3;
            this.btnCreate.Text = "Create";
            this.btnCreate.UseVisualStyleBackColor = true;
            this.btnCreate.Click += new System.EventHandler(this.btnCreate_Click);
            // 
            // btnInfo
            // 
            this.btnInfo.Location = new System.Drawing.Point(387, 10);
            this.btnInfo.Name = "btnInfo";
            this.btnInfo.Size = new System.Drawing.Size(50, 23);
            this.btnInfo.TabIndex = 4;
            this.btnInfo.Text = "Info";
            this.btnInfo.UseVisualStyleBackColor = true;
            this.btnInfo.Click += new System.EventHandler(this.btnInfo_Click);
            // 
            // btnRules
            // 
            this.btnRules.Location = new System.Drawing.Point(443, 10);
            this.btnRules.Name = "btnRules";
            this.btnRules.Size = new System.Drawing.Size(58, 23);
            this.btnRules.TabIndex = 5;
            this.btnRules.Text = "Rules";
            this.btnRules.UseVisualStyleBackColor = true;
            this.btnRules.Click += new System.EventHandler(this.btnRules_Click);
            // 
            // btnPlayers
            // 
            this.btnPlayers.Location = new System.Drawing.Point(507, 10);
            this.btnPlayers.Name = "btnPlayers";
            this.btnPlayers.Size = new System.Drawing.Size(53, 23);
            this.btnPlayers.TabIndex = 6;
            this.btnPlayers.Text = "Players";
            this.btnPlayers.UseVisualStyleBackColor = true;
            this.btnPlayers.Click += new System.EventHandler(this.btnPlayers_Click);
            // 
            // txtConsole
            // 
            this.txtConsole.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtConsole.BackColor = System.Drawing.Color.White;
            this.txtConsole.Location = new System.Drawing.Point(12, 142);
            this.txtConsole.Multiline = true;
            this.txtConsole.Name = "txtConsole";
            this.txtConsole.ReadOnly = true;
            this.txtConsole.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtConsole.Size = new System.Drawing.Size(629, 417);
            this.txtConsole.TabIndex = 0;
            // 
            // txtQuery
            // 
            this.txtQuery.Location = new System.Drawing.Point(12, 38);
            this.txtQuery.Name = "txtQuery";
            this.txtQuery.Size = new System.Drawing.Size(548, 20);
            this.txtQuery.TabIndex = 8;
            // 
            // btnQuery
            // 
            this.btnQuery.Location = new System.Drawing.Point(566, 36);
            this.btnQuery.Name = "btnQuery";
            this.btnQuery.Size = new System.Drawing.Size(75, 23);
            this.btnQuery.TabIndex = 9;
            this.btnQuery.Text = "Query";
            this.btnQuery.UseVisualStyleBackColor = true;
            this.btnQuery.Click += new System.EventHandler(this.btnQuery_Click);
            // 
            // btnFakeRcon
            // 
            this.btnFakeRcon.Location = new System.Drawing.Point(566, 10);
            this.btnFakeRcon.Name = "btnFakeRcon";
            this.btnFakeRcon.Size = new System.Drawing.Size(75, 23);
            this.btnFakeRcon.TabIndex = 7;
            this.btnFakeRcon.Text = "Fake Rcon";
            this.btnFakeRcon.UseVisualStyleBackColor = true;
            this.btnFakeRcon.Click += new System.EventHandler(this.btnFakeRcon_Click);
            // 
            // txtPassword
            // 
            this.txtPassword.Location = new System.Drawing.Point(12, 64);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.Size = new System.Drawing.Size(157, 20);
            this.txtPassword.TabIndex = 10;
            // 
            // btnPassword
            // 
            this.btnPassword.Location = new System.Drawing.Point(175, 62);
            this.btnPassword.Name = "btnPassword";
            this.btnPassword.Size = new System.Drawing.Size(82, 23);
            this.btnPassword.TabIndex = 11;
            this.btnPassword.Text = "Set Pass";
            this.btnPassword.UseVisualStyleBackColor = true;
            this.btnPassword.Click += new System.EventHandler(this.btnPassword_Click);
            // 
            // btnConnectLog
            // 
            this.btnConnectLog.Location = new System.Drawing.Point(206, 88);
            this.btnConnectLog.Name = "btnConnectLog";
            this.btnConnectLog.Size = new System.Drawing.Size(82, 23);
            this.btnConnectLog.TabIndex = 14;
            this.btnConnectLog.Text = "Connect Log";
            this.btnConnectLog.UseVisualStyleBackColor = true;
            this.btnConnectLog.Click += new System.EventHandler(this.btnConnectLog_Click);
            // 
            // btnDisconnectLog
            // 
            this.btnDisconnectLog.Location = new System.Drawing.Point(294, 88);
            this.btnDisconnectLog.Name = "btnDisconnectLog";
            this.btnDisconnectLog.Size = new System.Drawing.Size(86, 23);
            this.btnDisconnectLog.TabIndex = 15;
            this.btnDisconnectLog.Text = "Disonnect Log";
            this.btnDisconnectLog.UseVisualStyleBackColor = true;
            this.btnDisconnectLog.Click += new System.EventHandler(this.btnDisconnectLog_Click);
            // 
            // txtRcon
            // 
            this.txtRcon.Location = new System.Drawing.Point(12, 116);
            this.txtRcon.Name = "txtRcon";
            this.txtRcon.Size = new System.Drawing.Size(548, 20);
            this.txtRcon.TabIndex = 16;
            // 
            // btnRcon
            // 
            this.btnRcon.Location = new System.Drawing.Point(566, 114);
            this.btnRcon.Name = "btnRcon";
            this.btnRcon.Size = new System.Drawing.Size(75, 23);
            this.btnRcon.TabIndex = 17;
            this.btnRcon.Text = "Rcon";
            this.btnRcon.UseVisualStyleBackColor = true;
            this.btnRcon.Click += new System.EventHandler(this.btnRcon_Click);
            // 
            // btnDisconnectRcon
            // 
            this.btnDisconnectRcon.Location = new System.Drawing.Point(355, 62);
            this.btnDisconnectRcon.Name = "btnDisconnectRcon";
            this.btnDisconnectRcon.Size = new System.Drawing.Size(82, 23);
            this.btnDisconnectRcon.TabIndex = 13;
            this.btnDisconnectRcon.Text = "Disconnect";
            this.btnDisconnectRcon.UseVisualStyleBackColor = true;
            this.btnDisconnectRcon.Click += new System.EventHandler(this.btnDisconnectRcon_Click);
            // 
            // btnConnectRcon
            // 
            this.btnConnectRcon.Location = new System.Drawing.Point(259, 62);
            this.btnConnectRcon.Name = "btnConnectRcon";
            this.btnConnectRcon.Size = new System.Drawing.Size(93, 23);
            this.btnConnectRcon.TabIndex = 12;
            this.btnConnectRcon.Text = "Connect Rcon";
            this.btnConnectRcon.UseVisualStyleBackColor = true;
            this.btnConnectRcon.Click += new System.EventHandler(this.btnConnectRcon_Click);
            // 
            // btnPing
            // 
            this.btnPing.Location = new System.Drawing.Point(259, 10);
            this.btnPing.Name = "btnPing";
            this.btnPing.Size = new System.Drawing.Size(58, 23);
            this.btnPing.TabIndex = 2;
            this.btnPing.Text = "Ping";
            this.btnPing.UseVisualStyleBackColor = true;
            this.btnPing.Click += new System.EventHandler(this.btnPing_Click);
            // 
            // txtIPAddress
            // 
            this.txtIPAddress.Location = new System.Drawing.Point(12, 90);
            this.txtIPAddress.Name = "txtIPAddress";
            this.txtIPAddress.Size = new System.Drawing.Size(188, 20);
            this.txtIPAddress.TabIndex = 18;
            // 
            // btnStatus
            // 
            this.btnStatus.Location = new System.Drawing.Point(386, 88);
            this.btnStatus.Name = "btnStatus";
            this.btnStatus.Size = new System.Drawing.Size(82, 23);
            this.btnStatus.TabIndex = 19;
            this.btnStatus.Text = "Status";
            this.btnStatus.UseVisualStyleBackColor = true;
            this.btnStatus.Click += new System.EventHandler(this.btnStatus_Click);
            // 
            // btnCvar
            // 
            this.btnCvar.Location = new System.Drawing.Point(596, 62);
            this.btnCvar.Name = "btnCvar";
            this.btnCvar.Size = new System.Drawing.Size(45, 23);
            this.btnCvar.TabIndex = 21;
            this.btnCvar.Text = "Cvar";
            this.btnCvar.UseVisualStyleBackColor = true;
            this.btnCvar.Click += new System.EventHandler(this.btnCvar_Click);
            // 
            // txtCvar
            // 
            this.txtCvar.Location = new System.Drawing.Point(443, 64);
            this.txtCvar.Name = "txtCvar";
            this.txtCvar.Size = new System.Drawing.Size(147, 20);
            this.txtCvar.TabIndex = 20;
            // 
            // SourceForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(653, 571);
            this.Controls.Add(this.btnCvar);
            this.Controls.Add(this.txtCvar);
            this.Controls.Add(this.btnStatus);
            this.Controls.Add(this.txtIPAddress);
            this.Controls.Add(this.btnPing);
            this.Controls.Add(this.btnConnectRcon);
            this.Controls.Add(this.btnDisconnectRcon);
            this.Controls.Add(this.btnRcon);
            this.Controls.Add(this.txtRcon);
            this.Controls.Add(this.btnDisconnectLog);
            this.Controls.Add(this.btnConnectLog);
            this.Controls.Add(this.btnPassword);
            this.Controls.Add(this.txtPassword);
            this.Controls.Add(this.btnFakeRcon);
            this.Controls.Add(this.btnQuery);
            this.Controls.Add(this.txtQuery);
            this.Controls.Add(this.txtConsole);
            this.Controls.Add(this.btnPlayers);
            this.Controls.Add(this.btnRules);
            this.Controls.Add(this.btnInfo);
            this.Controls.Add(this.btnCreate);
            this.Controls.Add(this.txtIPort);
            this.Controls.Add(this.label1);
            this.Name = "SourceForm";
            this.Text = "SourceForm Testings";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.TestingsForm_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtIPort;
        private System.Windows.Forms.Button btnCreate;
        private System.Windows.Forms.Button btnInfo;
        private System.Windows.Forms.Button btnRules;
        private System.Windows.Forms.Button btnPlayers;
        private System.Windows.Forms.TextBox txtConsole;
        private System.Windows.Forms.TextBox txtQuery;
        private System.Windows.Forms.Button btnQuery;
        private System.Windows.Forms.Button btnFakeRcon;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.Button btnPassword;
        private System.Windows.Forms.Button btnConnectLog;
        private System.Windows.Forms.Button btnDisconnectLog;
        private System.Windows.Forms.TextBox txtRcon;
        private System.Windows.Forms.Button btnRcon;
        private System.Windows.Forms.Button btnDisconnectRcon;
        private System.Windows.Forms.Button btnConnectRcon;
        private System.Windows.Forms.Button btnPing;
        private System.Windows.Forms.TextBox txtIPAddress;
        private System.Windows.Forms.Button btnStatus;
        private System.Windows.Forms.Button btnCvar;
        private System.Windows.Forms.TextBox txtCvar;

    }
}

