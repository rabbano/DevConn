using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
//Для входа в параметры Mode, листаем до Pr, входим Set, далее номер параметра
//Set вход и выход из параметра, долгое нажатие Set запоминание.
//Используется режим передачи RS485 2 провода, для этого вмете соединяем плюсы и минусы(3+5, 4+6).
//на разъеме RJ45 используем четыре провода 3-(T+), 4-(T-), 5-(R+), 6-(R-)
//Параметр 79 ставим в 1, прибор при загрузке становиться в режим PU.
//надо поставить перемычки RL+PC, других перемычек не надо.
//Параметр 117 это номер устройства
namespace Connection.OtherDev
{
    public class MitsubishiFR_E5xx
    {
        public string Name { get; set; } = string.Empty;//имя прибора
        public int RS485Num { get; set; } = 0; //номер прибора в сети
        public int GetStateCounter { get; private set; } = 0;//количество запросов состояния
        public int LostConnectionCounter { get; private set; }//счетчик потери связи с прибором
        public int SleepTime { get; set; } = 200; //Время ожидания ответа
        public int RecieveLettersCounter { get; private set; } = 0; //полученных хороших ответов
        public int SendLettersCounter { get; private set; } = 0;  //счетчик запросов
        public string StateMess { get; private set; } = String.Empty; //сообщение о состоянии прибора
        public bool isNeedRun { get; set; } = false; //прибор Нужно запустить
        public bool fRun { get; private set; } = false; //прибор работает RUN
        public bool fRunF { get; private set; } = false; //происходит вращение вперед
        public bool fRunB { get; private set; } = false; //происходит вращение назад
        public bool fHzMax { get; private set; } = false; //Достигнута максимальная частота
        public bool fOL { get; private set; } = false; //Перегруз
        public bool fHzOk { get; private set; } = false; //Достигнута Заданная частота
        public bool fErr { get; private set; } = false; //в ошибке
        public int NoAnswerLimit { get; set; } = 20; //количестов опросов в который не пришел ответ, считается потеря связи
        public bool isNeedStart { get; set; } = false; //необходимо запустить частотник
        public bool isNeedStop { get; set; } = false; //необходимо остановить частотник
        public float CurrentHz { get; private set; } = 0; //Текущая частота
        public float PresetHz { get; set; } = 0; //Заданная частота
        public float CurrentA { get; private set; } = 0; //Текущий ток
        public float CurrentV { get; private set; } = 0; //Текущее напряжение

        public int ErrAnswerLenghtCounter { get; private set; } = 0; //полученных ответов не той длинны
        public int ErrConvertCounter { get; private set; } = 0; //ошибок преобразования в число из строки
        public int ErrCRC { get; private set; } = 0;//полученных ответов с ошибкой CRC
        public int ErrConvertCRC { get; private set; } = 0; //ошибка при преобразовании CRC в число при получении ответа
        public int ErrGetStateDevice { get; private set; } = 0; //ошибка произошла в процессе получения данных
        public string ErrGetStateDeviceMess { get; private set; } = string.Empty;//текст ошибки

        byte[] ByteBuff = new byte[200];
        int NoAnswerCounter = 0; //счетчик неполученных ответов

