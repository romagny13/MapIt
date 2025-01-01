using System;

namespace MapIt.Services
{
    public interface IAppInstanceManager : IDisposable
    {
        bool CanExecuteNewInstance(string mutexName);
    }
}
