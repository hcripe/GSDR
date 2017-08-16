//=================================================================
// MultiPSK Network control 
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
using System.Collections;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Data;
using System.Drawing;



namespace PowerSDR
{
    public class MultiPSKEthernetServer
    {
        #region Variable

        private Socket ServerSocket;
        private Socket WorkingSocket;
        private int ServerPort;
        private Console console;
        private string ServerIPAddress = "127.0.0.1";
        private delegate void ConnectCallback(byte[] data);
        public byte[] receive_buffer;
        public string MultiPSKpassword = "1234567890";
        Thread ServerThread;
        Thread BufferThread;
        private bool run_server_thread = false;
        private AutoResetEvent server_event;
        public AutoResetEvent send_event;
        Thread Send_thread;
        private bool run_MultiPSK_send_thread = false;
        public byte[] send_buffer;
        public bool ClientConnected = false;
        public int send_bytes = 512;
        public int receive_byte_count = 0;
        private bool run_buffer_send_thread = false;

        private bool Is_PTT = false;
        public bool IsPTT
        {
            get { return Is_PTT; }
            set { Is_PTT = value; }
        }


        #endregion

        public MultiPSKEthernetServer(Console c)
        {
            console = c;
            receive_buffer = new byte[65535];
            send_buffer = new byte[65535];
            send_event = new AutoResetEvent(false);
            server_event = new AutoResetEvent(false);
        }

        ~MultiPSKEthernetServer()
        {
            if (ServerSocket != null)
                ServerSocket.Close(1000);
        }

        public bool StartEthernetServer(string ipAddress, int port, string password)
        {
            try
            {
                MultiPSKpassword = password;
                ServerIPAddress = ipAddress;
                ServerPort = port;

                ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                EndPoint ipep = new IPEndPoint(IPAddress.Parse(ServerIPAddress), ServerPort);
                ServerSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1);

                ServerSocket.Bind(ipep);
                ServerSocket.Listen(100);

                if (WorkingSocket == null)
                    WorkingSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                run_server_thread = true;
                server_event.Reset();
                ServerThread = new Thread(new ThreadStart(ServerSocketThread));
                ServerThread.Name = "MultiPSK server thread";
                ServerThread.IsBackground = true;
                ServerThread.Priority = ThreadPriority.Normal;
                ServerThread.Start();

                run_MultiPSK_send_thread = true;
                Send_thread = new Thread(new ThreadStart(SendThread));
                Send_thread.Name = "MultiPSK send thread";
                Send_thread.IsBackground = true;
                Send_thread.Priority = ThreadPriority.Normal;
                Send_thread.Start();

                return true;
            }
            catch (SocketException ex)
            {
                MessageBox.Show("Cannot start MultiPSK server!\nCheck your Setting!\n"
                    + ex.ToString());
                return false;
            }
        }

        private void ServerSocketThread()
        {
            try
            {
                while (run_server_thread)
                {
                    if (ServerSocket != null)
                        ServerSocket.BeginAccept(new AsyncCallback(AsyncAcceptCallback), ServerSocket);
                    else
                        run_server_thread = false;
                    server_event.WaitOne();
                }
            }
            catch (Exception e)
            {
                Debug.Write(e.ToString());
            }
        }

        public bool close()
        {
            try
            {
                ClientConnected = false;
                run_MultiPSK_send_thread = false;
                send_event.Set();
                Thread.Sleep(100);
                run_server_thread = false;
                server_event.Set();

                if (ServerSocket != null)
                    ServerSocket.Close(1000);

                if (WorkingSocket != null && WorkingSocket.Connected)
                {
                    WorkingSocket.Shutdown(SocketShutdown.Both);
                    WorkingSocket.Close(1000);
                }
                else if (WorkingSocket != null)
                    WorkingSocket.Close(1000);

                return true;
            }
            catch (System.Exception e)
            {
                Debug.Print(e.Message);
                return false;
            }
        }

