using Connection;
using Connection.DiscreteIO;
using Connection.ScaleDev;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
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

            //ConnList.Add(new DevConnection("Соединение " + ConnList.Count().ToString(), ConnList.Count() + 1, 19200, 0, 8, 1, 0));
            //ConnList.Add(new DevConnection("Соединение " + ConnList.Count().ToString(), ConnList.Count() + 1, 9600, 2, 8, 1, 0));

            //ConnList[0].AddSmart("Smart 1", false, 1, 20, 20);
            //ConnList[0].SmartList[0].SleepTime = 50;

            //ConnList[1].AddМ06("M06 1", true, 1, 20, 300);

            //ConnList.Add(new DevConnection("Соединение " + ConnList.Count().ToString(), 1, 9600, 2, 8, 1, 0));
            //ConnList[0].AddOwen_110_224_1T("Owen 1", 0, 1, 20, 200);
            //ConnList[0].AddOwen_110_224_1T("Owen 2", 0, 2, 20, 200);
            //ConnList[0].AddOwen_110_224_1T("Owen 3", 0, 3, 20, 200);


            //ConnList.Add(new DevConnection("Соединение " + ConnList.Count().ToString(), 2, 19200, 0, 8, 2, 0));
            //ConnList[0].AddMitsubishiFR_E5xx("Mitsubishi 1", 1, 20, 100);
            //ConnList[0].AddMitsubishiFR_E5xx("Mitsubishi 2", 2, 20, 100);
            //ConnList[0].AddMitsubishiFR_E5xx("Mitsubishi 3", 3, 20, 100);
            //ConnList[0].AddMitsubishiFR_E5xx("Mitsubishi 4", 4, 20, 100);
            //ConnList[0].AddMitsubishiFR_E5xx("Mitsubishi 5", 5, 20, 100);
            //ConnList[0].AddMitsubishiFR_E5xx("Mitsubishi 6", 6, 20, 100);


            //ConnList.Add(new DevConnection("Соединение " + ConnList.Count().ToString(), 7, 1200, 2, 8, 1, 0));
            //ConnList[0].AddMiMda("МИ МДА/15Я", false, 20, 150);


            //ConnList.Add(new DevConnection("Соединение " + ConnList.Count().ToString(), 2, 9600, 2, 8, 1, 0));
            //ConnList[0].AddESQ_A1000("ESQ_A1000", 1, 20, 75);
            //ConnList[0].AddESQ_A1000("ESQ_A1000", 2, 20, 75);
            //ConnList[0].AddESQ_A1000("ESQ_A1000", 3, 20, 75);
            //ConnList[0].AddESQ_A1000("ESQ_A1000", 4, 20, 75);
            //ConnList[0].AddESQ_A1000("ESQ_A1000", 5, 20, 75);
            //ConnList[0].AddESQ_A1000("ESQ_A1000", 6, 20, 75);

            ConnList.Add(new DevConnection("Соединение " + ConnList.Count().ToString(), 6, 57600, 2, 8, 1, 0));
            ConnList[0].I7055list = new List<I7055>();
            ConnList[0].I7055list.Add(new I7055("I7055 - 1", 1, 20, 100, true, true));
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            foreach (DevConnection dc in ConnList)
            {
                if (ConnList[0].I7055list!=null)
                {
                    foreach (var m in ConnList[0].I7055list)
                    {
                        for (int i = 0; i < 8; i++)
                            m.OutputState[i] = 0;                        
                    }
                }
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

        private void Owen110_224_1T_Tick(object sender, EventArgs e)
        {
            int ConnNum = 0;
            int DevNum = 1;
            TestBox.Text = "----------------Прибор Owen110_224_1T-------------" + "\n";
            TestBox.Text += "\n";
            TestBox.Text += "----------------Статистика порта" + "\n";
            TestBox.Text += string.Format("Состояние отрытия -  {0}", ConnList[ConnNum].PortState) + "\n";
            TestBox.Text += string.Format("Получено много данных -  {0}", ConnList[ConnNum].ErrManyBytes) + "\n";

            TestBox.Text += "\n";
            TestBox.Text += "----------------Состояние прибора" + "\n";
            TestBox.Text += string.Format("Запросов состояния - {0}", ConnList[ConnNum].Owen110_224_1TList[DevNum].GetStateCounter) + "\n";
            TestBox.Text += string.Format("Послано пакетов - {0}", ConnList[ConnNum].Owen110_224_1TList[DevNum].SendLettersCounter) + "\n";
            TestBox.Text += string.Format("Принято пакетов - {0}", ConnList[ConnNum].Owen110_224_1TList[DevNum].RecieveLettersCounter) + "\n";
            TestBox.Text += string.Format("Значение А 1 - {0:f5}", ConnList[ConnNum].Owen110_224_1TList[DevNum].A) + "\n";
            TestBox.Text += string.Format("Значение А 2 - {0:f5}", ConnList[ConnNum].Owen110_224_1TList[1].A) + "\n";
            TestBox.Text += string.Format("Значение А 3 - {0:f5}", ConnList[ConnNum].Owen110_224_1TList[2].A) + "\n";
            TestBox.Text += string.Format("Значение Гц - {0:f5}", ConnList[ConnNum].Owen110_224_1TList[DevNum].Hz) + "\n";
            TestBox.Text += "Сообщение состояния 1- " + ConnList[ConnNum].Owen110_224_1TList[DevNum].StateMess + "\n";
            TestBox.Text += "Сообщение состояния 2- " + ConnList[ConnNum].Owen110_224_1TList[1].StateMess + "\n";
            TestBox.Text += "Сообщение состояния 3- " + ConnList[ConnNum].Owen110_224_1TList[2].StateMess + "\n";
            TestBox.Text += string.Format("Количество потерей связи - {0:f0}", ConnList[ConnNum].Owen110_224_1TList[DevNum].LostConnectionCounter) + "\n";

            TestBox.Text += "\n";
            TestBox.Text += "----------------Ошибки" + "\n";
            //TestBox.Text += string.Format("Пакет не той длинны - {0}", ConnList[ConnNum].Owen110_224_1TList[DevNum].ErrAnswerLenghtCounter) + "\n";
            TestBox.Text += string.Format("Ошибок преобразования в число - {0}", ConnList[ConnNum].Owen110_224_1TList[DevNum].ErrConvertCounter) + "\n";
            TestBox.Text += string.Format("Ошибок при запросе состояния - {0}", ConnList[ConnNum].Owen110_224_1TList[DevNum].ErrGetStateDevice) + "\n";
            TestBox.Text += string.Format("Ошибок контрольной суммы - {0}", ConnList[ConnNum].Owen110_224_1TList[DevNum].ErrCRC) + "\n";
            TestBox.Text += "Текст последней ошибки - " + ConnList[ConnNum].Owen110_224_1TList[DevNum].ErrGetStateDeviceMess + "\n";

        }

        private void MitsubishiE500_Tick(object sender, EventArgs e)
        {
            int ConnNum = 0;
            int DevNum = 3;
            TestBox.Text = "----------------Прибор Mitsubishi E500-------------" + "\n";
            TestBox.Text += "\n";
            TestBox.Text += "----------------Статистика порта" + "\n";
            TestBox.Text += string.Format("Состояние отрытия -  {0}", ConnList[ConnNum].PortState) + "\n";

            TestBox.Text += "\n";
            TestBox.Text += "----------------Состояние прибора" + "\n";
            TestBox.Text += string.Format("Запросов состояния - {0}", ConnList[ConnNum].MitsubishiFR_E5xxList[DevNum].GetStateCounter) + "\n";
            TestBox.Text += "Сообщение состояния - " + ConnList[ConnNum].MitsubishiFR_E5xxList[DevNum].StateMess + "\n";
            TestBox.Text += string.Format("Послано пакетов - {0}", ConnList[ConnNum].MitsubishiFR_E5xxList[DevNum].SendLettersCounter) + "\n";
            TestBox.Text += string.Format("Принято пакетов - {0}", ConnList[ConnNum].MitsubishiFR_E5xxList[DevNum].RecieveLettersCounter) + "\n";
            TestBox.Text += string.Format("Текущая частота 1- {0:f2}", ConnList[ConnNum].MitsubishiFR_E5xxList[0].CurrentHz) + "\n";
            TestBox.Text += string.Format("Текущая частота 2- {0:f2}", ConnList[ConnNum].MitsubishiFR_E5xxList[1].CurrentHz) + "\n";
            TestBox.Text += string.Format("Текущая частота 3- {0:f2}", ConnList[ConnNum].MitsubishiFR_E5xxList[2].CurrentHz) + "\n";
            TestBox.Text += string.Format("Текущая частота 4- {0:f2}", ConnList[ConnNum].MitsubishiFR_E5xxList[3].CurrentHz) + "\n";
            TestBox.Text += string.Format("Текущая частота 5- {0:f2}", ConnList[ConnNum].MitsubishiFR_E5xxList[4].CurrentHz) + "\n";
            TestBox.Text += string.Format("Текущая частота 6- {0:f2}", ConnList[ConnNum].MitsubishiFR_E5xxList[5].CurrentHz) + "\n";
            TestBox.Text += string.Format("Текущий ток- {0:f2}", ConnList[ConnNum].MitsubishiFR_E5xxList[DevNum].CurrentA) + "\n";
            TestBox.Text += string.Format("Текущее напряжение- {0:f2}", ConnList[ConnNum].MitsubishiFR_E5xxList[DevNum].CurrentV) + "\n";
            TestBox.Text += string.Format("Run - {0}", ConnList[ConnNum].MitsubishiFR_E5xxList[DevNum].fRun) + "\n";
            TestBox.Text += string.Format("Run F - {0}", ConnList[ConnNum].MitsubishiFR_E5xxList[DevNum].fRunF) + "\n";
            TestBox.Text += string.Format("Run B- {0}", ConnList[ConnNum].MitsubishiFR_E5xxList[DevNum].fRunB) + "\n";
            TestBox.Text += string.Format("Максимальная частота- {0}", ConnList[ConnNum].MitsubishiFR_E5xxList[DevNum].fHzMax) + "\n";
            TestBox.Text += string.Format("Перегруз- {0}", ConnList[ConnNum].MitsubishiFR_E5xxList[DevNum].fOL) + "\n";
            TestBox.Text += string.Format("Достигнута макс частота- {0}", ConnList[ConnNum].MitsubishiFR_E5xxList[DevNum].fHzMax) + "\n";
            TestBox.Text += string.Format("Error- {0}", ConnList[ConnNum].MitsubishiFR_E5xxList[DevNum].fErr) + "\n";
            TestBox.Text += "\n";
            TestBox.Text += "----------------Ошибки" + "\n";
            TestBox.Text += string.Format("Пакет не той длинны - {0}", ConnList[ConnNum].MitsubishiFR_E5xxList[DevNum].ErrAnswerLenghtCounter) + "\n";
            TestBox.Text += string.Format("Ошибок преобразования в число - {0}", ConnList[ConnNum].MitsubishiFR_E5xxList[DevNum].ErrConvertCounter) + "\n";
            TestBox.Text += string.Format("Ошибок при запросе состояния - {0}", ConnList[ConnNum].MitsubishiFR_E5xxList[DevNum].ErrGetStateDevice) + "\n";
            TestBox.Text += string.Format("Ошибок контрольной суммы - {0}", ConnList[ConnNum].MitsubishiFR_E5xxList[DevNum].ErrCRC) + "\n";
            TestBox.Text += string.Format("Ошибок конвертации контрольной суммы - {0}", ConnList[ConnNum].MitsubishiFR_E5xxList[DevNum].ErrConvertCRC) + "\n";
            TestBox.Text += "Текст последней ошибки - " + ConnList[ConnNum].MitsubishiFR_E5xxList[DevNum].ErrGetStateDeviceMess + "\n";

        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {

            ConnList[0].ESQ_A1000List[0].isNeedRun = checkBox1.Checked;
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            ConnList[0].ESQ_A1000List[1].PresetHz = 20;
            ConnList[0].ESQ_A1000List[1].isNeedRun = checkBox2.Checked;
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            ConnList[0].ESQ_A1000List[2].PresetHz = 20;
            ConnList[0].ESQ_A1000List[2].isNeedRun = checkBox3.Checked;
        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            ConnList[0].ESQ_A1000List[3].PresetHz = 20;
            ConnList[0].ESQ_A1000List[3].isNeedRun = checkBox4.Checked;
        }

        private void checkBox5_CheckedChanged(object sender, EventArgs e)
        {
            ConnList[0].ESQ_A1000List[4].PresetHz = 20;
            ConnList[0].ESQ_A1000List[4].isNeedRun = checkBox5.Checked;
        }

        private void button2_Click_2(object sender, EventArgs e)
        {

        }

        private void checkBox6_CheckedChanged(object sender, EventArgs e)
        {
            ConnList[0].ESQ_A1000List[5].PresetHz = 20;
            ConnList[0].ESQ_A1000List[5].isNeedRun = checkBox6.Checked;
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            label1.Text = trackBar1.Value.ToString();
            ConnList[0].ESQ_A1000List[5].PresetHz = trackBar1.Value;
        }

        private void Mi_MDA_Tick(object sender, EventArgs e)
        {
            int ConnNum = 0;
            int DevNum = 0;
            if ((ConnList != null) && (ConnList.Count >= ConnNum - 1) && (ConnList[ConnNum].MI_MDA_15YAList != null))
            {
                TestBox.Text = "----------------Прибор МИ МДА/15Я-------------" + "\n";
                TestBox.Text += "\n";
                TestBox.Text += "----------------Статистика порта" + "\n";
                TestBox.Text += string.Format("Состояние отрытия -  {0}", ConnList[ConnNum].PortState) + "\n";

                TestBox.Text += "\n";
                TestBox.Text += "----------------Состояние прибора" + "\n";
                TestBox.Text += string.Format("Запросов состояния - {0}", ConnList[ConnNum].MI_MDA_15YAList[DevNum].GetStateCounter) + "\n";
                TestBox.Text += "Сообщение состояния - " + ConnList[ConnNum].MI_MDA_15YAList[DevNum].StateMess + "\n";
                TestBox.Text += string.Format("Послано пакетов - {0}", ConnList[ConnNum].MI_MDA_15YAList[DevNum].SendLettersCounter) + "\n";
                TestBox.Text += string.Format("Принято пакетов - {0}", ConnList[ConnNum].MI_MDA_15YAList[DevNum].RecieveLettersCounter) + "\n";
                TestBox.Text += string.Format("Значение веса : {0:f3}", ConnList[ConnNum].MI_MDA_15YAList[DevNum].ScaleResult) + "\n";
                TestBox.Text += "\n";
                TestBox.Text += "----------------Ошибки" + "\n";
                TestBox.Text += string.Format("Пакет не той длинны - {0}", ConnList[ConnNum].MI_MDA_15YAList[DevNum].ErrAnswerLenghtCounter) + "\n";
                TestBox.Text += string.Format("Ошибок преобразования в число - {0}", ConnList[ConnNum].MI_MDA_15YAList[DevNum].ErrConvertCounter) + "\n";
                TestBox.Text += string.Format("Ошибок при запросе состояния - {0}", ConnList[ConnNum].MI_MDA_15YAList[DevNum].ErrGetStateDevice) + "\n";
                //TestBox.Text += string.Format("Ошибок контрольной суммы - {0}", ConnList[ConnNum].MI_MDA_15YAList[DevNum].ErrCRC) + "\n";
                //TestBox.Text += string.Format("Ошибок конвертации контрольной суммы - {0}", ConnList[ConnNum].MI_MDA_15YAList[DevNum].ErrConvertCRC) + "\n";
                TestBox.Text += "Текст последней ошибки - " + ConnList[ConnNum].MI_MDA_15YAList[DevNum].ErrGetStateDeviceMess + "\n";
                TestBox.Text += "\n";
                foreach (var Weigth in ConnList[ConnNum].MI_MDA_15YAList[DevNum].ResultList)
                {
                    TestBox.Text += Weigth.Result.ToString() + " : " + Weigth.ResultTime.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture) + "\n";
                }
            }

        }

        private void ESQ_A1000_Tick(object sender, EventArgs e)
        {
            int ConnNum = 0;
            int DevNum = 5;
            if ((ConnList != null) && (ConnList.Count >= ConnNum - 1) && (ConnList[ConnNum].ESQ_A1000List != null))
            {
                TestBox.Text = "----------------Прибор ESQ_A1000-------------" + "\n";
                TestBox.Text += "\n";
                TestBox.Text += "----------------Статистика порта" + "\n";
                TestBox.Text += string.Format("Состояние отрытия -  {0}", ConnList[ConnNum].PortState) + "\n";

                TestBox.Text += "\n";
                TestBox.Text += "----------------Состояние прибора" + "\n";
                TestBox.Text += string.Format("Запросов состояния - {0}", ConnList[ConnNum].ESQ_A1000List[DevNum].GetStateCounter) + "\n";
                TestBox.Text += "Сообщение состояния 1- " + ConnList[ConnNum].ESQ_A1000List[0].StateMess + "\n";
                TestBox.Text += "Сообщение состояния 2- " + ConnList[ConnNum].ESQ_A1000List[1].StateMess + "\n";
                TestBox.Text += "Сообщение состояния 3- " + ConnList[ConnNum].ESQ_A1000List[2].StateMess + "\n";
                TestBox.Text += "Сообщение состояния 4- " + ConnList[ConnNum].ESQ_A1000List[3].StateMess + "\n";
                TestBox.Text += "Сообщение состояния 5- " + ConnList[ConnNum].ESQ_A1000List[4].StateMess + "\n";
                TestBox.Text += "Сообщение состояния 6- " + ConnList[ConnNum].ESQ_A1000List[5].StateMess + "\n";
                TestBox.Text += string.Format("Послано пакетов - {0}", ConnList[ConnNum].ESQ_A1000List[DevNum].SendLettersCounter) + "\n";
                TestBox.Text += string.Format("Принято пакетов - {0}", ConnList[ConnNum].ESQ_A1000List[DevNum].RecieveLettersCounter) + "\n";
                TestBox.Text += string.Format("Режим работы - {0}", ConnList[ConnNum].ESQ_A1000List[DevNum].Mode) + "\n";
                TestBox.Text += string.Format("Текущая частота 1- {0:f2}", ConnList[ConnNum].ESQ_A1000List[DevNum].CurrentHz) + "\n";
                //TestBox.Text += string.Format("Текущая частота 2- {0:f2}", ConnList[ConnNum].ESQ_A1000List[1].CurrentHz) + "\n";
                //TestBox.Text += string.Format("Текущая частота 3- {0:f2}", ConnList[ConnNum].ESQ_A1000List[2].CurrentHz) + "\n";
                //TestBox.Text += string.Format("Текущая частота 4- {0:f2}", ConnList[ConnNum].ESQ_A1000List[3].CurrentHz) + "\n";
                //TestBox.Text += string.Format("Текущая частота 5- {0:f2}", ConnList[ConnNum].ESQ_A1000List[4].CurrentHz) + "\n";
                //TestBox.Text += string.Format("Текущая частота 6- {0:f2}", ConnList[ConnNum].ESQ_A1000List[5].CurrentHz) + "\n";
                TestBox.Text += string.Format("Текущий ток- {0:f2}", ConnList[ConnNum].ESQ_A1000List[DevNum].CurrentA) + "\n";
                TestBox.Text += string.Format("Текущее напряжение- {0:f2}", ConnList[ConnNum].ESQ_A1000List[DevNum].CurrentV) + "\n";
                TestBox.Text += string.Format("Run - {0}", ConnList[ConnNum].ESQ_A1000List[DevNum].fRun) + "\n";
                TestBox.Text += string.Format("Run F - {0}", ConnList[ConnNum].ESQ_A1000List[DevNum].fRunF) + "\n";
                TestBox.Text += string.Format("Run B- {0}", ConnList[ConnNum].ESQ_A1000List[DevNum].fRunB) + "\n";
                TestBox.Text += string.Format("Перегруз- {0}", ConnList[ConnNum].ESQ_A1000List[DevNum].fOL) + "\n";
                TestBox.Text += "\n";
            }
        }

        private void I7055_Tick(object sender, EventArgs e)
        {
            int ConnNum = 0;
            int DevNum = 0;
            if ((ConnList != null) && (ConnList.Count >= ConnNum - 1) && (ConnList[ConnNum].I7055list != null))
            {
                TestBox.Text = "----------------Прибор I7055-------------" + "\n";
                TestBox.Text += "\n";
                TestBox.Text += "----------------Статистика порта" + "\n";
                TestBox.Text += string.Format("Состояние отрытия -  {0}", ConnList[ConnNum].PortState) + "\n";

                TestBox.Text += "\n";
                TestBox.Text += "----------------Состояние прибора" + "\n";
                TestBox.Text += "Сообщение состояния- " + ConnList[ConnNum].I7055list[0].StateMess + "\n";
                TestBox.Text += string.Format("Послано пакетов - {0}", ConnList[ConnNum].I7055list[DevNum].SendLettersCounter) + "\n";
                TestBox.Text += string.Format("Принято пакетов - {0}", ConnList[ConnNum].I7055list[DevNum].RecieveLettersCounter) + "\n";
                string OutStr = String.Empty;
                for (int i = 0; i < 8; i++)
                    OutStr += " " + ConnList[ConnNum].I7055list[DevNum].InputState[i].ToString();
                TestBox.Text += "Входа : "+ OutStr + "\n";
                TestBox.Text += "\n";
                TestBox.Text += "----------------Ошибки" + "\n";
                TestBox.Text += string.Format("Пакет не той длинны - {0}", ConnList[ConnNum].I7055list[DevNum].ErrAnswerLenght) + "\n";
                TestBox.Text += string.Format("Ошибок при запросе состояния - {0}", ConnList[ConnNum].I7055list[DevNum].ErrGetStateDevice) + "\n";
                TestBox.Text += string.Format("Ошибок контрольной суммы - {0}", ConnList[ConnNum].I7055list[DevNum].ErrCRC) + "\n";
                TestBox.Text += string.Format("Ошибок конвертации контрольной суммы - {0}", ConnList[ConnNum].I7055list[DevNum].ErrCRC) + "\n";
                TestBox.Text += "Текст последней ошибки - " + ConnList[ConnNum].I7055list[DevNum].ErrGetStateDeviceMess + "\n";

            }
        }

        private void checkBox12_CheckedChanged(object sender, EventArgs e)
        {
            if ((sender as CheckBox).Checked)
                ConnList[0].I7055list[0].OutputState[Convert.ToInt32((sender as CheckBox).Tag)] = 1;
            else
                ConnList[0].I7055list[0].OutputState[Convert.ToInt32((sender as CheckBox).Tag)] = 0;

        }
    }
}
