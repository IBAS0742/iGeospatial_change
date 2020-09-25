namespace GisForm.shapefile
{
        partial class ShowShapeFile
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
                        this.button1 = new System.Windows.Forms.Button();
                        this.textBox1 = new System.Windows.Forms.TextBox();
                        this.pictureBox1 = new System.Windows.Forms.PictureBox();
                        this.dragBtn = new System.Windows.Forms.Button();
                        this.selectBtn = new System.Windows.Forms.Button();
                        this.ZoomInBtn = new System.Windows.Forms.Button();
                        this.zoomOutBtn = new System.Windows.Forms.Button();
                        this.textBox2 = new System.Windows.Forms.TextBox();
                        this.infoBtn = new System.Windows.Forms.Button();
                        ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
                        this.SuspendLayout();
                        // 
                        // button1
                        // 
                        this.button1.Location = new System.Drawing.Point(320, 3);
                        this.button1.Name = "button1";
                        this.button1.Size = new System.Drawing.Size(57, 23);
                        this.button1.TabIndex = 1;
                        this.button1.Text = "绘制";
                        this.button1.UseVisualStyleBackColor = true;
                        this.button1.Click += new System.EventHandler(this.button1_Click);
                        // 
                        // textBox1
                        // 
                        this.textBox1.Location = new System.Drawing.Point(4, 5);
                        this.textBox1.Name = "textBox1";
                        this.textBox1.Size = new System.Drawing.Size(310, 21);
                        this.textBox1.TabIndex = 2;
                        // 
                        // pictureBox1
                        // 
                        this.pictureBox1.BackColor = System.Drawing.SystemColors.ControlDark;
                        this.pictureBox1.Location = new System.Drawing.Point(215, 32);
                        this.pictureBox1.Name = "pictureBox1";
                        this.pictureBox1.Size = new System.Drawing.Size(585, 419);
                        this.pictureBox1.TabIndex = 0;
                        this.pictureBox1.TabStop = false;
                        // 
                        // dragBtn
                        // 
                        this.dragBtn.Location = new System.Drawing.Point(437, 5);
                        this.dragBtn.Name = "dragBtn";
                        this.dragBtn.Size = new System.Drawing.Size(56, 23);
                        this.dragBtn.TabIndex = 3;
                        this.dragBtn.Text = "拖动";
                        this.dragBtn.UseVisualStyleBackColor = true;
                        this.dragBtn.Click += new System.EventHandler(this.dragBtn_Click);
                        // 
                        // selectBtn
                        // 
                        this.selectBtn.Location = new System.Drawing.Point(383, 5);
                        this.selectBtn.Name = "selectBtn";
                        this.selectBtn.Size = new System.Drawing.Size(48, 23);
                        this.selectBtn.TabIndex = 4;
                        this.selectBtn.Text = "选择";
                        this.selectBtn.UseVisualStyleBackColor = true;
                        this.selectBtn.Click += new System.EventHandler(this.selectBtn_Click);
                        // 
                        // ZoomInBtn
                        // 
                        this.ZoomInBtn.Location = new System.Drawing.Point(499, 5);
                        this.ZoomInBtn.Name = "ZoomInBtn";
                        this.ZoomInBtn.Size = new System.Drawing.Size(46, 23);
                        this.ZoomInBtn.TabIndex = 5;
                        this.ZoomInBtn.Text = "放大";
                        this.ZoomInBtn.UseVisualStyleBackColor = true;
                        this.ZoomInBtn.Click += new System.EventHandler(this.ZoomInBtn_Click);
                        // 
                        // zoomOutBtn
                        // 
                        this.zoomOutBtn.Location = new System.Drawing.Point(551, 5);
                        this.zoomOutBtn.Name = "zoomOutBtn";
                        this.zoomOutBtn.Size = new System.Drawing.Size(50, 23);
                        this.zoomOutBtn.TabIndex = 6;
                        this.zoomOutBtn.Text = "缩小";
                        this.zoomOutBtn.UseVisualStyleBackColor = true;
                        this.zoomOutBtn.Click += new System.EventHandler(this.zoomOutBtn_Click);
                        // 
                        // textBox2
                        // 
                        this.textBox2.Location = new System.Drawing.Point(4, 32);
                        this.textBox2.Multiline = true;
                        this.textBox2.Name = "textBox2";
                        this.textBox2.Size = new System.Drawing.Size(205, 419);
                        this.textBox2.TabIndex = 7;
                        // 
                        // infoBtn
                        // 
                        this.infoBtn.Location = new System.Drawing.Point(607, 5);
                        this.infoBtn.Name = "infoBtn";
                        this.infoBtn.Size = new System.Drawing.Size(48, 23);
                        this.infoBtn.TabIndex = 8;
                        this.infoBtn.Text = "信息";
                        this.infoBtn.UseVisualStyleBackColor = true;
                        this.infoBtn.Click += new System.EventHandler(this.infoBtn_Click);
                        // 
                        // ShowShapeFile
                        // 
                        this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
                        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
                        this.ClientSize = new System.Drawing.Size(800, 450);
                        this.Controls.Add(this.infoBtn);
                        this.Controls.Add(this.textBox2);
                        this.Controls.Add(this.zoomOutBtn);
                        this.Controls.Add(this.ZoomInBtn);
                        this.Controls.Add(this.selectBtn);
                        this.Controls.Add(this.dragBtn);
                        this.Controls.Add(this.pictureBox1);
                        this.Controls.Add(this.textBox1);
                        this.Controls.Add(this.button1);
                        this.Name = "ShowShapeFile";
                        this.Text = "ShowShapeFile";
                        this.Load += new System.EventHandler(this.ShowShapeFile_Load);
                        ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
                        this.ResumeLayout(false);
                        this.PerformLayout();

                }

        #endregion
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Button dragBtn;
        private System.Windows.Forms.Button selectBtn;
                private System.Windows.Forms.Button ZoomInBtn;
                private System.Windows.Forms.Button zoomOutBtn;
                private System.Windows.Forms.TextBox textBox2;
                private System.Windows.Forms.Button infoBtn;
        }
}