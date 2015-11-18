using Windows.ApplicationModel.Background;

// The Background Application template is documented at http://go.microsoft.com/fwlink/?LinkID=533884&clcid=0x409

namespace Robot
{
    public sealed class StartupTask : IBackgroundTask {

        BackgroundTaskDeferral deferral;

        private Main _main = new Main();


        public void Run(IBackgroundTaskInstance taskInstance) {

            deferral = taskInstance.GetDeferral();

            _main.Initialise();

        }
    }
}
