using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.I2c;

namespace AdafruitMatrix {

    /// <summary>
    /// Represents a I2C connection to a PCF8574 I/O Expander.
    /// </summary>
    /// <remarks>See <see cref="http://www.adafruit.com/datasheets/ht16K33v110.pdf"/> for more information.</remarks>
    public class Ht16K33 : IDisposable {
        #region Fields
        private I2cDevice i2cDevice;

        private const byte OSCILLATOR_ON = 0x21;
        private const byte OSCILLATOR_OFF = 0x20;

        private const string I2C_CONTROLLER_NAME = "I2C1";        /* For Raspberry Pi 2, use I2C1 */
        private const byte I2C_ADDR = 0x70;

        public enum Display : byte {
            On = 0x81,
            Off = 0x80,
        }

        public enum BlinkRate : byte {
            Off = 0x00,
            Fast = 0x02, //2hz
            Medium = 0x04, //1hz
            Slow = 0x06, //0.5 hz
        }

        private Display display;
        private BlinkRate blinkrate;
        private byte brightness;

        #endregion

        /// <summary>
        /// Initializes a new instance of the Ht16K33 I2C controller as found on the Adafriut Mini LED Matrix.
        /// </summary>
        /// <param name="display">On or Off - defaults to On</param>
        /// <param name="brightness">Between 0 and 15</param>
        /// <param name="blinkrate">Defaults to Off.  Blink rates Fast = 2hz, Medium = 1hz, slow = 0.5hz</param>
        public Ht16K33(Display display = Display.On, byte brightness = 2, BlinkRate blinkrate = BlinkRate.Off) {

            this.display = display;
            this.brightness = brightness;
            this.blinkrate = blinkrate;

            Task.Run(() => I2cConnect()).Wait();

            InitController();
        }

        private async Task I2cConnect() {
            try {
                var settings = new I2cConnectionSettings(I2C_ADDR);
                settings.BusSpeed = I2cBusSpeed.FastMode;

                string aqs = I2cDevice.GetDeviceSelector(I2C_CONTROLLER_NAME);  /* Find the selector string for the I2C bus controller                   */
                var dis = await DeviceInformation.FindAllAsync(aqs);            /* Find the I2C bus controller device with our selector string           */
                i2cDevice = await I2cDevice.FromIdAsync(dis[0].Id, settings);    /* Create an I2cDevice with our selected bus controller and I2C settings */
            }
            catch (Exception e) {
                throw new Exception("ht16k33 initisation problem: " + e.Message);
            }
        }

        public void Write(byte[] frame) {
            i2cDevice.Write(frame);
        }

        public void SetBrightness(byte level) {
            if (level > 15) { level = 15; }
            Write(new byte[] { (byte)(0xE0 | level), 0x00 });
        }

        public void SetBlinkRate(BlinkRate blinkrate) {
            this.blinkrate = blinkrate;
            SetDisplayState();
        }

        public void SetDisplay(Display display) {
            this.display = display;
            SetDisplayState();
        }

        private void SetDisplayState() {
            Write(new byte[] { (byte)((byte)display | (byte)blinkrate), 0x00 });
        }

        private void InitController() {
            Write(new byte[] { OSCILLATOR_ON, 0x00 });
            SetDisplayState();
            SetBrightness(brightness);
        }

        void IDisposable.Dispose() {
            i2cDevice.Dispose();
        }
    }
}
