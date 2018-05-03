using Connection.AnalogInputDev;
using Connection.DiscreteIO;
using Connection.OtherDev;
using Connection.ScaleDev;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO.Ports;

using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Connection
{
    public class DevConnection
    {
        public string Name { get; set; }
        public DateTime OpenPortTime { get; private set; }
        public int DelayReopenPort { get; private set; }=0;
        public int OpenPortCounter { get; private set; } = 0;
        public int ErrManyBytes { get; set; } = 0;//ошибка много данных в порту
        internal SerialPort _serialPort;
        private int _comPortNum; //номер Com порта
        private int _baudRate; //Скорость порта
        private Parity _parity; //Parity порта //0-Even, 1-Mark, 2-None, 3-Odd, 4-Space; 
        private int _byteSize; //ByteSize порта
        private StopBits _stopBits; //StopBits порта
        private bool _threadWorkDone = false;//флаг окончания работы потока
        private int _threadSleepTime=0;
        private Thread _connThread;


        public List<UtilcellSmart> SmartList;
        public List<MicrosimM06> M06List;
        public List<Owen_110_224_1T> Owen110_224_1TList;
        public List<MitsubishiFR_E5xx> MitsubishiFR_E5xxList;
        public List<MI_MDA_15YA> MI_MDA_15YAList;
        public List<ESQ_A1000> ESQ_A1000List;
        public List<I7055> I7055list;

        public byte PortState { get; private set; } = 1;//состояние, 1- не открывался, 2- открывался, но с ошибкой, 3- нормально открылся
        //{
        //    get => _portState;
        //}
        private byte _connectionType = 0;//тип соединения, 1- ComPort, 2- TCP
        internal string uiSep = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;

        public byte ConnectionType
        {
            get => _connectionType;
        }

        public DevConnection(string Name, int ComPortNum, int BaudRate, int Parity, int ByteSize, int StopBit, int DelayReopenPort)  //constructor for COMPORT
        {
            this.Name = Name;
            _comPortNum = ComPortNum;
            _baudRate = BaudRate;
            this.DelayReopenPort = DelayReopenPort;
            switch (Parity)
            {
                case 0: _parity = System.IO.Ports.Parity.Even; break;
                case 1: _parity = System.IO.Ports.Parity.Mark; break;
                case 2: _parity = System.IO.Ports.Parity.None; break;
                case 3: _parity = System.IO.Ports.Parity.Odd; break;
                case 4: _parity = System.IO.Ports.Parity.Space; break;
                default: _parity = System.IO.Ports.Parity.None; break;
            }
            _byteSize = ByteSize;
            switch (StopBit)
            {
                case 1:
                    _stopBits = StopBits.One;
                    break;
                case 2:
                    _stopBits = StopBits.Two;
                    break;
                default:
                    _stopBits = StopBits.One;
                    break;
            }
            _connectionType = 1;

            _connThread = new Thread(new ThreadStart(ThreadMethod));
            _connThread.Start();
        }

        public void CloseConnection()
        {
            _threadWorkDone = true;
            _connThread.Join();
            if (PortState == 3)
            {
                _serialPort.Close();
                PortState = 1;
            }
        }

        private void ThreadMethod()
        {
            while (!_threadWorkDone)
            {
                try
                {
                    #region Открытие/переоткрытие порта
                    if (PortState==1)//если не открывали
                    {
                        OpenPortTime = DateTime.Now;
                        try
                        {
                            OpenPortCounter++;
                            
                            _serialPort = new SerialPort("COM" + _comPortNum.ToString(), _baudRate, _parity, _byteSize, _stopBits);
                            _serialPort.Open();
                            PortState = 3;
                        }
                        catch (Exception)
                        {
                            PortState = 2;
                        }                        
                    }
                    else if ((DelayReopenPort != 0)&&(PortState == 2)&&((DateTime.Now - OpenPortTime).TotalSeconds>= DelayReopenPort))
                    {
                        OpenPortTime = DateTime.Now;
                        try
                        {
                            //_serialPort = new SerialPort("COM" + _comPortNum.ToString(), _baudRate, _parity, _byteSize, _stopBits);
                            OpenPortCounter++;
                            _serialPort.Open();
                            PortState = 3;
                        }
                        catch (Exception)
                        {
                            PortState = 2;
                        }
                    }
                    #endregion
                    else 
                    {
                        if (M06List != null)
                            foreach (var M06 in M06List)
                            {
                                _threadSleepTime += M06.GetM06State(this);
                            }

                        if (SmartList != null)
                            foreach (var smart in SmartList)
                            {
                                _threadSleepTime += smart.GetSmartState(this);
                            }

                        if (Owen110_224_1TList != null)
                            foreach (var owen in Owen110_224_1TList)
                            {
                                _threadSleepTime += owen.GetState(this);
                            }

                        if (MitsubishiFR_E5xxList != null)
                            foreach (var m in MitsubishiFR_E5xxList)
                            {
                                _threadSleepTime += m.GetState(this);
                            }
                        if (MI_MDA_15YAList != null)
                            foreach (var m in MI_MDA_15YAList)
                            {
                                _threadSleepTime += m.GetState(this);
                            }
                        if (ESQ_A1000List != null)
                            foreach (var m in ESQ_A1000List)
                            {
                                _threadSleepTime += m.GetState(this);
                            }
                        if (I7055list != null)
                            foreach (var m in I7055list)
                            {
                                _threadSleepTime += m.GetState(this);
                            }
                    }

                    //Counter++;
                    if (_threadSleepTime == 0)//задержка если никто другой не сделал, чтобы не нагружать поток
                        Thread.Sleep(100);
                    _threadSleepTime = 0;
                }
                catch (Exception) //срабатывает при thread.Abort()
                {
                }
            }
        }

        #region Работа Прибором МИ МДА/15Я Россия
        public void AddMiMda(string Name, bool isActivExchangeMode, int NoAnswerLimit, int SleepTime)
        {
            if (MI_MDA_15YAList == null) MI_MDA_15YAList = new List<MI_MDA_15YA>();
            MI_MDA_15YA mi = new MI_MDA_15YA();
            mi.Name = Name;
            mi.isActivExchangeMode = isActivExchangeMode;
            mi.NoAnswerLimit = (NoAnswerLimit > 0) ? NoAnswerLimit : 20;
            mi.SleepTime = (SleepTime > 0) ? SleepTime : 50;
            this.MI_MDA_15YAList.Add(mi);
        }
        #endregion

        #region Работа с Utilcell Smart
        public void AddSmart(string Name, bool isActivExchangeMode, int RS485Num, int NoAnswerLimit, int SleepTime)
        {
            if (SmartList==null) SmartList = new List<UtilcellSmart>();
            UtilcellSmart sc = new UtilcellSmart();
            sc.Name = Name;
            sc.isActivExchangeMode = isActivExchangeMode;
            sc.RS485Num = RS485Num;
            sc.NoAnswerLimit = (NoAnswerLimit > 0) ? NoAnswerLimit : 20;
            sc.SleepTime = (SleepTime > 0) ? SleepTime : 50;
            this.SmartList.Add(sc);
        }
        #endregion

        #region Работа с Микросим М06
        public void AddМ06(string Name, bool isActivExchangeMode, int RS485Num, int NoAnswerLimit, int SleepTime)
        {
            if (M06List == null) M06List = new List<MicrosimM06>();
            MicrosimM06 M06 = new MicrosimM06();
            M06.Name = Name;
            M06.isActivExchangeMode = isActivExchangeMode;
            M06.RS485Num = RS485Num;
            M06.NoAnswerLimit = (NoAnswerLimit > 0) ? NoAnswerLimit : 20;
            M06.SleepTime = (SleepTime > 0) ? SleepTime : 50;
            this.M06List.Add(M06);
        }
        #endregion

        #region Работа с Owen_110_224_1T
        public void AddOwen_110_224_1T(string Name, int Protocol, int RS485Num, int NoAnswerLimit, int SleepTime)
        {
            if (Owen110_224_1TList == null) Owen110_224_1TList = new List<Owen_110_224_1T>();
            Owen_110_224_1T Owen = new Owen_110_224_1T();
            Owen.Name = Name;
            Owen.Protocol = (Protocol > 0) ? Protocol : 0;
            Owen.RS485Num = RS485Num;
            Owen.NoAnswerLimit = (NoAnswerLimit > 0) ? NoAnswerLimit : 20;
            Owen.SleepTime = (SleepTime > 0) ? SleepTime : 200;
            this.Owen110_224_1TList.Add(Owen);
        }
        #endregion

        #region Добавление Mitsubishi FR_E5xx
        public void AddMitsubishiFR_E5xx(string Name, int RS485Num, int NoAnswerLimit, int SleepTime)
        {
            if (MitsubishiFR_E5xxList == null) MitsubishiFR_E5xxList = new List<MitsubishiFR_E5xx>();
            MitsubishiFR_E5xx Mitsubishi = new MitsubishiFR_E5xx();
            Mitsubishi.Name = Name;
            Mitsubishi.RS485Num = RS485Num;
            Mitsubishi.NoAnswerLimit = (NoAnswerLimit > 0) ? NoAnswerLimit : 20;
            Mitsubishi.SleepTime = (SleepTime > 0) ? SleepTime : 200;
            this.MitsubishiFR_E5xxList.Add(Mitsubishi);
        }
        #endregion


        #region Добавление частотника ESQ_A1000
        public void AddESQ_A1000(string Name, int RS485Num, int NoAnswerLimit, int SleepTime)
        {
            if (ESQ_A1000List == null) ESQ_A1000List = new List<ESQ_A1000>();
            ESQ_A1000 ESQ = new ESQ_A1000();
            ESQ.Name = Name;
            ESQ.RS485Num = RS485Num;
            ESQ.NoAnswerLimit = (NoAnswerLimit > 0) ? NoAnswerLimit : 20;
            ESQ.SleepTime = (SleepTime > 0) ? SleepTime : 200;
            this.ESQ_A1000List.Add(ESQ);
        }
        #endregion

        public DevConnection(int TCPPortNum, string IPAddress)  //constructor for TCP
        {
            _connectionType = 2;
        }

    }
}
