//=================================================================
// Network Client 
//=================================================================
//
//  Copyright (C)2010,2011 YT7PWR Goran Radivojevic
//  contact via email at: yt7pwr@ptt.rs or yt7pwr2002@yahoo.com
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either version 3
// of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
//
//=================================================================


using System;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Text;
using Aced.Compression;

namespace PowerSDR
{

    public class ClientRecvData
    {
        private Socket s;
        private Console console;
        private byte[] buffer = new byte[65535];
        private byte[] decompBuffer;
        public bool IsOpen = false;
        public AutoResetEvent sendEvent;
        public byte[] send_buffer = new byte[65536];
        private byte[] compBuffer;
        public bool data_ready = false;
        public bool debug = false;
        delegate void DebugCallbackFunction(string name);

        #region Properties

        private bool UDP_recv = false;
        public bool UDPReceive
        {
            set { UDP_recv = value; }
        }

        #endregion

        public ClientRecvData(Console c)
        {
            console = c;
            if (sendEvent == null)
                sendEvent = new AutoResetEvent(false);
        }

        ~ClientRecvData()
        {
            if (s != null)
                s.Close();
        }

        public bool open(string mcastGroup, int port, string loacal_IP_address, int frameCount)
        {
            try
            {
                s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                EndPoint ipep = new IPEndPoint(IPAddress.Any, port);
                s.Bind(ipep);

                IPAddress ip = IPAddress.Parse(mcastGroup);

                s.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership,
                    new MulticastOption(ip, IPAddress.Parse(loacal_IP_address)));

                UDP_recv = true;
                Receive();
                IsOpen = true;

                Thread t = new Thread(new ThreadStart(send_task));
                t.Name = "Send Ethernet Data Thread";
                t.IsBackground = true;
                t.Priority = ThreadPriority.Normal;
                t.Start();

                return true;
            }
            catch (Exception ex)
            {
                Debug.Write(ex.ToString());
                return false;
            }
        }

        unsafe public void Receive()
        {
            try
            {
                bool buffer_ready = false;
                byte[] buffer_all = new byte[65535];
                float[] temp_buffer = new float[4096];

                while (UDP_recv)
                {
                    if (console.chkPower.Checked)
                    {
                        s.Receive(buffer, SocketFlags.None);

                        if (console.SetupForm.ClientDecompression)              // full spectrum decompress
                        {
                            if (buffer[0] == 0x56 && buffer[1] == 0x00)
                            {
                                decompBuffer = AcedInflator.Instance.Decompress(buffer, 2, 0, 0);
                                if (decompBuffer != null)
                                {
                                    buffer_ready = true;
                                }
                            }
                            else if (buffer[0] == 0x54 && buffer[1] == 0x00)    // AF decompress
                            {
                                decompBuffer = AcedInflator.Instance.Decompress(buffer, 2, 0, 0);
                                if (decompBuffer != null)
                                {
                                    buffer_ready = true;
                                }
                            }
                            else
                                buffer_ready = false;

                            if (debug)
                                console.Invoke(new DebugCallbackFunction(console.DebugCallback),
                                    "Received spectrum/decompress: " + decompBuffer.Length.ToString());
                        }
                        else
                        {                                                       // full spectrum without decompression
                            if (buffer[0] == 0x55 && buffer[1] == 0x00)
                            {
                                for (int i = 2; i < 16386; i++)
                                {
                                    buffer_all[i - 2] = buffer[i];
                                }

                                buffer_ready = true;
                            }
                            else if (buffer[0] == 0x53 && buffer[1] == 0x00)    // AF without decompress
                            {
                                for (int i = 2; i < 1026; i++)
                                {
                                    buffer_all[i - 2] = buffer[i];
                                }

                                buffer_ready = true;
                            }
                            else
                                buffer_ready = false;
                            if (debug)
                                console.Invoke(new DebugCallbackFunction(console.DebugCallback),
                                    "Received spectrum/no decompress! ");

                        }

                        if (buffer[0] == 0x60 && buffer[1] == 0x00)    // Display data
                        {
                            decompBuffer = AcedInflator.Instance.Decompress(buffer, 2, 0, 0);

                            if (decompBuffer != null)
                            {
                                Display_GDI.display_data_mutex.WaitOne();

                                if (decompBuffer.Length <= Display_GDI.client_W)
                                {
                                    fixed (void* src = &decompBuffer[0])
                                    fixed (void* dest = &Display_GDI.new_display_data[0])
                                        Win32.memcpy(dest, src, decompBuffer.Length);
                                }
                                else
                                {
                                    fixed (void* src = &decompBuffer[0])
                                    fixed (void* dest = &Display_GDI.new_display_data[0])
                                        Win32.memcpy(dest, src, Display_GDI.client_W);
                                }

                                Display_GDI.display_data_mutex.ReleaseMutex();

                                Display_GDI.DataReady = true;

                                buffer_ready = false;
                            }

                            if (debug)
                                console.Invoke(new DebugCallbackFunction(console.DebugCallback),
                                    "Received display data: " + decompBuffer.Length.ToString());
                        }

                        if (buffer_ready)
                        {
                            if (console.SetupForm.ClientDecompression)
                            {
                                Audio.CATNetwork_mutex.WaitOne();

                                if (Audio.ClientRFSpectar)
                                {
                                    fixed (void* src = &decompBuffer[2])
                                    fixed (void* dest = &Audio.network_input_bufer_l[0])
                                        Win32.memcpy(dest, src, 8192);

                                    fixed (void* src = &decompBuffer[8194])
                                    fixed (void* dest = &Audio.network_input_bufer_r[0])
                                        Win32.memcpy(dest, src, 8192);
                                }
                                else
                                {
                                    fixed (void* src = &decompBuffer[2])
                                    fixed (void* dest = &Audio.network_input_bufer_l[0])
                                        Win32.memcpy(dest, src, decompBuffer.Length);
                                }

                                Audio.CATNetwork_mutex.ReleaseMutex();
                            }
                            else
                            {
                                Audio.CATNetwork_mutex.WaitOne();

                                // send new data
                                fixed (void* src = &buffer_all[0])
                                fixed (void* dest = &Audio.network_input_bufer_l[0])
                                    Win32.memcpy(dest, src, 8192);

                                fixed (void* src = &buffer_all[8192])
                                fixed (void* dest = &Audio.network_input_bufer_r[0])
                                    Win32.memcpy(dest, src, 8192);

                                Audio.CATNetwork_mutex.ReleaseMutex();
                            }

                            buffer_ready = false;
                        }
                    }
                    else
                    {
                        Thread.Sleep(100);
                    }
                }
                s.Close();
            }
            catch (Exception e)
            {
                Debug.Print(e.Message);
                s.Close();
            }
        }

