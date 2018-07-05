namespace WuDaServer
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
            this.btnStartService = new System.Windows.Forms.Button();
            this.comboBoxIP = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.textBoxStartID = new System.Windows.Forms.TextBox();
            this.textBoxStopID = new System.Windows.Forms.TextBox();
            this.btnSearch = new System.Windows.Forms.Button();
            this.btnGetTimeAndLevel = new System.Windows.Forms.Button();
            this.btnResetLevel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnStartService
            // 
            this.btnStartService.Location = new System.Drawing.Point(250, 15);
            this.btnStartService.Name = "btnStartService";
            this.btnStartService.Size = new System.Drawing.Size(71, 21);
            this.btnStartService.TabIndex = 0;
            this.btnStartService.Text = "开启服务";
            this.btnStartService.UseVisualStyleBackColor = true;
            this.btnStartService.Click += new System.EventHandler(this.btnStartService_Click);
            // 
            // comboBoxIP
            // 
            this.comboBoxIP.FormattingEnabled = true;
            this.comboBoxIP.Location = new System.Drawing.Point(102, 12);
            this.comboBoxIP.Name = "comboBoxIP";
            this.comboBoxIP.Size = new System.Drawing.Size(110, 20);
            this.comboBoxIP.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(83, 12);
            this.label1.TabIndex = 2;
            this.label1.Text = "选择服务端IP:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 63);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(83, 12);
            this.label2.TabIndex = 3;
            this.label2.Text = "输入起始ID号:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(13, 108);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(83, 12);
            this.label3.TabIndex = 4;
            this.label3.Text = "输入结束ID号:";
            // 
            // textBoxStartID
            // 
            this.textBoxStartID.Location = new System.Drawing.Point(102, 60);
            this.textBoxStartID.Name = "textBoxStartID";
            this.textBoxStartID.Size = new System.Drawing.Size(110, 21);
            this.textBoxStartID.TabIndex = 5;
            // 
            // textBoxStopID
            // 
            this.textBoxStopID.Location = new System.Drawing.Point(102, 105);
            this.textBoxStopID.Name = "textBoxStopID";
            this.textBoxStopID.Size = new System.Drawing.Size(110, 21);
            this.textBoxStopID.TabIndex = 7;
            // 
            // btnSearch
            // 
            this.btnSearch.Location = new System.Drawing.Point(238, 86);
            this.btnSearch.Name = "btnSearch";
            this.btnSearch.Size = new System.Drawing.Size(83, 23);
            this.btnSearch.TabIndex = 8;
            this.btnSearch.Text = "搜索";
            this.btnSearch.UseVisualStyleBackColor = true;
            this.btnSearch.Click += new System.EventHandler(this.btnSearch_Click);
            // 
            // btnGetTimeAndLevel
            // 
            this.btnGetTimeAndLevel.Location = new System.Drawing.Point(238, 115);
            this.btnGetTimeAndLevel.Name = "btnGetTimeAndLevel";
            this.btnGetTimeAndLevel.Size = new System.Drawing.Size(83, 23);
            this.btnGetTimeAndLevel.TabIndex = 9;
            this.btnGetTimeAndLevel.Text = "读触发时间";
            this.btnGetTimeAndLevel.UseVisualStyleBackColor = true;
            this.btnGetTimeAndLevel.Click += new System.EventHandler(this.btnGetTimeAndLevel_Click);
            // 
            // btnResetLevel
            // 
            this.btnResetLevel.Location = new System.Drawing.Point(238, 144);
            this.btnResetLevel.Name = "btnResetLevel";
            this.btnResetLevel.Size = new System.Drawing.Size(83, 23);
            this.btnResetLevel.TabIndex = 10;
            this.btnResetLevel.Text = "探测器复位";
            this.btnResetLevel.UseVisualStyleBackColor = true;
            this.btnResetLevel.Click += new System.EventHandler(this.btnResetLevel_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(345, 169);
            this.Controls.Add(this.btnResetLevel);
            this.Controls.Add(this.btnGetTimeAndLevel);
            this.Controls.Add(this.btnSearch);
            this.Controls.Add(this.textBoxStopID);
            this.Controls.Add(this.textBoxStartID);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.comboBoxIP);
            this.Controls.Add(this.btnStartService);
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(361, 208);
            this.MinimumSize = new System.Drawing.Size(361, 208);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "激光探测器通信服务软件";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnStartService;
        private System.Windows.Forms.ComboBox comboBoxIP;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBoxStartID;
        private System.Windows.Forms.TextBox textBoxStopID;
        private System.Windows.Forms.Button btnSearch;
        private System.Windows.Forms.Button btnGetTimeAndLevel;
        private System.Windows.Forms.Button btnResetLevel;
    }
}