        private int sample_pointer = 0;
        unsafe private void ProcessData(byte[] data)
        {
            ASCIIEncoding command = new ASCIIEncoding();
            string command_type;
            string version;
            string password;
            string byte_count;

            try
            {
                if (data[0] == 0x01)
                    data[0] += 0x30;
                if (data[1] == 0x02)
                    data[1] += 0x30;
                if (data[2] == 0x03)
                    data[2] += 0x30;

                command_type = command.GetString(data, 0, 4);
                version = command.GetString(data, 30, 3);
                password = command.GetString(data, 11, 10);
                byte_count = command.GetString(data, 4, 2);

                if (command_type == "123I")
                {
                    run_buffer_send_thread = false;
                    IsPTT = false;
                }
                else if (command_type == "123F")
                {
                    run_buffer_send_thread = false;
                    IsPTT = false;
                }
                else if (command_type == "123S")        // standard transmit packet
                {
                    int i;
                    int count = (int)data[4];
                    count = count << 8;
                    count += data[5];

                    for (i = 0; i < 122; i++)
                    {
                        Audio.MultiPSK_input_bufer_l[sample_pointer + i] = (float)(data[i + 6] / 1e4);
//                        Audio.MultiPSK_input_bufer_r[sample_pointer + i] = (float)(data[i + 6] / 1e5);
//                        Audio.MultiPSK_input_bufer_r[sample_pointer + i+1] = (float)(data[i+1 + 6] / 1e5);
                        sample_pointer ++;
//                        sample_pointer += 2;
//                        i++;
                    }
                }
                else if (command_type == "123R")        // last transmit packet
                {
                    run_buffer_send_thread = false;
                    IsPTT = false;
                }
                else if (command_type == "123T")        // transmit start
                {
                    sample_pointer = 0;
                    IsPTT = true;
                    Thread.Sleep(100);

                    run_buffer_send_thread = true;
                    BufferThread = new Thread(new ThreadStart(send_buffer_size_thread));
                    BufferThread.Name = "MultiPSK send buffer thread";
                    BufferThread.IsBackground = true;
                    BufferThread.Priority = ThreadPriority.Normal;
                    BufferThread.Start();
                }
                else if (command_type == "123P" && MultiPSKpassword == password)        // ping
                {

                }
                else
                {
                    int i;

                    for (i = 0; i < 128; i++)
                    {
                        Audio.MultiPSK_input_bufer_l[i + sample_pointer] = (float)(data[i] / 1e4);
//                        Audio.MultiPSK_input_bufer_r[i + sample_pointer] = (float)(data[i] / 1e5);
//                        Audio.MultiPSK_input_bufer_r[i+1 + sample_pointer] = (float)(data[i+1] / 1e5);
                        sample_pointer ++;
//                        sample_pointer += 2;
//                        i++;
                    }

                    if (sample_pointer == 236 || sample_pointer > 236)
                    {
                        Audio.MultiPSK_event.Set();
                        Thread.Sleep(2);
                        sample_pointer -= 236;

                        for (i = 0; i < sample_pointer; i++)
                        {
                            Audio.MultiPSK_input_bufer_l[i] = Audio.MultiPSK_input_bufer_l[236 + i];
//                            Audio.MultiPSK_input_bufer_r[i] = Audio.MultiPSK_input_bufer_l[2048 + i];
//                            Audio.MultiPSK_input_bufer_l[i + 1] = Audio.MultiPSK_input_bufer_l[2048 + i+1];
//                            i++;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }

        private void AsyncAcceptCallback(IAsyncResult result)
        {
            try
            {
                if (WorkingSocket == null)
                    WorkingSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                if (ServerSocket != null)
                {
                    if (WorkingSocket != null && !WorkingSocket.Connected)
                    {
                        Socket listener = (Socket)result.AsyncState;
                        WorkingSocket = listener.EndAccept(result);

                        StateObject state = new StateObject();
                        state.workSocket = WorkingSocket;

                        WorkingSocket.BeginReceive(receive_buffer, 0, 128, SocketFlags.None,
                            new AsyncCallback(ReceiveCallback), WorkingSocket);

                        ClientConnected = true;
                    }
                }
            }
            catch (ObjectDisposedException exception)
            {
                if (WorkingSocket != null)
                    WorkingSocket.Close();
                if (ServerSocket != null)
                    ServerSocket.Close();
                server_event.Set();
                ClientConnected = false;
                Debug.Write(exception.Message);
            }
        }

        private void ReceiveCallback(IAsyncResult result)
        {
            try
            {
                if (WorkingSocket.Connected)
                {
                    int num_read = WorkingSocket.EndReceive(result);
                    if (0 != num_read)
                    {
                        if (WorkingSocket.Connected)
                            ProcessData(receive_buffer);
                        receive_buffer = null;
                        receive_buffer = new byte[65535];
                        WorkingSocket.BeginReceive(receive_buffer, 0, 128, SocketFlags.None,
                            new AsyncCallback(ReceiveCallback), null);
                    }
                    else
                    {
                        Debug.Write("Disconnected!\n");
                        WorkingSocket.Close();
                        server_event.Set();
                    }
                }
                else
                {
                    Debug.Write("Disconnected!\n");
                    WorkingSocket.Close();
                    server_event.Set();
                }
            }
            catch (SocketException socketException)
            {
                //WSAECONNRESET, the other side closed impolitely
                if (socketException.ErrorCode == 10054)
                {
                    ClientConnected = false;
                    WorkingSocket.Close(1000);
                    server_event.Set();
                }
            }
            catch (ObjectDisposedException)
            {
                // The socket was closed out from under me
                ClientConnected = false;
                WorkingSocket.Close(1000);
                Thread.Sleep(1000);
                server_event.Set();
            }
        }

        private void DisconnectCallback(IAsyncResult result)
        {
            try
            {
                ClientConnected = false;
                // Complete the disconnect request.
                Socket client = (Socket)result.AsyncState;
                client.EndDisconnect(result);
                Debug.Write("Disconnected!\n");
                server_event.Set();
            }
            catch (Exception ex)
            {
                ClientConnected = false;
                Debug.Write(ex.Message);
                WorkingSocket.Close();
                server_event.Set();
            }
        }

        private void SendThread()
        {
            try
            {
                while (run_MultiPSK_send_thread)
                {
                    send_event.WaitOne();

                    if (WorkingSocket.Connected && run_MultiPSK_send_thread)
                    {
                        int sendBytes = WorkingSocket.Send(send_buffer, send_bytes, SocketFlags.None);
                        if (sendBytes != send_bytes)
                        {
                            WorkingSocket.Close(1000);
                            console.btnNetwork.BackColor = Color.Red;
                            run_MultiPSK_send_thread = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Write("Error!\n" + ex.ToString());
                close();
                server_event.Set();
            }
        }

        private void send_buffer_size_thread()
        {
            while (run_buffer_send_thread && console.PowerOn && console.MOX)
            {
                send_buffer[0] = 0x01;
                send_buffer[1] = 0x02;
                send_buffer[2] = 0x03;
                send_buffer[3] = 0x4e;
                send_buffer[4] = 0x1a;
                send_buffer[5] = 0x00;

                send_event.Set();

                Thread.Sleep(200);
            }
        }
    }
}