namespace MJ_FormsServer
{
    partial class Form1
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.btnRestart = new System.Windows.Forms.Button();
            this.btnStop = new System.Windows.Forms.Button();
            this.btnExit = new System.Windows.Forms.Button();
            this.lblMJtatus = new System.Windows.Forms.Label();
            this.lblNacyStatus = new System.Windows.Forms.Label();
            this.lblDBStatus = new System.Windows.Forms.Label();
            this.btnStart = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnRestart
            // 
            this.btnRestart.Enabled = false;
            this.btnRestart.Location = new System.Drawing.Point(152, 227);
            this.btnRestart.Name = "btnRestart";
            this.btnRestart.Size = new System.Drawing.Size(50, 23);
            this.btnRestart.TabIndex = 0;
            this.btnRestart.Text = "重启";
            this.btnRestart.UseVisualStyleBackColor = true;
            this.btnRestart.Click += new System.EventHandler(this.btnRestart_Click);
            // 
            // btnStop
            // 
            this.btnStop.Enabled = false;
            this.btnStop.Location = new System.Drawing.Point(83, 227);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(50, 23);
            this.btnStop.TabIndex = 1;
            this.btnStop.Text = "停止";
            this.btnStop.UseVisualStyleBackColor = true;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // btnExit
            // 
            this.btnExit.Location = new System.Drawing.Point(221, 227);
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new System.Drawing.Size(50, 23);
            this.btnExit.TabIndex = 2;
            this.btnExit.Text = "退出";
            this.btnExit.UseVisualStyleBackColor = true;
            this.btnExit.Click += new System.EventHandler(this.btnExit_Click);
            // 
            // lblMJtatus
            // 
            this.lblMJtatus.AutoSize = true;
            this.lblMJtatus.Location = new System.Drawing.Point(12, 60);
            this.lblMJtatus.Name = "lblMJtatus";
            this.lblMJtatus.Size = new System.Drawing.Size(113, 12);
            this.lblMJtatus.TabIndex = 3;
            this.lblMJtatus.Text = "游戏服务器：未启动";
            // 
            // lblNacyStatus
            // 
            this.lblNacyStatus.AutoSize = true;
            this.lblNacyStatus.Location = new System.Drawing.Point(12, 20);
            this.lblNacyStatus.Name = "lblNacyStatus";
            this.lblNacyStatus.Size = new System.Drawing.Size(113, 12);
            this.lblNacyStatus.TabIndex = 4;
            this.lblNacyStatus.Text = "登录服务器：未启动";
            // 
            // lblDBStatus
            // 
            this.lblDBStatus.AutoSize = true;
            this.lblDBStatus.Location = new System.Drawing.Point(12, 100);
            this.lblDBStatus.Name = "lblDBStatus";
            this.lblDBStatus.Size = new System.Drawing.Size(113, 12);
            this.lblDBStatus.TabIndex = 5;
            this.lblDBStatus.Text = "数据库配置：未连接";
            // 
            // btnStart
            // 
            this.btnStart.Enabled = false;
            this.btnStart.Location = new System.Drawing.Point(14, 227);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(50, 23);
            this.btnStart.TabIndex = 6;
            this.btnStart.Text = "启动";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 262);
            this.Controls.Add(this.btnStart);
            this.Controls.Add(this.lblDBStatus);
            this.Controls.Add(this.lblNacyStatus);
            this.Controls.Add(this.lblMJtatus);
            this.Controls.Add(this.btnExit);
            this.Controls.Add(this.btnStop);
            this.Controls.Add(this.btnRestart);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.Text = "麻将服务器";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.Shown += new System.EventHandler(this.Form1_Shown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnRestart;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.Button btnExit;
        private System.Windows.Forms.Label lblMJtatus;
        private System.Windows.Forms.Label lblNacyStatus;
        private System.Windows.Forms.Label lblDBStatus;
        private System.Windows.Forms.Button btnStart;
    }
}

