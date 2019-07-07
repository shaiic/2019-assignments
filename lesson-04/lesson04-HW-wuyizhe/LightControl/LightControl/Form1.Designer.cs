namespace LightControl
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
            this.button1 = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.lblTop = new System.Windows.Forms.Label();
            this.pbTop = new System.Windows.Forms.PictureBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.pbTable = new System.Windows.Forms.PictureBox();
            this.pbBed = new System.Windows.Forms.PictureBox();
            this.lbLights = new System.Windows.Forms.ListBox();
            this.btnTurnOn = new System.Windows.Forms.Button();
            this.btnTurnOff = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbTop)).BeginInit();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbTable)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbBed)).BeginInit();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Font = new System.Drawing.Font("微软雅黑", 24F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button1.Location = new System.Drawing.Point(12, 527);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(214, 79);
            this.button1.TabIndex = 0;
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(454, 336);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ReadOnly = true;
            this.textBox1.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBox1.Size = new System.Drawing.Size(744, 270);
            this.textBox1.TabIndex = 2;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.lblTop);
            this.groupBox1.Controls.Add(this.pbTop);
            this.groupBox1.Location = new System.Drawing.Point(12, 27);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(425, 289);
            this.groupBox1.TabIndex = 5;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "客厅";            
            // 
            // lblTop
            // 
            this.lblTop.AutoSize = true;
            this.lblTop.Location = new System.Drawing.Point(23, 19);
            this.lblTop.Name = "lblTop";
            this.lblTop.Size = new System.Drawing.Size(29, 12);
            this.lblTop.TabIndex = 3;
            this.lblTop.Text = "顶灯";
            // 
            // pbTop
            // 
            this.pbTop.Location = new System.Drawing.Point(23, 37);
            this.pbTop.Name = "pbTop";
            this.pbTop.Size = new System.Drawing.Size(355, 235);
            this.pbTop.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pbTop.TabIndex = 2;
            this.pbTop.TabStop = false;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.pbTable);
            this.groupBox2.Controls.Add(this.pbBed);
            this.groupBox2.Location = new System.Drawing.Point(454, 27);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(744, 289);
            this.groupBox2.TabIndex = 7;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "卧室";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(372, 19);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(29, 12);
            this.label2.TabIndex = 8;
            this.label2.Text = "台灯";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(11, 19);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 12);
            this.label1.TabIndex = 7;
            this.label1.Text = "床头灯";
            // 
            // pbTable
            // 
            this.pbTable.Location = new System.Drawing.Point(374, 37);
            this.pbTable.Name = "pbTable";
            this.pbTable.Size = new System.Drawing.Size(355, 235);
            this.pbTable.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pbTable.TabIndex = 6;
            this.pbTable.TabStop = false;
            // 
            // pbBed
            // 
            this.pbBed.Location = new System.Drawing.Point(13, 37);
            this.pbBed.Name = "pbBed";
            this.pbBed.Size = new System.Drawing.Size(355, 235);
            this.pbBed.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pbBed.TabIndex = 5;
            this.pbBed.TabStop = false;
            // 
            // lbLights
            // 
            this.lbLights.FormattingEnabled = true;
            this.lbLights.ItemHeight = 12;
            this.lbLights.Items.AddRange(new object[] {
            "顶灯",
            "床头灯",
            "台灯",
            "卧室灯",
            "客厅灯",
            "全部"});
            this.lbLights.Location = new System.Drawing.Point(13, 336);
            this.lbLights.Name = "lbLights";
            this.lbLights.SelectionMode = System.Windows.Forms.SelectionMode.MultiSimple;
            this.lbLights.Size = new System.Drawing.Size(424, 88);
            this.lbLights.TabIndex = 8;            
            // 
            // btnTurnOn
            // 
            this.btnTurnOn.Location = new System.Drawing.Point(13, 430);
            this.btnTurnOn.Name = "btnTurnOn";
            this.btnTurnOn.Size = new System.Drawing.Size(75, 23);
            this.btnTurnOn.TabIndex = 9;
            this.btnTurnOn.Text = "手动打开";
            this.btnTurnOn.UseVisualStyleBackColor = true;
            this.btnTurnOn.Click += new System.EventHandler(this.BtnTurnOn_Click);
            // 
            // btnTurnOff
            // 
            this.btnTurnOff.Location = new System.Drawing.Point(95, 430);
            this.btnTurnOff.Name = "btnTurnOff";
            this.btnTurnOff.Size = new System.Drawing.Size(75, 23);
            this.btnTurnOff.TabIndex = 10;
            this.btnTurnOff.Text = "手动关闭";
            this.btnTurnOff.UseVisualStyleBackColor = true;
            this.btnTurnOff.Click += new System.EventHandler(this.BtnTurnOff_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1210, 618);
            this.Controls.Add(this.btnTurnOff);
            this.Controls.Add(this.btnTurnOn);
            this.Controls.Add(this.lbLights);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.button1);
            this.Name = "Form1";
            this.Text = "LightControl";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbTop)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbTable)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbBed)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label lblTop;
        private System.Windows.Forms.PictureBox pbTop;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.PictureBox pbTable;
        private System.Windows.Forms.PictureBox pbBed;
        private System.Windows.Forms.ListBox lbLights;
        private System.Windows.Forms.Button btnTurnOn;
        private System.Windows.Forms.Button btnTurnOff;
    }
}