        internal int GetState(DevConnection DevConn)
        {
            GetStateCounter++;
            if (DevConn.PortState == 3)
            {
                try
                {
                    int BytesToRead = 0;
                    string Rs485 = String.Format("{0:D2}", RS485Num);
                    string CRC = string.Empty;

                    #region Проверка состояния
                    ByteBuff[0] = 0x05;//начало пакета
                    ByteBuff[1] = Convert.ToByte(Rs485[0]);
                    ByteBuff[2] = Convert.ToByte(Rs485[1]);
                    ByteBuff[3] = 0x37;//7
                    ByteBuff[4] = 0x41;//A
                    ByteBuff[5] = 0x30; //время ожидания
                    ByteBuff[20] = 0;
                    for (int i = 1; i <= 5; i++)
                        ByteBuff[20] += ByteBuff[i];
                    CRC = String.Format("{0:X2}", ByteBuff[20]);
                    ByteBuff[6] = Convert.ToByte(CRC[0]);
                    ByteBuff[7] = Convert.ToByte(CRC[1]);
                    ByteBuff[8] = 13;
                    DevConn._serialPort.Write(ByteBuff, 0, 9);

                    SendLettersCounter++;
                    Thread.Sleep(SleepTime);

                    BytesToRead = DevConn._serialPort.BytesToRead;


                    if (BytesToRead == 9)
                    {
                        Array.Clear(ByteBuff, 0, ByteBuff.Length);
                        DevConn._serialPort.Read(ByteBuff, 0, BytesToRead);
                        //проверяем CRC
                        ByteBuff[20] = 0;
                        for (int i = 1; i < BytesToRead - 4; i++)
                            ByteBuff[20] += ByteBuff[i];
                        int RecievedCRC = -1;
                        try
                        {
                            RecievedCRC = Convert.ToInt32(ASCIIEncoding.ASCII.GetString(ByteBuff, BytesToRead - 3, 2), 16);
                        }
                        catch (Exception)
                        {
                            ErrConvertCRC++;
                        }

                        if ((RecievedCRC != -1) && (ByteBuff[20] == RecievedCRC))
                        {
                            string BinState = Convert.ToString(Convert.ToInt32(ASCIIEncoding.ASCII.GetString(ByteBuff, 3, 2), 16), 2).PadLeft(8, '0');
                            fRun = (BinState[7].Equals('1')) ? true : false;
                            fRunF = (BinState[6].Equals('1')) ? true : false;
                            fRunB = (BinState[5].Equals('1')) ? true : false;
                            fHzMax = (BinState[4].Equals('1')) ? true : false;
                            fOL = (BinState[3].Equals('1')) ? true : false;
                            fHzOk = (BinState[1].Equals('1')) ? true : false;
                            fErr = (BinState[0].Equals('1')) ? true : false;

                            StateMess = "Ok";
                            NoAnswerCounter = 0;
                            RecieveLettersCounter++;
                        }
                        else
                        {
                            if (RecievedCRC != -1) ErrCRC++;
                        }
                    }
                    else
                    {
                        if (BytesToRead > 0)
                        {
                            ErrAnswerLenghtCounter++;
                            //Array.Clear(ByteBuff, 0, ByteBuff.Length);
                            //DevConn._serialPort.Read(ByteBuff, 0, BytesToRead);
                            DevConn._serialPort.DiscardInBuffer();
                        }
                    }
                    #endregion

                    #region Запрос текущей частоты
                    ByteBuff[0] = 0x05;//начало пакета
                    ByteBuff[1] = Convert.ToByte(Rs485[0]);
                    ByteBuff[2] = Convert.ToByte(Rs485[1]);
                    ByteBuff[3] = 0x36;//6
                    ByteBuff[4] = 0x46;//F
                    ByteBuff[5] = 0x30; //время ожидания
                    ByteBuff[20] = 0;
                    for (int i = 1; i <= 5; i++)
                        ByteBuff[20] += ByteBuff[i];
                    CRC = String.Format("{0:X2}", ByteBuff[20]);
                    ByteBuff[6] = Convert.ToByte(CRC[0]);
                    ByteBuff[7] = Convert.ToByte(CRC[1]);
                    ByteBuff[8] = 13;
                    DevConn._serialPort.Write(ByteBuff, 0, 9);

                    SendLettersCounter++;
                    Thread.Sleep(SleepTime);

                    BytesToRead = DevConn._serialPort.BytesToRead;

                    if (BytesToRead == 11)
                    {
                        Array.Clear(ByteBuff, 0, ByteBuff.Length);
                        DevConn._serialPort.Read(ByteBuff, 0, BytesToRead);
                        //проверяем CRC
                        ByteBuff[20] = 0;
                        for (int i = 1; i < BytesToRead - 4; i++)
                            ByteBuff[20] += ByteBuff[i];
                        int RecievedCRC = -1;
                        try
                        {
                            RecievedCRC = Convert.ToInt32(ASCIIEncoding.ASCII.GetString(ByteBuff, BytesToRead - 3, 2), 16);
                        }
                        catch (Exception)
                        {
                            ErrConvertCRC++;
                        }
                        if ((RecievedCRC != -1) && (ByteBuff[20] == RecievedCRC))
                        {
                            CurrentHz = Convert.ToInt32(ASCIIEncoding.ASCII.GetString(ByteBuff, 3, 4), 16) / 100;

                            StateMess = "Ok";
                            NoAnswerCounter = 0;
                            RecieveLettersCounter++;
                        }
                        else
                        {
                            if (RecievedCRC != -1) ErrCRC++;
                        }
                    }
                    else
                    {
                        if (BytesToRead > 0)
                        {
                            ErrAnswerLenghtCounter++;
                            DevConn._serialPort.DiscardInBuffer();
                        }
                    }
                    #endregion

                    #region Запрос текущего тока
                    ByteBuff[0] = 0x05;//начало пакета
                    ByteBuff[1] = Convert.ToByte(Rs485[0]);
                    ByteBuff[2] = Convert.ToByte(Rs485[1]);
                    ByteBuff[3] = 0x37;//7
                    ByteBuff[4] = 0x30;//0
                    ByteBuff[5] = 0x30; //время ожидания
                    ByteBuff[20] = 0;
                    for (int i = 1; i <= 5; i++)
                        ByteBuff[20] += ByteBuff[i];
                    CRC = String.Format("{0:X2}", ByteBuff[20]);
                    ByteBuff[6] = Convert.ToByte(CRC[0]);
                    ByteBuff[7] = Convert.ToByte(CRC[1]);
                    ByteBuff[8] = 13;
                    DevConn._serialPort.Write(ByteBuff, 0, 9);

                    SendLettersCounter++;
                    Thread.Sleep(SleepTime);

                    BytesToRead = DevConn._serialPort.BytesToRead;

                    if (BytesToRead == 11)
                    {
                        Array.Clear(ByteBuff, 0, ByteBuff.Length);
                        DevConn._serialPort.Read(ByteBuff, 0, BytesToRead);
                        //проверяем CRC
                        ByteBuff[20] = 0;
                        for (int i = 1; i < BytesToRead - 4; i++)
                            ByteBuff[20] += ByteBuff[i];
                        int RecievedCRC = -1;
                        try
                        {
                            RecievedCRC = Convert.ToInt32(ASCIIEncoding.ASCII.GetString(ByteBuff, BytesToRead - 3, 2), 16);
                        }
                        catch (Exception)
                        {
                            ErrConvertCRC++;
                        }
                        if ((RecievedCRC != -1) && (ByteBuff[20] == RecievedCRC))
                        {
                            CurrentA = Convert.ToInt32(ASCIIEncoding.ASCII.GetString(ByteBuff, 3, 4), 16) / 10;

                            StateMess = "Ok";
                            NoAnswerCounter = 0;
                            RecieveLettersCounter++;
                        }
                        else
                        {
                            if (RecievedCRC != -1) ErrCRC++;
                        }
                    }
                    else
                    {
                        if (BytesToRead > 0)
                        {
                            ErrAnswerLenghtCounter++;
                            DevConn._serialPort.DiscardInBuffer();
                        }
                    }
                    #endregion

                    #region Запрос текущего напряжения
                    ByteBuff[0] = 0x05;//начало пакета
                    ByteBuff[1] = Convert.ToByte(Rs485[0]);
                    ByteBuff[2] = Convert.ToByte(Rs485[1]);
                    ByteBuff[3] = 0x37;//7
                    ByteBuff[4] = 0x31;//1
                    ByteBuff[5] = 0x30; //время ожидания
                    ByteBuff[20] = 0;
                    for (int i = 1; i <= 5; i++)
                        ByteBuff[20] += ByteBuff[i];
                    CRC = String.Format("{0:X2}", ByteBuff[20]);
                    ByteBuff[6] = Convert.ToByte(CRC[0]);
                    ByteBuff[7] = Convert.ToByte(CRC[1]);
                    ByteBuff[8] = 13;
                    DevConn._serialPort.Write(ByteBuff, 0, 9);

                    SendLettersCounter++;
                    Thread.Sleep(SleepTime);

                    BytesToRead = DevConn._serialPort.BytesToRead;

                    if (BytesToRead == 11)
                    {
                        Array.Clear(ByteBuff, 0, ByteBuff.Length);
                        DevConn._serialPort.Read(ByteBuff, 0, BytesToRead);
                        //проверяем CRC
                        ByteBuff[20] = 0;
                        for (int i = 1; i < BytesToRead - 4; i++)
                            ByteBuff[20] += ByteBuff[i];
                        int RecievedCRC = -1;
                        try
                        {
                            RecievedCRC = Convert.ToInt32(ASCIIEncoding.ASCII.GetString(ByteBuff, BytesToRead - 3, 2), 16);
                        }
                        catch (Exception)
                        {
                            ErrConvertCRC++;
                        }
                        if ((RecievedCRC != -1) && (ByteBuff[20] == RecievedCRC))
                        {
                            CurrentV = Convert.ToInt32(ASCIIEncoding.ASCII.GetString(ByteBuff, 3, 4), 16) / 10;

                            StateMess = "Ok";
                            NoAnswerCounter = 0;
                            RecieveLettersCounter++;
                        }
                        else
                        {
                            if (RecievedCRC != -1) ErrCRC++;
                        }
                    }
                    else
                    {
                        if (BytesToRead > 0)
                        {
                            ErrAnswerLenghtCounter++;
                            DevConn._serialPort.DiscardInBuffer();
                        }
                    }
                    #endregion

                    #region Старт/Стоп
                    if ((isNeedRun && !fRun) || (!isNeedRun && fRun))
                    {
                        ByteBuff[0] = 0x05;//начало пакета
                        ByteBuff[1] = Convert.ToByte(Rs485[0]);
                        ByteBuff[2] = Convert.ToByte(Rs485[1]);
                        ByteBuff[3] = 0x46;//F
                        ByteBuff[4] = 0x41;//A
                        ByteBuff[5] = 0x30; //время ожидания
                        ByteBuff[6] = 0x30; //
                        ByteBuff[7] = (isNeedRun) ? (Byte)0x32 : (Byte)0x30;


                        ByteBuff[20] = 0;
                        for (int i = 1; i <= 7; i++)
                            ByteBuff[20] += ByteBuff[i];
                        CRC = String.Format("{0:X2}", ByteBuff[20]);
                        ByteBuff[8] = Convert.ToByte(CRC[0]);
                        ByteBuff[9] = Convert.ToByte(CRC[1]);
                        ByteBuff[10] = 13;
                        DevConn._serialPort.Write(ByteBuff, 0, 11);

                        SendLettersCounter++;
                        Thread.Sleep(SleepTime);

                        BytesToRead = DevConn._serialPort.BytesToRead;

                        if (BytesToRead == 4)
                        {
                            Array.Clear(ByteBuff, 0, ByteBuff.Length);
                            DevConn._serialPort.Read(ByteBuff, 0, BytesToRead);
                            RecieveLettersCounter++;
                        }
                        else
                        {
                            if (BytesToRead > 0)
                            {
                                ErrAnswerLenghtCounter++;
                                DevConn._serialPort.DiscardInBuffer();
                            }
                        }
                    }
                    #endregion

                    #region Задание частоты

                    if (fRun && (CurrentHz != PresetHz))
                    {
                        String Hez = String.Format("{0:X4}", (int)PresetHz * 100);

                        ByteBuff[0] = 0x05;//начало пакета
                        ByteBuff[1] = Convert.ToByte(Rs485[0]);
                        ByteBuff[2] = Convert.ToByte(Rs485[1]);
                        ByteBuff[3] = 0x45;//E
                        ByteBuff[4] = 0x45;//E
                        ByteBuff[5] = 0x30; //время ожидания
                        ByteBuff[6] = Convert.ToByte(Hez[0]); //
                        ByteBuff[7] = Convert.ToByte(Hez[1]); //
                        ByteBuff[8] = Convert.ToByte(Hez[2]); //
                        ByteBuff[9] = Convert.ToByte(Hez[3]); //

                        ByteBuff[20] = 0;
                        for (int i = 1; i <= 9; i++)
                            ByteBuff[20] += ByteBuff[i];
                        CRC = String.Format("{0:X2}", ByteBuff[20]);
                        ByteBuff[10] = Convert.ToByte(CRC[0]);
                        ByteBuff[11] = Convert.ToByte(CRC[1]);
                        ByteBuff[12] = 13;
                        DevConn._serialPort.Write(ByteBuff, 0, 13);

                        SendLettersCounter++;
                        Thread.Sleep(SleepTime);

                        BytesToRead = DevConn._serialPort.BytesToRead;

                        if (BytesToRead == 4)
                        {
                            Array.Clear(ByteBuff, 0, ByteBuff.Length);
                            DevConn._serialPort.Read(ByteBuff, 0, BytesToRead);
                            RecieveLettersCounter++;
                        }
                        else
                        {
                            if (BytesToRead > 0)
                            {
                                ErrAnswerLenghtCounter++;
                                DevConn._serialPort.DiscardInBuffer();
                            }
                        }
                    }
                    #endregion

                    if (NoAnswerCounter < NoAnswerLimit)
                        NoAnswerCounter++;
                    if (NoAnswerCounter == NoAnswerLimit)//нет ответа определенное время
                    {
                        NoAnswerCounter++;
                        StateMess = "Нет связи с Mitsubishi E5xx";
                        CurrentHz = 0;
                        CurrentA = 0;
                        CurrentV = 0;
                        fRun = false;
                        fRunB = false;
                        fRunF = false;
                        LostConnectionCounter++;
                    }
                }
                catch (Exception e)
                {
                    //ErrGetStateDeviceMess = e.Message;
                    ErrGetStateDevice++;
                    // Get stack trace for the exception with source file information
                    var st = new StackTrace(e, true);
                    // Get the top stack frame
                    var frame = st.GetFrame(0);
                    // Get the line number from the stack frame
                    var line = frame.GetFileLineNumber();
                    ErrGetStateDeviceMess = e.Message + " " + line;
                }

                return SleepTime;
            }
            else
            {
                StateMess = "Ошибка открытия порта";
                return 0;
            }
        }
    }
}
