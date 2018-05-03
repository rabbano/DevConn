using System;
using System.Text;
using System.Threading;


namespace Connection.AnalogInputDev
{
    public class Owen_110_224_1T
    {
        public string Name { get; set; } = string.Empty;//имя прибора
        public int Protocol { get; set; } = 0; //Протокол обмена, 0-Dcon, 1-Owen, 2-Modbus
        public int RS485Num { get; set; } = 0; //номер прибора в сети
        public int NoAnswerLimit { get; set; } = 20; //количестов опросов в который не пришел ответ, считается потеря связи
        public int SleepTime { get; set; } = 200; //Время ожидания ответа
        public int RecieveLettersCounter { get; private set; } = 0; //полученных хороших ответов
        public int SendLettersCounter { get; private set; } = 0;  //счетчик запросов
        public string StateMess { get; private set; } = String.Empty; //сообщение о состоянии прибора
        public float A { get; private set; } = 0; //Текущее значение тока
        public float Hz { get; private set; } = 0; //Текущее значение герц
        public int GetStateCounter { get; private set; } = 0;//количество запросов состояния
        public int LostConnectionCounter { get; private set; }//счетчик потери связи с прибором

        public int ErrGetStateDevice { get; private set; } = 0; //ошибка произошла в процессе получения данных
        public int ErrConvertCounter { get; private set; } = 0; //ошибок преобразования в число из строки
        public string ErrGetStateDeviceMess { get; private set; }//текст ошибки
        public int ErrCRC { get; private set; } //полученных ответов с ошибкой CRC


        byte[] ByteBuff = new byte[200];
        int NoAnswerCounter = 0; //счетчик неполученных ответов

        internal int GetState(DevConnection DevConn)
        {
            GetStateCounter++;
            if (DevConn.PortState == 3)
            {
                if (Protocol == 0)//используется протокол DCON
                {
                    int BytesToRead = 0;
                    try
                    {
                        ByteBuff[0] = Convert.ToByte('#'); ;
                        string Rs485 = String.Format("{0:D2}", RS485Num);
                        ByteBuff[1] = Convert.ToByte(Rs485[0]);
                        ByteBuff[2] = Convert.ToByte(Rs485[1]);
                        ByteBuff[20] = 0;
                        for (int i = 0; i <= 2; i++)
                        {
                            ByteBuff[20] += ByteBuff[i];
                        }
                        string CRC = String.Format("{0:X2}", ByteBuff[20]);
                        ByteBuff[3] = Convert.ToByte(CRC[0]);
                        ByteBuff[4] = Convert.ToByte(CRC[1]);
                        ByteBuff[5] = 13;

                        DevConn._serialPort.Write(ByteBuff, 0, 6);
                        SendLettersCounter++;
                        Thread.Sleep(SleepTime);
                        BytesToRead = DevConn._serialPort.BytesToRead;
                        if (BytesToRead == 19)
                        {
                            Array.Clear(ByteBuff, 0, ByteBuff.Length);
                            DevConn._serialPort.Read(ByteBuff, 0, BytesToRead);
                            //проверяем CRC
                            ByteBuff[20] = 0;
                            for (int i = 0; i < BytesToRead - 3; i++)
                            {
                                ByteBuff[20] += ByteBuff[i];
                            }
                            if (ByteBuff[20] == Convert.ToInt32(ASCIIEncoding.ASCII.GetString(ByteBuff, 16, 2), 16))
                            {
                                StringBuilder sb = new StringBuilder();
                                for (int i = 0; i < 19; i++)
                                    sb.Append(Convert.ToChar(ByteBuff[i]));
                                if (DevConn.uiSep == ",") sb.Replace(".", ",");//заменяем точки если системный разделитель запятая
                                if ((float.TryParse(sb.ToString(1, 9), out float fResult1)) && (float.TryParse(sb.ToString(11, 5), out float fResult2)))
                                {
                                    A = fResult1;
                                    Hz = fResult2;
                                    StateMess = "Ok";
                                    NoAnswerCounter = 0;
                                    RecieveLettersCounter++;
                                }
                                //else
                                //{
                                //    ErrConvertCounter++;
                                //    A = -999;
                                //    Hz = -99;
                                //}
                            }
                            else
                            {
                                ErrCRC++;
                            }
                        }
                        else
                        {
                            if (BytesToRead>0)
                                DevConn._serialPort.DiscardInBuffer();                 
                        }

                        if (NoAnswerCounter < NoAnswerLimit)
                            NoAnswerCounter++;
                        if (NoAnswerCounter == NoAnswerLimit)//нет ответа определенное время
                        {
                            NoAnswerCounter++;
                            A = -999;
                            Hz = -999;
                            StateMess = "Нет связи с контроллером Owen 110 224.1T";
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
                }
                return SleepTime;
            }
            else
            {
                StateMess = "Ошибка открытия порта";
                A = -999;
                Hz = -999;
                return 0;
            }
            
        }

    }
}
