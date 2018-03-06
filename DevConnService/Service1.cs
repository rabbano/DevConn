using Connection;
using Microsoft.Owin.Hosting;
using System;
using System.Collections.Generic;
using System.ServiceProcess;
using System.Xml;
using WindowsService;

namespace DevConnService
{
    public partial class DevConnService : ServiceBase
    {
        
        public static string baseUrl = String.Empty;

        public static XmlDocument sysit_xml = new XmlDocument();

        static void Main(string[] args)
        {
            #region Считывание параметров из XML
            try
            {
                sysit_xml.Load(AppDomain.CurrentDomain.BaseDirectory + "\\DevConn.xml");
                XmlNode WebOptionsXML = sysit_xml.DocumentElement.SelectSingleNode("ServiceOptions");
                //параметры Web Сервиса
                baseUrl = WebOptionsXML.SelectSingleNode("BaseUrl").InnerText;
                Common.httpUser = WebOptionsXML.SelectSingleNode("ApiUserName").InnerText;
                Common.httpPassword = WebOptionsXML.SelectSingleNode("ApiUserPass").InnerText;
                //параметры соединения
                XmlNodeList ConnectionList = sysit_xml.DocumentElement.SelectNodes("Connection");
                if (ConnectionList.Count > 0)
                {
                    foreach (XmlNode c in ConnectionList)
                    {
                        string p1 = c.Attributes["Type"].Value;
                        if (p1 == "Com")
                        {
                            string p2 = c.Attributes["Name"].Value;
                            int p3 = Convert.ToInt32(c.Attributes["PortNum"].Value);
                            int p4 = Convert.ToInt32(c.Attributes["BaudRate"].Value);
                            int p5 = Convert.ToInt32(c.Attributes["Parity"].Value);
                            int p6 = Convert.ToInt32(c.Attributes["ByteSize"].Value);
                            int p7 = Convert.ToInt32(c.Attributes["StopBit"].Value);
                            int p8 = Convert.ToInt32(c.Attributes["DelayReopenPort"].Value);
                            Common.ConnList.Add(new DevConnection(p2, p3, p4, p5, p6, p7, p8));
                        }
                        XmlNodeList DeviceList = c.SelectNodes("Device");
                        if (DeviceList.Count > 0)
                        {
                            foreach (XmlNode d in DeviceList)
                            {
                                String DevType = d.InnerText;
                                if (DevType == "MicrosimM06")
                                {
                                    string p8 = d.Attributes["Name"].Value;
                                    bool p9 = (d.Attributes["ExchangeMode"].Value == "1") ? true : false;
                                    int p10 = Convert.ToInt32(d.Attributes["RS485"].Value);
                                    int p11 = Convert.ToInt32(d.Attributes["NoAnswerLimit"].Value);
                                    int p12 = Convert.ToInt32(d.Attributes["DelayTime"].Value);
                                    Common.ConnList[Common.ConnList.Count - 1].AddМ06(p8, p9, p10, p11, p12);
                                }
                            }

                        }
                    }
                }
            }
            catch (Exception e)
            {
                baseUrl = "http://*:8000";
                Console.WriteLine(e.Message);
            }
            #endregion



            if (Environment.UserInteractive)
            {
                DevConnService service = new DevConnService();
                service.OnStart(args);
                Console.WriteLine("Server running at {0} - press Enter to quit. ", baseUrl);
                Console.ReadLine();
                service.OnStop();
            }
            else
            {
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[]
                {
                    new DevConnService()
                };
                ServiceBase.Run(ServicesToRun);
            }
        }

        public DevConnService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            WebApp.Start<Startup>(baseUrl);
            //try
            //{
            //    System.IO.File.Create(AppDomain.CurrentDomain.BaseDirectory + "OnStart.txt");
            //}
            //catch { }
        }

        protected override void OnStop()
        {
            foreach (DevConnection dc in Common.ConnList)
            {
                dc.CloseConnection();
            }
            //try
            //{
            //    System.IO.File.Create(AppDomain.CurrentDomain.BaseDirectory + "OnStop.txt");
            //}
            //catch { }
        }
    }
}
