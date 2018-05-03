using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
//Прибор МИ МДА/15Я Россия
//Для входа в параметры нажимает одновременно кнопки Ввод+Итг, далее Вводом листаем до параметра Con
//Ставим цифрами параметр 01 (режим передачи + 1200) и запоминаем Ввод, первая цифра 0, значит постоянную передачу, 1 по запросу, 2 Принтер.
//С моксой работает только до скорости 4800, больше ставить нельзя
namespace Connection.ScaleDev
{
    public class WeightResult
    {
        public float Result { get; set; }
        public DateTime ResultTime { get; set; }
        public WeightResult(float Result, DateTime ResultTime)
        {
            this.Result = Result;
            this.ResultTime = ResultTime;
        }
    }
    public class MI_MDA_15YA : ScaleController
    {
        public int GetStateCounter { get; private set; } = 0;//количество запросов состояния
        public int ErrGetStateDevice { get; private set; } = 0; //ошибка произошла в процессе получения данных
        public string ErrGetStateDeviceMess { get; private set; } = string.Empty;//текст ошибки
        public string StateMess { get; private set; } = String.Empty; //сообщение о состоянии прибора
        public int RecieveLettersCounter { get; private set; } = 0; //полученных хороших ответов
        public int SendLettersCounter { get; private set; } = 0;  //счетчик запросов
        int NoAnswerCounter = 0; //счетчик неполученных ответов
        public float ScaleResult { get; private set; } = 0; //Полученный вес в КГ
        public bool IsStable { get; private set; } = false; //стабильность веса
        public bool IsZero { get; private set; } = false; //ноль весов
        public float ZeroRange { get; set; } = 0; //Если есть диапазон нуля внутри которого вес считается равным нулю
        public int LostConnectionCounter { get; private set; }//счетчик потери связи с прибором
        public int ErrAnswerLenghtCounter { get; private set; } = 0; //полученных ответов не той длинны
        public int ErrConvertCounter { get; private set; } = 0; //ошибок преобразования в число из строки
        public float StableWeightRange { get; private set; } = 0.5F; //Вес изменения для стабильности
        public int StableTimeRange { get; private set; } = 1000; //Время изменения для стабильности
        public List<WeightResult> ResultList = new List<WeightResult>();//Значения веса для вычисления стабильности

        byte[] ByteBuff = new byte[200];
        int storedBytes = 0;

        internal int GetState(DevConnection DevConn)
        {            
            GetStateCounter++;
            if (DevConn.PortState == 3)
            {
                #region Режим обмена по запросу
                if (isActivExchangeMode)//по запросу
                {
                    try
                    {
                        Thread.Sleep(SleepTime);
                    }
                    catch (Exception e)
                    {
                        ErrGetStateDeviceMess = e.Message;
                        ErrGetStateDevice++;
                    }
                }
                #endregion
                else //постоянная передача с прибора
                {
                    try
                    {
                        int BytesToRead = DevConn._serialPort.BytesToRead;
                        if (BytesToRead > 0)
                        {
                            if (BytesToRead > 0)
                            {
                                if (BytesToRead > 100)
                                {
                                    DevConn.ErrManyBytes++;
                                    DevConn._serialPort.DiscardInBuffer();
                                }
                                else
                                {
                                    Array.Clear(ByteBuff, 0, ByteBuff.Length);
                                    DevConn._serialPort.Read(ByteBuff, storedBytes, BytesToRead);
                                    if ((BytesToRead == 9)&&(ByteBuff[0]==61))
                                    {
                                        StringBuilder sb = new StringBuilder();
                                        Array.Reverse(ByteBuff, 0, 9);
                                        for (int i = 0; i < 8; i++)
                                            sb.Append(Convert.ToChar(ByteBuff[i]));
                                        if (uiSep == ",") sb.Replace(".", ",");//заменяем точки если системный разделитель запятая
                                        if (float.TryParse(sb.ToString(), out float fResult))
                                        {
                                            ScaleResult = fResult;
                                            StateMess = "Ok";
                                            NoAnswerCounter = 0;
                                            RecieveLettersCounter++;
                                            IsStable = true;
                                            if (ZeroRange == 0)//ноль весов
                                                IsZero = (ScaleResult != 0) ? false : true;
                                            else
                                                IsZero = (Math.Abs(ScaleResult) <= ZeroRange) ? false : true;
                                            ResultList.Add(new WeightResult(5, DateTime.Now));
                                            if (ResultList.Count > 20)
                                                ResultList.RemoveAt(0);
                                        }
                                        else
                                            ErrConvertCounter++;
                                    }
                                    else
                                        ErrAnswerLenghtCounter++;
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
                                StateMess = "Нет связи с терминалом МИ МДА/15Я";
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
