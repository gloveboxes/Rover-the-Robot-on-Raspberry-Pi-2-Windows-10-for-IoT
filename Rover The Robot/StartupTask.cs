using AdafruitMatrix;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Devices.Gpio;

// The Background Application template is documented at http://go.microsoft.com/fwlink/?LinkID=533884&clcid=0x409

namespace Robot {
    public sealed class StartupTask : IBackgroundTask {
        BackgroundTaskDeferral _deferral;

        GpioController gpio = GpioController.GetDefault();

        Motor leftMotor;
        Motor rightMotor;
        HCSR04 distanceRight;
        HCSR04 distanceLeft;
        HCSR04 distanceCenter;

        Adafruit8x8Matrix matrix = new Adafruit8x8Matrix("matrix");

        public void Run(IBackgroundTaskInstance taskInstance) {

            _deferral = taskInstance.GetDeferral();

            if (gpio == null) { return; }

            leftMotor = new Motor(gpio, 22, 24);
            rightMotor = new Motor(gpio, 12, 25);
            distanceRight = new HCSR04(gpio, 16, 26);
            distanceLeft = new HCSR04(gpio, 27, 23);
            distanceCenter = new HCSR04(gpio, 13, 6);



            // as long as the GPIO pins initialized properly, get moving
            while (true) {
                // start moving forward
                Forward(20);

                // as long as there is an obstacle in the way
                while (
                    distanceRight.ObstacleDetected() || distanceLeft.ObstacleDetected() ||
                    distanceCenter.ObstacleDetected()) {
                    Reverse(300);
                    TurnRight(250);
                    FullStop(50);
                }
            }
        }

        private void Forward(uint milliseconds = 0) {
            leftMotor.Forward();
            rightMotor.Forward();

            matrix.DrawSymbol(Adafruit8x8Matrix.Symbols.UpArrow);
            matrix.FrameDraw();
            Task.Delay(TimeSpan.FromMilliseconds(milliseconds)).Wait();

            Debug.WriteLine("Forward");
        }

        private void Reverse(uint milliseconds = 0) {
            leftMotor.Backward();
            rightMotor.Backward();

            matrix.DrawSymbol(Adafruit8x8Matrix.Symbols.DownArrow);
            matrix.FrameDraw();
            Task.Delay(TimeSpan.FromMilliseconds(milliseconds)).Wait();

            
            Debug.WriteLine("Reverse");
        }


        private void TurnLeft(uint milliseconds = 0) {
            // spin the left motor in the reverse direction
            leftMotor.Backward();
            rightMotor.Forward();

            matrix.DrawSymbol(Adafruit8x8Matrix.Symbols.LeftArrow);
            matrix.FrameDraw();
            Task.Delay(TimeSpan.FromMilliseconds(milliseconds)).Wait();

            Debug.WriteLine("Left");
        }


        private void TurnRight(uint milliseconds = 0) {
            leftMotor.Forward();
            rightMotor.Backward();

            matrix.DrawSymbol(Adafruit8x8Matrix.Symbols.RightArrow);
            matrix.FrameDraw();
            Task.Delay(TimeSpan.FromMilliseconds(milliseconds)).Wait();

            Debug.WriteLine("Right");
        }

        private void FullStop(uint milliseconds = 0) {
            leftMotor.Stop();
            rightMotor.Stop();

            matrix.DrawSymbol(Adafruit8x8Matrix.Symbols.Block);
            matrix.FrameDraw();
            Task.Delay(TimeSpan.FromMilliseconds(milliseconds)).Wait();

            Debug.WriteLine("Neutral");
        }
    }
}
