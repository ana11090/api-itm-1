using System.Windows.Forms;

namespace api_itm.UserControler
{
    partial class ControlSidebarMenu : UserControl
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            SuspendLayout();
            // 
            // ControlSidebarMenu
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            Name = "ControlSidebarMenu";
            Load += ControlSidebarMenu_Load;
            ResumeLayout(false);
        }
    }
}
