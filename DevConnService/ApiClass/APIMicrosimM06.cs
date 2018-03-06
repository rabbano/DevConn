using Connection;
using Connection.ScaleDev;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsService;

namespace ApiClass.DevConnService
{
    public class ApiMicrosim
    {
        public string Name;
        public float ScaleResult;
        public bool IsStable;
        public string State;

        public int ACPResult;
        public float Zero;


        public int RecieveLettersCounter;
        public int SendLettersCounter;
        public bool fNoLoadCell;
        public bool fOverLoad;
        public bool fUnderLoad;
        public bool fMin20d;
        public bool fNearZero;
        public bool fClb;
        public bool fZeroClb;

        public int ErrAnswerLenghtCounter;
        public int ErrConvertCounter;
        public int ErrCRC;
        public int ErrGetStateDevice;
        public string ErrGetStateDeviceMess;


        public static List<ApiMicrosim> GetDevState(int Connection)
        {
            var DevList = new List<ApiMicrosim>();
            foreach (var M06 in Common.ConnList[Connection].M06List)
            {
                ApiMicrosim sr = new ApiMicrosim();
                sr.Name = M06.Name;
                sr.ScaleResult =  M06.ScaleResult;
                sr.IsStable = M06.IsStable;
                sr.State = M06.StateMess;

                sr.ACPResult = M06.ACPResult;
                sr.Zero = M06.Zero;

                sr.SendLettersCounter = M06.SendLettersCounter;
                sr.RecieveLettersCounter = M06.RecieveLettersCounter;

                sr.fNoLoadCell = M06.fNoLoadCell;
                sr.fOverLoad = M06.fOverLoad;
                sr.fUnderLoad = M06.fUnderLoad;
                sr.fMin20d = M06.fMin20d;
                sr.fNearZero = M06.fNearZero;
                sr.fClb = M06.fClb;
                sr.fZeroClb = M06.fZeroClb;

                sr.ErrAnswerLenghtCounter = M06.ErrAnswerLenghtCounter;
                sr.ErrConvertCounter = M06.ErrConvertCounter;
                sr.ErrCRC = M06.ErrCRC;
                sr.ErrGetStateDevice = M06.ErrGetStateDevice;
                sr.ErrGetStateDeviceMess = (M06.ErrGetStateDeviceMess==null)?string.Empty: M06.ErrGetStateDeviceMess;

                DevList.Add(sr);
            }
            return DevList;
        }

    }

}


