namespace api_itm.UserControler.Employee
{
    partial class ControlerEmployeeView
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            dgvViewSalariati = new DataGridView();
            ((System.ComponentModel.ISupportInitialize)dgvViewSalariati).BeginInit();
            SuspendLayout();
            // 
            // dgvViewSalariati
            // 
            dgvViewSalariati.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvViewSalariati.Location = new Point(125, 107);
            dgvViewSalariati.Name = "dgvViewSalariati";
            dgvViewSalariati.RowHeadersWidth = 51;
            dgvViewSalariati.Size = new Size(534, 188);
            dgvViewSalariati.TabIndex = 0;
            // 
            // ControlerEmployeeView
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(dgvViewSalariati);
            Name = "ControlerEmployeeView";
            Size = new Size(892, 364);
            Load += ControlerEmployeeView_Load;
            ((System.ComponentModel.ISupportInitialize)dgvViewSalariati).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private DataGridView dgvViewSalariati;
    }
}
