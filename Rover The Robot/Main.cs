namespace Robot
{
    class Main : RobotMgr
    {
        public async void Initialise()
        { 
            await InitSpeech();

            InitAllJoyn();

            InitRobot();

            StartSensing();      

            adapter.AllJoynMethod += Adapter_AllJoynMethod;
        }
    }
}
