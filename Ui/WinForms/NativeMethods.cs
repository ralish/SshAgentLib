// @formatter:off
// ReSharper disable InconsistentNaming

#pragma warning disable CS0649  // Field is never assigned to

using System;
using System.Runtime.InteropServices;
using System.Text;

namespace dlech.SshAgentLib.WinForms
{
  internal static class NativeMethods
  {
    #region Dialog Boxes

    [DllImport("User32", SetLastError = true)]
    internal static extern int GetDlgCtrlID(IntPtr hWnd);

    [DllImport("User32", SetLastError = true)]
    internal static extern IntPtr GetDlgItem(IntPtr hDlg, int nIDDlgItem);

    [DllImport("User32", CharSet = CharSet.Unicode, EntryPoint = "SetDlgItemTextW", SetLastError = true)]
    internal static extern bool SetDlgItemText(IntPtr hDlg, int nIDDlgItem, string lpString);

    #endregion

    #region Keyboard and Mouse Input

    [DllImport("User32")]
    internal static extern bool EnableWindow(IntPtr hWnd, bool bEnable);

    #endregion

    #region Registry

    internal const int HKEY_CURRENT_USER = 0x7FFFFFFF;

    [DllImport("Advapi32")]
    internal static extern int RegCloseKey(IntPtr hKey);

    [DllImport("Advapi32", CharSet = CharSet.Unicode, EntryPoint = "RegCreateKeyW")]
    internal static extern int RegCreateKey(IntPtr hKey, string lpSubKey, out IntPtr phkResult);

    [DllImport("Advapi32")]
    internal static extern int RegOverridePredefKey(IntPtr hKey, IntPtr hNewHKey);

    #endregion

    #region Windows and Messages

    internal delegate bool EnumChildProc(IntPtr hWnd, IntPtr lParam);
    internal delegate IntPtr WndProc(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam);

    [DllImport("User32", CharSet = CharSet.Unicode, EntryPoint = "CallWindowProcW")]
    internal static extern IntPtr CallWindowProc(WndProc lpPrevWndFunc,
                                                 IntPtr hWnd,
                                                 uint Msg,
                                                 IntPtr wParam,
                                                 IntPtr lParam);

    [DllImport("User32", CharSet = CharSet.Unicode, EntryPoint = "CreateWindowExW", SetLastError = true)]
    internal static extern IntPtr CreateWindowEx(uint dwExStyle,
                                                 string lpClassName,
                                                 string lpWindowName,
                                                 uint dwStyle,
                                                 int X,
                                                 int Y,
                                                 int nWidth,
                                                 int nHeight,
                                                 IntPtr hWndParent,
                                                 IntPtr hMenu,
                                                 IntPtr hInstance,
                                                 IntPtr lpParam);

    [DllImport("User32", SetLastError = true)]
    internal static extern bool DestroyWindow(IntPtr hWnd);

    [DllImport("User32")]
    internal static extern bool EnumChildWindows(HandleRef hWndParent, EnumChildProc lpEnumFunc, IntPtr lParam);

    [DllImport("User32", CharSet = CharSet.Unicode, EntryPoint = "FindWindowExW", SetLastError = true)]
    internal static extern IntPtr FindWindowEx(IntPtr hWndParent, IntPtr hWndChildAfter, string lpszClass, IntPtr lpszWindow);

    [DllImport("User32", CharSet = CharSet.Unicode, EntryPoint = "FindWindowExW", SetLastError = true)]
    internal static extern IntPtr FindWindowEx(IntPtr hWndParent, IntPtr hWndChildAfter, string lpszClass, string lpszWindow);

    [DllImport("User32", CharSet = CharSet.Unicode, EntryPoint = "GetClassNameW", SetLastError = true)]
    internal static extern int GetClassName(HandleRef hWnd, StringBuilder param, int nMaxCount);

    [DllImport("User32", SetLastError = true)]
    internal static extern bool GetClientRect(HandleRef hWnd, out RECT lpRect);

    [DllImport("User32", SetLastError = true)]
    internal static extern IntPtr GetParent(IntPtr hWnd);

    [DllImport("User32", SetLastError = true)]
    internal static extern bool GetWindowInfo(HandleRef hWnd, out WINDOWINFO pWi);

    [DllImport("User32", SetLastError = true)]
    internal static extern bool GetWindowRect(HandleRef hWnd, out RECT lpRect);

    [DllImport("User32")]
    internal static extern bool IsWindow(IntPtr hWnd);

