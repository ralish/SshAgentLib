//
// WinInternals.cs
//
// Copyright (c) 2015 David Lechner
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;

using static dlech.SshAgentLib.NativeMethods;


namespace dlech.SshAgentLib
{
    public static class WinInternals
    {
        /// <summary>
        /// Searches all current TCP connections (IPv4 only) for the matching
        /// port (local port of the connection).
        /// </summary>
        /// <param name="port">The TCP port to look for.</param>
        /// <returns>The process that owns this connection.</returns>
        public static Process GetProcessForTcpPort(IPEndPoint localEndpoint, IPEndPoint remoteEndpoint)
        {
            if (localEndpoint == null) {
                throw new ArgumentNullException("localEndpoint");
            }
            if (remoteEndpoint == null) {
                throw new ArgumentNullException("remoteEndpoint");
            }
            if (localEndpoint.AddressFamily != AddressFamily.InterNetwork) {
                throw new ArgumentException("Must be IPv4 address.", "localEndpoint");
            }
            if (remoteEndpoint.AddressFamily != AddressFamily.InterNetwork) {
                throw new ArgumentException("Must be IPv4 address.", "remoteEndpoint");
            }

            // The MIB_TCPROW_OWNER_PID struct stores address as integers in
            // network byte order, so we fixup the address to match.
            var localAddressBytes = localEndpoint.Address.GetAddressBytes();
            var localAddress = localAddressBytes[0] + (localAddressBytes[1] << 8)
                + (localAddressBytes[2] << 16) + (localAddressBytes[3] << 24);
            var remoteAddressBytes = localEndpoint.Address.GetAddressBytes();
            var remoteAddress = remoteAddressBytes[0] + (remoteAddressBytes[1] << 8)
                + (remoteAddressBytes[2] << 16) + (remoteAddressBytes[3] << 24);

            // The MIB_TCPROW_OWNER_PID struct stores ports in network byte
            // order, so we have to swap the port to match.
            var localPort = (ushort)IPAddress.HostToNetworkOrder((short)localEndpoint.Port);
            var remotePort = (ushort)IPAddress.HostToNetworkOrder((short)remoteEndpoint.Port);

            // first find out the size needed to get the data

            var buf = IntPtr.Zero;
            var bufSize = 0U;
            var result = GetExtendedTcpTable(buf, ref bufSize, false, AF_INET,
                TCP_TABLE_CLASS.TCP_TABLE_OWNER_PID_CONNECTIONS);
            if (result != ERROR_INSUFFICIENT_BUFFER) {
                throw new Exception(string.Format("Error: {0}", result));
            }

            // then alloc some memory so we can acutally get the data
            buf = Marshal.AllocHGlobal((int)bufSize);
            try {
                result = GetExtendedTcpTable(buf, ref bufSize, false, AF_INET,
                    TCP_TABLE_CLASS.TCP_TABLE_OWNER_PID_CONNECTIONS);
                if (result != NO_ERROR) {
                    throw new Exception(string.Format("Error: {0}", result));
                }
                var count = Marshal.ReadInt32(buf);
                var tablePtr = buf + sizeof(int);
                var rowSize = Marshal.SizeOf(typeof(MIB_TCPROW_OWNER_PID));
                var match = (MIB_TCPROW_OWNER_PID?)null;
                for (int i = 0; i < count; i++) {
                    var row = (MIB_TCPROW_OWNER_PID)Marshal.PtrToStructure(
                        tablePtr, typeof(MIB_TCPROW_OWNER_PID));
                    if (localAddress == row.dwLocalAddr && localPort == row.dwLocalPort
                        && remoteAddress == row.dwRemoteAddr && remotePort == row.dwRemotePort)
                    {
                        match = row;
                        break;
                    }
                    tablePtr += rowSize;
                }
                if (!match.HasValue) {
                    throw new Exception("Match not found.");
                }
                return Process.GetProcessById((int)match.Value.dwOwningPid);;
            } finally {
                Marshal.FreeHGlobal(buf);
            }
        }

        /// <summary>
        /// Iterates all open handles on the system (using internal system call)
        /// to find the other process that has a handle open to the same memtory
        /// mapped file.
        /// </summary>
        /// <remarks>
        /// Code based on http://forum.sysinternals.com/overview-handle-enumeration_topic14546.html
        /// </remarks>
        public static Process FindProcessWithMatchingHandle(MemoryMappedFile mmf)
        {
            // hopefully this is enough room (16MiB)
            const int sysInfoPtrLength = 4096 * 4096;
            var sysInfoPtr = Marshal.AllocHGlobal(sysInfoPtrLength);
            try {
                uint resultLength;
                var result = NtQuerySystemInformation(
                    SYSTEM_INFORMATION_CLASS.SystemHandleInformation,
                    sysInfoPtr, sysInfoPtrLength, out resultLength);
                if (result != 0) {
                    throw new Exception(result.ToString());
                }
                var info = (SYSTEM_HANDLE_INFORMATION)Marshal.PtrToStructure(
                    sysInfoPtr, typeof(SYSTEM_HANDLE_INFORMATION));

                // set entryPtr to position of info.Handle
                var entryPtr = sysInfoPtr
                    + Marshal.SizeOf(typeof(SYSTEM_HANDLE_INFORMATION))
                    - IntPtr.Size;
                var entries = new List<SYSTEM_HANDLE_ENTRY>();
                // we loop through a large number (10s of thousands) of handles,
                // so dereferencig everything first should improve perfomarnce
                var entryLength = Marshal.SizeOf(typeof(SYSTEM_HANDLE_ENTRY));
                var pid = Process.GetCurrentProcess().Id;
                var handle = (ushort)mmf.SafeMemoryMappedFileHandle.DangerousGetHandle();
                var match = IntPtr.Zero;
                for (int i = 0; i < info.Count; i++) {
                    var entry = (SYSTEM_HANDLE_ENTRY)Marshal.PtrToStructure(
                        entryPtr, typeof(SYSTEM_HANDLE_ENTRY));
                    // search for a handle for this process that matches the
                    // memory mapped file.
                    if (entry.OwnerPid == pid && entry.HandleValue == handle) {
                        match = entry.ObjectPointer;
                    } else {
                        // Save all other entries excpt for the match to a list
                        // so we can search it again later
                        entries.Add(entry);
                    }
                    entryPtr += entryLength;
                }
                if (match == IntPtr.Zero) {
                    throw new Exception("Match not found.");
                }

                var otherHandle = entries.Single(e => e.ObjectPointer == match);
                return Process.GetProcessById((int)otherHandle.OwnerPid);
            } finally {
                Marshal.FreeHGlobal(sysInfoPtr);
            }
        }
    }
}
