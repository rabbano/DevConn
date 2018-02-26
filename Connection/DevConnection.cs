using Connection.ScaleDev;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Reflection;

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
        private SerialPort _serialPort;
        private int _comPortNum; //номер Com порта
        private int _baudRate; //Скорость порта
        private Parity _parity; //Parity порта
        private int _byteSize; //ByteSize порта
        private StopBits _stopBits; //StopBits порта
        private bool _threadWorkDone = false;//флаг окончания работы потока
        private int _threadSleepTime=0;
        private Thread _connThread;

        //private byte _portState=1;
        public byte PortState { get; private set; } = 1;//состояние, 1- не открывался, 2- открывался, но с ошибкой, 3- нормально открылся
        //{
        //    get => _portState;
        //}
        private byte _connectionType = 0;//тип соединения, 1- ComPort, 2- TCP
        public byte ConnectionType
        {
            get => _connectionType;
        }
        public DevConnection(string Name, int ComPortNum, int BaudRate, Parity Parity, int ByteSize, int StopBit)  //constructor for COMPORT
        {
            this.Name = Name;
            _comPortNum = ComPortNum;
            _baudRate = BaudRate;
            _parity = Parity;
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
            if (PortState == 2)
            {
                _serialPort.Close();
                PortState = 1;
            }
        }
        ~DevConnection()  // destructor
        {
            Name = "";
            _connectionType = 100;
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
                            _serialPort = new SerialPort("COM" + _comPortNum.ToString(), _baudRate, _parity, _byteSize, _stopBits);
                            _serialPort.Open();
                            PortState = 2;
                        }
                        catch (Exception)
                        {
                            PortState = 3;
                        }                        
                    }
                    else if ((DelayReopenPort != 0)&&(PortState == 3)&&((DateTime.Now - OpenPortTime).TotalSeconds>= DelayReopenPort))
                    {
                        OpenPortTime = DateTime.Now;
                        try
                        {
                            _serialPort = new SerialPort("COM" + _comPortNum.ToString(), _baudRate, _parity, _byteSize, _stopBits);
                            _serialPort.Open();
                            PortState = 2;
                        }
                        catch (Exception)
                        {
                            PortState = 3;
                        }

                    }
                    else if (PortState == 2)//если открыт нормально
                    {

                    }
                    #endregion
                    //Counter++;
                    if (_threadSleepTime == 0)
                        Thread.Sleep(100);
                    else
                        Thread.Sleep(_threadSleepTime);
                    _threadSleepTime = 0;
                }
                catch (AppDomainUnloadedException ex) //срабатывает при thread.Abort()
                {
                }
            }
        }
        #region Работа с Utilcell Smart
        public List<UtilcellSmart> SmartList = new List<UtilcellSmart>();
        public void AddSmart(string Name, bool isActivExchangeMode, int RS485Num, int LostConnectionDataCount, int SleepTime)
        {
            UtilcellSmart sc = new UtilcellSmart();
            sc.Name = Name;
            sc.Enabled = true;
            sc.isActivExchangeMode = isActivExchangeMode;
            sc.RS485Num = RS485Num;
            sc.LostConnectionDataCount = (LostConnectionDataCount > 0) ? LostConnectionDataCount : 20;
            sc.SleepTime = (SleepTime > 0) ? SleepTime : 50;
            //sc.OnSartoMsg += this.OnConnMsg;
            this.SmartList.Add(sc);
        }

        #endregion
        public DevConnection(int TCPPortNum, string IPAddress)  //constructor for TCP
        {
            _connectionType = 2;
        }

    }
}
