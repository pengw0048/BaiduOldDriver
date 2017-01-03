using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NetDisk;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;

namespace Client
{
    public partial class MainForm : Form
    {
        private const string DirLabel = "<文件夹>";
        private Credential cred;
        private string path = "";
        public MainForm(Credential cred)
        {
            this.cred = cred;
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            LoadFileList();
        }
        private void LoadFileList()
        {
            listView1.Items.Clear();
            listView1.Enabled = false;
            toolStripButton1.Enabled = false;
            toolStripLabel1.Text = path == "" ? "/" : path;
            Application.DoEvents();
            var res = Operation.GetFileList("/" + path, cred);
            if (res.success == false || res.errno != 0)
            {
                MessageBox.Show("Error: " + res.exception);
                path = "";
            }
            else
            {
                if (path != "") listView1.Items.Add(new ListViewItem(new[] { "", ".." }));
                foreach (var item in res.list)
                {
                    listView1.Items.Add(new ListViewItem(new[] { "", item.server_filename, item.isdir == 0 ? BytesToString(item.size) : DirLabel }));
                }
            }
            listView1.Enabled = true;
            toolStripButton1.Enabled = true;
        }
        private static string BytesToString(long byteCount)
        {
            string[] suf = { "B", "KB", "MB", "GB", "TB", "PB", "EB" }; //Longs run out around EB
            if (byteCount == 0)
                return "0" + suf[0];
            long bytes = Math.Abs(byteCount);
            int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            double num = Math.Round(bytes / Math.Pow(1024, place), 1);
            return (Math.Sign(byteCount) * num).ToString() + suf[place];
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            LoadFileList();
        }

        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                var line = listView1.SelectedItems[0];
                if (line.SubItems.Count == 2 || line.SubItems[2].Text == DirLabel)
                {
                    if (line.SubItems[1].Text == "..")
                        path = path.Substring(0, path.LastIndexOf('/'));
                    else
                        path += "/" + line.SubItems[1].Text;
                    LoadFileList();
                }
                else
                {
                    listView1.Enabled = false;
                    toolStripButton1.Enabled = false;
                    Application.DoEvents();
                    var pwd = GeneratePwd();
                    var sres = Operation.Share(new[] { path + "/" + line.SubItems[1].Text }, cred, pwd);
                    try
                    {
                        if (sres.success == false || sres.errno != 0) throw sres.exception;
                        using (var wc = new WebClient())
                        {
                            var str = wc.DownloadString("http://stomakun.me:9999/" + Uri.EscapeDataString(sres.link) + "/" + pwd);
                            if (str.Contains("Error:") || !str.Contains("http://")) throw new Exception(str);
                            ShowLinks(str);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                    listView1.Enabled = true;
                    toolStripButton1.Enabled = true;
                }
            }
        }
        private void ShowLinks(string str)
        {
            using(var sw=new StreamWriter("link.html"))
            {
                var html = "";
                var i = 1;
                var del = "\n";
                if (str.Contains("\r\n")) del = "\r\n";
                else if (str.Contains("\r")) del = "\r";
                foreach (var item in str.Split(new[] { del }, StringSplitOptions.RemoveEmptyEntries))
                {
                    var match = Regex.Match(item, "https?:\\/\\/(.+?)\\/");
                    html += "<p><a href=\"" + item + "\">下载地址" + i + (match.Success ? "&nbsp;(" + match.Groups[1].Value + ")" : "") + "</a></p>";
                    i++;
                }
                sw.WriteLine(html);
            }
            System.Diagnostics.Process.Start("link.html");
        }
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }
        private string GeneratePwd()
        {
            var sb = new StringBuilder();
            var rand = new Random();
            for(int i = 0; i < 4; i++)
            {
                var type = rand.Next(3);
                if (type == 0) sb.Append((char)('0' + rand.Next(10)));
                else if (type == 1) sb.Append((char)('a' + rand.Next(26)));
                else if (type == 2) sb.Append((char)('A' + rand.Next(26)));
            }
            return sb.ToString();
        }
    }
}
