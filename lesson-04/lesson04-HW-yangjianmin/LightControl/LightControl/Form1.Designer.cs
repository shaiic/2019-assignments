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
            this.Message = new System.Windows.Forms.TextBox();
            this.Livingroom = new System.Windows.Forms.GroupBox();
            this.Toplightbox = new System.Windows.Forms.PictureBox();
            this.Toplightlabel = new System.Windows.Forms.Label();
            this.Bedroom = new System.Windows.Forms.GroupBox();
            this.Tablelightbox = new System.Windows.Forms.PictureBox();
            this.Bedlightbox = new System.Windows.Forms.PictureBox();
            this.Tablelightlabel = new System.Windows.Forms.Label();
            this.Bedlightlabel = new System.Windows.Forms.Label();
            this.Switch = new System.Windows.Forms.Button();
            this.Livingroom.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Toplightbox)).BeginInit();
            this.Bedroom.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Tablelightbox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Bedlightbox)).BeginInit();
            this.SuspendLayout();
            // 
            // Message
            // 
            this.Message.Location = new System.Drawing.Point(454, 27);
            this.Message.Multiline = true;
            this.Message.Name = "Message";
            this.Message.ReadOnly = true;
            this.Message.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.Message.Size = new System.Drawing.Size(744, 579);
            this.Message.TabIndex = 2;
            // 
            // Livingroom
            // 
            this.Livingroom.Controls.Add(this.Toplightbox);
            this.Livingroom.Controls.Add(this.Toplightlabel);
            this.Livingroom.Location = new System.Drawing.Point(12, 27);
            this.Livingroom.Name = "Livingroom";
            this.Livingroom.Size = new System.Drawing.Size(415, 230);
            this.Livingroom.TabIndex = 5;
            this.Livingroom.TabStop = false;
            this.Livingroom.Text = "客厅";
            // 
            // Toplightbox
            // 
            this.Toplightbox.Location = new System.Drawing.Point(6, 35);
            this.Toplightbox.Name = "Toplightbox";
            this.Toplightbox.Size = new System.Drawing.Size(180, 180);
            this.Toplightbox.TabIndex = 4;
            this.Toplightbox.TabStop = false;
            // 
            // Toplightlabel
            // 
            this.Toplightlabel.AutoSize = true;
            this.Toplightlabel.Location = new System.Drawing.Point(64, 17);
            this.Toplightlabel.Name = "Toplightlabel";
            this.Toplightlabel.Size = new System.Drawing.Size(29, 12);
            this.Toplightlabel.TabIndex = 3;
            this.Toplightlabel.Text = "顶灯";
            // 
            // Bedroom
            // 
            this.Bedroom.Controls.Add(this.Tablelightbox);
            this.Bedroom.Controls.Add(this.Bedlightbox);
            this.Bedroom.Controls.Add(this.Tablelightlabel);
            this.Bedroom.Controls.Add(this.Bedlightlabel);
            this.Bedroom.Location = new System.Drawing.Point(12, 262);
            this.Bedroom.Name = "Bedroom";
            this.Bedroom.Size = new System.Drawing.Size(415, 230);
            this.Bedroom.TabIndex = 7;
            this.Bedroom.TabStop = false;
            this.Bedroom.Text = "卧室";
            // 
            // Tablelightbox
            // 
            this.Tablelightbox.Location = new System.Drawing.Point(229, 41);
            this.Tablelightbox.Name = "Tablelightbox";
            this.Tablelightbox.Size = new System.Drawing.Size(180, 180);
            this.Tablelightbox.TabIndex = 10;
            this.Tablelightbox.TabStop = false;
            // 
            // Bedlightbox
            // 
            this.Bedlightbox.Location = new System.Drawing.Point(6, 41);
            this.Bedlightbox.Name = "Bedlightbox";
            this.Bedlightbox.Size = new System.Drawing.Size(180, 180);
            this.Bedlightbox.TabIndex = 9;
            this.Bedlightbox.TabStop = false;
            // 
            // Tablelightlabel
            // 
            this.Tablelightlabel.AutoSize = true;
            this.Tablelightlabel.Location = new System.Drawing.Point(300, 17);
            this.Tablelightlabel.Name = "Tablelightlabel";
            this.Tablelightlabel.Size = new System.Drawing.Size(29, 12);
            this.Tablelightlabel.TabIndex = 8;
            this.Tablelightlabel.Text = "台灯";
            // 
            // Bedlightlabel
            // 
            this.Bedlightlabel.AutoSize = true;
            this.Bedlightlabel.Location = new System.Drawing.Point(64, 17);
            this.Bedlightlabel.Name = "Bedlightlabel";
            this.Bedlightlabel.Size = new System.Drawing.Size(41, 12);
            this.Bedlightlabel.TabIndex = 7;
            this.Bedlightlabel.Text = "床头灯";
            // 
            // Switch
            // 
            this.Switch.Location = new System.Drawing.Point(144, 551);
            this.Switch.Name = "Switch";
            this.Switch.Size = new System.Drawing.Size(128, 55);
            this.Switch.TabIndex = 8;
            this.Switch.UseVisualStyleBackColor = true;
            this.Switch.Click += new System.EventHandler(this.testBtn_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1210, 618);
            this.Controls.Add(this.Switch);
            this.Controls.Add(this.Bedroom);
            this.Controls.Add(this.Livingroom);
            this.Controls.Add(this.Message);
            this.Name = "Form1";
            this.Text = "LightControl";
            this.Livingroom.ResumeLayout(false);
            this.Livingroom.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Toplightbox)).EndInit();
            this.Bedroom.ResumeLayout(false);
            this.Bedroom.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Tablelightbox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Bedlightbox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TextBox Message;
        private System.Windows.Forms.GroupBox Livingroom;
        private System.Windows.Forms.Label Toplightlabel;
        private System.Windows.Forms.GroupBox Bedroom;
        private System.Windows.Forms.Label Tablelightlabel;
        private System.Windows.Forms.Label Bedlightlabel;
        private System.Windows.Forms.Button Switch;
        private System.Windows.Forms.PictureBox Toplightbox;
        private System.Windows.Forms.PictureBox Tablelightbox;
        private System.Windows.Forms.PictureBox Bedlightbox;
    }
}

