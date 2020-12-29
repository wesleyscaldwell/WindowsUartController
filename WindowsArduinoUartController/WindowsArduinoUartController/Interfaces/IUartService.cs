using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WindowsArduinoUartController.Interfaces
{
    public interface IUartService
    {
        void Serial(string PortName, uint BaudRate);
        Task<List<Tuple<string, uint>>> SendStringToConnectedUart(List<string> sendList);
        Task<string> ReadSerialAsync();
    }
}
