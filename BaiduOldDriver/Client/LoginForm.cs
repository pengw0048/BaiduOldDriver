using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;

using System.Windows.Forms;
using NetDisk;
using System.IO;

namespace Client
{
    public partial class LoginForm : Form
    {
        public LoginForm()
        {
            InitializeComponent();
        }
        private LoginCheckResult lcr;
        private void textBox1_Leave(object sender, EventArgs e)
        {
            if (textBox1.Text != "")
            {
                this.Enabled = false;
                lcr = Authentication.LoginCheck(textBox1.Text);
                if (lcr.success == false)
                {
                    MessageBox.Show("Error: " + lcr.exception);
                    Application.Exit();
                }
                label3.Visible = textBox3.Visible = pictureBox1.Visible = lcr.needVCode;
                textBox3.Text = "";
                if (lcr.needVCode) pictureBox1.Image = ByteToImage(lcr.image);
                this.Enabled = true;
            }
        }
        private static Bitmap ByteToImage(byte[] blob)
        {
            MemoryStream mStream = new MemoryStream();
            byte[] pData = blob;
            mStream.Write(pData, 0, Convert.ToInt32(pData.Length));
            Bitmap bm = new Bitmap(mStream, false);
            mStream.Dispose();
            return bm;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (lcr == null) return;
            lcr.verifyCode = textBox3.Text;
            var res = Authentication.Login(textBox1.Text, textBox2.Text, lcr);
            if (res.success == false)
            {
                MessageBox.Show("Error: " + res.exception);
                textBox2.Text = "";
                textBox1_Leave(null, null);
                return;
            }
            new MainForm(res.credential).Show();
            this.Hide();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            textBox3.Text = "";
            textBox1_Leave(null, null);
        }
    }
}
