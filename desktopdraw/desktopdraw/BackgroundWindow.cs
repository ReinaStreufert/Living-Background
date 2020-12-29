using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace desktopdraw
{
    public static class BackgroundWindow
    {
        public static IntPtr ObtainBackgroundHandle()
        {
            IntPtr progman = WinApi.FindWindow("Progman", null);

            IntPtr result = IntPtr.Zero;
            WinApi.SendMessageTimeout(progman,
                       0x052C,
                       new IntPtr(0),
                       IntPtr.Zero,
                       WinApi.SendMessageTimeoutFlags.SMTO_NORMAL,
                       1000,
                       out result);

            IntPtr workerw = IntPtr.Zero;

            WinApi.EnumWindows(new WinApi.EnumWindowsProc((tophandle, topparamhandle) =>
            {
                IntPtr p = WinApi.FindWindowEx(tophandle, IntPtr.Zero, "SHELLDLL_DefView", null);

                if (p != IntPtr.Zero)
                {
                    workerw = WinApi.FindWindowEx(IntPtr.Zero, tophandle, "WorkerW", null);
                }

                return true;
            }), IntPtr.Zero);

            return workerw;
        }
        private static class WinApi
        {
            [DllImport("user32.dll", SetLastError = true)]
            public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

            [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
            public static extern IntPtr SendMessageTimeout(
                IntPtr hWnd,
                uint Msg,
                UIntPtr wParam,
                IntPtr lParam,
                SendMessageTimeoutFlags fuFlags,
                uint uTimeout,
                out UIntPtr lpdwResult);

            [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
            public static extern IntPtr SendMessageTimeout(
                IntPtr windowHandle,
                uint Msg,
                IntPtr wParam,
                IntPtr lParam,
                SendMessageTimeoutFlags flags,
                uint timeout,
                out IntPtr result);

            /* Version specifically setup for use with WM_GETTEXT message */

            [DllImport("user32.dll", EntryPoint = "SendMessageTimeout", SetLastError = true, CharSet = CharSet.Auto)]
            public static extern uint SendMessageTimeoutText(
                IntPtr hWnd,
                int Msg,              // Use WM_GETTEXT
                int countOfChars,
                StringBuilder text,
                SendMessageTimeoutFlags flags,
                uint uTImeoutj,
                out IntPtr result);

            [Flags]
            public enum SendMessageTimeoutFlags : uint
            {
                SMTO_NORMAL = 0x0,
                SMTO_BLOCK = 0x1,
                SMTO_ABORTIFHUNG = 0x2,
                SMTO_NOTIMEOUTIFNOTHUNG = 0x8,
                SMTO_ERRORONEXIT = 0x20
            }

            [DllImport("user32.dll", SetLastError = true)]
            public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

            [DllImport("user32.dll")]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

            public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
        }
    }
}