    [DllImport("User32", CharSet = CharSet.Unicode, EntryPoint = "SendMessageW", SetLastError = true)]
    internal static extern IntPtr SendMessage(HandleRef hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

    [DllImport("User32", CharSet = CharSet.Unicode, EntryPoint = "SendMessageW", SetLastError = true)]
    internal static extern IntPtr SendMessage(HandleRef hWnd, uint Msg, IntPtr wParam, StringBuilder lParam);

    [DllImport("User32", SetLastError = true)]
    internal static extern IntPtr SetParent(HandleRef hWndChild, HandleRef hWndNewParent);

    [DllImport("User32", CharSet = CharSet.Unicode, EntryPoint = "SetWindowLongW", SetLastError = true)]
    internal static extern int SetWindowLong(IntPtr hWnd, WINDOW_LONG_INDEX nIndex, int dwNewLong);

    [DllImport("User32", CharSet = CharSet.Unicode, EntryPoint = "SetWindowLongPtrW", SetLastError = true)]
    internal static extern long SetWindowLongPtr(IntPtr hWnd, WINDOW_LONG_INDEX nIndex, long dwNewLong);

    [DllImport("User32", SetLastError = true)]
    internal static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, WINDOW_POS_FLAGS uFlags);

    [DllImport("User32", CharSet = CharSet.Unicode, EntryPoint = "SetWindowTextW", SetLastError = true)]
    internal static extern bool SetWindowText(HandleRef hWnd, string lpString);

    // DWLP_DLGPROC & DWLP_USER excluded as they vary by pointer size
    internal enum WINDOW_LONG_INDEX
    {
      GWL_USERDATA    = -21,
      GWL_EXSTYLE     = -20,
      GWL_STYLE       = -16,
      GWL_ID          = -12,
      GWL_HINSTANCE   = -6,
      GWL_WNDPROC     = -4,
      DWLP_MSGRESULT  = 0
    }

    [Flags]
    internal enum WINDOW_POS_FLAGS : uint
    {
      SWP_NOSIZE          = 0x1,
      SWP_NOMOVE          = 0x2,
      SWP_NOZORDER        = 0x4,
      SWP_NOREDRAW        = 0x8,
      SWP_NOACTIVATE      = 0x10,
      SWP_DRAWFRAME       = 0x20,
      SWP_FRAMECHANGED    = 0x20,
      SWP_SHOWWINDOW      = 0x40,
      SWP_HIDEWINDOW      = 0x80,
      SWP_NOCOPYBITS      = 0x100,
      SWP_NOOWNERZORDER   = 0x200,
      SWP_NOREPOSITION    = 0x200,
      SWP_NOSENDCHANGING  = 0x400,
      SWP_DEFERERASE      = 0x2000,
      SWP_ASYNCWINDOWPOS  = 0x4000
    }

    #endregion





    internal struct HELPINFO
    {
      public uint   cbSize;
      public int    iContextType;
      public int    iCtrlId;
      public IntPtr hItemHandle;
      public IntPtr dwContextId;
      public POINT  MousePos;
    }

    internal struct POINT
    {
      public int  X;
      public int  Y;

      public POINT(int X, int Y)
      {
        this.X = X;
        this.Y = Y;
      }

      public POINT(POINT point)
      {
        X = point.X;
        Y = point.Y;
      }
    }

    internal struct RECT
    {
      public int  left;
      public int  top;
      public int  right;
      public int  bottom;

      public POINT Location
      {
        get { return new POINT(left, top); }

        set
        {
          left     = value.X;
          top      = value.Y;
          right   -= (left - value.X);
          bottom  -= (bottom - value.Y);
        }
      }

      internal uint Height
      {
        get { return (uint)Math.Abs(bottom - top); }
        set { bottom = top + (int)value; }
      }

      internal uint Width
      {
        get { return (uint)Math.Abs(right - left); }
        set { right = left + (int)value; }
      }

      public override string ToString()
      {
        return $"{left} : {top} : {right} : {bottom}";
      }
    }

    internal struct WINDOWINFO
    {
      public uint   cbSize;
      public RECT   rcWindow;
      public RECT   rcClient;
      public uint   dwStyle;
      public uint   dwExStyle;
      public uint   dwWindowStatus;
      public uint   cxWindowBorders;
      public uint   cyWindowBorders;
      public ushort atomWindowType;
      public ushort wCreatorVersion;
    }
  }
}
