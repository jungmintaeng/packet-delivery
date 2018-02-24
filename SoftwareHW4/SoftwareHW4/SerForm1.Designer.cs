namespace SoftwareHW4
{
    partial class ServerForm
    {
        /// <summary>
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 디자이너에서 생성한 코드

        /// <summary>
        /// 디자이너 지원에 필요한 메서드입니다. 
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.IPTextBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.PortTextBox = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.PathTextBox = new System.Windows.Forms.TextBox();
            this.PathBtn = new System.Windows.Forms.Button();
            this.StartBtn = new System.Windows.Forms.Button();
            this.ProceedTextBox = new System.Windows.Forms.TextBox();
            this.folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(30, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "IP :";
            // 
            // IPTextBox
            // 
            this.IPTextBox.Location = new System.Drawing.Point(49, 10);
            this.IPTextBox.Name = "IPTextBox";
            this.IPTextBox.ReadOnly = true;
            this.IPTextBox.Size = new System.Drawing.Size(325, 25);
            this.IPTextBox.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(380, 13);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(42, 15);
            this.label2.TabIndex = 2;
            this.label2.Text = "port :";
            // 
            // PortTextBox
            // 
            this.PortTextBox.Location = new System.Drawing.Point(429, 10);
            this.PortTextBox.Name = "PortTextBox";
            this.PortTextBox.Size = new System.Drawing.Size(79, 25);
            this.PortTextBox.TabIndex = 3;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(13, 47);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(122, 15);
            this.label3.TabIndex = 4;
            this.label3.Text = "file storage path :";
            // 
            // PathTextBox
            // 
            this.PathTextBox.Location = new System.Drawing.Point(16, 66);
            this.PathTextBox.Name = "PathTextBox";
            this.PathTextBox.Size = new System.Drawing.Size(358, 25);
            this.PathTextBox.TabIndex = 5;
            // 
            // PathBtn
            // 
            this.PathBtn.Location = new System.Drawing.Point(383, 67);
            this.PathBtn.Name = "PathBtn";
            this.PathBtn.Size = new System.Drawing.Size(125, 23);
            this.PathBtn.TabIndex = 6;
            this.PathBtn.Text = "Path";
            this.PathBtn.UseVisualStyleBackColor = true;
            this.PathBtn.Click += new System.EventHandler(this.PathBtn_Click);
            // 
            // StartBtn
            // 
            this.StartBtn.ForeColor = System.Drawing.SystemColors.ControlText;
            this.StartBtn.Location = new System.Drawing.Point(158, 97);
            this.StartBtn.Name = "StartBtn";
            this.StartBtn.Size = new System.Drawing.Size(216, 65);
            this.StartBtn.TabIndex = 7;
            this.StartBtn.Text = "Start";
            this.StartBtn.UseVisualStyleBackColor = true;
            this.StartBtn.Click += new System.EventHandler(this.StartBtn_Click);
            // 
            // ProceedTextBox
            // 
            this.ProceedTextBox.Location = new System.Drawing.Point(13, 174);
            this.ProceedTextBox.Multiline = true;
            this.ProceedTextBox.Name = "ProceedTextBox";
            this.ProceedTextBox.ReadOnly = true;
            this.ProceedTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.ProceedTextBox.Size = new System.Drawing.Size(495, 253);
            this.ProceedTextBox.TabIndex = 8;
            // 
            // ServerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(520, 438);
            this.Controls.Add(this.ProceedTextBox);
            this.Controls.Add(this.StartBtn);
            this.Controls.Add(this.PathBtn);
            this.Controls.Add(this.PathTextBox);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.PortTextBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.IPTextBox);
            this.Controls.Add(this.label1);
            this.Name = "ServerForm";
            this.Text = "Server";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ServerForm_FormClosed);
            this.Load += new System.EventHandler(this.ServerForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox IPTextBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox PortTextBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox PathTextBox;
        private System.Windows.Forms.Button PathBtn;
        private System.Windows.Forms.Button StartBtn;
        private System.Windows.Forms.TextBox ProceedTextBox;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog;
    }
}

