// @formatter:off
// ReSharper disable InconsistentNaming

#pragma warning disable CS0649  // Field is never assigned to

using System;
using System.Runtime.InteropServices;


namespace dlech.SshAgentLib
{
  internal static class NativeMethods
  {
    #region Error codes

    internal const uint NO_ERROR = 0;
    internal const uint ERROR_INSUFFICIENT_BUFFER = 122;

    #endregion

    #region IP Helper

    internal const uint AF_INET = 2;
    internal const uint AF_INET6 = 23;

    [DllImport("Iphlpapi")]
    internal static extern uint GetExtendedTcpTable(IntPtr pTcpTable,
                                                    ref uint pdwSize,
                                                    bool bOrder,
                                                    uint ulAf,
                                                    TCP_TABLE_CLASS TableClass,
                                                    uint Reserved = 0);

    internal enum TCP_TABLE_CLASS
    {
      TCP_TABLE_BASIC_LISTENER,
      TCP_TABLE_BASIC_CONNECTIONS,
      TCP_TABLE_BASIC_ALL,
      TCP_TABLE_OWNER_PID_LISTENER,
      TCP_TABLE_OWNER_PID_CONNECTIONS,
      TCP_TABLE_OWNER_PID_ALL,
      TCP_TABLE_OWNER_MODULE_LISTENER,
      TCP_TABLE_OWNER_MODULE_CONNECTIONS,
      TCP_TABLE_OWNER_MODULE_ALL
    }

    #endregion

    #region Management Information Base

    internal enum MIB_TCP_STATE : uint
    {
      MIB_TCP_STATE_CLOSED      = 1,
      MIB_TCP_STATE_LISTEN      = 2,
      MIB_TCP_STATE_SYN_SENT    = 3,
      MIB_TCP_STATE_SYN_RCVD    = 4,
      MIB_TCP_STATE_ESTAB       = 5,
      MIB_TCP_STATE_FIN_WAIT1   = 6,
      MIB_TCP_STATE_FIN_WAIT2   = 7,
      MIB_TCP_STATE_CLOSE_WAIT  = 8,
      MIB_TCP_STATE_CLOSING     = 9,
      MIB_TCP_STATE_LAST_ACK    = 10,
      MIB_TCP_STATE_TIME_WAIT   = 11,
      MIB_TCP_STATE_DELETE_TCB  = 12,
    }

    internal struct MIB_TCPROW_OWNER_PID
    {
      public MIB_TCP_STATE  dwState;
      public uint           dwLocalAddr;
      public uint           dwLocalPort;
      public uint           dwRemoteAddr;
      public uint           dwRemotePort;
      public uint           dwOwningPid;
    }

    #endregion

    #region Native API

    [DllImport("Ntdll")]
    internal static extern NTSTATUS NtQuerySystemInformation(SYSTEM_INFORMATION_CLASS SystemInformationClass,
                                                             IntPtr SystemInformation,
                                                             uint SystemInformationLength,
                                                             out uint ReturnLength);

    // A tiny subset of the possible status codes, but these are the
    // only ones we expect to plausibly encounter in our use case.
    internal enum NTSTATUS : uint
    {
      STATUS_SUCCESS              = 0x00000000,
      STATUS_INVALID_INFO_CLASS   = 0xC0000003,
      STATUS_INFO_LENGTH_MISMATCH = 0xC0000004,
      STATUS_ACCESS_VIOLATION     = 0xC0000005,
      STATUS_ACCESS_DENIED        = 0xC0000022,
      STATUS_BUFFER_TOO_SMALL     = 0xC0000023,
    }

    // Only referencing classes we actually use
    internal enum SYSTEM_INFORMATION_CLASS : uint
    {
      SystemHandleInformation = 16
    }

    internal struct SYSTEM_HANDLE_ENTRY {
      public uint   OwnerPid;
      public byte   ObjectType;
      public byte   HandleFlags;
      public ushort HandleValue;
      public IntPtr ObjectPointer;
      public uint   AccessMask;
    }

    internal struct SYSTEM_HANDLE_INFORMATION {
      public uint   Count;
      public IntPtr Handle; // TODO: Actually a SYSTEM_HANDLE_ENTRY array
    }

    #endregion

    #region Security and Identity

    [DllImport("Advapi32")]
    internal static extern uint GetSecurityInfo(IntPtr handle,
                                                SE_OBJECT_TYPE ObjectType,
                                                SECURITY_INFORMATION SecurityInfo,
                                                out IntPtr ppsidOwner,
                                                out IntPtr ppsidGroup,
                                                out IntPtr ppDacl,
                                                out IntPtr ppSacl,
                                                out IntPtr ppSecurityDescriptor);

