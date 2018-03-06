using System.Globalization;


namespace Connection
{
    public class ScaleController
    {
        public string Name { get; set; } = string.Empty;//имя прибора
        public int SleepTime { get; set; } = 100; //Время ожидания ответа
        public int NoAnswerLimit { get; set; } = 20; //количестов опросов в который не пришел ответ, считается потеря связи
        public bool isActivExchangeMode { get; set; } = false; //режим обмена с прибором, по запросу или прослушивать
        protected string uiSep = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;


        //public int NoAnswerTime = 50;





    }
}
