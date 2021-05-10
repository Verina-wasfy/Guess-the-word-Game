using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace dashes
{
    public partial class Start_ClientForm : Form
    {
        public Start_ClientForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Form1 f2 = new Form1();
            this.Visible = false;
            if (!f2.IsDisposed)
            {
                f2.ShowDialog();
            }
        }

        private void Start_ClientForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }

        private void Start_ClientForm_Load(object sender, EventArgs e)
        {

        }
    }
}
