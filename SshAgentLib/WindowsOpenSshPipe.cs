//
// WindowsOpenSshPipe.cs
//
// Author(s): David Lechner <david@lechnology.com>
//
// Copyright (c) 2017 David Lechner
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
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Runtime.InteropServices;

using static dlech.SshAgentLib.NativeMethods;


namespace dlech.SshAgentLib
{
  public sealed class WindowsOpenSshPipe : IDisposable
  {
    private const string AgentPipeName = "openssh-ssh-agent";

    private const int BufferSizeIn = 5 * 1024;
    private const int BufferSizeOut = 5 * 1024;

    private bool _disposed;
    private NamedPipeServerStream _listeningServer;


    public delegate void ConnectionHandlerFunc(Stream stream, Process process);


    public ConnectionHandlerFunc ConnectionHandler { get; set; }

    public WindowsOpenSshPipe()
    {
      if (File.Exists($"//./pipe/{AgentPipeName}")) {
        throw new PageantRunningException();
      }

      AwaitConnection();
    }

    private void AwaitConnection()
    {
      if (_disposed) {
        return;
      }

      _listeningServer = new NamedPipeServerStream(AgentPipeName,
                                                   PipeDirection.InOut,
                                                   NamedPipeServerStream.MaxAllowedServerInstances,
                                                   PipeTransmissionMode.Byte,
                                                   // TODO: Consider setting PipeOptions.CurrentUserOnly
                                                   PipeOptions.Asynchronous | PipeOptions.WriteThrough,
                                                   BufferSizeIn,
                                                   BufferSizeOut);

      try {
        _listeningServer.BeginWaitForConnection(AcceptConnection, _listeningServer);
        Debug.WriteLine("Started new server and awaiting connection ...");
      }
      catch (ObjectDisposedException) {
        // Could happen if we're disposing while starting a server
      }
      catch (Exception ex) {
        // Should never happen but we don't want to crash KeePass
        Debug.WriteLine($"{ex.GetType()} in AwaitConnection(): {ex.Message}");
        _listeningServer.Dispose();
      }
    }

    private void AcceptConnection(IAsyncResult result)
    {
      Debug.WriteLine("Received new connection ...");
      AwaitConnection();

      var server = result.AsyncState as NamedPipeServerStream;
      try {
        server.EndWaitForConnection(result);

        if (!GetNamedPipeClientProcessId(server.SafePipeHandle.DangerousGetHandle(), out var clientPid)) {
          throw new IOException("Failed to get client PID", Marshal.GetHRForLastWin32Error());
        }

        var clientProcess = Process.GetProcessById((int)clientPid);
        Debug.WriteLine($"Processing request from process: {clientProcess.MainModule.ModuleName} (PID: {clientPid})");
        ConnectionHandler(server, clientProcess);
      }
      catch (ObjectDisposedException) {
        // Server has been disposed
      }
      catch (Exception ex) {
        // Should never happen but we don't want to crash KeePass
        Debug.WriteLine($"{ex.GetType()} in AcceptConnection(): {ex.Message}");
      }
      finally {
        server.Dispose();
      }
    }

    public void Dispose()
    {
      _disposed = true;
      _listeningServer?.Dispose();
      _listeningServer = null;
    }
  }
}
