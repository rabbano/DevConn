using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Connection
{
    public class ScaleController
    {
        public string Name { get; set; }
        public bool Enabled { get; set; }
        public int RS485Num;
        public int SendLettersCounter;
        public int RecieveLettersCounter;
        public int ErrAnswerLenghtCounter;
        public int NoAnswerCounter;
        public int NoAnswerTime = 50;
        public int LostConnectionDataCount=20;

        public int SleepTime = 100;
        public bool isActivExchangeMode; //режим обмена с прибором, по запросу или прослушивать

    }
}
