using System;

namespace MapIt.Services
{
    public interface INotifyIconService: IDisposable
    {
        void Initialize();
        void ShowBalloonTip(string title, string message);
    }


}
