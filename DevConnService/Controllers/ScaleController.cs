
using ApiClass.DevConnService;
using System;
using System.Collections.Generic;
using System.Web.Http;
using WindowsService.ApiClass;

namespace WindowsService.Controllers
{
    [BasicAuthentication]
    public class ScaleController : ApiController
    {
        [HttpGet]
        [Route("api/{controller}/{Connection}/{DevType}")]
        public IEnumerable<dynamic> Get(int Connection, string DevType)
        {
            string ErrMess = string.Empty;
            ErrMess = (Connection >= Common.ConnList.Count) ? "cоединения не существует" : string.Empty;
            ErrMess = ((ErrMess == string.Empty)&&(Common.ConnList[Connection].M06List == null)) ? "прибора в соединении нет" : ErrMess;
            if (ErrMess == string.Empty)
            {
                switch (DevType)
                {
                    case "MicrosimM06": List<ApiMicrosim> ListM06 = ApiMicrosim.GetDevState(Connection); return ListM06;
                    default:
                        return new string[] { "Ошибка: " + DevType + " не найден" };
                }
            }
            else
                return new string[] { "Ошибка: "+ErrMess };
        }

        [HttpGet]
        [Route("api/{controller}/{Connection}")]
        public dynamic Get(int Connection)
        {
            string ErrMess = string.Empty;
            ErrMess = (Connection >= Common.ConnList.Count) ? "cоединения не существует" : string.Empty;
            if (ErrMess == string.Empty)
            {
                ApiConnection ac = ApiConnection.GetConnState(Connection);
                return ac;
            }
            else
                return new string[] { "Ошибка: " + ErrMess };
        }

        [HttpPost]
        public string Put(string DevType, int Connection, int DevNum)
        {
            if (DevType == "MicrosimM06")//получение состояния по прибору Микросим М06
            {
                if ((Connection < Common.ConnList.Count) && (DevNum < Common.ConnList[Connection].M06List.Count))
                {
                    if (Common.ConnList[Connection].M06List[DevNum].StateMess=="Ok")
                    {
                        Common.ConnList[Connection].M06List[DevNum].isNeedZero = true;
                        return "Ok";
                    }
                    else
                    {
                        return "Проверьте связь с прибором";
                    }
                }
                else
                    return "В соединении " + Connection + " Микросим М06 не найден";
            }
            else
                return DevType + " не найден";
        }

    }
}
