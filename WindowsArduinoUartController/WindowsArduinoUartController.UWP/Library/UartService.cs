using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.SerialCommunication;
using Windows.Storage.Streams;
using WindowsArduinoUartController.Interfaces;
using WindowsArduinoUartController.UWP.Library;

[assembly: Xamarin.Forms.Dependency(typeof(UartService))]
namespace WindowsArduinoUartController.UWP.Library
{
    public class UartService : IUartService
    {
        string aqs;
        //DeviceInformationCollection dic;
        SerialDevice SerialPort;
        public async void Serial(string PortName, uint BaudRate)    // UART1, 115200
        {

            string AqsFilter = SerialDevice.GetDeviceSelector();
            var dis = await DeviceInformation.FindAllAsync(AqsFilter);
            int Count = 0;
            bool foundFTDI = false;
            for (Count = 0; Count < dis.Count; Count++)
            {
                if (dis[Count].Id.Contains("FTDI"))         //We are looking for a FDTI USB to serial USB device
                {
                    foundFTDI = true;
                    break;
                }

            }

            //aqs = SerialDevice.GetDeviceSelector();                   /* Find the selector string for the serial device   */
            //aqs = SerialDevice.GetDeviceSelector("FT231X USB UART");                   /* Find the selector string for the serial device   */
            //aqs = SerialDevice.GetDeviceSelector(PortName);                   /* Find the selector string for the serial device   */
            //dic = await DeviceInformation.FindAllAsync(aqs);                    /* Find the serial device with our selector string  */
            if (!foundFTDI)// (dis.Count == 0)
                return;// throw new Exception("Nothing Found");
            //var dis = await DeviceInformation.FindAllAsync();                    /* Find the serial device with our selector string  */
            SerialPort = await SerialDevice.FromIdAsync(dis[Count].Id);    /* Create an serial device with our selected device */

            /* Configure serial settings */
            SerialPort.WriteTimeout = TimeSpan.FromMilliseconds(1000);
            SerialPort.ReadTimeout = TimeSpan.FromMilliseconds(1000);

            //SerialPort.BaudRate = 9600;
            //SerialPort.BaudRate = 115200;
            SerialPort.BaudRate = BaudRate;

            SerialPort.Parity = SerialParity.None;
            SerialPort.StopBits = SerialStopBitCount.One;
            SerialPort.DataBits = 8;

            /* Write a string out over serial */
            //string txBuffer = "Hello Serial";
            //DataWriter dataWriter = new DataWriter();
            //dataWriter.WriteString(txBuffer);
            //uint bytesWritten = await SerialPort.OutputStream.WriteAsync(dataWriter.DetachBuffer());
            //await LoadThinkifyTR65();


            /* Read data in from the serial port */
            //const uint maxReadLength = 1024;
            //DataReader dataReader = new DataReader(SerialPort.InputStream);
            //uint bytesToRead = await dataReader.LoadAsync(maxReadLength);
            //string rxBuffer = dataReader.ReadString(bytesToRead);
        }


        public async Task<List<Tuple<string, uint>>> SendStringToConnectedUart(List<string> sendList)
        {
            string currentString = "";
            try
            {
                List<Tuple<string, uint>> returnList = new List<Tuple<string, uint>>();
                using (DataWriter dataWriter = new DataWriter())
                {
                    foreach (var sendStringValue in sendList)
                    {
                        currentString = sendStringValue;
                        dataWriter.WriteString(sendStringValue);
                        var bytesWritten = await SerialPort.OutputStream.WriteAsync(dataWriter.DetachBuffer());
                        returnList.Add(new Tuple<string, uint>(sendStringValue, bytesWritten));
                        currentString = "End";
                    }
                }
                return returnList;
            }
            catch (Exception ex)
            {
                throw new Exception($@"Error: Current String: {currentString}", ex);
                //return $@"Error: {ex.Message}";
            }
        }

        DataReader dataReader = null;
        public async Task<string> ReadSerialAsync()
        {
            string returnString = "";
            try
            {
                const uint maxReadLength = 1024;

                if (dataReader == null)
                    dataReader = new DataReader(SerialPort.InputStream);

                while (returnString != "BOB")
                {
                    uint bytesToRead = await dataReader.LoadAsync(maxReadLength);
                    var stringData = dataReader.ReadString(bytesToRead);
                    System.Diagnostics.Debug.WriteLine(stringData);
                    returnString = stringData;
                }
                return returnString;
            }
            catch (Exception ex)
            {
                throw new Exception($@"Error: Current String: {returnString}", ex);
                //return $@"Error: {ex.Message}";
            }
        }

        public async Task<string> ReadSerialOldAsync()
        {
            string returnString = "";
            try
            {
                const uint maxReadLength = 1024;
                using (DataReader dataReader = new DataReader(SerialPort.InputStream))
                {
                    bool foundBytesToRead = true;
                    while (foundBytesToRead)
                    {
                        uint bytesToRead;
                        try
                        {
                            bytesToRead = await dataReader.LoadAsync(maxReadLength);
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                        var stringData = dataReader.ReadString(bytesToRead);
                        if (string.IsNullOrEmpty(stringData))
                            foundBytesToRead = false;
                        else
                        {
                            foundBytesToRead = false;
                            returnString += stringData;
                        }

                    }

                }
                return returnString;
            }
            catch (Exception ex)
            {
                throw new Exception($@"Error: Current String: {returnString}", ex);
                //return $@"Error: {ex.Message}";
            }
        }


        //Copied from the below address to connect db 410c using uart1
        //https://docs.microsoft.com/en-us/windows/iot-core/learn-about-hardware/pinmappings/pinmappingsdb
        public async void Serial()
        {
            string aqs = SerialDevice.GetDeviceSelector("UART1");                   /* Find the selector string for the serial device   */
            var dis = await DeviceInformation.FindAllAsync(aqs);                    /* Find the serial device with our selector string  */
            SerialDevice SerialPort = await SerialDevice.FromIdAsync(dis[0].Id);    /* Create an serial device with our selected device */

            /* Configure serial settings */
            SerialPort.WriteTimeout = TimeSpan.FromMilliseconds(1000);
            SerialPort.ReadTimeout = TimeSpan.FromMilliseconds(1000);
            SerialPort.BaudRate = 9600;
            SerialPort.Parity = SerialParity.None;
            SerialPort.StopBits = SerialStopBitCount.One;
            SerialPort.DataBits = 8;

            /* Write a string out over serial */
            string txBuffer = "Hello Serial";
            DataWriter dataWriter = new DataWriter();
            dataWriter.WriteString(txBuffer);
            uint bytesWritten = await SerialPort.OutputStream.WriteAsync(dataWriter.DetachBuffer());

            /* Read data in from the serial port */
            const uint maxReadLength = 1024;
            DataReader dataReader = new DataReader(SerialPort.InputStream);
            uint bytesToRead = await dataReader.LoadAsync(maxReadLength);
            string rxBuffer = dataReader.ReadString(bytesToRead);
        }
    }
}
