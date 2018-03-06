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
using System.Xml;

namespace TestApp
{
    public partial class Form1 : Form
    {
        public List<DevConnection> ConnList = new List<DevConnection>();

        public Form1()
        {
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e)
        {
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            int ConnNum = 0;
            int DevNum = 0;
            TestBox.Text = "----------------Прибор Smart-------------" + "\n";
            TestBox.Text += "\n";
            TestBox.Text += "----------------Статистика порта" + "\n";
            TestBox.Text += string.Format("Состояние отрытия -  {0}", ConnList[ConnNum].PortState) + "\n";
            TestBox.Text += string.Format("Получено много данных -  {0}", ConnList[ConnNum].ErrManyBytes) + "\n";

            TestBox.Text += "\n";
            TestBox.Text += "----------------Состояние прибора" + "\n";
            TestBox.Text += string.Format("Послано пакетов - {0}", ConnList[ConnNum].SmartList[DevNum].SendLettersCounter) + "\n";
            TestBox.Text += string.Format("Принято пакетов - {0}", ConnList[ConnNum].SmartList[DevNum].RecieveLettersCounter) + "\n";
            TestBox.Text += string.Format("Запросов состояния - {0}", ConnList[ConnNum].SmartList[DevNum].GetStateCounter) + "\n";
            TestBox.Text += string.Format("Значение веса - {0:f2}", ConnList[ConnNum].SmartList[DevNum].ScaleResult) + "\n";
            TestBox.Text += string.Format("Стабильность веса - {0:f2}", ConnList[ConnNum].SmartList[DevNum].IsStable) + "\n";
            TestBox.Text += string.Format("Перегруз весов - {0:f2}", ConnList[ConnNum].SmartList[DevNum].IsOverload) + "\n";
            TestBox.Text += string.Format("Ошибка веса - {0:f2}", ConnList[ConnNum].SmartList[DevNum].IsInvalid) + "\n";
            TestBox.Text += "Сообщение состояния - " + ConnList[ConnNum].SmartList[DevNum].StateMess + "\n";
            TestBox.Text += string.Format("Количество потерей связи - {0:f0}", ConnList[ConnNum].SmartList[DevNum].LostConnectionCounter) + "\n";

            TestBox.Text += "\n";
            TestBox.Text += "----------------Ошибки" + "\n";
            TestBox.Text += string.Format("Пакет не той длинны - {0}", ConnList[ConnNum].SmartList[DevNum].ErrAnswerLenghtCounter) + "\n";
            TestBox.Text += string.Format("Ошибок преобразования в число - {0}", ConnList[ConnNum].SmartList[DevNum].ErrConvertCounter) + "\n";
            TestBox.Text += string.Format("Ошибок при запросе состояния - {0}", ConnList[ConnNum].SmartList[DevNum].ErrGetStateDevice) + "\n";


        }

        private void button2_Click_1(object sender, EventArgs e)
        {
        }

        private void button3_Click(object sender, EventArgs e)//sdfsdf
        {
            int TreeLevel = -1;
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
            DevConnection element = ConnList.FirstOrDefault(l => l.Name == "Соединение 0" && l.Name == "Соединение 0");
            if (element != null)
                ConnList.Remove(element);
        }

