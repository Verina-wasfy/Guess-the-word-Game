using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Sockets;
using System.IO;

namespace dashes
{
    public partial class Form1 : Form
    {
        public int ServerCount = 0;
        public int ClientCount = 0;

        #region Variables

        public string[] wordBank;
        string letterFontName;
        int letterFontSize;
        Font letterFont;
        Color dashColor;
        Color letterColor;
        Brush letterBrush;
        Pen dashPen;

        int formWidth;
        int dashLength;
        int dashSpacing;
        int dashNumber;
        int x;
        int dashY;

        private Socket sock;
        private BackgroundWorker messagereceve = new BackgroundWorker();
        private TcpListener server = null;
        private TcpClient client;
        bool checkval=false;

        BinaryReader br;
        BinaryWriter bw;
        NetworkStream nStream;

        int wordlength = 0;
        bool yours;
        bool other;
        string word;

        byte[] val = new byte[2];

        char rematch_server;
        char rematch_client;

        bool game = false;

        #endregion


        protected override void OnPaint(PaintEventArgs e)
        {
            PrintDashes(e.Graphics);
        }

        protected override void OnResize(EventArgs e)
        {
            this.Invalidate();
        }

        bool is_on = false;

        private void start() {

            CheckForIllegalCrossThreadCalls = false;
            messagereceve.DoWork += messagerecever_DoWork;
            messagereceve.WorkerSupportsCancellation = true;

            try
            {
                game = false;
                yours = false;
                other = true;
                checkval = false;

                if (!is_on)
                {
                    is_on = true;
                    label1.Text = "client";
                    client = new TcpClient("192.168.1.2", 9010);
                    sock = client.Client;

                    nStream = client.GetStream();
                    br = new BinaryReader(nStream);
                    bw = new BinaryWriter(nStream);
                    char[] dif_cat = br.ReadChars(2);
                    label1.Text = dif_cat[0].ToString();
                    label2.Text = dif_cat[1].ToString();

                    if (dif_cat[0] == 'a')
                    {
                        label1.Text = "Animals";
                    }
                    else if (dif_cat[0] == 'c')
                    {
                        label1.Text = "Country";
                    }
                    else if (dif_cat[0] == 'f')
                    {
                        label1.Text = "Food";
                    }

                    if (dif_cat[1] == 'd')
                    {
                        label2.Text = "Difficulty";
                    }
                    else if (dif_cat[1] == 'm')
                    {
                        label2.Text = "Midum";
                    }
                    else if (dif_cat[1] == 'e')
                    {
                        label2.Text = "Easy";
                    }
                  
                }
                word = br.ReadString();
                messagereceve.RunWorkerAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            //letter
            letterFontName = "Times New Roman";
            letterFontSize = 50;
            letterFont = new Font(letterFontName, letterFontSize);
            letterColor = Color.White;
            letterBrush = new SolidBrush(letterColor);

            //dash
            dashColor = Color.Black;
            dashPen = new Pen(dashColor, 5);
            dashLength = 50;
            dashSpacing = 10;
            //this variable adjust the number of dashes printed on screen
            dashNumber = word.Trim().Length;
            dashY = 200;


        }//start

        public Form1()
        {
            InitializeComponent();
          
            start();
        }
       
        public void PrintDashes(Graphics g)
        {
            
            int formWidth = this.Width;
            int x = (formWidth / 2) - (dashNumber * (dashLength + dashSpacing)) / 2;
            for (int i = 0; i < dashNumber; i++)
            {
                Point dashStartPoint = new Point(x + ((dashLength + dashSpacing) * i), dashY);
                Point dashEndPoint = new Point(x + dashLength * (i + 1) + (dashSpacing * i), dashY);
                g.DrawLine(dashPen, dashStartPoint, dashEndPoint);
            }
        }

        public byte PrintLetter(char letter)
        {
            Graphics g = CreateGraphics();
            
            var foundIndexes = new List<int>();
            for (int i = 0; i < word.Length; i++)
            {
                if (word[i] == letter)
                    foundIndexes.Add(i);
            }
            int formWidth = this.Width;
            SizeF stringSize = g.MeasureString(letter.ToString(), letterFont);
            float letterY = dashY - stringSize.Height;
            int x = (formWidth / 2) - (dashNumber * (dashLength + dashSpacing)) / 2;
            if (foundIndexes.Any())
            {
                foreach (int i in foundIndexes)
                {
                    g.DrawString(letter.ToString(), letterFont, letterBrush, x + ((dashLength + dashSpacing) * i) + (dashLength/2 - stringSize.Width/2), letterY);
                    wordlength++;
                    if (!yours)
                    {
                        check();
                    }
                    
                }
                return 1;
            }
            return 0;
        }

        private void messagerecever_DoWork(object sender, DoWorkEventArgs e)
        {
            

            if (checkval == false)//client
            {

                freez();
                receveAction();
            }

            yours = true;
            other = false;
            checkval = true;



            unfreez();


        }

        private void check_winner()
        {
            if (yours)
            {
                MessageBox.Show("Client Won");
                ClientCount++;
                FileStream fs = new FileStream("F:\\Final P\\Result.txt", FileMode.OpenOrCreate);
                StreamWriter sw = new StreamWriter(fs);
                sw.WriteLine("Server:" + (ServerCount));
                sw.WriteLine("Client:" + (ClientCount));
                MessageBox.Show("Client Score:" + ClientCount);
                sw.Close();
                freez();
            }
            else
            {
                MessageBox.Show("Client lost");
                freez();
            }

        }//check_winner

        private void check_rematch()
        {

            PlayAgainChecker DAC = new PlayAgainChecker();
            DialogResult DR;

            DR = DAC.ShowDialog();

            if (DR == DialogResult.OK || DR == DialogResult.Cancel)
            {
                rematch_client = DAC.Result;
            }
            rematch_server = br.ReadChar();
            bw.Write(rematch_client);


            if (rematch_server == 'Y' && rematch_client == 'Y')
            {
                messagereceve.CancelAsync();
                messagereceve.DoWork += null;
                messagereceve.Dispose();
                this.Hide();
                Form1 f1 = new Form1();
                f1.ShowDialog();
               
            }
            else
            {
                messagereceve.Dispose();
                messagereceve.CancelAsync();
                
                Application.Exit();
            }
        }//check_rematch

        private void check()
        {
            if (wordlength == word.Trim().Length)
            {
                check_winner();
                check_rematch();
                
                game = true;
            }
        }
      
        private void receveAction()
        {
            byte[] action = new byte[2];

        here:;
            if (game)
            {
                return;
            }
            sock.Receive(action);
            PrintLetter(br.ReadChar());
            if (action[0] == 1)
            {
                button1.Enabled = false;
                button1.Text = "";
                if (action[1] == 1)
                {
                    goto here;
                }
            }
            else if (action[0] == 2)
            {
                button2.Enabled = false;
                button2.Text = "";
                if (action[1] == 1)
                {
                    goto here;
                }
            }
            else if (action[0] == 3)
            {
                button3.Enabled = false;
                button3.Text = "";
                if (action[1] == 1)
                {
                    goto here;
                }
            }
            else if (action[0] == 4)
            {
                button4.Enabled = false;
                button4.Text = "";
                if (action[1] == 1)
                {
                    goto here;
                }
            }
            else if (action[0] == 5)
            {
                button5.Enabled = false;
                button5.Text = "";
                if (action[1] == 1)
                {
                    goto here;
                }
            }
            else if (action[0] == 6)
            {
                button6.Enabled = false;
                button6.Text = "";
                if (action[1] == 1)
                {
                    goto here;
                }
            }
            else if (action[0] == 7)
            {
                button7.Enabled = false;
                button7.Text = "";
                if (action[1] == 1)
                {
                    goto here;
                }
            }
            else if (action[0] == 8)
            {
                button8.Enabled = false;
                button8.Text = "";
                if (action[1] == 1)
                {
                    goto here;
                }
            }
            else if (action[0] == 9)
            {
                button9.Enabled = false;
                button9.Text = "";
                if (action[1] == 1)
                {
                    goto here;
                }
            }
            else if (action[0] == 10)
            {
                button10.Enabled = false;
                button10.Text = "";
                if (action[1] == 1)
                {
                    goto here;
                }
            }
            else if (action[0] == 11)
            {
                button11.Enabled = false;
                button11.Text = "";
                if (action[1] == 1)
                {
                    goto here;
                }
            }
            else if (action[0] == 12)
            {
                button12.Enabled = false;
                button12.Text = "";
                if (action[1] == 1)
                {
                    goto here;
                }
            }
            else if (action[0] == 13)
            {
                button13.Enabled = false;
                button13.Text = "";
                if (action[1] == 1)
                {
                    goto here;
                }
            }
            else if (action[0] == 14)
            {
                button14.Enabled = false;
                button14.Text = "";
                if (action[1] == 1)
                {
                    goto here;
                }
            }
            else if (action[0] == 15)
            {
                button15.Enabled = false;
                button15.Text = "";
                if (action[1] == 1)
                {
                    goto here;
                }
            }
            else if (action[0] == 16)
            {
                button16.Enabled = false;
                button16.Text = "";
                if (action[1] == 1)
                {
                    goto here;
                }
            }
            else if (action[0] == 17)
            {
                button17.Enabled = false;
                button17.Text = "";
                if (action[1] == 1)
                {
                    goto here;
                }
            }
            else if (action[0] == 18)
            {
                button18.Enabled = false;
                button18.Text = "";
                if (action[1] == 1)
                {
                    goto here;
                }
            }
            else if (action[0] == 19)
            {
                button19.Enabled = false;
                button19.Text = "";
                if (action[1] == 1)
                {
                    goto here;
                }
            }
            else if (action[0] == 20)
            {
                button20.Enabled = false;
                button20.Text = "";
                if (action[1] == 1)
                {
                    goto here;
                }
            }
            else if (action[0] == 21)
            {
                button21.Enabled = false;
                button21.Text = "";
                if (action[1] == 1)
                {
                    goto here;
                }
            }
            else if (action[0] == 22)
            {
                button22.Enabled = false;
                button22.Text = "";
                if (action[1] == 1)
                {
                    goto here;
                }
            }
            else if (action[0] == 23)
            {
                button23.Enabled = false;
                button23.Text = "";
                if (action[1] == 1)
                {
                    goto here;
                }
            }
            else if (action[0] == 24)
            {
                button24.Enabled = false;
                button24.Text = "";
                if (action[1] == 1)
                {
                    goto here;
                }
            }
            else if (action[0] == 25)
            {
                button25.Enabled = false;
                button25.Text = "";
                if (action[1] == 1)
                {
                    goto here;
                }
            }
            else if (action[0] == 26)
            {
                button26.Enabled = false;
                button26.Text = "";
                if (action[1] == 1)
                {
                    goto here;
                }
            }
        }

        private void freez()
        {
            button1.Enabled = false;
            button2.Enabled = false;
            button3.Enabled = false;
            button4.Enabled = false;
            button5.Enabled = false;
            button6.Enabled = false;
            button7.Enabled = false;
            button8.Enabled = false;
            button9.Enabled = false;
            button10.Enabled = false;
            button11.Enabled = false;
            button12.Enabled = false;
            button13.Enabled = false;
            button14.Enabled = false;
            button15.Enabled = false;
            button16.Enabled = false;
            button17.Enabled = false;
            button18.Enabled = false;
            button19.Enabled = false;
            button20.Enabled = false;
            button21.Enabled = false;
            button22.Enabled = false;
            button23.Enabled = false;
            button24.Enabled = false;
            button25.Enabled = false;
            button26.Enabled = false;

        }//FREEZ    

        private void unfreez()
        {
            if (button1.Text != "")
            {
                button1.Enabled = true;
            }
            if (button2.Text != "")
            {
                button2.Enabled = true;
            }
            if (button3.Text != "")
            {
                button3.Enabled = true;
            }
            if (button4.Text != "")
            {
                button4.Enabled = true;
            }
            if (button5.Text != "")
            {
                button5.Enabled = true;
            }
            if (button6.Text != "")
            {
                button6.Enabled = true;
            }
            if (button7.Text != "")
            {
                button7.Enabled = true;
            }
            if (button8.Text != "")
            {
                button8.Enabled = true;
            }
            if (button9.Text != "")
            {
                button9.Enabled = true;
            }
            if (button10.Text != "")
            {
                button10.Enabled = true;
            }
            if (button11.Text != "")
            {
                button11.Enabled = true;
            }
            if (button12.Text != "")
            {
                button12.Enabled = true;
            }
            if (button13.Text != "")
            {
                button13.Enabled = true;
            }
            if (button14.Text != "")
            {
                button14.Enabled = true;
            }
            if (button15.Text != "")
            {
                button15.Enabled = true;
            }
            if (button16.Text != "")
            {
                button16.Enabled = true;
            }
            if (button17.Text != "")
            {
                button17.Enabled = true;
            }
            if (button18.Text != "")
            {
                button18.Enabled = true;
            }
            if (button19.Text != "")
            {
                button19.Enabled = true;
            }
            if (button20.Text != "")
            {
                button20.Enabled = true;
            }
            if (button21.Text != "")
            {
                button21.Enabled = true;
            }
            if (button22.Text != "")
            {
                button22.Enabled = true;
            }
            if (button23.Text != "")
            {
                button23.Enabled = true;
            }
            if (button24.Text != "")
            {
                button24.Enabled = true;
            }
            if (button25.Text != "")
            {
                button25.Enabled = true;
            }
            if (button26.Text != "")
            {
                button26.Enabled = true;
            }
            
        }//UNFREEZ  
        
        private void button1_Click(object sender, EventArgs e)
        {
            val[0] = 1;
            val[1]=PrintLetter('a');
            
            sock.Send(val);
            bw.Write('a');

            
            if (val[1] == 0)
            {
                checkval = false;
                yours = false;
                other = true;
                messagereceve.RunWorkerAsync();
            }
            if (yours){check();}
        }

        private void button2_Click(object sender, EventArgs e)
        {
            val[0] = 2;
            val[1] = PrintLetter('b');

            sock.Send(val);
            bw.Write('b');

            button2.Enabled = false;
            button2.Text = "";

            if (val[1] == 0)
            {
                checkval = false;
                yours = false;
                other = true;
                messagereceve.RunWorkerAsync();
            }
            if (yours){check();}
        }

        private void button3_Click(object sender, EventArgs e)
        {
            val[0] = 3;
            val[1] = PrintLetter('c');

            sock.Send(val);
            bw.Write('c');
            button3.Enabled = false;
            button3.Text = "";

            if (val[1] == 0)
            {
                checkval = false;
                yours = false;
                other = true;
                messagereceve.RunWorkerAsync();
            }
            if (yours){check();}
        }

        private void button4_Click(object sender, EventArgs e)
        {
            val[0] = 4;
            val[1] = PrintLetter('d');

            sock.Send(val);
            bw.Write('d');
            button4.Enabled = false;
            button4.Text = "";

            if (val[1] == 0)
            {
                checkval = false;
                yours = false;
                other = true;
                messagereceve.RunWorkerAsync();
            }
            if (yours){check();}
        }

        private void button5_Click(object sender, EventArgs e)
        {
            val[0] = 5;
            val[1] = PrintLetter('e');

            sock.Send(val);
            bw.Write('e');
            button5.Enabled = false;
            button5.Text = "";

            if (val[1] == 0)
            {
                checkval = false;
                yours = false;
                other = true;
                messagereceve.RunWorkerAsync();
            }
            if (yours){check();}
        }

        private void button6_Click(object sender, EventArgs e)
        {
            val[0] = 6;
            val[1] = PrintLetter('f');

            sock.Send(val);
            bw.Write('f');
            button6.Enabled = false;
            button6.Text = "";

            if (val[1] == 0)
            {
                checkval = false;
                yours = false;
                other = true;
                messagereceve.RunWorkerAsync();
            }
            if (yours){check();}
        }

        private void button7_Click(object sender, EventArgs e)
        {
            val[0] = 7;
            val[1] = PrintLetter('g');

            sock.Send(val);
            bw.Write('g');
            button7.Enabled = false;
            button7.Text = "";

            if (val[1] == 0)
            {
                checkval = false;
                yours = false;
                other = true;
                messagereceve.RunWorkerAsync();
            }
            if (yours){check();}
        }

        private void button8_Click(object sender, EventArgs e)
        {
            val[0] = 8;
            val[1] = PrintLetter('h');

            sock.Send(val);
            bw.Write('h');
            button8.Enabled = false;
            button8.Text = "";

            if (val[1] == 0)
            {
                checkval = false;
                yours = false;
                other = true;
                messagereceve.RunWorkerAsync();
            }
            if (yours){check();}
        }

        private void button9_Click(object sender, EventArgs e)
        {
            val[0] = 9;
            val[1] = PrintLetter('i');

            sock.Send(val);
            bw.Write('i');
            button9.Enabled = false;
            button9.Text = "";

            if (val[1] == 0)
            {
                checkval = false;
                yours = false;
                other = true;
                messagereceve.RunWorkerAsync();
            }
            if (yours){check();}
        }

        private void button10_Click(object sender, EventArgs e)
        {
            val[0] = 10;
            val[1] = PrintLetter('j');

            sock.Send(val);
            bw.Write('j');
            button10.Enabled = false;
            button10.Text = "";

            if (val[1] == 0)
            {
                checkval = false;
                yours = false;
                other = true;
                messagereceve.RunWorkerAsync();
            }
            if (yours){check();}
        }

        private void button18_Click(object sender, EventArgs e)
        {
            val[0] = 18;
            val[1] = PrintLetter('k');

            sock.Send(val);
            bw.Write('k');
            button18.Enabled = false;
            button18.Text = "";

            if (val[1] == 0)
            {
                checkval = false;
                yours = false;
                other = true;
                messagereceve.RunWorkerAsync();
            }
            if (yours){check();}
        }

        private void button17_Click(object sender, EventArgs e)
        {
            val[0] = 17;
            val[1] = PrintLetter('l');

            sock.Send(val);
            bw.Write('l');
            button17.Enabled = false;
            button17.Text = "";

            if (val[1] == 0)
            {
                checkval = false;
                yours = false;
                other = true;
                messagereceve.RunWorkerAsync();
            }
            if (yours){check();}
        }

        private void button15_Click(object sender, EventArgs e)
        {
            val[0] = 15;
            val[1] = PrintLetter('n');

            sock.Send(val);
            bw.Write('n');
            button15.Enabled = false;
            button15.Text = "";

            if (val[1] == 0)
            {
                checkval = false;
                yours = false;
                other = true;
                messagereceve.RunWorkerAsync();
            }
            if (yours){check();}
        }

        private void button16_Click(object sender, EventArgs e)
        {
            val[0] = 16;
            val[1] = PrintLetter('m');

            sock.Send(val);
            bw.Write('m');


            if (val[1] == 0)
            {
                checkval = false;
                yours = false;
                other = true;
                messagereceve.RunWorkerAsync();
            }
            if (yours){check();}
        }

        private void button14_Click(object sender, EventArgs e)
        {
            val[0] = 14;
            val[1] = PrintLetter('o');

            sock.Send(val);
            bw.Write('o');
            button14.Enabled = false;
            button14.Text = "";

            if (val[1] == 0)
            {
                checkval = false;
                yours = false;
                other = true;
                messagereceve.RunWorkerAsync();
            }
            if (yours){check();}
        }

        private void button13_Click(object sender, EventArgs e)
        {
            val[0] = 13;
            val[1] = PrintLetter('p');

            sock.Send(val);
            bw.Write('p');
            button13.Enabled = false;
            button13.Text = "";

            if (val[1] == 0)
            {
                checkval = false;
                yours = false;
                other = true;
                messagereceve.RunWorkerAsync();
            }
            if (yours){check();}
        }

        private void button12_Click(object sender, EventArgs e)
        {
            val[0] = 12;
            val[1] = PrintLetter('q');

            sock.Send(val);
            bw.Write('q');
            button12.Enabled = false;
            button12.Text = "";

            if (val[1] == 0)
            {
                checkval = false;
                yours = false;
                other = true;
                messagereceve.RunWorkerAsync();
            }
            if (yours){check();}
        }

        private void button11_Click(object sender, EventArgs e)
        {
            val[0] = 11;
            val[1] = PrintLetter('r');

            sock.Send(val);
            bw.Write('r');
            button11.Enabled = false;
            button11.Text = "";

            if (val[1] == 0)
            {
                checkval = false;
                yours = false;
                other = true;
                messagereceve.RunWorkerAsync();
            }
            if (yours){check();}
        }

        private void button24_Click(object sender, EventArgs e)
        {
            val[0] = 24;
            val[1] = PrintLetter('s');

            sock.Send(val);
            bw.Write('s');
            button24.Enabled = false;
            button24.Text = "";

            if (val[1] == 0)
            {
                checkval = false;
                yours = false;
                other = true;
                messagereceve.RunWorkerAsync();
            }
            if (yours){check();}
        }

        private void button23_Click(object sender, EventArgs e)
        {
            val[0] = 23;
            val[1] = PrintLetter('t');

            sock.Send(val);
            bw.Write('t');
            button23.Enabled = false;
            button23.Text = "";


            if (val[1] == 0)
            {
                checkval = false;
                yours = false;
                other = true;
                messagereceve.RunWorkerAsync();
            }
            if (yours){check();}
        }

        private void button22_Click(object sender, EventArgs e)
        {
            val[0] = 22;
            val[1] = PrintLetter('u');

            sock.Send(val);
            bw.Write('u');
            button22.Enabled = false;
            button22.Text = "";

            if (val[1] == 0)
            {
                checkval = false;
                yours = false;
                other = true;
                messagereceve.RunWorkerAsync();
            }
            if (yours){check();}
        }

        private void button21_Click(object sender, EventArgs e)
        {
            val[0] = 21;
            val[1] = PrintLetter('v');

            sock.Send(val);
            bw.Write('v');

            button21.Enabled = false;
            button21.Text = "";

            if (val[1] == 0)
            {
                checkval = false;
                yours = false;
                other = true;
                messagereceve.RunWorkerAsync();
            }
            if (yours){check();}
        }

        private void button20_Click(object sender, EventArgs e)
        {
            val[0] = 20;
            val[1] = PrintLetter('w');

            sock.Send(val);
            bw.Write('w');

            button20.Enabled = false;
            button20.Text = "";

            if (val[1] == 0)
            {
                checkval = false;
                yours = false;
                other = true;
                messagereceve.RunWorkerAsync();
            }
            if (yours){check();}
        }

        private void button19_Click(object sender, EventArgs e)
        {
            val[0] = 19;
            val[1] = PrintLetter('x');

            sock.Send(val);
            bw.Write('x');

            button19.Enabled = false;
            button19.Text = "";

            if (val[1] == 0)
            {
                checkval = false;
                yours = false;
                other = true;
                messagereceve.RunWorkerAsync();
            }
            if (yours){check();}
        }

        private void button25_Click(object sender, EventArgs e)
        {
            val[0] = 25;
            val[1] = PrintLetter('y');

            sock.Send(val);
            bw.Write('y');

            button25.Enabled = false;
            button25.Text = "";

            if (val[1] == 0)
            {
                checkval = false;
                yours = false;
                other = true;
                messagereceve.RunWorkerAsync();
            }
            if (yours){check();}
        }

        private void button26_Click(object sender, EventArgs e)
        {
            val[0] = 26;
            val[1] = PrintLetter('z');

            sock.Send(val);
            bw.Write('z');

            button26.Enabled = false;
            button26.Text = "";

            if (val[1] == 0)
            {
                checkval = false;
                yours = false;
                other = true;
                messagereceve.RunWorkerAsync();
            }
            if (yours){check();}
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            
        }

        private void resultToolStripMenuItem_Click(object sender, EventArgs e)
        {

            FileStream fs = new FileStream("F:\\Final P\\Result.txt", FileMode.Open);
            StreamReader sr = new StreamReader(fs);
            MessageBox.Show(sr.ReadToEnd());
            sr.Close();
        }
    }
}
