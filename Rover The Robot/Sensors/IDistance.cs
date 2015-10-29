namespace Robot.Sensors
{
    interface IDistance {
        double Distance { get; }
        string Name { get; }
        uint TriggerDistanceCMs { get; }

        bool ObstacleDetected();
    }
}