    [Flags]
    internal enum ACCESS_MASK : uint
    {
      DELETE                  = 0x10000,
      READ_CONTROL            = 0x20000,
      WRITE_DAC               = 0x40000,
      WRITE_OWNER             = 0x80000,
      SYNCHRONIZE             = 0x100000,

      ACCESS_SYSTEM_SECURITY  = 0x1000000,
      MAXIMUM_ALLOWED         = 0x2000000,
      GENERIC_ALL             = 0x10000000,
      GENERIC_EXECUTE         = 0x20000000,
      GENERIC_WRITE           = 0x40000000,
      GENERIC_READ            = 0x80000000
    }

    internal enum SE_OBJECT_TYPE
    {
      SE_UNKNOWN_OBJECT_TYPE,
      SE_FILE_OBJECT,
      SE_SERVICE,
      SE_PRINTER,
      SE_REGISTRY_KEY,
      SE_LMSHARE,
      SE_KERNEL_OBJECT,
      SE_WINDOW_OBJECT,
      SE_DS_OBJECT,
      SE_DS_OBJECT_ALL,
      SE_PROVIDER_DEFINED_OBJECT,
      SE_WMIGUID_OBJECT,
      SE_REGISTRY_WOW64_32KEY,
      SE_REGISTRY_WOW64_64KEY
    }

    [Flags]
    internal enum SECURITY_INFORMATION : uint
    {
      OWNER_SECURITY_INFORMATION                = 0x1,
      GROUP_SECURITY_INFORMATION                = 0x2,
      DACL_SECURITY_INFORMATION                 = 0x4,
      SACL_SECURITY_INFORMATION                 = 0x8,
      LABEL_SECURITY_INFORMATION                = 0x10,
      ATTRIBUTE_SECURITY_INFORMATION            = 0x20,
      SCOPE_SECURITY_INFORMATION                = 0x40,
      PROCESS_TRUST_LABEL_SECURITY_INFORMATION  = 0x80,
      ACCESS_FILTER_SECURITY_INFORMATION        = 0x100,

      BACKUP_SECURITY_INFORMATION               = 0x10000,

      UNPROTECTED_SACL_SECURITY_INFORMATION     = 0x10000000,
      UNPROTECTED_DACL_SECURITY_INFORMATION     = 0x20000000,
      PROTECTED_SACL_SECURITY_INFORMATION       = 0x40000000,
      PROTECTED_DACL_SECURITY_INFORMATION       = 0x80000000
    }

    #endregion

    #region System Services

    [DllImport("Kernel32", SetLastError = true)]
    internal static extern bool CloseHandle(IntPtr hObject);

    [DllImport("Kernel32", SetLastError = true)]
    internal static extern bool GetNamedPipeClientProcessId(IntPtr Pipe, out uint ClientProcessId);

    [DllImport("Kernel32", SetLastError = true)]
    internal static extern IntPtr OpenProcess(ACCESS_MASK dwDesiredAccess, bool bInheritHandle, uint dwProcessId);

    #endregion

    #region Windows and Messages

    internal delegate IntPtr WndProc(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam);

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

    [DllImport("User32", CharSet = CharSet.Unicode, EntryPoint = "DefWindowProcW")]
    internal static extern IntPtr DefWindowProc(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

    [DllImport("User32", SetLastError = true)]
    internal static extern bool DestroyWindow(IntPtr hWnd);

    [DllImport("User32", CharSet = CharSet.Unicode, EntryPoint = "FindWindowW", SetLastError = true)]
    internal static extern IntPtr FindWindow(string sClassName, string sAppName);

    [DllImport("User32", CharSet = CharSet.Unicode, EntryPoint = "RegisterClassW", SetLastError = true)]
    internal static extern ushort RegisterClass(ref WNDCLASS lpWndClass);

    [DllImport("User32", CharSet = CharSet.Unicode, EntryPoint = "SendMessageW", SetLastError = true)]
    internal static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

    internal struct COPYDATASTRUCT
    {
      public IntPtr dwData;
      public uint   cbData;
      public IntPtr lpData;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct WNDCLASS
    {
      public uint     style;
      public WndProc  lpfnWndProc;
      public int      cbClsExtra;
      public int      cbWndExtra;
      public IntPtr   hInstance;
      public IntPtr   hIcon;
      public IntPtr   hCursor;
      public IntPtr   hbrBackground;
      public string   lpszMenuName;
      public string   lpszClassName;
    }

    #endregion
  }
}
