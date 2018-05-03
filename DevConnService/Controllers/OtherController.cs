using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using WindowsService.ApiClass;

namespace WindowsService.Controllers
{
    [BasicAuthentication]
    public class OtherController : ApiController
    {
        [HttpGet]
        [Route("api/{controller}/{Connection}/{DevType}")]
        public IEnumerable<dynamic> Get(int Connection, string DevType)
        {
            string ErrMess = string.Empty;
            ErrMess = (Connection >= Common.ConnList.Count) ? "cоединения не существует" : string.Empty;
            ErrMess = ((ErrMess == string.Empty) && (DevType== "Mitsubishi_E5xx") && (Common.ConnList[Connection].MitsubishiFR_E5xxList == null)) ? "прибора в соединении нет" : ErrMess;
            ErrMess = ((ErrMess == string.Empty) && (DevType == "Owen110_224_1T") && (Common.ConnList[Connection].Owen110_224_1TList == null)) ? "прибора в соединении нет" : ErrMess;
            ErrMess = ((ErrMess == string.Empty) && (DevType == "ESQ_A1000") && (Common.ConnList[Connection].ESQ_A1000List == null)) ? "прибора в соединении нет" : ErrMess;
            if (ErrMess == string.Empty)
            {
                switch (DevType)
                {
                    case "Mitsubishi_E5xx": List<ApiMitsubishiFR_E5xx> ListM = ApiMitsubishiFR_E5xx.GetState(Connection); return ListM;
                    case "Owen110_224_1T": List<ApiOwen110_224_1T> RetunList = ApiOwen110_224_1T.GetState(Connection); return RetunList;
                    case "ESQ_A1000": List<ApiESQ_A1000> ESQ_A1000L = ApiESQ_A1000.GetState(Connection); return ESQ_A1000L;
                    default:
                        return new string[] { "Ошибка: " + DevType + " не найден" };
                }
            }
            else
                return new string[] { "Ошибка: " + ErrMess };
        }

        public class RequestMitsubishi_E5xx
        {
            public float Hz { get; set; }
            public int isRun { get; set; }
        }

        [HttpPost]
        public string Post(string DevType, int Connection, int DevNum, [FromBody] RequestMitsubishi_E5xx Request)
        {
            if (DevType == "MicrosimM06")//получение состояния по прибору Микросим М06
            {
                if ((Connection < Common.ConnList.Count) && (DevNum < Common.ConnList[Connection].M06List.Count))
                {
                    if (Common.ConnList[Connection].M06List[DevNum].StateMess == "Ok")
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
            else if (DevType == "Mitsubishi_E5xx")
            {
                if ((Connection < Common.ConnList.Count) && (DevNum < Common.ConnList[Connection].MitsubishiFR_E5xxList.Count))
                {
                    if (Common.ConnList[Connection].MitsubishiFR_E5xxList[DevNum].StateMess == "Ok")
                    {
                        if (Request.isRun > 0)
                        {
                            Common.ConnList[Connection].MitsubishiFR_E5xxList[DevNum].isNeedRun = (Request.isRun == 1) ? true : false;
                            Common.ConnList[Connection].MitsubishiFR_E5xxList[DevNum].PresetHz = Request.Hz;
                        } 
                        return "Ok";
                    }
                    else
                    {
                        return "Нет связи с прибором";
                    }
                }
                else
                    return "В соединении " + Connection + " Микросим М06 не найден";
            }
            else if (DevType == "ESQ_A1000")
            {
                if ((Connection < Common.ConnList.Count) && (DevNum < Common.ConnList[Connection].ESQ_A1000List.Count))
                {
                    if (Common.ConnList[Connection].ESQ_A1000List[DevNum].StateMess == "Ok")
                    {
                        if (Request.isRun > 0)
                        {
                            Common.ConnList[Connection].ESQ_A1000List[DevNum].isNeedRun = (Request.isRun == 1) ? true : false;
                            Common.ConnList[Connection].ESQ_A1000List[DevNum].PresetHz = Request.Hz;
                        }
                        return "Ok";
                    }
                    else
                    {
                        return "Нет связи с прибором";
                    }
                }
                else
                    return "В соединении " + Connection + " Микросим М06 не найден";
            }
            else
                return DevType + " не найден";
        }


        //[HttpPost]
        //public string Post([FromBody] RequestMitsubishi_E5xx content)
        //{
        //    //string Input = JsonConvert.SerializeObject(content);
        //    //if (content.Hz != null)
        //    //{

        //    //}
        //    return content.Hz;
        //}
    }
}
