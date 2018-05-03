using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
//Для входа в параметры нажимаем моde и выбираем set параметр
//79 параметр надо поставить в 2, 36 параметр номер станции.
namespace Connection.OtherDev
{
    public class ESQ_A1000
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
        public bool fOL { get; private set; } = false; //Перегруз
        public bool fHzOk { get; private set; } = false; //Достигнута Заданная частота
        public int NoAnswerLimit { get; set; } = 20; //количестов опросов в который не пришел ответ, считается потеря связи
        public bool isNeedStart { get; set; } = false; //необходимо запустить частотник
        public bool isNeedStop { get; set; } = false; //необходимо остановить частотник
        public float CurrentHz { get; private set; } = 0; //Текущая частота
        public float PresetHz { get; set; } = 0; //Заданная частота
        public float CurrentA { get; private set; } = 0; //Текущий ток
        public float CurrentV { get; private set; } = 0; //Текущее напряжение
        public int Mode { get; private set; } = -1; //Текущий режим работы(нам нужен 0)

        public int ErrAnswerLenghtCounter { get; private set; } = 0; //полученных ответов не той длинны
        public int ErrConvertCounter { get; private set; } = 0; //ошибок преобразования в число из строки
        public int ErrCRC { get; private set; } = 0;//полученных ответов с ошибкой CRC
        public int ErrConvertCRC { get; private set; } = 0; //ошибка при преобразовании CRC в число при получении ответа
        public int ErrGetStateDevice { get; private set; } = 0; //ошибка произошла в процессе получения данных
        public string ErrGetStateDeviceMess { get; private set; } = string.Empty;//текст ошибки

        byte[] ByteBuff = new byte[200];
        int NoAnswerCounter = 0; //счетчик неполученных ответов

        /*
            5 48 48 55 49 48 70 56 13 Запрос
            05 30 30 37 31 30 46 38 0D
             2 48 48 53 66 55 49 50 3 55 49 13//Ответ напряжения 234.09(35 42 37 31)
            02 30 30 35 42 37 31 32 03 37 31 0D


             5 48 48 54 70 48 48 67 13  Запрос частоты
            05 30 30 36 46 30 30 43 0D

            2 48 48 48 66 66 56 50 3 55 69 13
            02 30 30 30 42 42 38 32 03 37 45 0D

             5 48 48 55 48 48 70 55 13  Запрос тока
            05 30 30 37 30 30 46 37 0D

              2 48 48 48 48 69 56 50 3 54 70 13
               02 30 30 30 30 45 38 32 03 36 46 0D

            5 48 48 55 65 48 48 56 13   Запрос состояния
            05 30 30 37 41 30 30 38 0D

             2 48 48 48 48 52 66 48 3 54 54 13
            02 30 30 30 30 34 42 30 03 36 36 0D

             5 48 48 69 69 48 49 55 55 48 69 57 13//Задал частоту 60.00
            05 30 30 45 45 30 31 37 37 30 45 39 0D

            6 48 48 13
            06 30 30 0D

             5 48 48 69 69 48 49 51 56 55 69 68 13//Задал частоту 49.99(49 51 56 55)
             05 30 30 45 45 30 31 33 38 37 45 44 0D
            00EE01387ED

             6 48 48 13
             06 30 30 0D



             5 48 48 70 66 48 48 48 48 48 68 56 13  //В режим CU
             05 30 30 46 42 30 30 30 30 30 44 38 0D
            00FB00000D8

             6 48 48 13
             06 30 30 0D
         */
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

                    #region Проверка текущего состояния работы, Mode
                    ByteBuff[0] = 0x05;//начало пакета
                    ByteBuff[1] = Convert.ToByte(Rs485[0]);
                    ByteBuff[2] = Convert.ToByte(Rs485[1]);
                    ByteBuff[3] = 0x37;//7
                    ByteBuff[4] = 0x42;//B
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

                    if (BytesToRead == 12)
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
                            Mode = Convert.ToInt32(ASCIIEncoding.ASCII.GetString(ByteBuff, 3, 4), 16);
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

