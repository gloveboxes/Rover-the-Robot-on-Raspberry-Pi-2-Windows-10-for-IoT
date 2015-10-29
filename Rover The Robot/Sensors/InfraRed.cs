using System;
using Windows.Devices.Gpio;

namespace Robot.Sensors
{
    class InfraRed : Glovebox.IoT.Devices.Sensors.Distance.InfraRed, IDistance {

        public double Distance { get { return TriggerDistanceCMs; } }
        public string Name { get; private set; }
        public uint TriggerDistanceCMs { get; private set; }

        public InfraRed(byte IR_Pin, string name, uint triggerDistanceCMs): base(IR_Pin) {

            this.Name = name;
            this.TriggerDistanceCMs = triggerDistanceCMs;
        }


        public bool ObstacleDetected() {
            double dummy = 0;
            return GetDistanceToObstacle(ref dummy);
        }
    }
}
