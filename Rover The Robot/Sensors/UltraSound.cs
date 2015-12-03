namespace Robot.Sensors
{
    class UltraSound : Glovebox.IoT.Devices.Sensors.Distance.HCSR04, IDistance {

        public double Distance { get; private set; }
        public string Name { get; private set; }
        public uint TriggerDistanceCMs { get; private set; }


        public UltraSound(byte trig_Pin, byte echo_Pin, string name, int triggerDistanceCMs): base(trig_Pin, echo_Pin) {
            this.Name = name;
            this.TriggerDistanceCMs = (uint)triggerDistanceCMs;
        }

        public bool ObstacleDetected() {
            Distance = GetDistance(UnitsNet.Length.FromCentimeters(TriggerDistanceCMs)).Centimeters;

            bool detected = Distance != 0;

            if (detected)
            {
                return Distance < TriggerDistanceCMs;
            }

            return false;
        }
    }
}
