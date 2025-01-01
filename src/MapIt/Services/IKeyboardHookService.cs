using System.Collections.Generic;
using System.Windows.Forms;

namespace MapIt.Services
{
    public interface IKeyboardHookService
    {
        bool IsListening { get; }

        void SetKeyMapping(Dictionary<Keys, ushort> keyMapping);
        void StartListening();
        void StopListening();
    }


}
