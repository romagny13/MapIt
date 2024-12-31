using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Windows.Forms;

namespace MapIt
{
    internal static class Program
    {
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;
        private static LowLevelKeyboardProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;

        // NotifyIcon pour afficher dans la barre des tâches
        private static NotifyIcon _notifyIcon;

        // Mapping des touches
        private static Dictionary<Keys, ushort> keyMapping = new Dictionary<Keys, ushort>();

        // Structure pour l'input
        [StructLayout(LayoutKind.Sequential)]
        public struct INPUT
        {
            public uint type;
            public InputUnion u;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct InputUnion
        {
            [FieldOffset(0)] public KEYBDINPUT ki;
            [FieldOffset(0)] public MOUSEINPUT mi;
            [FieldOffset(0)] public HARDWAREINPUT hi;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct KEYBDINPUT
        {
            public ushort wVk;
            public ushort wScan;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public uint mouseData;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct HARDWAREINPUT
        {
            public uint uMsg;
            public ushort wParamL;
            public ushort wParamH;
        }

        [STAThread]
        static void Main()
        {
            // Charger les keyMappings depuis un fichier JSON
            LoadKeyMappings();

            // NotifyIcon
            _notifyIcon = new NotifyIcon
            {
                Icon = new Icon("MapIt.ico"),
                Visible = true,
                Text = "MapIt"
            };

            _notifyIcon.DoubleClick += (sender, e) =>
            {
                MessageBox.Show("MapIt en cours d'exécution !");
            };

            var contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("Quitter", null, (sender, e) => Application.Exit());
            _notifyIcon.ContextMenuStrip = contextMenu;


            // Démarrer l'écoute des touches
            _hookID = SetHook(_proc);
            Application.Run();

            // Nettoyer et libérer les ressources
            UnhookWindowsHookEx(_hookID);
            _notifyIcon.Dispose();
        }

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                Keys key = (Keys)vkCode;

                // Vérification si la touche est dans le mappage
                if (keyMapping.ContainsKey(key))
                {
                    bool isKeyDown = wParam == (IntPtr)WM_KEYDOWN;
                    bool isKeyUp = wParam == (IntPtr)WM_KEYUP;

                    if (isKeyDown || isKeyUp)
                    {
                        // Simuler la touche mappée
                        SendKey(keyMapping[key], isKeyUp);
                        return (IntPtr)1; // Bloquer la touche originale
                    }
                }
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        private static void SendKey(ushort virtualKey, bool keyUp)
        {
            var input = new INPUT
            {
                type = 1, // INPUT_KEYBOARD
                u = new InputUnion
                {
                    ki = new KEYBDINPUT
                    {
                        wVk = virtualKey,
                        wScan = 0,
                        dwFlags = keyUp ? (uint)0x0002 : (uint)0x0000, // KEYEVENTF_KEYUP : 0
                        time = 0,
                        dwExtraInfo = IntPtr.Zero
                    }
                }
            };

            SendInput(1, new INPUT[] { input }, Marshal.SizeOf(typeof(INPUT)));
        }

        private static void LoadKeyMappings()
        {
            try
            {
                // Charger les key mappings depuis un fichier JSON
                var filePath = "keyMappings.json";
                if (File.Exists(filePath))
                {
                    var json = File.ReadAllText(filePath);
                    keyMapping = JsonSerializer.Deserialize<Dictionary<Keys, ushort>>(json);
                    Console.WriteLine("Key mappings chargés depuis JSON.");
                }
                else
                {
                    Console.WriteLine("Le fichier JSON des key mappings n'a pas été trouvé.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement du fichier JSON : {ex.Message}");
            }
        }

        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                    GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);
    }

    //internal static class Program
    //{
    //    private const int WH_KEYBOARD_LL = 13;
    //    private const int WM_KEYDOWN = 0x0100;
    //    private const int WM_KEYUP = 0x0101;
    //    private static LowLevelKeyboardProc _proc = HookCallback;
    //    private static IntPtr _hookID = IntPtr.Zero;

    //    // Dictionnaire pour suivre les touches maintenues
    //    private static Dictionary<Keys, bool> keyState = new Dictionary<Keys, bool>();

    //    // Mappage des touches simples (pas de combinaison)
    //    private static Dictionary<Keys, Action> simpleKeyMapping = new Dictionary<Keys, Action>
    //    {
    //        { Keys.NumPad1, () => HandleKey("NumPad1") },
    //        { Keys.NumPad2, () => HandleKey("NumPad2") },
    //        { Keys.NumPad3, () => HandleKey("NumPad3") },
    //        { Keys.Z, () => HandleKey("Z") },
    //        { Keys.W, () => HandleKey("W") }
    //    };

    //    // Mappage des combinaisons de touches
    //    private static Dictionary<Keys[], Action> keyCombinationMapping = new Dictionary<Keys[], Action>
    //    {
    //        { new[] { Keys.Control, Keys.Alt, Keys.NumPad1 }, () => HandleCombination("Ctrl + Alt + NumPad1") },
    //        { new[] { Keys.Control, Keys.NumPad2 }, () => HandleCombination("Ctrl + NumPad2") },
    //        { new[] { Keys.NumPad3, Keys.Z }, () => HandleCombination("NumPad3 + Z") }
    //    };

    //    [STAThread]
    //    static void Main()
    //    {
    //        _hookID = SetHook(_proc);
    //        Application.Run();
    //        UnhookWindowsHookEx(_hookID);
    //    }

    //    private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    //    {
    //        if (nCode >= 0)
    //        {
    //            int vkCode = Marshal.ReadInt32(lParam);
    //            Keys key = (Keys)vkCode;

    //            bool isKeyDown = wParam == (IntPtr)WM_KEYDOWN;
    //            bool isKeyUp = wParam == (IntPtr)WM_KEYUP;

    //            // Enregistrer l'état de la touche
    //            if (isKeyDown)
    //            {
    //                if (!keyState.ContainsKey(key))
    //                    keyState[key] = false;
    //                keyState[key] = true;
    //            }
    //            else if (isKeyUp)
    //            {
    //                keyState[key] = false;
    //            }

    //            // Vérifier si une touche simple a été pressée
    //            if (simpleKeyMapping.ContainsKey(key) && isKeyDown)
    //            {
    //                simpleKeyMapping[key].Invoke(); // Exécuter l'action pour la touche simple
    //            }

    //            // Vérifier les combinaisons de touches
    //            foreach (var combination in keyCombinationMapping)
    //            {
    //                if (AreKeysPressed(combination.Key))
    //                {
    //                    combination.Value.Invoke(); // Exécuter l'action pour la combinaison
    //                }
    //            }

    //            // Bloquer la touche originale si nécessaire
    //            return (IntPtr)1;
    //        }

    //        return CallNextHookEx(_hookID, nCode, wParam, lParam);
    //    }

    //    // Vérifier si toutes les touches d'une combinaison sont enfoncées
    //    private static bool AreKeysPressed(Keys[] keys)
    //    {
    //        foreach (var key in keys)
    //        {
    //            if (!keyState.ContainsKey(key) || !keyState[key])
    //            {
    //                return false;
    //            }
    //        }
    //        return true;
    //    }

    //    // Fonction à exécuter lors de la détection d'une touche simple
    //    private static void HandleKey(string key)
    //    {
    //        Console.WriteLine($"Touche simple détectée : {key}");
    //    }

    //    // Fonction à exécuter lors de la détection d'une combinaison de touches
    //    private static void HandleCombination(string combination)
    //    {
    //        Console.WriteLine($"Combinaison de touches détectée : {combination}");
    //    }

    //    private static IntPtr SetHook(LowLevelKeyboardProc proc)
    //    {
    //        using (Process curProcess = Process.GetCurrentProcess())
    //        using (ProcessModule curModule = curProcess.MainModule)
    //        {
    //            return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
    //                GetModuleHandle(curModule.ModuleName), 0);
    //        }
    //    }

    //    private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

    //    [DllImport("user32.dll")]
    //    private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

    //    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    //    private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

    //    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    //    [return: MarshalAs(UnmanagedType.Bool)]
    //    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    //    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    //    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    //    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    //    private static extern IntPtr GetModuleHandle(string lpModuleName);
    //}


    // OK

    //internal static class Program
    //{
    //    private const int WH_KEYBOARD_LL = 13;
    //    private const int WM_KEYDOWN = 0x0100;
    //    private const int WM_KEYUP = 0x0101;
    //    private static LowLevelKeyboardProc _proc = HookCallback;
    //    private static IntPtr _hookID = IntPtr.Zero;

    //    // Structure pour l'input
    //    [StructLayout(LayoutKind.Sequential)]
    //    public struct INPUT
    //    {
    //        public uint type;
    //        public InputUnion u;
    //    }

    //    [StructLayout(LayoutKind.Explicit)]
    //    public struct InputUnion
    //    {
    //        [FieldOffset(0)] public KEYBDINPUT ki;
    //        [FieldOffset(0)] public MOUSEINPUT mi;
    //        [FieldOffset(0)] public HARDWAREINPUT hi;
    //    }

    //    [StructLayout(LayoutKind.Sequential)]
    //    public struct KEYBDINPUT
    //    {
    //        public ushort wVk;
    //        public ushort wScan;
    //        public uint dwFlags;
    //        public uint time;
    //        public IntPtr dwExtraInfo;
    //    }

    //    [StructLayout(LayoutKind.Sequential)]
    //    public struct MOUSEINPUT
    //    {
    //        public int dx;
    //        public int dy;
    //        public uint mouseData;
    //        public uint dwFlags;
    //        public uint time;
    //        public IntPtr dwExtraInfo;
    //    }

    //    [StructLayout(LayoutKind.Sequential)]
    //    public struct HARDWAREINPUT
    //    {
    //        public uint uMsg;
    //        public ushort wParamL;
    //        public ushort wParamH;
    //    }

    //    // Mapping des touches
    //    private static Dictionary<Keys, ushort> keyMapping = new Dictionary<Keys, ushort>
    //    {
    //        { Keys.NumPad1, 0x55 }, // U key
    //        { Keys.NumPad2, 0x4E }, // N key
    //        { Keys.NumPad4, 0x48 }, // H key
    //        { Keys.Z, 0x57 },       // W key
    //        { Keys.Q, 0x41 },       // A key
    //        { Keys.C, 0x20 }        // SPACE
    //    };

    //    [STAThread]
    //    static void Main()
    //    {
    //        _hookID = SetHook(_proc);
    //        Application.Run();
    //        UnhookWindowsHookEx(_hookID);
    //    }

    //    private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    //    {
    //        if (nCode >= 0)
    //        {
    //            int vkCode = Marshal.ReadInt32(lParam);
    //            Keys key = (Keys)vkCode;

    //            // Vérification si la touche est dans le mappage
    //            if (keyMapping.ContainsKey(key))
    //            {
    //                bool isKeyDown = wParam == (IntPtr)WM_KEYDOWN;
    //                bool isKeyUp = wParam == (IntPtr)WM_KEYUP;

    //                if (isKeyDown || isKeyUp)
    //                {
    //                    // Simuler la touche mappée
    //                    SendKey(keyMapping[key], isKeyUp);
    //                    return (IntPtr)1; // Bloquer la touche originale
    //                }
    //            }
    //        }
    //        return CallNextHookEx(_hookID, nCode, wParam, lParam);
    //    }

    //    private static void SendKey(ushort virtualKey, bool keyUp)
    //    {
    //        var input = new INPUT
    //        {
    //            type = 1, // INPUT_KEYBOARD
    //            u = new InputUnion
    //            {
    //                ki = new KEYBDINPUT
    //                {
    //                    wVk = virtualKey,
    //                    wScan = 0,
    //                    dwFlags = keyUp ? (uint)0x0002 : (uint)0x0000, // KEYEVENTF_KEYUP : 0
    //                    time = 0,
    //                    dwExtraInfo = IntPtr.Zero
    //                }
    //            }
    //        };

    //        SendInput(1, new INPUT[] { input }, Marshal.SizeOf(typeof(INPUT)));
    //    }

    //    private static IntPtr SetHook(LowLevelKeyboardProc proc)
    //    {
    //        using (Process curProcess = Process.GetCurrentProcess())
    //        using (ProcessModule curModule = curProcess.MainModule)
    //        {
    //            return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
    //                GetModuleHandle(curModule.ModuleName), 0);
    //        }
    //    }

    //    private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

    //    [DllImport("user32.dll")]
    //    private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

    //    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    //    private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

    //    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    //    [return: MarshalAs(UnmanagedType.Bool)]
    //    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    //    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    //    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    //    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    //    private static extern IntPtr GetModuleHandle(string lpModuleName);
    //}


    //internal static class Program
    //{
    //    //[STAThread]
    //    //static void Main()
    //    //{
    //    //    ApplicationConfiguration.Initialize();
    //    //    Application.Run(new Form1());
    //    //}

    //    private const int WH_KEYBOARD_LL = 13;
    //    private const int WM_KEYDOWN = 0x0100;

    //    private static LowLevelKeyboardProc _proc = HookCallback;
    //    private static IntPtr _hookID = IntPtr.Zero;

    //    // Dictionnaire de remappage
    //    // à gauche la tocuhe sur la quelle on appuie, à droite la touche (qwerty) qui devrait être send
    //    private static Dictionary<Keys, string> keyMapping = new Dictionary<Keys, string>
    //    {
    //        //{ Keys.F, "w" },       // F → W
    //        //{ Keys.W, "z" },       // W → Z
    //        //{ Keys.A, "q" },       // A → Q
    //        { Keys.NumPad1, "u" },
    //        { Keys.NumPad2, "n" },
    //        { Keys.NumPad3, "h" },
    //        //{ Keys.Z, "w" },
    //        //{ Keys.Q, "a" },
    //        //{ Keys.C, "space" },
    //        //{ Keys.NumPad4, "1" },
    //        //{ Keys.NumPad5, "2" }
    //    };

    //    static void Main()
    //    {
    //        _hookID = SetHook(_proc);
    //        Application.Run();
    //        UnhookWindowsHookEx(_hookID);
    //    }

    //    private static IntPtr SetHook(LowLevelKeyboardProc proc)
    //    {
    //        using (Process curProcess = Process.GetCurrentProcess())
    //        using (ProcessModule curModule = curProcess.MainModule)
    //        {
    //            return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
    //                GetModuleHandle(curModule.ModuleName), 0);
    //        }
    //    }

    //    private delegate IntPtr LowLevelKeyboardProc(
    //        int nCode, IntPtr wParam, IntPtr lParam);

    //    private static IntPtr HookCallback(
    //        int nCode, IntPtr wParam, IntPtr lParam)
    //    {
    //        if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
    //        {
    //            int vkCode = Marshal.ReadInt32(lParam);
    //            Keys key = (Keys)vkCode;
    //            if (key != Keys.Tab)
    //            {
    //                // Vérifiez si la touche est dans le dictionnaire
    //                if (keyMapping.ContainsKey(key))
    //                {
    //                    string action = keyMapping[key];

    //                    // Exécutez l'action correspondante
    //                    PerformKeyAction(action);

    //                    return (IntPtr)1; // Bloquez la touche originale
    //                }
    //            }
    //        }
    //        return CallNextHookEx(_hookID, nCode, wParam, lParam);
    //    }

    //    private static void PerformKeyAction(string action)
    //    {
    //        // Décomposez l'action pour supporter les modificateurs
    //        if (action.Contains("+"))
    //        {
    //            string[] keys = action.Split('+');
    //            foreach (string k in keys)
    //            {
    //                SendKeys.SendWait($"^{k.Trim()}"); // Simule Ctrl+Key
    //            }
    //        }
    //        else
    //        {
    //            SendKeys.Send(action); // Touche simple ou séquence
    //        }
    //    }

    //    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    //    private static extern IntPtr SetWindowsHookEx(int idHook,
    //        LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

    //    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    //    [return: MarshalAs(UnmanagedType.Bool)]
    //    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    //    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    //    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
    //        IntPtr wParam, IntPtr lParam);

    //    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    //    private static extern IntPtr GetModuleHandle(string lpModuleName);
    //}

}