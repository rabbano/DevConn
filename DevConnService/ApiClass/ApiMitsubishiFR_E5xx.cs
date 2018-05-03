using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsService.ApiClass
{
    public class ApiMitsubishiFR_E5xx
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

        public static List<ApiMitsubishiFR_E5xx> GetState(int Connection)
        {
            var DevList = new List<ApiMitsubishiFR_E5xx>();
            foreach (var M in Common.ConnList[Connection].MitsubishiFR_E5xxList)
            {
                ApiMitsubishiFR_E5xx sr = new ApiMitsubishiFR_E5xx();

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
