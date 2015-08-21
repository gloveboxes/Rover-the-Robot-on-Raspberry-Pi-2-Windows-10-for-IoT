namespace Robot {
    interface IDistance {
        double Distance { get; }
        string Name { get; }
        uint TriggerDistanceCMs { get; }

        bool ObstacleDetected();
    }
}