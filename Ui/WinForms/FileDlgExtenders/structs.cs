//  Copyright (c) 2006, Gustavo Franco
//  Copyright © Decebal Mihailescu 2007-2010

//  Email:  gustavo_franco@hotmail.com
//  All rights reserved.

//  Redistribution and use in source and binary forms, with or without modification, 
//  are permitted provided that the following conditions are met:

//  Redistributions of source code must retain the above copyright notice, 
//  this list of conditions and the following disclaimer. 
//  Redistributions in binary form must reproduce the above copyright notice, 
//  this list of conditions and the following disclaimer in the documentation 
//  and/or other materials provided with the distribution. 

//  THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
//  KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
//  IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR
//  PURPOSE. IT CAN BE DISTRIBUTED FREE OF CHARGE AS LONG AS THIS HEADER 
//  REMAINS UNCHANGED.

using System;
using System.Text;
using System.Drawing;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using FileDialogExtenders;

namespace Win32Types
{
    #region WINDOWPOS

    [StructLayout(LayoutKind.Sequential)]
	internal struct WINDOWPOS
	{
		public IntPtr   hwnd;
		public IntPtr   hwndAfter;
		public int      x;
		public int      y;
		public int      cx;
		public int      cy;
		public uint     flags;

        #region Overrides
        public override string ToString()
        {
            return x + ":" + y + ":" + cx + ":" + cy + ":" + ((SWP_Flags) flags).ToString();
        }
        #endregion
    }
    #endregion

    #region NMHDR
    
    [StructLayout(LayoutKind.Sequential)]
    internal struct NMHDR 
    {
        public IntPtr  hwndFrom;
        public IntPtr  idFrom;
        public uint    code;
    } 
    #endregion
    #region NMHEADER
    [StructLayout(LayoutKind.Sequential)]
    internal struct NMHEADER
    {
        internal NMHDR hdr;
        internal int iItem;
        internal int iButton;
        internal IntPtr pItem;
    } 
    #endregion
    #region OPENFILENAME

    /// <summary>
    /// Defines the shape of hook procedures that can be called by the OpenFileDialog
    /// </summary>
    internal delegate IntPtr OfnHookProc(IntPtr hWnd, UInt16 msg, Int32 wParam, Int32 lParam);
    /// <summary>
    /// See the documentation for OPENFILENAME
    /// </summary>
    //typedef struct tagOFN { 
    //  DWORD         lStructSize; 
    //  HWND          hwndOwner; 
    //  HINSTANCE     hInstance; 
    //  LPCTSTR       lpstrFilter; 
    //  LPTSTR        lpstrCustomFilter; 
    //  DWORD         nMaxCustFilter; 
    //  DWORD         nFilterIndex; 
    //  LPTSTR        lpstrFile; 
    //  DWORD         nMaxFile; 
    //  LPTSTR        lpstrFileTitle; 
    //  DWORD         nMaxFileTitle; 
    //  LPCTSTR       lpstrInitialDir; 
    //  LPCTSTR       lpstrTitle; 
    //  DWORD         Flags; 
    //  WORD          nFileOffset; 
    //  WORD          nFileExtension; 
    //  LPCTSTR       lpstrDefExt; 
    //  LPARAM        lCustData; 
    //  LPOFNHOOKPROC lpfnHook; 
    //  LPCTSTR       lpTemplateName; 
    //#if (_WIN32_WINNT >= 0x0500)
    //  void *        pvReserved;
    //  DWORD         dwReserved;
    //  DWORD         FlagsEx;
    //#endif // (_WIN32_WINNT >= 0x0500)
    //} OPENFILENAME, *LPOPENFILENAME;
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto )]
    internal struct OPENFILENAME
    {
        public UInt32 lStructSize;
        public IntPtr hwndOwner;
        public IntPtr hInstance;
        public String lpstrFilter;
        public String lpstrCustomFilter;
        public UInt32 nMaxCustFilter;
        public Int32 nFilterIndex;
        public String lpstrFile;
        public UInt32 nMaxFile;
        public String lpstrFileTitle;
        public UInt32 nMaxFileTitle;
        public String lpstrInitialDir;
        public String lpstrTitle;
        public UInt32 Flags;
        public UInt16 nFileOffset;
        public UInt16 nFileExtension;
        public String lpstrDefExt;
        public IntPtr lCustData;
        public OfnHookProc lpfnHook;
        public String lpTemplateName;
        public IntPtr pvReserved;
        public UInt32 dwReserved;
        public UInt32 FlagsEx;
    };
    #endregion
    #region OFNOTIFY

    [StructLayout(LayoutKind.Sequential)]
    internal struct OFNOTIFY 
    {
        public NMHDR    hdr;
        public IntPtr OpenFileName;
        public IntPtr   fileNameShareViolation;
    }
    #endregion
}
