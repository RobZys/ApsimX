﻿namespace UserInterface.Views
{
    partial class GridView
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
            this.components = new System.ComponentModel.Container();
            this.PopupMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.Grid = new System.Windows.Forms.DataGridView();
            ((System.ComponentModel.ISupportInitialize)(this.Grid)).BeginInit();
            this.SuspendLayout();
            // 
            // PopupMenu
            // 
            this.PopupMenu.Name = "contextMenuStrip1";
            this.PopupMenu.Size = new System.Drawing.Size(61, 4);
            // 
            // Grid
            // 
            this.Grid.AllowUserToAddRows = false;
            this.Grid.AllowUserToDeleteRows = false;
            this.Grid.BackgroundColor = System.Drawing.SystemColors.Window;
            this.Grid.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.Grid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.Grid.ContextMenuStrip = this.PopupMenu;
            this.Grid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Grid.Location = new System.Drawing.Point(0, 0);
            this.Grid.Name = "Grid";
            this.Grid.RowHeadersVisible = false;
            this.Grid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.Grid.ShowRowErrors = false;
            this.Grid.Size = new System.Drawing.Size(354, 345);
            this.Grid.TabIndex = 1;
            this.Grid.CellBeginEdit += new System.Windows.Forms.DataGridViewCellCancelEventHandler(this.OnCellBeginEdit);
            this.Grid.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.OnCellClick);
            this.Grid.CellValidating += new System.Windows.Forms.DataGridViewCellValidatingEventHandler(this.Grid_CellValidating);
            this.Grid.DataError += new System.Windows.Forms.DataGridViewDataErrorEventHandler(this.OnDataError);
            this.Grid.EditingControlShowing += new System.Windows.Forms.DataGridViewEditingControlShowingEventHandler(this.OnEditingControlShowing);
            // 
            // GridView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.Grid);
            this.Name = "GridView";
            this.Size = new System.Drawing.Size(354, 345);
            ((System.ComponentModel.ISupportInitialize)(this.Grid)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ContextMenuStrip PopupMenu;
        private System.Windows.Forms.DataGridView Grid;

    }
}