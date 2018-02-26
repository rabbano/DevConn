using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Connection.ScaleDev
{
    public class UtilcellSmart : ScaleController
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        internal void GetSmartState(DevConnection DevConn)
        {
            switch (isActivExchangeMode)
            {
                case true: //по запросу

                    break;
                case false: //постоянная передача с прибора
                    int BytesToRead = DevConn._serialPort.BytesToRead;
                    if (BytesToRead > 0)
                    {
                        if (BytesToRead > 100)
                        {
                            //DevSerialPort.ErrManyBytes++;
                            DevConn._serialPort.DiscardInBuffer();
                        }
                        else
                        {
                            int byteRecieved = BytesToRead;
                            byte[] messByte = new byte[byteRecieved];
                            DevConn._serialPort.Read(messByte, 0, byteRecieved);

                        }
                    }
                    break;

            }
        }
    }
}
