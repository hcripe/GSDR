//=================================================================
// Network Server
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
    public class ServerSendData
    {
        private Socket s;
        private Console console;
        public bool IsOpen = false;
        private IPEndPoint ipep = new IPEndPoint(IPAddress.Parse("127.0.0.0"), 5001);
        private IPEndPoint ipep2 = new IPEndPoint(IPAddress.Parse("224.0.0.0"), 5000);
        public byte[] send_buffer = new byte[65536];
        public bool run = true;
        public AutoResetEvent sendEvent;
        public byte[] display_data;
        public bool data_ready = false;
        private byte[] compBuffer;
        public bool debug = false;
        delegate void DebugCallbackFunction(string name);

        #region Properties


        #endregion

        public ServerSendData(Console c)
        {
            console = c;
        }

        ~ServerSendData()
        {
            IsOpen = false;
            if (s != null)
                s.Close();
        }

        public bool open(string mcastGroup, int mcast_port, string local_host, int local_port, int ttl)
        {
            try
            {
                ipep.Address = IPAddress.Parse(local_host);
                ipep.Port = local_port;

                ipep2.Address = IPAddress.Parse(mcastGroup);
                ipep2.Port = mcast_port;

                s = new Socket(AddressFamily.InterNetwork,
                                SocketType.Dgram, ProtocolType.Udp);

                s.Bind(ipep);

                s.SendTimeout = 1;

                s.Connect(ipep2);

                s.SetSocketOption(SocketOptionLevel.IP,
                    SocketOptionName.AddMembership, new MulticastOption(IPAddress.Parse(mcastGroup)));

                s.SetSocketOption(SocketOptionLevel.IP,
                    SocketOptionName.MulticastTimeToLive, ttl);

                if (sendEvent == null)
                    sendEvent = new AutoResetEvent(false);
                IsOpen = true;

                Thread t = new Thread(new ThreadStart(send_task));
                t.Name = "Send Ethernet Data Thread";
                t.IsBackground = true;
                t.Priority = ThreadPriority.Normal;
                t.Start();

                return true;
            }
            catch (System.Exception e)
            {
                Debug.Print(e.Message);
                IsOpen = false;
                s.Close();
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
                    display_data = new byte[Display_GDI.server_W];

                    sendEvent.WaitOne();

                    if (IsOpen)                    // full spectrum with compression
                    {
                        if (console.SetupForm.ServerCompression)
                        {
                            //Audio.CATNetwork_mutex.WaitOne();

                            compBuffer = AcedDeflator.Instance.Compress(send_buffer, 0, 16386,
                                AcedCompressionLevel.Fast, 2, 0);

                            //Audio.CATNetwork_mutex.ReleaseMutex();

                            if (compBuffer != null)
                            {
                                compBuffer[0] = 0x56;
                                compBuffer[1] = 0x00;
                                s.SendTo(compBuffer, 0, compBuffer.Length, SocketFlags.None, ipep2);
                            }
                        }
                        else
                        {                                           // full spectrum without compression
                            send_buffer[0] = 0x55;
                            send_buffer[1] = 0x00;
                            s.SendTo(send_buffer, 0, 16386, SocketFlags.None, ipep2);
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
                                s.SendTo(compBuffer, 0, compBuffer.Length, SocketFlags.None, ipep2);
                            }
                        }
                        else
                        {                           // AF without compression
                            send_buffer[0] = 0x53;
                            send_buffer[1] = 0x00;
                            s.SendTo(send_buffer, 0, 1026, SocketFlags.None, ipep2);
                        }

                        if (data_ready)
                        {
                            Display_GDI.display_data_mutex.WaitOne();

                            compBuffer = AcedDeflator.Instance.Compress(display_data, 0, Display_GDI.server_W + 2,
                                AcedCompressionLevel.Fast, 2, 0);

                            Display_GDI.display_data_mutex.ReleaseMutex();

                            compBuffer[0] = 0x60;
                            compBuffer[1] = 0x00;
                            s.SendTo(compBuffer, 0, compBuffer.Length,
                                SocketFlags.None, ipep2);
                            data_ready = false;
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

        public bool close()
        {
            try
            {
                IsOpen = false;
                Debug.Print("Closing Connection...");
                sendEvent.Set();
                s.Close();
                return true;
            }
            catch (System.Exception e)
            {
                Debug.Print(e.Message);
                return false;
            }
        }
    }
}