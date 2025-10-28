namespace api_itm.UserControler.Contracts
{
    partial class ControlerAddContractsView
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
            dgvAddContracts = new DataGridView();
            ((System.ComponentModel.ISupportInitialize)dgvAddContracts).BeginInit();
            SuspendLayout();
            // 
            // dgvAddContracts
            // 
            dgvAddContracts.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvAddContracts.Location = new Point(26, 25);
            dgvAddContracts.Name = "dgvAddContracts";
            dgvAddContracts.RowHeadersWidth = 51;
            dgvAddContracts.Size = new Size(356, 214);
            dgvAddContracts.TabIndex = 0;
            //dgvAddContracts.CellContentClick += dgvAddContracts_CellContentClick_1;
            // 
            // ControlerAddContractsView
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(dgvAddContracts);
            Name = "ControlerAddContractsView";
            Size = new Size(970, 408);
            Load += ControlerAddContractsView_Load;
            ((System.ComponentModel.ISupportInitialize)dgvAddContracts).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private DataGridView dgvAddContracts;
    }
}
