using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace winmaped
{
    public partial class MainWindow : Form
    {
        public string currentDir;
        public Font dirFont;
        public Font fileFont;

        ContextMenuStrip tabContextMenu;

        public MainWindow()
        {
            InitializeComponent();

            // tabview setup

            tabContextMenu = new ContextMenuStrip();
            tabContextMenu.Items.Add("Save");
            tabContextMenu.Items[0].MouseUp += delegate (object sender, MouseEventArgs e)
            {
                for (int i = 0; i < tabview.TabCount; i++)
                {
                    TabPage p = tabview.TabPages[i];
                    File.WriteAllText(Path.GetFullPath(p.Name), p.Controls[0].Text);
                }
            };

            tabContextMenu.Items.Add("Close");
            tabContextMenu.Items[1].MouseUp += delegate (object sender, MouseEventArgs e)
            {
                for (int i = 0; i < tabview.TabCount; i++)
                {
                    Rectangle r = tabview.GetTabRect(i);
                    if (r.Contains(e.Location)) tabview.TabPages.RemoveAt(i);
                }
            };

            tabview.Click += delegate(object sender, EventArgs e)
            {
                MouseEventArgs me = (MouseEventArgs)e;
                if (me.Button == MouseButtons.Right)
                {
                    tabContextMenu.Show(PointToScreen(me.Location));
                }
            };

            fileFont = new Font(FontFamily.GenericSansSerif, 8.5f);
            dirFont = new Font(fileFont, FontStyle.Bold);

            UpdateDirectoryView(@"C:\Source\_collegeproj\bin\quiver");
            //AddTab(@"C:\Source\_collegeproj\bin\quiver\maps\episode1.dst");
        }

        public void AddTab(string path)
        {
            AddTab(path, Path.GetFileName(path));
        }
        public void AddTab(string fpath, string fname)
        {
            fpath = FixPath(fpath);

            if (Path.GetExtension(fname) == ".dst")
            {
                if (tabview.TabPages.ContainsKey(fname))
                    return;

                TabPage page = new TabPage();

                var x = new XmlEditor() {AllowXmlFormatting = true, Text = File.ReadAllText(fpath)};
                //XmlEditor.FormatXml(x.GetTextBox());
                page.Controls.Add(x);
                page.Controls[0].Dock = DockStyle.Fill;
                page.Name = fpath;
                page.Text = fname;

                tabview.TabPages.Add(page);
                tabview.SelectTab(fpath);
            }
        }

        string FixPath(string p)
        {
            return p.Replace("\\", "/").Replace("//", "/");
        }

        public void UpdateDirectoryView(string dir = "")
        {
            if (dir != "") currentDir = dir;
            ListDirectory(treeView, currentDir);
        }

        private void ListDirectory(TreeView tree, string path)
        {
            tree.Nodes.Clear();
            var rootDirectoryInfo = new DirectoryInfo(path);
            tree.Nodes.Add(CreateDirectoryNode(rootDirectoryInfo));
            tree.Nodes[0].Expand();
            
            tree.AfterSelect += delegate(object sender, TreeViewEventArgs args)
            {
                TreeNode n = tree.SelectedNode;
                if (Path.HasExtension(n.Name))
                {
                    AddTab(n.Name);
                }
            };
        }
        private TreeNode CreateDirectoryNode(DirectoryInfo directoryInfo)
        {
            var directoryNode = new TreeNode(directoryInfo.Name) {NodeFont = dirFont};

            foreach (var directory in directoryInfo.GetDirectories())
                directoryNode.Nodes.Add(CreateDirectoryNode(directory));
            foreach (var file in directoryInfo.GetFiles())
                directoryNode.Nodes.Add(new TreeNode(file.Name)
                {
                    NodeFont = fileFont,
                    Name = directoryInfo.FullName + "/" + file.Name
                });

            return directoryNode;
        }
    }
}
