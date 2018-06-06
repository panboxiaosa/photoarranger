﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Yzmeir.NamedPipes;

namespace NamedPipesServer
{
    public sealed class ServerNamedPipe : IDisposable
    {
        internal Thread PipeThread;
        internal ServerPipeConnection PipeConnection;
        internal bool Listen = true;
        internal DateTime LastAction;
        private bool disposed = false;

        private void PipeListener()
        {
            CheckIfDisposed();
            try
            {
                Listen = Program.PipeManager.Listen;
                //Form1.ActivityRef.AppendText("Pipe " + this.PipeConnection.NativeHandle.ToString() + ": new pipe started" + Environment.NewLine);
                while (Listen)
                {
                    LastAction = DateTime.Now;
                    string request = PipeConnection.Read();
                    //byte[] bytes = PipeConnection.ReadBytes();
                    LastAction = DateTime.Now;
                    if (request.Trim() != "")
                    {
                        Console.WriteLine(request);
                        PipeConnection.Write("Ok");
                    }
                    else
                    {
                        PipeConnection.Write("Error: bad request");
                    }
                    LastAction = DateTime.Now;
                    PipeConnection.Disconnect();
                    if (Listen)
                    {
                        //Form1.ActivityRef.AppendText("Pipe " + this.PipeConnection.NativeHandle.ToString() + ": listening" + Environment.NewLine);
                        Connect();
                    }
                    Program.PipeManager.WakeUp();
                }
            }
            catch (System.Threading.ThreadAbortException ex) { }
            catch (System.Threading.ThreadStateException ex) { }
            catch (Exception ex)
            {
                // Log exception
            }
            finally
            {
                this.Close();
            }
        }
        internal void Connect()
        {
            CheckIfDisposed();
            PipeConnection.Connect();
        }
        internal void Close()
        {
            CheckIfDisposed();
            this.Listen = false;
            Program.PipeManager.RemoveServerChannel(this.PipeConnection.NativeHandle);
            this.Dispose();
        }
        internal void Start()
        {
            CheckIfDisposed();
            PipeThread.Start();
        }
        private void CheckIfDisposed()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("ServerNamedPipe");
            }
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        private void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                PipeConnection.Dispose();
                if (PipeThread != null)
                {
                    try
                    {
                        PipeThread.Abort();
                    }
                    catch (System.Threading.ThreadAbortException ex) { }
                    catch (System.Threading.ThreadStateException ex) { }
                    catch (Exception ex)
                    {
                        // Log exception
                    }
                }
            }
            disposed = true;
        }
        ~ServerNamedPipe()
        {
            Dispose(false);
        }
        internal ServerNamedPipe(string name, uint outBuffer, uint inBuffer, int maxReadBytes, bool secure)
        {
            PipeConnection = new ServerPipeConnection(name, outBuffer, inBuffer, maxReadBytes, secure);
            PipeThread = new Thread(new ThreadStart(PipeListener));
            PipeThread.IsBackground = true;
            PipeThread.Name = "Pipe Thread " + this.PipeConnection.NativeHandle.ToString();
            LastAction = DateTime.Now;
        }
    }
}
