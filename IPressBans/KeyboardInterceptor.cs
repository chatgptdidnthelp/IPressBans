using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;

namespace IPressBans
{
    public static class KeyboardInterceptor
    {
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;

        private static LowLevelKeyboardProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;

        // Track active profiles
        private static HashSet<int> _activeProfiles = new();

        private static Func<string>? _getText1;
        private static Func<string>? _getText2;
        private static Func<string>? _getText3;

        public static void SetTextSources(Func<string> text1, Func<string> text2, Func<string> text3)
        {
            _getText1 = text1;
            _getText2 = text2;
            _getText3 = text3;
        }

        public static void Start()
        {
            _hookID = SetHook(_proc);
        }

        public static void Stop()
        {
            UnhookWindowsHookEx(_hookID);
        }

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                Key key = KeyInterop.KeyFromVirtualKey(vkCode);

                if (key == Key.F1)
                {
                    ToggleProfile(1);
                    return (IntPtr)1;
                }
                if (key == Key.F2)
                {
                    ToggleProfile(2);
                    return (IntPtr)1;
                }
                if (key == Key.F3)
                {
                    ToggleProfile(3);
                    return (IntPtr)1;
                }

                // Combine all active blocked letters
                var combined = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                if (_activeProfiles.Contains(1) && _getText1 != null)
                    combined.UnionWith(_getText1().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries));

                if (_activeProfiles.Contains(2) && _getText2 != null)
                    combined.UnionWith(_getText2().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries));

                if (_activeProfiles.Contains(3) && _getText3 != null)
                    combined.UnionWith(_getText3().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries));

                if (key >= Key.A && key <= Key.Z && combined.Contains(key.ToString(), StringComparer.OrdinalIgnoreCase))
                {
                    return (IntPtr)1; // Block the key
                }
            }

            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        public static event Action<HashSet<int>>? ProfilesChanged;

        private static void ToggleProfile(int profileNumber)
        {
            if (_activeProfiles.Contains(profileNumber))
            {
                _activeProfiles.Remove(profileNumber);
            }
            else
            {
                _activeProfiles.Add(profileNumber);
            }

            ProfilesChanged?.Invoke(new HashSet<int>(_activeProfiles));
        }


        #region WinAPI

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook,
            LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk,
            int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using var curProcess = System.Diagnostics.Process.GetCurrentProcess();
            using var curModule = curProcess.MainModule!;
            return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                GetModuleHandle(curModule.ModuleName), 0);
        }

        #endregion
    }
}
