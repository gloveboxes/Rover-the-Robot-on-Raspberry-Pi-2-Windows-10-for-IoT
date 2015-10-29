namespace Robot.Sensors
{
    class UltraSound : Glovebox.IoT.Devices.Sensors.Distance.HCSR04, IDistance {

        public double Distance { get; private set; }
        public string Name { get; private set; }
        public uint TriggerDistanceCMs { get; private set; }


        public UltraSound(byte trig_Pin, byte echo_Pin, string name, uint triggerDistanceCMs): base(trig_Pin, echo_Pin) {
            this.Name = name;
            this.TriggerDistanceCMs = triggerDistanceCMs;
        }

        public bool ObstacleDetected() {
            double distance = 0.0;
            bool detected = GetDistanceToObstacle(ref distance);
            Distance = distance;

            if (detected)
            {
                return Distance < TriggerDistanceCMs;
            }

            return false;
        }
    }
}
