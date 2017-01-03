using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Client
{
    public partial class ShowLinkForm : Form
    {
        public ShowLinkForm(string str)
        {
            InitializeComponent();
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
            webBrowser1.DocumentText = html;
        }

        private void ShowLinkForm_Load(object sender, EventArgs e)
        {

        }
    }
}
