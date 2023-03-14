using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ScreenShotter {
    class InterceptKeys {
        const int WH_KEYBOARD_LL = 13;
        const int WM_KEYDOWN = 0x0100;
        static LowLevelKeyboardProc _proc = HookCallback;
        static IntPtr _hookID = IntPtr.Zero;

        public static void Main() {
            _hookID = SetHook(_proc);
            Application.Run();
            UnhookWindowsHookEx(_hookID);
        }

        static IntPtr SetHook(LowLevelKeyboardProc proc) {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule) return SetWindowsHookEx(WH_KEYBOARD_LL, proc,  GetModuleHandle(curModule.ModuleName), 0);
        }

        delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
        static Keys KeyLast=Keys.None;

        static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam) {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN) {
                Keys vkCode = (Keys)Marshal.ReadInt32(lParam);
                
                if (vkCode==Keys.PrintScreen) { 
                    Rectangle bounds = Screen.GetBounds(Point.Empty);
                    Bitmap bitmap = new Bitmap(bounds.Width, bounds.Height);
                    using (Graphics g = Graphics.FromImage(bitmap)) g.CopyFromScreen(Point.Empty, Point.Empty, bounds.Size);
                    
                    DateTime now=DateTime.Now;
                    string username=System.Security.Principal.WindowsIdentity.GetCurrent().Name;
                    bitmap.Save(@"C:\Users\"+username+@"\Screenshots\Screenshot "+now.Year+" "+now.Month+"."+now.Day+". "+now.Hour+"-"+now.Minute+"-"+now.Second+"-"+(int)(now.Millisecond/1000f*16)+".png", ImageFormat.Png); 
                }
                if (KeyLast==Keys.F1 && vkCode==Keys.F12) Application.Exit();
                KeyLast=vkCode;
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern IntPtr GetModuleHandle(string lpModuleName);
    }
}