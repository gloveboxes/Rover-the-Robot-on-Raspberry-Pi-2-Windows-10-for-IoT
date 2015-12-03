using AdapterLib;
using Glovebox.IoT.Devices.Actuators;
using Robot.Sensors;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Gpio;

namespace Robot
{
    class RobotMgr : Speech
    {
        GpioController gpio = GpioController.GetDefault();

        Motor leftMotor;
        Motor rightMotor;
        IDistance[] distanceSensors;

        Random rnd = new Random();

        bool autonomous = false;

        IDistance closestObject;
        uint turnDuration = 10;
        bool sharpTurn = false;

        protected void InitRobot()
        {

            if (gpio == null) { return; }

            leftMotor = new Motor(23, 22);
            rightMotor = new Motor(24, 25);

            distanceSensors = new IDistance[] {
                new UltraSound(26, 20, "right", 50),
                new UltraSound(12, 5, "left", 50),
                new UltraSound(13, 6, "center", 50),
           //     new InfraRed(18, "centerIR", 10)
            };
        }

        protected async void StartSensing()
        {
            while (true)
            {
                if (autonomous)
                {

                    closestObject = await DetectObject();

                    while (closestObject != null && autonomous)
                    {
                        sharpTurn = closestObject.Distance < 15 ? true : false;

                        Speak(closestObject.Name);

                        switch (closestObject.Name)
                        {
                            case "left":
                                await TurnRight(sharpTurn, turnDuration);
                                break;
                            case "right":
                                await TurnLeft(sharpTurn, turnDuration);
                                break;
                            case "center":
                                if (distanceSensors[0].Distance < distanceSensors[1].Distance)
                                { // is right closer than left}
                                    await TurnLeft(sharpTurn, turnDuration);
                                }
                                else
                                {
                                    await TurnRight(sharpTurn, turnDuration);
                                }
                                break;
                            case "centerIR":

                                await FullStop(100);
                                await Reverse(150);
                                await FullStop(100);

                                if (distanceSensors[0].Distance < distanceSensors[1].Distance)
                                { // is right closer than left}
                                    await TurnLeft(sharpTurn, (uint)rnd.Next(10, 30));
                                }
                                else
                                {
                                    await TurnRight(sharpTurn, (uint)rnd.Next(10, 30));
                                }

                                break;
                            default:
                                break;
                        }
                        //Forward();      
                        closestObject = await DetectObject();
                    }
                    await Forward();
                }
                else
                {
                    await Task.Delay(1000);
                }
            }
        }

        private async Task<IDistance> DetectObject()
        {
            await Task.Yield();

            double closestDistance = double.MaxValue;
            IDistance closestSensor = null;

            foreach (var sensor in distanceSensors)
            {
                if (sensor.ObstacleDetected())
                {
                    if (sensor.Distance < closestDistance)
                    {
                        closestDistance = sensor.Distance;
                        closestSensor = sensor;
                    }
                }
            }

            return closestSensor;
        }

        private async Task Forward(uint milliseconds = 0)
        {
            leftMotor.Forward();
            rightMotor.Forward();

            await Task.Delay(TimeSpan.FromMilliseconds(milliseconds));
        }

        private async Task Reverse(uint milliseconds = 0)
        {
            leftMotor.Backward();
            rightMotor.Backward();

            await Task.Delay(TimeSpan.FromMilliseconds(milliseconds));
        }


        private async Task TurnLeft(bool sharpTurn, uint milliseconds = 0)
        {
            // spin the left motor in the reverse direction

            rightMotor.Forward();

            if (sharpTurn)
            {
                leftMotor.Backward();
            }
            else
            {
                leftMotor.Stop();
            }

            await Task.Delay(TimeSpan.FromMilliseconds(milliseconds));
        }


        private async Task TurnRight(bool sharpTurn, uint milliseconds = 0)
        {
            leftMotor.Forward();

            if (sharpTurn)
            {
                rightMotor.Backward();
            }
            else
            {
                rightMotor.Stop();
            }


            await Task.Delay(TimeSpan.FromMilliseconds(milliseconds));
        }

        private async Task FullStop(uint milliseconds = 0)
        {
            leftMotor.Stop();
            rightMotor.Stop();

            await Task.Delay(TimeSpan.FromMilliseconds(milliseconds));
        }

        public async override void Adapter_AllJoynMethod(object sender, AllJoynMethodData e)
        {
            switch (e.Method.Name.ToLower())
            {
                case "stop":
                    await FullStop();
                    break;
                case "left":
                    await TurnLeft(false);
                    break;
                case "right":
                    await TurnRight(false);
                    break;
                case "forward":
                    await Forward();
                    break;
                case "manual":
                    autonomous = false;
                    break;
                case "autonomous":
                    autonomous = true;
                    break;
                case "speak":
                    preMessage = e.AdapterDevice.Properties.Where(x => x.Name == "Speech").First()
                        .Attributes.Where(y => y.Value.Name == "Message").First().Value.Data as string;
                    Speak(preMessage);
                    break;

                default:
                    break;
            }

            base.Adapter_AllJoynMethod(sender, e);
        }
    }
}