        private void Form1_Load(object sender, EventArgs e)
        {

            ConnList.Add(new DevConnection("Соединение " + ConnList.Count().ToString(), ConnList.Count() + 1, 19200, 0, 8, 1, 0));
            ConnList.Add(new DevConnection("Соединение " + ConnList.Count().ToString(), ConnList.Count() + 1, 9600, 2, 8, 1, 0));

            ConnList[0].AddSmart("Smart 1", false, 1, 20, 20);
            //ConnList[0].SmartList[0].SleepTime = 50;

            ConnList[1].AddМ06("M06 1", true, 1, 20, 300);

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
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void M06_Tick(object sender, EventArgs e)
        {
            int ConnNum = 1;
            int DevNum = 0;
            TestBox.Text = "----------------Прибор M06-------------" + "\n";
            TestBox.Text += "\n";
            TestBox.Text += "----------------Статистика порта" + "\n";
            TestBox.Text += string.Format("Состояние отрытия -  {0}", ConnList[ConnNum].PortState) + "\n";
            TestBox.Text += string.Format("Получено много данных -  {0}", ConnList[ConnNum].ErrManyBytes) + "\n";

            TestBox.Text += "\n";
            TestBox.Text += "----------------Состояние прибора" + "\n";
            TestBox.Text += string.Format("Послано пакетов - {0}", ConnList[ConnNum].M06List[DevNum].SendLettersCounter) + "\n";
            TestBox.Text += string.Format("Принято пакетов - {0}", ConnList[ConnNum].M06List[DevNum].RecieveLettersCounter) + "\n";
            TestBox.Text += string.Format("Запросов состояния - {0}", ConnList[ConnNum].M06List[DevNum].GetStateCounter) + "\n";
            TestBox.Text += string.Format("Значение веса - {0:f3}", ConnList[ConnNum].M06List[DevNum].ScaleResult) + "\n";
            TestBox.Text += string.Format("Значение брутто - {0:f3}", ConnList[ConnNum].M06List[DevNum].Brutto) + "\n";
            TestBox.Text += string.Format("Значение Тара - {0:f3}", ConnList[ConnNum].M06List[DevNum].Tara) + "\n";
            TestBox.Text += string.Format("Значение Нетто - {0:f3}", ConnList[ConnNum].M06List[DevNum].Netto) + "\n";
            TestBox.Text += string.Format("Значение Ноль - {0:f3}", ConnList[ConnNum].M06List[DevNum].Zero) + "\n";
            TestBox.Text += string.Format("Значение АЦП - {0:f0}", ConnList[ConnNum].M06List[DevNum].ACPResult) + "\n";

            TestBox.Text += "Сообщение состояния - " + ConnList[ConnNum].M06List[DevNum].StateMess + "\n";
            TestBox.Text += string.Format("Количество потерей связи - {0:f0}", ConnList[ConnNum].M06List[DevNum].LostConnectionCounter) + "\n";
            TestBox.Text += string.Format("Ошибка подключения датчика - {0}", ConnList[ConnNum].M06List[DevNum].fNoLoadCell) + "\n";
            TestBox.Text += string.Format("Перегруз весов - {0}", ConnList[ConnNum].M06List[DevNum].fOverLoad) + "\n";
            TestBox.Text += string.Format("Значение веса существенно меньше нуля - {0}", ConnList[ConnNum].M06List[DevNum].fUnderLoad) + "\n";
            TestBox.Text += string.Format("Значение веса меньше 20d - {0}", ConnList[ConnNum].M06List[DevNum].fMin20d) + "\n";
            TestBox.Text += string.Format("Значение веса близко к нулю +-0.5d - {0}", ConnList[ConnNum].M06List[DevNum].fNearZero) + "\n";
            TestBox.Text += string.Format("Стабильность веса - {0}", ConnList[ConnNum].M06List[DevNum].IsStable) + "\n";
            TestBox.Text += string.Format("Прибор в режиме калибровки нуля - {0}", ConnList[ConnNum].M06List[DevNum].fZeroClb) + "\n";
            TestBox.Text += string.Format("Прибор в режиме калибровки - {0}", ConnList[ConnNum].M06List[DevNum].fClb) + "\n";

            TestBox.Text += "\n";
            TestBox.Text += "----------------Ошибки" + "\n";
            TestBox.Text += string.Format("Пакет не той длинны - {0}", ConnList[ConnNum].M06List[DevNum].ErrAnswerLenghtCounter) + "\n";
            TestBox.Text += string.Format("Ошибок преобразования в число - {0}", ConnList[ConnNum].M06List[DevNum].ErrConvertCounter) + "\n";
            TestBox.Text += string.Format("Ошибок при запросе состояния - {0}", ConnList[ConnNum].M06List[DevNum].ErrGetStateDevice) + "\n";
            TestBox.Text += string.Format("Ошибок контрольной суммы - {0}", ConnList[ConnNum].M06List[DevNum].ErrCRC) + "\n";
            TestBox.Text += "Текст последней ошибки - " + ConnList[ConnNum].M06List[DevNum].ErrGetStateDeviceMess + "\n";

        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            ConnList[1].M06List[0].isNeedZero = true;
        }
    }
}
