using System;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.Devices.Enumeration;
using Windows.Devices.Gpio;
using Windows.Devices.I2c;
using Windows.Devices.SerialCommunication;
using Windows.Devices.Spi;
using Windows.Devices.Pwm;
using WindowsArduinoUartController.Interfaces;
using WindowsArduinoUartController.UWP.Library;

[assembly: Xamarin.Forms.Dependency(typeof(I2CService))]
namespace WindowsArduinoUartController.UWP.Library
{

    //https://docs.microsoft.com/en-us/windows/iot-core/learn-about-hardware/pinmappings/pinmappingsdb
    public class I2CService : II2CService
    {

        public void GPIO()
        {
            GpioController Controller = GpioController.GetDefault(); /* Get the default GPIO controller on the system */
            GpioPin Pin = Controller.OpenPin(35);       /* Open GPIO 35                      */
            Pin.SetDriveMode(GpioPinDriveMode.Output);  /* Set the IO direction as output   */
            Pin.Write(GpioPinValue.High);               /* Output a digital '1'             */
        }


        public async void I2C()
        {
            // 0x40 is the I2C device address
            var settings = new I2cConnectionSettings(0x40);
            // FastMode = 400KHz
            settings.BusSpeed = I2cBusSpeed.FastMode;

            // Get a selector string that will return our wanted I2C controller
            string aqs = I2cDevice.GetDeviceSelector("I2C0");

            // Find the I2C bus controller devices with our selector string
            var dis = await DeviceInformation.FindAllAsync(aqs);

            // Create an I2cDevice with our selected bus controller and I2C settings 
            using (I2cDevice device = await I2cDevice.FromIdAsync(dis[0].Id, settings))
            {
                byte[] writeBuf = { 0x01, 0x02, 0x03, 0x04 };
                device.Write(writeBuf);
            }
        }

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

        public async void SPI()
        {
            // Use chip select line CS0
            var settings = new SpiConnectionSettings(0);

            // Create an SpiDevice with the specified Spi settings
            var controller = await SpiController.GetDefaultAsync();

            using (SpiDevice device = controller.GetDevice(settings))
            {
                byte[] writeBuf = { 0x01, 0x02, 0x03, 0x04 };
                device.Write(writeBuf);
            }
        }
    }
}
