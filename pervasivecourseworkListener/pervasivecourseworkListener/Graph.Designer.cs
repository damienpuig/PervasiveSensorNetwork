using GraphLib;
namespace pervasivecourseworkListener
{
    partial class Graph
    {
        PlotterDisplayEx display = null;

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

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.display = new GraphLib.PlotterDisplayEx();
            this.SuspendLayout();
            // 
            // display
            // 
            this.display.BackColor = System.Drawing.Color.White;
            this.display.BackgroundColorBot = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(64)))));
            this.display.BackgroundColorTop = System.Drawing.Color.Navy;
            this.display.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.display.DashedGridColor = System.Drawing.Color.Blue;
            this.display.Dock = System.Windows.Forms.DockStyle.Fill;
            this.display.DoubleBuffering = true;
            this.display.Location = new System.Drawing.Point(0, 0);
            this.display.Name = "display";
            this.display.PlaySpeed = 0.5F;
            this.display.Size = new System.Drawing.Size(620, 496);
            this.display.SolidGridColor = System.Drawing.Color.Blue;
            this.display.TabIndex = 1;
            // 
            // Graph
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(620, 496);
            this.Controls.Add(this.display);
            this.Name = "Graph";
            this.Text = "Arduino graph report";
            this.ResumeLayout(false);

        }
    }
}