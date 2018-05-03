using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Connection.DiscreteIO
{
    public class I7055
    {
        string _name { get; set; } = string.Empty;//имя прибора
        int _rs485Num { get; set; } = 0; //номер прибора в сети
        int _noAnswerLimit { get; set; } = 20; //количестов опросов в который не пришел ответ, считается потеря связи
        int _sleepTime { get; set; } = 200; //Время ожидания ответа
        bool _cRCEnabled { get; set; } = true;//Опрос с контрольной суммой
        public int RecieveLettersCounter { get; private set; } = 0; //полученных хороших ответов
        public int SendLettersCounter { get; private set; } = 0;  //счетчик запросов
        public string StateMess { get; private set; } = String.Empty; //сообщение о состоянии прибора
        public int LostConnectionCounter { get; private set; } = 0;//счетчик потери связи с прибором

        public int ErrGetStateDevice { get; private set; } = 0; //ошибка произошла в процессе получения данных
        public string ErrGetStateDeviceMess { get; private set; } = String.Empty;//текст ошибки
        public int ErrCRC { get; private set; } = 0; //полученных ответов с ошибкой CRC
        public int ErrAnswerLenght { get; private set; } = 0; //пришел ответ не той длинны
        public byte[] OutputState { get; set; } = new byte[8];
        byte[] _outputStateFact = new byte[8] { 1, 1, 1, 1, 1, 1, 1, 1 };//Фактическое значение выходов, если совпадает с основным не шлем пакет
        public bool _diEnabled { get; private set; } = false;//опрашивать ли состояние входов
        //public byte[] InputState { get; private set; } = new byte[8]; //состояние входов
        public BitArray InputState = new BitArray(8);
        byte[] ByteBuff = new byte[200];
        int NoAnswerCounter = 0; //счетчик неполученных ответов

        public I7055(string Name, int RS485Num, int NoAnswerLimit, int SleepTime, bool DiEnabled, bool CRCEnabled)
        {
            _name = Name;
            _rs485Num = RS485Num;
            _noAnswerLimit = NoAnswerLimit;
            _sleepTime = SleepTime;
            _diEnabled = DiEnabled;
            _cRCEnabled = CRCEnabled;
        }

        internal int GetState(DevConnection DevConn)
        {
            if (DevConn.PortState == 3)
            {
                int BytesToRead = 0;
                try
                {
                    #region Установка 8-ми выходов
                    bool ChangeFlag = false;
                    for (int i = 0; i < 8; i++)
                        if (OutputState[i] != _outputStateFact[i])
                            ChangeFlag = true;
                    if (ChangeFlag)//есть изменения в состоянии выходов
                    {
                        ByteBuff[0] = Convert.ToByte('@');
                        string Rs485 = String.Format("{0:D2}", _rs485Num);
                        ByteBuff[1] = Convert.ToByte(Rs485[0]);
                        ByteBuff[2] = Convert.ToByte(Rs485[1]);
                        StringBuilder sb = new StringBuilder();
                        for (int i = 7; i >= 0; i--)
                            sb.Append(OutputState[i]);
                        string OutputString = String.Format("{0:X2}", Convert.ToInt32(sb.ToString(), 2));
                        ByteBuff[3] = Convert.ToByte(OutputString[0]);
                        ByteBuff[4] = Convert.ToByte(OutputString[1]);
                        if (_cRCEnabled)
                        {
                            ByteBuff[20] = 0;
                            for (int i = 0; i <= 4; i++)
                                ByteBuff[20] += ByteBuff[i];
                            string CRC = String.Format("{0:X2}", ByteBuff[20]);
                            ByteBuff[5] = Convert.ToByte(CRC[0]);
                            ByteBuff[6] = Convert.ToByte(CRC[1]);
                            ByteBuff[7] = 13;

                            DevConn._serialPort.Write(ByteBuff, 0, 8);
                        }
                        else
                        {
                            ByteBuff[5] = 13;
                            DevConn._serialPort.Write(ByteBuff, 0, 6);
                        }

                        SendLettersCounter++;
                        Thread.Sleep(_sleepTime);
                        BytesToRead = DevConn._serialPort.BytesToRead;
                        if (BytesToRead > 0)
                        {
                            Array.Clear(ByteBuff, 0, ByteBuff.Length);
                            DevConn._serialPort.Read(ByteBuff, 0, BytesToRead);
                            bool AnswerOk = false;
                            if (_cRCEnabled && BytesToRead == 4)
                            {
                                if (ByteBuff[0] == Convert.ToInt32(ASCIIEncoding.ASCII.GetString(ByteBuff, 1, 2), 16))
                                    AnswerOk = true;
                                else
                                    ErrCRC++;

                            }
                            else if (!_cRCEnabled && BytesToRead == 2)
                            {
                                AnswerOk = true;
                            }
                            else
                            {
                                DevConn._serialPort.DiscardInBuffer();
                                ErrAnswerLenght++;
                            }

                            if (AnswerOk)
                            {
                                StateMess = "Ok";
                                NoAnswerCounter = 0;
                                RecieveLettersCounter++;
                                Array.Copy(OutputState, _outputStateFact, 8);
                            }
                        }
                    }
                    #endregion

                    #region Опрос входов
                    if (_diEnabled)
                    {
                        ByteBuff[0] = Convert.ToByte('@');
                        string Rs485 = String.Format("{0:D2}", _rs485Num);
                        ByteBuff[1] = Convert.ToByte(Rs485[0]);
                        ByteBuff[2] = Convert.ToByte(Rs485[1]);
                        if (_cRCEnabled)
                        {
                            ByteBuff[20] = 0;
                            for (int i = 0; i <= 2; i++)
                                ByteBuff[20] += ByteBuff[i];
                            string CRC = String.Format("{0:X2}", ByteBuff[20]);
                            ByteBuff[3] = Convert.ToByte(CRC[0]);
                            ByteBuff[4] = Convert.ToByte(CRC[1]);
                            ByteBuff[5] = 13;

                            DevConn._serialPort.Write(ByteBuff, 0, 6);
                        }
                        else
                        {
                            ByteBuff[3] = 13;
                            DevConn._serialPort.Write(ByteBuff, 0, 4);
                        }

                        SendLettersCounter++;
                        Thread.Sleep(_sleepTime);
                        BytesToRead = DevConn._serialPort.BytesToRead;
                        if (BytesToRead > 0)
                        {
                            Array.Clear(ByteBuff, 0, ByteBuff.Length);
                            DevConn._serialPort.Read(ByteBuff, 0, BytesToRead);
                            bool AnswerOk = false;
                            if (_cRCEnabled && BytesToRead == 8)
                            {
                                ByteBuff[20] = 0;
                                for (int i = 0; i < BytesToRead-3; i++)
                                    ByteBuff[20] += ByteBuff[i];
                                if (ByteBuff[20] == Convert.ToInt32(ASCIIEncoding.ASCII.GetString(ByteBuff, BytesToRead-3, 2), 16))
                                    AnswerOk = true;
                                else
                                    ErrCRC++;
                            }
                            else if (!_cRCEnabled && BytesToRead == 6)
                            {
                                AnswerOk = true;
                            }
                            else
                            {
                                DevConn._serialPort.DiscardInBuffer();
                                ErrAnswerLenght++;
                            }

                            if (AnswerOk)
                            {
                                string BinState = Convert.ToString(Convert.ToInt32(ASCIIEncoding.ASCII.GetString(ByteBuff, 3, 2), 16), 2).PadLeft(8, '0');
                                for (int i = 0; i < 8; i++)
                                {
                                    InputState[7-i] = (BinState[i].Equals('1')) ? true : false;
                                }
                                StateMess = "Ok";
                                NoAnswerCounter = 0;
                                RecieveLettersCounter++;
                            }
                        }
                    }
                    #endregion
                    if (NoAnswerCounter < _noAnswerLimit)
                        NoAnswerCounter++;
                    if (NoAnswerCounter == _noAnswerLimit)//нет ответа определенное время
                    {
                        NoAnswerCounter++;
                        StateMess = "Нет связи с модулем I7055";
                        LostConnectionCounter++;
                    }

                }
                catch (Exception ex)
                {

                    ErrGetStateDevice++;
                    //// Get stack trace for the exception with source file information
                    //var st = new StackTrace(ex, true);
                    //// Get the top stack frame
                    //var frame = st.GetFrame(0);
                    //// Get the line number from the stack frame
                    //var line = frame.GetFileLineNumber();
                    //ErrGetStateDeviceMess = ex.Message+" "+ line+""+ BytesToRead;
                    ErrGetStateDeviceMess = ex.Message;
                }
                return _sleepTime;
            }
            else
            {
                StateMess = "Ошибка открытия порта";
                return 0;
            }

        }
    }
}
