using System.Threading;

namespace MapIt.Services
{
    public class SingleInstanceAppManager : IAppInstanceManager
    {
        private Mutex _mutex;

        public bool CanExecuteNewInstance(string mutexName)
        {
            _mutex = new Mutex(true, mutexName, out bool isNewInstance);
            return isNewInstance;
        }

        public void Dispose()
        {
            DisposeMutex();
        }

        private void DisposeMutex()
        {
            _mutex.ReleaseMutex();
        }
    }


}
