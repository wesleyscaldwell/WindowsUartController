using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.I2c;

namespace WindowsArduinoUartController.UWP.Services
{
    public class I2CBase
    {
        public enum I2CSpeed { I2C_100kHz, I2C_400kHz };

        #region Properties

        private int I2CAddr { get; set; }
        protected I2cDevice Device { get; set; }

        #endregion Properties

        #region Constructor

        protected I2CBase(int addr)
        {
            I2CAddr = addr;
        }

        #endregion Constructor

        #region Initialization

        /// <summary>
        /// InitI2C
        /// Initialize I2C Communications
        /// </summary>
        /// <returns>async Task</returns>
        public async Task InitI2CAsync(I2CSpeed i2cSpeed = I2CSpeed.I2C_100kHz)
        {
            // initialize I2C communications
            try
            {
                I2cConnectionSettings i2cSettings = new I2cConnectionSettings(I2CAddr);
                if (i2cSpeed == I2CSpeed.I2C_400kHz)
                    i2cSettings.BusSpeed = I2cBusSpeed.FastMode;
                else
                    i2cSettings.BusSpeed = I2cBusSpeed.StandardMode;

                string deviceSelector = I2cDevice.GetDeviceSelector();
                var i2cDeviceControllers = await DeviceInformation.FindAllAsync(deviceSelector);
                Device = await I2cDevice.FromIdAsync(i2cDeviceControllers[0].Id, i2cSettings);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Exception: {0}", e.Message);
                return;
            }
        }

        #endregion Initialization

        #region I2C primitives

