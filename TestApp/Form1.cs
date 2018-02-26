using Connection;
using Connection.ScaleDev;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestApp
{
    public partial class Form1 : Form
    {
        public List<DevConnection> ConnList = new List<DevConnection>();

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //DevConnection NewConnection = new DevConnection("Соединение "+ ConnCounter.ToString(), 1, 9600, 0, 8, 1);
            ConnList.Add(new DevConnection("Соединение " + ConnList.Count().ToString(), ConnList.Count()+1, 19200, 0, 8, 1));
            //ConnList[ConnList.Count() - 1].Counter = ConnList.Count() * 1000;
            //ConnList.Add(NewConnection);
        }

        private void button2_Click(object sender, EventArgs e)
        {
        }

        private void timer1_Tick(object sender, EventArgs e)
        {

            //treeView1.EndUpdate();
            label1.Text = ConnList.Count().ToString();
            foreach (DevConnection dc in ConnList)
            {
                label1.Text += " - " + dc.ErrManyBytes.ToString();
            }
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            ConnList[0].AddSmart("Smart 1", false, 1, 0, 0);
            //ConnList.RemoveAt(0);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            int TreeLevel = -1;
            //treeView1.BeginUpdate();
            treeView1.Nodes.Clear();
            if (ConnList.Count() > 0)
            {
                foreach (DevConnection dc in ConnList)
                {
                    TreeLevel++;
                    treeView1.Nodes.Add(dc.Name);
                    if (dc.SmartList.Count() > 0)
                    {
                        treeView1.Nodes[TreeLevel].Nodes.Add("Приборы Utilcell Smart");
                        foreach (UtilcellSmart smart in dc.SmartList)
                        {
                            treeView1.Nodes[TreeLevel].Nodes[0].Nodes.Add(smart.Name);
                        }
                        treeView1.Nodes[TreeLevel].Nodes[0].Expand();
                    }
                    treeView1.Nodes[TreeLevel].Expand();
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            //ConnList.RemoveAt(0);
            //ConnList = null;
            DevConnection element = ConnList.FirstOrDefault(l => l.Name == "Соединение 0" && l.Name == "Соединение 0");
            if (element != null)
                ConnList.Remove(element);
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            foreach (DevConnection dc in ConnList)
            {
                dc.CloseConnection();
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            ConnList = null;
        }
    }
}
