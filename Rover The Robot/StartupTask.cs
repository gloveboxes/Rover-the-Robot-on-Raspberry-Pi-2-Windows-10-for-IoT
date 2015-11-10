using Glovebox.Graphics.Components;
using Glovebox.Graphics.Drivers;
using Glovebox.IoT.Devices.Actuators;
using Robot.Sensors;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Devices.Gpio;

// The Background Application template is documented at http://go.microsoft.com/fwlink/?LinkID=533884&clcid=0x409

namespace Robot
{
    public sealed class StartupTask : IBackgroundTask {
        BackgroundTaskDeferral _deferral;

        GpioController gpio = GpioController.GetDefault();

        Motor leftMotor;
        Motor rightMotor;
        IDistance[] distanceSensors;

     //   LED8x8Matrix  matrix = new LED8x8Matrix(new Ht16K33());
        Random rnd = new Random();

        public void Run(IBackgroundTaskInstance taskInstance) {
            IDistance closestObject;
            uint turnDuration = 10;
            bool sharpTurn = false;

            _deferral = taskInstance.GetDeferral();

            if (gpio == null) { return; }

            leftMotor = new Motor(23, 22);
            rightMotor = new Motor(24, 25);

            distanceSensors = new IDistance[] {
                new UltraSound(26, 20, "right", 30),
                new UltraSound(12, 5, "left", 30),
                new UltraSound(13, 6, "center", 35),
           //     new InfraRed(18, "centerIR", 10)
            };


            // as long as the GPIO pins initialized properly, get moving
            while (true) {
                // start moving forward   

                closestObject = DetectObject();

                while (closestObject != null) {

                    sharpTurn = closestObject.Distance < 15 ? true : false;

              //      turnDuration = ((uint)rnd.Next((int)(turnDuration - 20), (int)(turnDuration + 20)));

             //       Debug.WriteLine(closestObject.Name + ", distance: " + closestObject.Distance.ToString() + ", turn:" + turnDuration.ToString());

                    switch (closestObject.Name) {
                        case "left":
                            TurnRight(sharpTurn, turnDuration);
                            break;
                        case "right":
                            TurnLeft(sharpTurn,turnDuration);
                            break;
                        case "center":
                            if (distanceSensors[0].Distance < distanceSensors[1].Distance) { // is right closer than left}
                                TurnLeft(sharpTurn, turnDuration);
                            }
                            else {
                                TurnRight(sharpTurn, turnDuration);
                            }
                            break;
                        case "centerIR":
                            FullStop(100);
                            Reverse(150);
                            FullStop(100);

                            if (distanceSensors[0].Distance < distanceSensors[1].Distance) { // is right closer than left}
                                TurnLeft(sharpTurn, (uint)rnd.Next(10, 30));
                            }
                            else {
                                TurnRight(sharpTurn, (uint)rnd.Next(10, 30));
                            }

                            break;
                        default:
                            break;
                    }
                    //Forward();      
                    closestObject = DetectObject();
                }
                Forward();
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

            //matrix.DrawSymbol(LED8x8Matrix.Symbols.UpArrow);
            //matrix.FrameDraw();
            Task.Delay(TimeSpan.FromMilliseconds(milliseconds)).Wait();

            //Debug.WriteLine("Forward");
        }

        private void Reverse(uint milliseconds = 0) {
            leftMotor.Backward();
            rightMotor.Backward();

            //matrix.DrawSymbol(LED8x8Matrix.Symbols.DownArrow);
            //matrix.FrameDraw();
            Task.Delay(TimeSpan.FromMilliseconds(milliseconds)).Wait();


            //Debug.WriteLine("Reverse");
        }


        private void TurnLeft(bool sharpTurn, uint milliseconds = 0) {
            // spin the left motor in the reverse direction

            rightMotor.Forward();

            if (sharpTurn) {
                leftMotor.Backward();
            }
            else {
                leftMotor.Stop();
            }            

            //matrix.DrawSymbol(LED8x8Matrix.Symbols.LeftArrow);
            //matrix.FrameDraw();
            Task.Delay(TimeSpan.FromMilliseconds(milliseconds)).Wait();

            //Debug.WriteLine("Left");
        }


        private void TurnRight(bool sharpTurn, uint milliseconds = 0) {
            leftMotor.Forward();

            if (sharpTurn) {
                rightMotor.Backward();
            }
            else {
                rightMotor.Stop();
            }

            //matrix.DrawSymbol(LED8x8Matrix.Symbols.RightArrow);
            //matrix.FrameDraw();
            Task.Delay(TimeSpan.FromMilliseconds(milliseconds)).Wait();

            //Debug.WriteLine("Right");
        }

        private void FullStop(uint milliseconds = 0) {
            leftMotor.Stop();
            rightMotor.Stop();

            //matrix.DrawSymbol(LED8x8Matrix.Symbols.Block);
            //matrix.FrameDraw();
            Task.Delay(TimeSpan.FromMilliseconds(milliseconds)).Wait();

            //Debug.WriteLine("Neutral");
        }
    }
}
