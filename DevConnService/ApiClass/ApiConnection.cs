using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsService.ApiClass
{
    public class ApiConnection
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string State { get; set; }
        public string OpenTime { get; set; }
        public int OpenPortCounter { get; set; }

        public static ApiConnection GetConnState(int Connection)
        {
            var c = new ApiConnection();
            c.Name = Common.ConnList[Connection].Name;
            c.Type = (Common.ConnList[Connection].ConnectionType == 1) ? "COM" : "TCP";
            c.State = (Common.ConnList[Connection].PortState == 3) ? "Порт открыт" : "Ошибка открытия порта";
            c.OpenTime = Common.ConnList[Connection].OpenPortTime.ToString();
            c.OpenPortCounter = Common.ConnList[Connection].OpenPortCounter;
            return c;
        }
    }
}
