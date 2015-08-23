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
        IDistance[] distanceSensors;

        Adafruit8x8Matrix matrix = new Adafruit8x8Matrix("matrix");
        Random rnd = new Random();

        public void Run(IBackgroundTaskInstance taskInstance) {
            IDistance closestObject;
            uint turnDuration;

            _deferral = taskInstance.GetDeferral();

            if (gpio == null) { return; }

            leftMotor = new Motor(gpio, 22, 24);
            rightMotor = new Motor(gpio, 12, 25);

            distanceSensors = new IDistance[] {
                new HCSR04(gpio, 16, 26, "right", 20),
                new HCSR04(gpio, 27, 23, "left", 20),
                new HCSR04(gpio, 13, 6, "center", 30),
                new IRDistance(gpio, 18, "centerIR", 8)
            };


            // as long as the GPIO pins initialized properly, get moving
            while (true) {
                // start moving forward   
                turnDuration = 100;

                closestObject = DetectObject();

                while (closestObject != null) {
                    //turnDuration = (uint)(closestObject.TriggerDistanceCMs / closestObject.Distance * 30 + 10);

                    if (closestObject.Distance < 10) { turnDuration = 70; }
                    else if (closestObject.Distance < 20) { turnDuration = 50; }
                    else if (closestObject.Distance < 30) { turnDuration = 30; }

                    turnDuration = ((uint)rnd.Next((int)(turnDuration - 20), (int)(turnDuration + 20)));

                    Debug.WriteLine(closestObject.Name +  ", distance: " + closestObject.Distance.ToString() + ", turn:" + turnDuration.ToString());

                    switch (closestObject.Name) {
                        case "left":
                            TurnRight(turnDuration);
                            break;
                        case "right":
                            TurnLeft(turnDuration);
                            break;
                        case "center":
                            if (distanceSensors[0].Distance < distanceSensors[1].Distance) { // is right closer than left}
                                TurnLeft(turnDuration);
                            }
                            else {
                                 TurnRight(turnDuration);
                            }
                            break;
                        case "centerIR":
                            Reverse(250);

                            if (distanceSensors[0].Distance < distanceSensors[1].Distance) { // is right closer than left}
                                TurnLeft(turnDuration);
                            }
                            else {
                                TurnRight(turnDuration);
                            }
                            break;

                        default:
                            break;
                    }
                    //Forward();
                    closestObject = DetectObject();
                }
               Forward(50);
            }
        }

        private IDistance DetectObject() {
            double closestDistance = double.MaxValue;
            IDistance closestSensor = null;

            foreach (var sensor in distanceSensors) {
                if (sensor.ObstacleDetected()) {
                    if (sensor.Distance < closestDistance) {
                        closestDistance = sensor.Distance;
                        closestSensor = sensor;
                    }
                }
            }

            return closestSensor;
        }

        private void Forward(uint milliseconds = 0) {
            leftMotor.Forward();
            rightMotor.Forward();

            matrix.DrawSymbol(Adafruit8x8Matrix.Symbols.UpArrow);
            matrix.FrameDraw();
            Task.Delay(TimeSpan.FromMilliseconds(milliseconds)).Wait();

            //Debug.WriteLine("Forward");
        }

        private void Reverse(uint milliseconds = 0) {
            leftMotor.Backward();
            rightMotor.Backward();

            matrix.DrawSymbol(Adafruit8x8Matrix.Symbols.DownArrow);
            matrix.FrameDraw();
            Task.Delay(TimeSpan.FromMilliseconds(milliseconds)).Wait();

            
            //Debug.WriteLine("Reverse");
        }


        private void TurnLeft(uint milliseconds = 0) {
            // spin the left motor in the reverse direction
            leftMotor.Backward();
            rightMotor.Forward();

            matrix.DrawSymbol(Adafruit8x8Matrix.Symbols.LeftArrow);
            matrix.FrameDraw();
            Task.Delay(TimeSpan.FromMilliseconds(milliseconds)).Wait();

            //Debug.WriteLine("Left");
        }


        private void TurnRight(uint milliseconds = 0) {
            leftMotor.Forward();
            rightMotor.Backward();

            matrix.DrawSymbol(Adafruit8x8Matrix.Symbols.RightArrow);
            matrix.FrameDraw();
            Task.Delay(TimeSpan.FromMilliseconds(milliseconds)).Wait();

            //Debug.WriteLine("Right");
        }

        private void FullStop(uint milliseconds = 0) {
            leftMotor.Stop();
            rightMotor.Stop();

            matrix.DrawSymbol(Adafruit8x8Matrix.Symbols.Block);
            matrix.FrameDraw();
            Task.Delay(TimeSpan.FromMilliseconds(milliseconds)).Wait();

            //Debug.WriteLine("Neutral");
        }
    }
}
