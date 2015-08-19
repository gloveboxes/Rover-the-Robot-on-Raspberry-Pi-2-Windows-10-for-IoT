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
        HCSR04 distance;

        public void Run(IBackgroundTaskInstance taskInstance) {

            _deferral = taskInstance.GetDeferral();

            if (gpio == null) { return; }

            leftMotor = new Motor(gpio, 27, 22);
            rightMotor = new Motor(gpio, 6, 5);
            distance = new HCSR04(gpio, 23, 24);

            // as long as the GPIO pins initialized properly, get moving
            while (true) {
                // start moving forward
                Forward(100);

                // as long as there is an obstacle in the way
                while (distance.ObstacleDetected()) {
                    Reverse(250);
                    //TurnRight(100);
                    //Reverse(150);
                    TurnRight(250);
                    FullStop();
                }
            }
        }

        private void Forward(uint milliseconds=0) {
            leftMotor.Forward();
            rightMotor.Forward();

            Task.Delay(TimeSpan.FromMilliseconds(milliseconds)).Wait();

            Debug.WriteLine("Forward");
        }

        private void Reverse(uint milliseconds = 0) {
            leftMotor.Backward();
            rightMotor.Backward();

            Task.Delay(TimeSpan.FromMilliseconds(milliseconds)).Wait();

            Debug.WriteLine("Reverse");
        }


        private void TurnLeft(uint milliseconds = 0) {
            // spin the left motor in the reverse direction
            leftMotor.Backward();
            rightMotor.Forward();

            Task.Delay(TimeSpan.FromMilliseconds(milliseconds)).Wait();

            Debug.WriteLine("Left");
        }


        private void TurnRight(uint milliseconds = 0) {
            leftMotor.Forward();
            rightMotor.Backward();

            Task.Delay(TimeSpan.FromMilliseconds(milliseconds)).Wait();

            Debug.WriteLine("Right");
        }

        private void FullStop(uint milliseconds = 0) {
            leftMotor.Stop();
            rightMotor.Stop();

            Task.Delay(TimeSpan.FromMilliseconds(milliseconds)).Wait();

            Debug.WriteLine("Neutral");
        }
    }
}
