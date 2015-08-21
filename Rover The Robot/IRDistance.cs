using System;
using Windows.Devices.Gpio;

namespace Robot {
    class IRDistance : IDistance {
        private GpioController gpio;
        private GpioPin input;
        byte IR_Pin;

        public double Distance { get { return TriggerDistanceCMs; } }
        public string Name { get; private set; }
        public uint TriggerDistanceCMs { get; private set; }

        public IRDistance(GpioController gpio, byte IR_Pin, string name, uint triggerDistanceCMs) {
            this.gpio = gpio;
            this.IR_Pin = IR_Pin;
            this.Name = name;
            this.TriggerDistanceCMs = triggerDistanceCMs;

            input = gpio.OpenPin(IR_Pin);
            input.SetDriveMode(GpioPinDriveMode.Input);
        }


        public bool ObstacleDetected() {
            return input.Read() == GpioPinValue.Low;
        }
    }
}
