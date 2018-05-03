using Connection.OtherDev;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace WindowsService.ApiClass
{
    public class ApiESQ_A1000
    {
        public string Name;
        public float CurrentHz;
        public float PresetHz;
        public float CurrentA;
        public float CurrentV;
        public int isRun;

        public int RecieveLettersCounter;
        public int SendLettersCounter;
        public string StateMess;

        public static List<ApiESQ_A1000> GetState(int Connection)
        {
            var DevList = new List<ApiESQ_A1000>();
            foreach (var M in Common.ConnList[Connection].ESQ_A1000List)
            {
                ApiESQ_A1000 sr = new ApiESQ_A1000();

                sr.Name = M.Name;
                sr.CurrentHz = M.CurrentHz;
                sr.PresetHz = M.PresetHz;
                sr.CurrentA = M.CurrentA;
                sr.CurrentV = M.CurrentV;
                sr.isRun = (M.fRun) ? 1 : 2;
                sr.RecieveLettersCounter = M.RecieveLettersCounter;
                sr.SendLettersCounter = M.SendLettersCounter;
                sr.StateMess = M.StateMess;

                DevList.Add(sr);
            }
            return DevList;
        }
    }
}
