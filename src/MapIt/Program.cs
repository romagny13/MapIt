using System;

namespace MapIt
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            var bootstrapper = new Bootstrapper();
            bootstrapper.Run();
        }
    }
}