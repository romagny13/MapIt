using System.Collections.Generic;
using System.Windows.Forms;

namespace MapIt.Services
{
    public interface IKeyMappingService
    {
        Dictionary<Keys, ushort> LoadKeyMappings();
    }


}