        public bool close()
        {
            try
            {
                s.Close();
                return true;
            }
            catch (System.Exception e)
            {
                Debug.Print(e.Message);
                return false;
            }
        }

        public bool stopReceive()
        {
            try
            {
                s.Close();
                return true;
            }
            catch (System.Exception e)
            {
                Debug.Print(e.Message);
                return false;
            }
        }

        public void send_task()
        {
            try
            {
                byte[] temp_buffer = new byte[65536];

                while (IsOpen)
                {
                    sendEvent.WaitOne();

                    if (IsOpen)                    // full spectrum with compression
                    {
                        if (console.SetupForm.ServerCompression)
                        {
                            Audio.CATNetwork_mutex.WaitOne();

                            compBuffer = AcedDeflator.Instance.Compress(send_buffer, 0, 16386,
                                AcedCompressionLevel.Fast, 2, 0);

                            Audio.CATNetwork_mutex.ReleaseMutex();

                            if (compBuffer != null)
                            {
                                compBuffer[0] = 0x56;
                                compBuffer[1] = 0x00;
//                                s.SendTo(compBuffer, 0, compBuffer.Length, SocketFlags.None, ipe);
                            }
                        }
                        else
                        {                                           // full spectrum without compression
                            send_buffer[0] = 0x55;
                            send_buffer[1] = 0x00;
//                            s.SendTo(send_buffer, 0, 16386, SocketFlags.None, ipep2);
                        }
                    }
                    else if (IsOpen)                                // AF spectrum and display data with compression
                    {
                        if (console.SetupForm.ServerCompression)
                        {
                            compBuffer = AcedDeflator.Instance.Compress(send_buffer, 0, 1024,
                                AcedCompressionLevel.Fast, 2, 0);
                            if (compBuffer != null)
                            {
                                compBuffer[0] = 0x54;
                                compBuffer[1] = 0x00;
//                                s.SendTo(compBuffer, 0, compBuffer.Length, SocketFlags.None, ipep2);
                            }
                        }
                        else
                        {                           // AF without compression
                            send_buffer[0] = 0x53;
                            send_buffer[1] = 0x00;
//                            s.SendTo(send_buffer, 0, 1026, SocketFlags.None, ipep2);
                        }
                    }
                }

                Debug.Write("Send task terminated!");
            }
            catch (Exception e)
            {
                Debug.Print(e.Message);
            }
        }
    }
}