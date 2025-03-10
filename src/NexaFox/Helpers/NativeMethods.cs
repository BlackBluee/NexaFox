using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NexaFox.Helpers
{
    public static class NativeMethods
    {
        public const int GWL_STYLE = -16;
        public const int WS_POPUP = unchecked((int)0x80000000);

        [DllImport("user32.dll")]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
    }
}
