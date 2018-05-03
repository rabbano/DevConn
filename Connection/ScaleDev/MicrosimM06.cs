using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Connection.ScaleDev
{
    public class MicrosimM06 : ScaleController
    {
        public int RecieveLettersCounter { get; private set; } = 0; //полученных хороших ответов
        public int SendLettersCounter { get; private set; } = 0;  //счетчик запросов
        public int GetStateCounter { get; private set; } = 0;//количество запросов состояния
        public int ErrAnswerLenghtCounter { get; private set; } = 0; //полученных ответов не той длинны
        public int ErrConvertCounter { get; private set; } = 0; //ошибок преобразования в число из строки
        public int ErrCRC { get; private set; } //полученных ответов с ошибкой CRC
        public int ErrGetStateDevice { get; private set; } = 0; //ошибка произошла в процессе получения данных
        public string ErrGetStateDeviceMess { get; private set; }//текст ошибки
        public float ScaleResult { get; private set; } = 0; //Полученный вес в КГ
        public int ACPResult { get; private set; } = 0; //Полученный вес в КГ
        public float Brutto { get; private set; }//текущий вес брутто
        public float Netto { get; private set; }//текущий вес нетто
        public float Tara { get; private set; }//текущий вес Тара
        public float Zero { get; private set; }//текущий вес Ноль
        public bool IsStable { get; private set; } = false; //стабильность веса
        public bool IsZero { get; private set; } = false; //ноль весов

        public string StateMess { get; private set; } = String.Empty; //сообщение о состоянии прибора
        public int LostConnectionCounter { get; private set; }//счетчик потери связи с прибором
        public int PointPosition { get; private set; } //позиция точки на приборе

        public bool fNoLoadCell { get; private set; } //флаг состояния подключения датчика, ошибка подключения датчика
        public bool fOverLoad { get; private set; } //флаг состояния подключения датчика, значение веса больше НПВ+9d
        public bool fUnderLoad { get; private set; } //флаг состояния подключения датчика, значение веса существенно меньше нуля
        public bool fMin20d { get; private set; } //флаг состояния подключения датчика, значение веса меньше 20d
        public bool fNearZero { get; private set; } //флаг состояния подключения датчика, значение веса близко к нулю +-0.5d
        public bool fClb { get; private set; } //флаг состояния подключения датчика, прибор в режиме калибровки
        public bool fZeroClb { get; private set; } //флаг состояния подключения датчика, прибор в режиме калибровки нуля

        public int RS485Num { get; set; } = 0; //номер прибора в сети
        public float ZeroRange { get; set; } = 0; //Если есть диапазон нуля внутри которого вес считается равным нулю
        public bool isNeedZero { get; set; } = false; //Необходимо обнулить весы
        byte[] AnswerByteBuff = new byte[200];
        int storedBytes = 0;
        int NoAnswerCounter = 0; //счетчик неполученных ответов
        int BytesToRead = 0;

        internal int GetM06State(DevConnection DevConn)
        {
            GetStateCounter++;
            if (DevConn.PortState == 3)
            {
                #region Режим обмена по запросу
                if (isActivExchangeMode)//по запросу
                {
                    try
                    {
                        byte SendCommandNum = 0;
                        AnswerByteBuff[0] = 0xFF;
                        AnswerByteBuff[1] = Convert.ToByte(0x20 + RS485Num);
                        AnswerByteBuff[2] = 0x20;

                        if (isNeedZero) //Если необходимо обнулить
                        {
                            isNeedZero = false;
                            AnswerByteBuff[3] = 0x4B;
                            AnswerByteBuff[4] = 0x41;
                            SendCommandNum = 1;
                        }
                        else //запрос состояния прибора
                        {
                            AnswerByteBuff[3] = 0x2E;
                            AnswerByteBuff[4] = 0x7F;
                            SendCommandNum = 2;
                        }

                        //Считаем контрольную сумму
                        int crc = 0;
                        for (int i = 0; i <= 4; i++)
                            crc = crc ^ AnswerByteBuff[i];
                        AnswerByteBuff[5] = Convert.ToByte(crc);
                        AnswerByteBuff[6] = 0x3;
                        DevConn._serialPort.Write(AnswerByteBuff, 0, 7);
                        SendLettersCounter++;
                        Thread.Sleep(SleepTime);
                        BytesToRead = DevConn._serialPort.BytesToRead;
                        if ((BytesToRead > 0)&& (BytesToRead > 30))
                        {
                            if (BytesToRead > 100)
                            {
                                DevConn.ErrManyBytes++;
                                DevConn._serialPort.DiscardInBuffer();
                            }
                            else
                            {
                                Array.Clear(AnswerByteBuff, 0, AnswerByteBuff.Length);
                                DevConn._serialPort.Read(AnswerByteBuff, 0, BytesToRead);

                                if (AnswerByteBuff[BytesToRead - 3] == 16) //проверяем контрольную сумму на предмет зарезервированного символа
                                {
                                    AnswerByteBuff[BytesToRead - 3] = Convert.ToByte(255 - AnswerByteBuff[BytesToRead - 2]);
                                    AnswerByteBuff[BytesToRead - 2] = AnswerByteBuff[BytesToRead - 1];
                                    BytesToRead--;
                                }
                                //проверяем контрольную сумму
                                int counter = 0;
                                crc = 0;

                                do
                                {
                                    if (AnswerByteBuff[counter] != 16)
                                    {
                                        crc = crc ^ AnswerByteBuff[counter];
                                    }
                                    else
                                    {
                                        counter++;
                                        crc = crc ^ (255 - AnswerByteBuff[counter]);
                                    }
                                    counter++;
                                } while (counter < (BytesToRead - 2));


                                if (AnswerByteBuff[BytesToRead - 2] == crc)
                                {
                                    //Убираем все резервированные символы
                                    int posSource = 0;
                                    int posDest = 0;
                                    do
                                    {
                                        if (AnswerByteBuff[posSource] != 16)
                                        {
                                            AnswerByteBuff[posDest] = AnswerByteBuff[posSource];
                                            posDest++;
                                            posSource++;
                                        }
                                        else
                                        {
                                            AnswerByteBuff[posDest] = Convert.ToByte(255 - AnswerByteBuff[posSource + 1]);
                                            posDest++;
                                            posSource += 2;
                                        }
                                    } while (posSource <= BytesToRead);


                                    if (SendCommandNum == 2) //если посылали запрос на состояние
                                    {
                                        if ((posDest == 36) || (posDest == 35))
                                        {
                                            try
                                            {
                                                //состояние веса, считываем флаги
                                                string BinState = Convert.ToString(AnswerByteBuff[18], 2).PadLeft(8, '0');
                                                fNoLoadCell = (BinState[0].Equals('1')) ? true : false;
                                                fOverLoad = (BinState[1].Equals('1')) ? true : false;
                                                fUnderLoad = (BinState[2].Equals('1')) ? true : false;
                                                fMin20d = (BinState[3].Equals('1')) ? true : false;
                                                fNearZero = (BinState[4].Equals('1')) ? true : false;
                                                IsStable = (BinState[5].Equals('1')) ? true : false;
                                                fZeroClb = (BinState[6].Equals('1')) ? true : false;
                                                fClb = (BinState[7].Equals('1')) ? true : false;
                                                //АЦП
                                                string ACPHex = Convert.ToString(AnswerByteBuff[6], 16).PadLeft(2, '0');
                                                ACPHex += Convert.ToString(AnswerByteBuff[7], 16).PadLeft(2, '0');
                                                ACPHex += Convert.ToString(AnswerByteBuff[8], 16).PadLeft(2, '0');
                                                ACPHex += Convert.ToString(AnswerByteBuff[9], 16).PadLeft(2, '0');
                                                ACPResult = int.Parse(ACPHex, System.Globalization.NumberStyles.HexNumber);
                                                //определение позиции точки
                                                switch (AnswerByteBuff[23])
                                                {
                                                    case 3: PointPosition = 3; break;
                                                    case 4: PointPosition = 2; break;
                                                    case 5: PointPosition = 1; break;
                                                    case 6: PointPosition = 0; break;
                                                    default: PointPosition = 0; break;
                                                }
                                                //текущее брутто
                                                double dTemp = (AnswerByteBuff[10] << 8) + AnswerByteBuff[11];
                                                if (AnswerByteBuff[10] > 127) dTemp -= 65536;
                                                dTemp = dTemp / (Math.Exp(PointPosition * Math.Log(10)));
                                                Brutto = (float)dTemp;
                                                //текущее нетто
                                                dTemp = (AnswerByteBuff[12] << 8) + AnswerByteBuff[13];
                                                if (AnswerByteBuff[12] > 127) dTemp -= 65536;
                                                dTemp = dTemp / (Math.Exp(PointPosition * Math.Log(10)));
                                                Netto = (float)dTemp;
                                                //текущее тара
                                                dTemp = (AnswerByteBuff[14] << 8) + AnswerByteBuff[15];
                                                if (AnswerByteBuff[14] > 127) dTemp -= 65536;
                                                dTemp = dTemp / (Math.Exp(PointPosition * Math.Log(10)));
                                                Tara = (float)dTemp;
                                                //текущий ноль
                                                dTemp = (AnswerByteBuff[16] << 8) + AnswerByteBuff[17];
                                                if (AnswerByteBuff[16] > 127) dTemp -= 65536;
                                                dTemp = dTemp / (Math.Exp(PointPosition * Math.Log(10)));
                                                Zero = (float)dTemp;

                                                ScaleResult = Netto;

                                                StateMess = "Ok";
                                                NoAnswerCounter = 0;
                                                RecieveLettersCounter++;
                                            }
                                            catch (Exception)
                                            {
                                                ErrConvertCounter++;
                                            }
                                        }
                                        else if (posDest == 9) //прибор занят и не может ответить
                                        {
                                            RecieveLettersCounter++;
                                        }
                                        else
                                            ErrAnswerLenghtCounter++;
                                    }
                                    else if (SendCommandNum == 1) //если посылали запрос на обнуление
                                    {

                                    }

                                }
                                else
                                    ErrCRC++;
                            }

                        }
                        if (NoAnswerCounter < NoAnswerLimit)
                            NoAnswerCounter++;
                        if (NoAnswerCounter == NoAnswerLimit)//нет ответа определенное время
                        {
                            NoAnswerCounter++;
                            ScaleResult = -999;
                            Tara = -999;
                            Brutto = -999;
                            Netto = -999;
                            ACPResult = -999;
                            Zero = -999;
                            IsStable = false;
                            StateMess = "Нет связи с контроллером Микросим М06";
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
                    }


                }
                #endregion

                #region Режим постоянной передачи
                else      //постоянная передача с прибора
                {
                    try
                    {
                        int BytesToRead = DevConn._serialPort.BytesToRead;
                        if (BytesToRead > 0)
                        {
                            if (BytesToRead > 100)
                            {
                                DevConn.ErrManyBytes++;
                                DevConn._serialPort.DiscardInBuffer();
                            }
                            else
                            {
                                DevConn._serialPort.Read(AnswerByteBuff, storedBytes, BytesToRead);
                                StringBuilder sb = new StringBuilder();
                                for (int i = 0; i < BytesToRead + storedBytes; i++)
                                {
                                    sb.Append(Convert.ToChar(AnswerByteBuff[i]));
                                    if (AnswerByteBuff[i] == 10)
                                    {
                                        if (sb.Length == 15)
                                        {
                                            if (uiSep == ",") sb.Replace(".", ",");//заменяем точки если системный разделитель запятая
                                            if (float.TryParse(sb.ToString(3, 6), out float fResult))
                                            {
                                                ScaleResult = fResult;
                                                if (sb[2] == Convert.ToChar("-")) ScaleResult = ScaleResult * (-1);//если число отрицательное
                                                IsStable = (sb[9] == Convert.ToChar("?")) ? false : true;
                                                if (ZeroRange == 0)//ноль весов
                                                    IsZero = (ScaleResult != 0) ? false : true;
                                                else
                                                    IsZero = (Math.Abs(ScaleResult) <= ZeroRange) ? false : true;
                                                StateMess = "Ok";
                                                NoAnswerCounter = 0;
                                                RecieveLettersCounter++;
                                            }
                                            else
                                            {
                                                ErrConvertCounter++;
                                                ScaleResult = -999;
                                                StateMess = sb.ToString(2, 6);
                                            }
                                        }
                                        else
                                            ErrAnswerLenghtCounter++;
                                        sb.Clear();
                                    }
                                }
                                if (sb.Length != 0)
                                {
                                    //storedBytes = sb.Length;
                                    Array.Clear(AnswerByteBuff, 0, AnswerByteBuff.Length);
                                    //for (int i = 0; i < sb.Length; i++)
                                    //{
                                    //    AnswerByteBuff[i] = Convert.ToByte(sb[i]);
                                    //}
                                }
                            }
                        }
                        if (NoAnswerCounter < NoAnswerLimit)
                            NoAnswerCounter++;
                        if (NoAnswerCounter == NoAnswerLimit)//нет ответа определенное время
                        {
                            NoAnswerCounter++;
                            ScaleResult = -999;
                            IsStable = false;
                            StateMess = "Нет связи с контроллером Микросим М06";
                            LostConnectionCounter++;
                        }
                        Thread.Sleep(SleepTime);
                    }
                    catch (Exception e)
                    {
                        ErrGetStateDeviceMess = e.Message;
                        ErrGetStateDevice++;
                    }
                }
                #endregion
                return SleepTime;
            }
            else  //если порт не открылся
            {
                StateMess = "Ошибка открытия порта";
                ScaleResult = -999;
                return 0;
            }
        }



    }
}
