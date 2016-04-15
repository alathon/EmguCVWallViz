namespace WallVizOpenCV
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
            this.components = new System.ComponentModel.Container();
            this.cameraFps = new System.Windows.Forms.Label();
            this.msTotal = new System.Windows.Forms.Label();
            this.msRetrieveBuffer = new System.Windows.Forms.Label();
            this.msConvertImg = new System.Windows.Forms.Label();
            this.msFilters = new System.Windows.Forms.Label();
            this.msBlobDetection = new System.Windows.Forms.Label();
            this.imageBox2 = new Emgu.CV.UI.ImageBox();
            this.imageBox1 = new Emgu.CV.UI.ImageBox();
            this.imageBox3 = new Emgu.CV.UI.ImageBox();
            this.imageBox4 = new Emgu.CV.UI.ImageBox();
            ((System.ComponentModel.ISupportInitialize)(this.imageBox2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.imageBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.imageBox3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.imageBox4)).BeginInit();
            this.SuspendLayout();
            // 
            // cameraFps
            // 
            this.cameraFps.AutoSize = true;
            this.cameraFps.Location = new System.Drawing.Point(757, 405);
            this.cameraFps.Name = "cameraFps";
            this.cameraFps.Size = new System.Drawing.Size(72, 13);
            this.cameraFps.TabIndex = 7;
            this.cameraFps.Text = "Camera  FPS:";
            // 
            // msTotal
            // 
            this.msTotal.AutoSize = true;
            this.msTotal.Location = new System.Drawing.Point(754, 440);
            this.msTotal.Name = "msTotal";
            this.msTotal.Size = new System.Drawing.Size(50, 13);
            this.msTotal.TabIndex = 8;
            this.msTotal.Text = "Total ms:";
            // 
            // msRetrieveBuffer
            // 
            this.msRetrieveBuffer.AutoSize = true;
            this.msRetrieveBuffer.Location = new System.Drawing.Point(757, 467);
            this.msRetrieveBuffer.Name = "msRetrieveBuffer";
            this.msRetrieveBuffer.Size = new System.Drawing.Size(81, 13);
            this.msRetrieveBuffer.TabIndex = 9;
            this.msRetrieveBuffer.Text = "Retrieve Buffer:";
            // 
            // msConvertImg
            // 
            this.msConvertImg.AutoSize = true;
            this.msConvertImg.Location = new System.Drawing.Point(757, 491);
            this.msConvertImg.Name = "msConvertImg";
            this.msConvertImg.Size = new System.Drawing.Size(63, 13);
            this.msConvertImg.TabIndex = 10;
            this.msConvertImg.Text = "Conversion:";
            // 
            // msFilters
            // 
            this.msFilters.AutoSize = true;
            this.msFilters.Location = new System.Drawing.Point(757, 520);
            this.msFilters.Name = "msFilters";
            this.msFilters.Size = new System.Drawing.Size(37, 13);
            this.msFilters.TabIndex = 11;
            this.msFilters.Text = "Filters:";
            // 
            // msBlobDetection
            // 
            this.msBlobDetection.AutoSize = true;
            this.msBlobDetection.Location = new System.Drawing.Point(757, 544);
            this.msBlobDetection.Name = "msBlobDetection";
            this.msBlobDetection.Size = new System.Drawing.Size(80, 13);
            this.msBlobDetection.TabIndex = 12;
            this.msBlobDetection.Text = "Blob Detection:";
            // 
            // imageBox2
            // 
            this.imageBox2.Location = new System.Drawing.Point(344, 12);
            this.imageBox2.Name = "imageBox2";
            this.imageBox2.Size = new System.Drawing.Size(282, 276);
            this.imageBox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.imageBox2.TabIndex = 13;
            this.imageBox2.TabStop = false;
            // 
            // imageBox1
            // 
            this.imageBox1.Location = new System.Drawing.Point(25, 12);
            this.imageBox1.Name = "imageBox1";
            this.imageBox1.Size = new System.Drawing.Size(282, 276);
            this.imageBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.imageBox1.TabIndex = 14;
            this.imageBox1.TabStop = false;
            // 
            // imageBox3
            // 
            this.imageBox3.Location = new System.Drawing.Point(25, 320);
            this.imageBox3.Name = "imageBox3";
            this.imageBox3.Size = new System.Drawing.Size(282, 276);
            this.imageBox3.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.imageBox3.TabIndex = 15;
            this.imageBox3.TabStop = false;
            // 
            // imageBox4
            // 
            this.imageBox4.Location = new System.Drawing.Point(344, 320);
            this.imageBox4.Name = "imageBox4";
            this.imageBox4.Size = new System.Drawing.Size(282, 276);
            this.imageBox4.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.imageBox4.TabIndex = 16;
            this.imageBox4.TabStop = false;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1069, 769);
            this.Controls.Add(this.imageBox4);
            this.Controls.Add(this.imageBox3);
            this.Controls.Add(this.imageBox1);
            this.Controls.Add(this.imageBox2);
            this.Controls.Add(this.msBlobDetection);
            this.Controls.Add(this.msFilters);
            this.Controls.Add(this.msConvertImg);
            this.Controls.Add(this.msRetrieveBuffer);
            this.Controls.Add(this.msTotal);
            this.Controls.Add(this.cameraFps);
            this.Name = "Form1";
            this.Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)(this.imageBox2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.imageBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.imageBox3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.imageBox4)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label cameraFps;
        private System.Windows.Forms.Label msTotal;
        private System.Windows.Forms.Label msRetrieveBuffer;
        private System.Windows.Forms.Label msConvertImg;
        private System.Windows.Forms.Label msFilters;
        private System.Windows.Forms.Label msBlobDetection;
        private Emgu.CV.UI.ImageBox imageBox2;
        private Emgu.CV.UI.ImageBox imageBox1;
        private Emgu.CV.UI.ImageBox imageBox3;
        private Emgu.CV.UI.ImageBox imageBox4;
    }
}