                    #region Перевод в режим режим передачи данных CU
                    if (Mode != 0)
                    {
                        ByteBuff[0] = 0x05;//начало пакета
                        ByteBuff[1] = Convert.ToByte(Rs485[0]);
                        ByteBuff[2] = Convert.ToByte(Rs485[1]);
                        ByteBuff[3] = 0x46;//F
                        ByteBuff[4] = 0x42;//B
                        ByteBuff[5] = 0x30; //время ожидания 05 30 30 46 42 30 30 30 30 30 44 38 0D
                        ByteBuff[6] = 0x30;
                        ByteBuff[7] = 0x30;
                        ByteBuff[8] = 0x30;
                        ByteBuff[9] = 0x30;
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
                            StateMess = "Ok";
                            NoAnswerCounter = 0;
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


                    if (BytesToRead == 12)
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
                            string BinState = Convert.ToString(Convert.ToInt32(ASCIIEncoding.ASCII.GetString(ByteBuff, 3, 4), 16), 2).PadLeft(8, '0');
                            fRun = (BinState[7].Equals('1')) ? true : false;
                            fRunF = (BinState[6].Equals('1')) ? true : false;
                            fRunB = (BinState[5].Equals('1')) ? true : false;
                            fHzOk = (BinState[4].Equals('1')) ? true : false;
                            fOL = (BinState[3].Equals('1')) ? true : false;

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

                    if (BytesToRead == 12)
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
                            float _currentHz = Convert.ToInt32(ASCIIEncoding.ASCII.GetString(ByteBuff, 3, 4), 16);
                            switch (ByteBuff[7])
                            {
                                case 49:
                                    CurrentHz = _currentHz / 10;
                                    break;
                                case 50:
                                    CurrentHz = _currentHz / 100;
                                    break;
                                case 51:
                                    CurrentHz = _currentHz / 1000;
                                    break;
                                default:
                                    CurrentHz = _currentHz;
                                    break;
                            }
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

                    if (BytesToRead == 12)
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
                            float _currentA = Convert.ToInt32(ASCIIEncoding.ASCII.GetString(ByteBuff, 3, 4), 16);
                            switch (ByteBuff[7])
                            {
                                case 49:
                                    CurrentA = _currentA/10;
                                    break;
                                case 50:
                                    CurrentA = _currentA / 100;
                                    break;
                                case 51:
                                    CurrentA = _currentA / 1000;
                                    break;
                                default:
                                    CurrentA = _currentA;
                                    break;
                            }
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

                    if (BytesToRead == 12)
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
                            float _currentV = Convert.ToInt32(ASCIIEncoding.ASCII.GetString(ByteBuff, 3, 4), 16) ;
                            switch (ByteBuff[7])
                            {
                                case 49:
                                    CurrentV = _currentV / 10;
                                    break;
                                case 50:
                                    CurrentV = _currentV / 100;
                                    break;
                                case 51:
                                    CurrentV = _currentV / 1000;
                                    break;
                                default:
                                    CurrentV = _currentV;
                                    break;
                            }
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
                        ByteBuff[7] = 0x30; //
                        ByteBuff[8] = 0x30; //
                        ByteBuff[9] = (isNeedRun) ? (Byte)0x32 : (Byte)0x30;

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

                    #region Задание частоты

                    if (fRun && (CurrentHz != PresetHz))
                    {
                        String Hez = String.Format("{0:X4}", (int)PresetHz * 100);

                        ByteBuff[0] = 0x05;//начало пакета5 48 48 69 69 48 49 51 56 55 69 68 13//Задал частоту 49.99(49 51 56 55)
                        ByteBuff[1] = Convert.ToByte(Rs485[0]);
                        ByteBuff[2] = Convert.ToByte(Rs485[1]);
                        ByteBuff[3] = 0x45;//
                        ByteBuff[4] = 0x45;//
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
                        StateMess = "Нет связи с ESQ-A1000";
                        CurrentHz = 0;
                        CurrentA = 0;
                        CurrentV = 0;
                        fRun = false;
                        fRunB = false;
                        fRunF = false;
                        Mode = -1;
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
                    return SleepTime;
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
