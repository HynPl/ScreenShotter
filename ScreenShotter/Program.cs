using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ScreenShotter {
    static class Program {
        // Using key
        const Keys KeyMain = Keys.PrintScreen;

        // Exit key
        const Keys KeyExit = Keys.F1;
       
        const int WH_KEYBOARD_LL = 13, WM_KEYDOWN = 0x0100;
        static LowLevelKeyboardProc _proc = HookCallback;
        static IntPtr _hookID = IntPtr.Zero;

        [STAThread]
        static void Main() {   
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
              //  Debug.WriteLine(vkCode);
                if (vkCode==KeyMain) { 
                    string username=Environment.UserName;
                    string pthDir=@"C:\Users\"+Environment.UserName+@"\Screenshots\Pictures";
                    if (!System.IO.Directory.Exists(pthDir)) System.IO.Directory.CreateDirectory(pthDir);
                    Debug.WriteLine(pthDir);

                    Rectangle bounds = Screen.GetBounds(Point.Empty);
                    Bitmap bitmap = new Bitmap(bounds.Width, bounds.Height);
                    using (Graphics g = Graphics.FromImage(bitmap)) g.CopyFromScreen(Point.Empty, Point.Empty, bounds.Size);
                    
                    DateTime now=DateTime.Now;

                    bitmap.Save(@"C:\Users\"+username+@"\Pictures\Screenshots\Screenshot "+now.Year+" "+now.Month+"."+now.Day+". "+now.Hour+"-"+now.Minute+"-"+now.Second+"-"+(int)(now.Millisecond/1000f*16)+".png", ImageFormat.Png); 
                }
                if (KeyLast==Keys.F1 && vkCode==KeyExit) Application.Exit();
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