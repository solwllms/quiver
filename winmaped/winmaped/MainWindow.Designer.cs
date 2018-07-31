namespace winmaped
{
    partial class MainWindow
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
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.split_horizontal = new System.Windows.Forms.SplitContainer();
            this.tabview = new System.Windows.Forms.TabControl();
            this.split_tools = new System.Windows.Forms.SplitContainer();
            this.treeView = new System.Windows.Forms.TreeView();
            this.propertyGrid = new System.Windows.Forms.PropertyGrid();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.split_horizontal)).BeginInit();
            this.split_horizontal.Panel1.SuspendLayout();
            this.split_horizontal.Panel2.SuspendLayout();
            this.split_horizontal.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.split_tools)).BeginInit();
            this.split_tools.Panel1.SuspendLayout();
            this.split_tools.Panel2.SuspendLayout();
            this.split_tools.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.editToolStripMenuItem,
            this.toolsToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1018, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Size = new System.Drawing.Size(39, 20);
            this.editToolStripMenuItem.Text = "Edit";
            // 
            // toolsToolStripMenuItem
            // 
            this.toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
            this.toolsToolStripMenuItem.Size = new System.Drawing.Size(47, 20);
            this.toolsToolStripMenuItem.Text = "Tools";
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.helpToolStripMenuItem.Text = "Help";
            // 
            // split_horizontal
            // 
            this.split_horizontal.Dock = System.Windows.Forms.DockStyle.Fill;
            this.split_horizontal.Location = new System.Drawing.Point(0, 24);
            this.split_horizontal.Name = "split_horizontal";
            // 
            // split_horizontal.Panel1
            // 
            this.split_horizontal.Panel1.Controls.Add(this.tabview);
            this.split_horizontal.Panel1MinSize = 50;
            // 
            // split_horizontal.Panel2
            // 
            this.split_horizontal.Panel2.Controls.Add(this.split_tools);
            this.split_horizontal.Size = new System.Drawing.Size(1018, 623);
            this.split_horizontal.SplitterDistance = 670;
            this.split_horizontal.TabIndex = 1;
            // 
            // tabview
            // 
            this.tabview.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabview.Location = new System.Drawing.Point(0, 0);
            this.tabview.Name = "tabview";
            this.tabview.SelectedIndex = 0;
            this.tabview.Size = new System.Drawing.Size(670, 623);
            this.tabview.TabIndex = 0;
            // 
            // split_tools
            // 
            this.split_tools.Dock = System.Windows.Forms.DockStyle.Fill;
            this.split_tools.Location = new System.Drawing.Point(0, 0);
            this.split_tools.Name = "split_tools";
            this.split_tools.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // split_tools.Panel1
            // 
            this.split_tools.Panel1.Controls.Add(this.treeView);
            // 
            // split_tools.Panel2
            // 
            this.split_tools.Panel2.Controls.Add(this.propertyGrid);
            this.split_tools.Size = new System.Drawing.Size(344, 623);
            this.split_tools.SplitterDistance = 276;
            this.split_tools.TabIndex = 0;
            // 
            // treeView
            // 
            this.treeView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeView.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.treeView.Location = new System.Drawing.Point(0, 0);
            this.treeView.Name = "treeView";
            this.treeView.Size = new System.Drawing.Size(344, 276);
            this.treeView.TabIndex = 0;
            this.treeView.TabStop = false;
            this.treeView.Text = "treeView";
            // 
            // propertyGrid
            // 
            this.propertyGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.propertyGrid.Location = new System.Drawing.Point(0, 0);
            this.propertyGrid.Name = "propertyGrid";
            this.propertyGrid.Size = new System.Drawing.Size(344, 343);
            this.propertyGrid.TabIndex = 0;
            this.propertyGrid.TabStop = false;
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(61, 4);
            // 
            // MainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1018, 647);
            this.Controls.Add(this.split_horizontal);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "MainWindow";
            this.Text = "MainWindow";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.split_horizontal.Panel1.ResumeLayout(false);
            this.split_horizontal.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.split_horizontal)).EndInit();
            this.split_horizontal.ResumeLayout(false);
            this.split_tools.Panel1.ResumeLayout(false);
            this.split_tools.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.split_tools)).EndInit();
            this.split_tools.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.SplitContainer split_horizontal;
        private System.Windows.Forms.TabControl tabview;
        private System.Windows.Forms.SplitContainer split_tools;
        private System.Windows.Forms.PropertyGrid propertyGrid;
        private System.Windows.Forms.TreeView treeView;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
    }
}