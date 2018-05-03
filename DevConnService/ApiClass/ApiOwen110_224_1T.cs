using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsService.ApiClass
{
    public class ApiOwen110_224_1T
    {
        public string Name;
        public float A;
        public float Hz;
        public string State;

        public int RecieveLettersCounter;
        public int SendLettersCounter;

        //public int ErrAnswerLenghtCounter;
        public int ErrConvertCounter;
        public int ErrCRC;
        public int ErrGetStateDevice;
        public string ErrGetStateDeviceMess;

        public static List<ApiOwen110_224_1T> GetState(int Connection)
        {
            var DevList = new List<ApiOwen110_224_1T>();
            foreach (var Owen in Common.ConnList[Connection].Owen110_224_1TList)
            {
                ApiOwen110_224_1T owen = new ApiOwen110_224_1T();
                owen.Name = Owen.Name;
                owen.A = Owen.A;
                owen.Hz = Owen.Hz;
                owen.State = Owen.StateMess;

                owen.SendLettersCounter = Owen.SendLettersCounter;
                owen.RecieveLettersCounter = Owen.RecieveLettersCounter;

                owen.ErrConvertCounter = Owen.ErrConvertCounter;
                owen.ErrCRC = Owen.ErrCRC;
                owen.ErrGetStateDevice = Owen.ErrGetStateDevice;
                owen.ErrGetStateDeviceMess = (Owen.ErrGetStateDeviceMess == null) ? string.Empty : Owen.ErrGetStateDeviceMess;

                DevList.Add(owen);
            }
            return DevList;
        }
    }
}
