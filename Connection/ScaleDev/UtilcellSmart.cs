using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Connection.ScaleDev
{
    public class UtilcellSmart : ScaleController
    {
        public int RecieveLettersCounter { get; private set; } = 0; //полученных хороших ответов
        public int SendLettersCounter { get; private set; } = 0;  //счетчик запросов
        public int GetStateCounter { get; private set; } = 0;//количество запросов состояния
        public int ErrAnswerLenghtCounter { get; private set; } = 0; //полученных ответов не той длинны
        public int ErrConvertCounter { get; private set; } = 0; //ошибок преобразования в число из строки
        public int ErrGetStateDevice { get; private set; } = 0; //ошибка произошла в процессе получения данных
        public float ScaleResult { get; private set; } = 0; //Полученный вес в КГ
        public bool IsStable { get; private set; } = false; //стабильность веса
        public bool IsOverload { get; private set; } = false; //перегрузка весов
        public bool IsInvalid { get; private set; } = false; //ошибка в весе
        public string StateMess { get; private set; } = String.Empty; //сообщение о состоянии прибора
        public int LostConnectionCounter { get; private set; }//счетчик потери связи с прибором
        private int NoAnswerCounter = 0;
        public int RS485Num { get; set; } = 0; //номер прибора в сети

        byte[] AnswerByteBuff = new byte[200];

        int storedBytes = 0;

        internal int GetSmartState(DevConnection DevConn)
        {
            int FuncSleepTime = 0;
            GetStateCounter++;
            if (isActivExchangeMode)//по запросу
            {

            }
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
                                    if (sb.Length == 14)
                                    {
                                        if (uiSep == ",") sb.Replace(".", ",");//заменяем точки если системный разделитель запятая

                                        if (float.TryParse(sb.ToString(2, 7), out float fResult))
                                        {
                                            ScaleResult = fResult;
                                            if (sb[1] == Convert.ToChar("-")) ScaleResult = ScaleResult * (-1);//если число отрицательное
                                            if (sb[11] == Convert.ToChar("O"))
                                            {
                                                IsOverload = true;
                                                IsStable = IsInvalid = false;
                                            }
                                            else if (sb[11] == Convert.ToChar("I"))
                                            {
                                                IsInvalid = true;
                                                IsStable = IsOverload = false;
                                            }
                                            else if (sb[11] == Convert.ToChar(" "))
                                            {
                                                IsStable = true;
                                                IsInvalid = IsOverload = false;
                                            }
                                            else if (sb[11] == Convert.ToChar("M"))
                                            {
                                                IsStable = IsInvalid = IsOverload = false;
                                            }
                                            RecieveLettersCounter++;
                                            if (StateMess != "Ok")
                                            {
                                                StateMess = "Ok";
                                            }
                                            NoAnswerCounter = 0;
                                        }
                                        else
                                        {
                                            ErrConvertCounter++;
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
                        IsStable = IsInvalid = IsOverload = false;
                        StateMess = "Нет связи с контроллером SMART";
                        LostConnectionCounter++;
                    }

                    Thread.Sleep(SleepTime);
                    FuncSleepTime += SleepTime;
                }
                catch (Exception)
                {
                    ErrGetStateDevice++;
                }
            }
            return FuncSleepTime;
        }
    }
}
