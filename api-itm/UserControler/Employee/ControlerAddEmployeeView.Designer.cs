namespace api_itm.UserControler.Employee
{
    partial class ControlerAddEmployeeView
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
            label1 = new Label();
            ((System.ComponentModel.ISupportInitialize)dgvViewSalariati).BeginInit();
            SuspendLayout();
            // 
            // dgvViewSalariati
            // 
            dgvViewSalariati.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvViewSalariati.Location = new Point(40, 82);
            dgvViewSalariati.Name = "dgvViewSalariati";
            dgvViewSalariati.RowHeadersWidth = 51;
            dgvViewSalariati.Size = new Size(804, 303);
            dgvViewSalariati.TabIndex = 0;
            dgvViewSalariati.CellContentClick += dgvViewSalariati_CellContentClick;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(40, 20);
            label1.Name = "label1";
            label1.Size = new Size(0, 20);
            label1.TabIndex = 1;
            label1.Click += label1_Click;
            // 
            // ControlerEmployeeView
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(label1);
            Controls.Add(dgvViewSalariati);
            Name = "ControlerEmployeeView";
            Size = new Size(1176, 492);
            Load += ControlerEmployeeView_Load;
            ((System.ComponentModel.ISupportInitialize)dgvViewSalariati).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private DataGridView dgvViewSalariati;
        private Label label1;
    }
}