        /// <summary>
        /// WriteRead
        /// writes to I2C and reads back the result
        /// </summary>
        /// <param name="writeBuffer"></param>
        /// <param name="readBuffer"></param>
        protected void WriteRead(byte[] writeBuffer, byte[] readBuffer)
        {
            try
            {
                lock (Device)
                {
                    Device.WriteRead(writeBuffer, readBuffer);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("I2C WriteRead Exception: {0}", ex.Message);
            }
        }

        /// <summary>
        /// Read
        /// Reads a PCA9685 register
        /// </summary>
        /// <param name="readBuffer"></param>
        protected void Read(byte[] readBuffer)
        {
            try
            {
                lock (Device)
                {
                    Device.Read(readBuffer);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("I2C Read Exception: {0}", ex.Message);
            }
        }

        /// <summary>
        /// Write
        /// Writes to a PCA9685 register
        /// </summary>
        /// <param name="writeBuffer"></param>
        protected void Write(byte[] writeBuffer)
        {
            try
            {
                lock (Device)
                {
                    Device.Write(writeBuffer);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("I2C Write Exception: {0}", ex.Message);
            }
        }

        #endregion I2C primitives
    }

    public class PCA9685  : I2CBase
    {
        #region Constants

        private const byte PCA9685_ADDRESS = 0x40;
        private const byte PCA9685_MODE1 = 0x00;
        private const byte PCA9685_MODE2 = 0x01;
        private const byte PCA9685_SUBADR1 = 0x02;
        private const byte PCA9685_SUBADR2 = 0x03;
        private const byte PCA9685_SUBADR3 = 0x04;
        private const byte PCA9685_PRESCALE = 0xFE;
        private const byte LED0_ON_L = 0x06;
        private const byte LED0_ON_H = 0x07;
        private const byte LED0_OFF_L = 0x08;
        private const byte LED0_OFF_H = 0x09;
        private const byte ALL_LED_ON_L = 0xFA;
        private const byte ALL_LED_ON_H = 0xFB;
        private const byte ALL_LED_OFF_L = 0xFC;
        private const byte ALL_LED_OFF_H = 0xFD;

        // Bits:
        private const byte RESTART = 0x80;

        private const byte SLEEP = 0x10;
        private const byte ALLCALL = 0x01;
        private const byte INVRT = 0x10;
        private const byte OUTDRV = 0x04;

        #endregion Constants

        #region Constructor

        public PCA9685(int addr = PCA9685_ADDRESS) : base(addr)
        {
        }

        #endregion Constructor

        #region Initialization

        /// <summary>
        /// InitPCA9685Async
        /// Initialize PCA9685 chip
        /// </summary>
        /// <returns>async Task</returns>
        public async Task InitPCA9685Async(I2CSpeed i2cSpeed = I2CSpeed.I2C_100kHz)
        {
            try
            {
                await InitI2CAsync(i2cSpeed);

                Reset();
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Exception: {0}", e.Message);
                return;
            }
        }

        /// <summary>
        /// Reset
        /// Issue Reset command to PCA9685
        /// </summary>
        public void Reset()
        {
            byte[] writeBuffer;
            // set defaults!
            // all outputs on Port A
            writeBuffer = new byte[] { PCA9685_MODE1, 0x0 };
            Write(writeBuffer);
            SetPWMFrequency(1000);  //default frequency
        }

        #endregion Initialization

        #region Operations

        public void SetPWMFrequency(double freq)
        {
            byte[] readBuffer;
            byte[] writeBuffer;

            freq *= 0.9;  // Correct for overshoot in the frequency setting

            double preScaleVal = 25000000;
            preScaleVal /= 4096;
            preScaleVal /= freq;
            preScaleVal -= 1;
            byte prescale = (byte)Math.Floor(preScaleVal + 0.5);

            lock (Device)
            {
                writeBuffer = new byte[] { PCA9685_MODE1 };
                readBuffer = new byte[1];
                WriteRead(writeBuffer, readBuffer);
                byte oldmode = readBuffer[0];
                byte newmode = (byte)((oldmode & 0x7F) | 0x10); // sleep

                writeBuffer = new byte[] { PCA9685_MODE1, newmode };
                Write(writeBuffer); // go to sleep

                writeBuffer = new byte[] { PCA9685_PRESCALE, prescale };
                Write(writeBuffer); // set the prescaler

                writeBuffer = new byte[] { PCA9685_MODE1, oldmode };
                Write(writeBuffer); // wake

                Task.Delay(5).Wait();

                writeBuffer = new byte[] { PCA9685_MODE1, (byte)(oldmode | 0xa1) };
                Write(writeBuffer);  // turn on auto mode
            }
        }

        /// <summary>
        /// Set PWM on a specified pin
        /// </summary>
        /// <param name="num"></param>
        /// <param name="on"></param>
        /// <param name="off"></param>
        public void SetPWM(int num, ushort on, ushort off)
        {
            byte[] writeBuffer;
            writeBuffer = new byte[] { (byte)(LED0_ON_L + 4 * num), (byte)on, (byte)(on >> 8), (byte)off, (byte)(off >> 8) };
            Write(writeBuffer);
        }

        /// <summary>
        /// Set PWM on all pins
        /// </summary>
        /// <param name="on"></param>
        /// <param name="off"></param>
        public void SetAllPWM(ushort on, ushort off)
        {
            byte[] writeBuffer;
            writeBuffer = new byte[] { (byte)(ALL_LED_ON_L), (byte)on, (byte)(on >> 8), (byte)off, (byte)(off >> 8) };
            Write(writeBuffer);
        }

        /// <summary>
        /// Sets pin without having to deal with on/off tick placement and properly handles
        /// a zero value as completely off.  Optional invert parameter supports inverting
        /// the pulse for sinking to ground.  Val should be a value from 0 to 4095 inclusive
        /// </summary>
        /// <param name="num"></param>
        /// <param name="val"></param>
        /// <param name="invert"></param>
        public void SetPin(int num, ushort val, bool invert)
        {
            // Clamp value between 0 and 4095 inclusive.
            val = Math.Min(val, (ushort)4095);
            if (invert)
            {
                if (val == 0)
                {
                    // Special value for signal fully on.
                    SetPWM(num, 4096, 0);
                }
                else if (val == 4095)
                {
                    // Special value for signal fully off.
                    SetPWM(num, 0, 4096);
                }
                else
                {
                    SetPWM(num, 0, (ushort)(4095 - val));
                }
            }
            else
            {
                if (val == 4095)
                {
                    // Special value for signal fully on.
                    SetPWM(num, 4096, 0);
                }
                else if (val == 0)
                {
                    // Special value for signal fully off.
                    SetPWM(num, 0, 4096);
                }
                else
                {
                    SetPWM(num, 0, val);
                }
            }
        }

        #endregion Operations

        /// <summary>
        /// usDelay
        /// function with delay argument in microseconds
        /// </summary>
        /// <param name="duration"></param>
        protected void usDelay(long duration)
        {
            // Static method to initialize and start stopwatch
            var sw = Stopwatch.StartNew();

            long nanosecPerTick = (1000L * 1000L * 1000L) / Stopwatch.Frequency;
            long durationTicks = (duration * 1000) / nanosecPerTick;

            while (sw.ElapsedTicks < durationTicks)
            {
            }
        }
    }
}
