using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace dashes
{
    public partial class Start_ServerForm : Form
    {
        public string Rbtn;
        public string Dbtn;
        public string Select;
        public int num;
        public string SelectedWord;
        public string[] ChosenWords;
        public Start_ServerForm()
        {
            InitializeComponent();
        }

        public char[] dif_cat = new char[2];

        private void button1_Click(object sender, EventArgs e)
        {

            Select = "select Words From Word Where Difficulty = '" + Dbtn + "' and Category = '" + Rbtn + "'";
            sqlCommand1.CommandText = Select;
            SqlDataReader dReader;
            sqlConnection1.Open();
            dReader = sqlCommand1.ExecuteReader();
            DataTable dTable = new DataTable();
            dTable.Load(dReader);
            num = dTable.Rows.Count;
            ChosenWords = new String[num];
            for (int i = 0; i < dTable.Rows.Count; i++)
            {
                SelectedWord = dTable.Rows[i]["Words"].ToString();
               // MessageBox.Show(SelectedWord);
                ChosenWords[i] = SelectedWord;

            }
            dReader.Close();
            sqlConnection1.Close();



            Form1 f1 = new Form1(dif_cat,ChosenWords);
          
            this.Visible = false;
            if (!f1.IsDisposed)
            {
                f1.ShowDialog();
            }
            

        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            Rbtn = radioButton1.Text;
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            Rbtn = radioButton2.Text;
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            Rbtn = radioButton3.Text;
        }

        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {
            Dbtn = radioButton4.Text;
        }

        private void radioButton5_CheckedChanged(object sender, EventArgs e)
        {
           Dbtn = radioButton5.Text;
        }

        private void radioButton6_CheckedChanged(object sender, EventArgs e)
        {
            Dbtn = radioButton6.Text;
        }

        private void Start_ServerForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }

        private void Start_ServerForm_Load(object sender, EventArgs e)
        {

        }
    }
}
