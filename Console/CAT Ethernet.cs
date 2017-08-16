//=================================================================
// Network CAT control 
//=================================================================
//
//  Copyright (C)2010,2011,2012 YT7PWR Goran Radivojevic
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
    #region CAT Server class

    /// <summary>
    /// CAT Ethernet server class
    /// </summary>
    public class CAToverEthernetServer : IDisposable
    {
        #region Variable

        Socket Listener;
        Socket client1;
        Socket client2;
        private int ServerPort;
        private Console console;
        private CATParser parser;
        private string ServerIPAddress = "127.0.0.1";
        private delegate void CATConnectCallback(byte[] data);
        public delegate void CATReceiveCallback(byte[] data);
        public byte[] receive_buffer1;
        public byte[] receive_buffer2;
        IntPtr client1_hanle = new IntPtr();
        IntPtr client2_hanle = new IntPtr();
        private string CATpassword = "12345678";
        private string CATversion = "01";
        private bool run_server = false;
        private AutoResetEvent server_event;
        private Thread Poll_thread;
        private System.Windows.Forms.Timer ConnectionWatchdog = null;
        public int WatchdogInterval = 1000;
        public bool IPv6_enabled = false;
        private bool run_watchdog = false;
        private delegate void DebugCallbackFunction(string text);
        bool client1_connected = false;
        bool client2_connected = false;
        public bool HRDserver = false;
        public delegate void CATCrossThreadCallback(string type, int parm1, int[] parm2, string parm3);
        public VoIP voip;
        public float[] display_data = new float[4096];

        #endregion

        #region properties

        private bool debug = false;
        public bool Debug_enable
        {
            get { return debug; }
            set
            {
                debug = value;

                if (!HRDserver && voip != null)
                    voip.debug = value;
            }
        }

        #endregion

        #region constructor/destructor

        public CAToverEthernetServer(Console c)
        {
            console = c;
            parser = new CATParser(console);
            receive_buffer1 = new byte[2048];
            receive_buffer2 = new byte[2048];
            server_event = new AutoResetEvent(false);
            ConnectionWatchdog = new System.Windows.Forms.Timer();
            ConnectionWatchdog.Tick += new System.EventHandler(ServerWatchDogTimerTick);
        }

        ~CAToverEthernetServer()
        {

        }

        public virtual void Dispose()
        {
            if (!HRDserver)
            {
                if (voip != null)
                    voip.Dispose();
            }
            else
            {
                if (ConnectionWatchdog != null)
                    ConnectionWatchdog.Dispose();
            }
        }

        #endregion

        #region Start/Stop

        public bool Start(string ipAddress, int port, string password)
        {
            try
            {
                if (!HRDserver)
                {
                    /*CATpassword = password;
                    ServerIPAddress = ipAddress;
                    ServerPort = port;
                    // VoIP initialization
                    voip = new VoIP(console);
                    voip.debug = debug;
                    voip.OpMode = VoIP_mode.Server;
                    voip.Text = "Remote server";
                    voip.Show();
                    Win32.SetWindowPos(voip.Handle.ToInt32(),
                        -1, voip.Left, voip.Top, voip.Width, voip.Height, 0);
                    voip.Start(ServerIPAddress, ServerPort);

                    if (debug && !console.ConsoleClosing)
                        console.Invoke(new DebugCallbackFunction(console.DebugCallback),
                            "VoIP Server started!");*/
                }
                else
                {
                    CATpassword = password;
                    ServerIPAddress = ipAddress;
                    ServerPort = port;
                    console.SetupForm.txtCATLocalIPAddress.ForeColor = Color.Red;
                    client1_connected = false;
                    client2_connected = false;

                    IPHostEntry ipEntry = Dns.GetHostByAddress(ServerIPAddress);
                    IPAddress[] aryLocalAddr = ipEntry.AddressList;
                    client1 = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    client2 = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                    switch (console.WinVer)
                    {
                        case WindowsVersion.Windows2000:
                        case WindowsVersion.WindowsXP:
                            {
                                // Create the listener socket in this machines IP address
                                Listener = new Socket(AddressFamily.InterNetwork,
                                                  SocketType.Stream, ProtocolType.Tcp);
                            }
                            break;

                        case WindowsVersion.WindowsVista:
                        case WindowsVersion.Windows7:
                        case WindowsVersion.Windows8:
                            {
                                if (IPv6_enabled && aryLocalAddr[0].AddressFamily == AddressFamily.InterNetworkV6)
                                {
                                    // Create the listener socket in this machines IPv6 address
                                    Listener = new Socket(AddressFamily.InterNetworkV6,
                                                      SocketType.Stream, ProtocolType.Tcp);
                                }
                                else
                                {
                                    // Create the listener socket in this machines IP address
                                    Listener = new Socket(AddressFamily.InterNetwork,
                                                      SocketType.Stream, ProtocolType.Tcp);
                                }
                            }
                            break;
                    }

                    Listener.Bind(new IPEndPoint(aryLocalAddr[0], port));
                    Listener.Listen(1);

                    // Setup a callback to be notified of connection requests
                    Listener.BeginAccept(new AsyncCallback(OnConnectRequest), Listener);
                    console.SetupForm.txtCATLocalIPAddress.ForeColor = Color.Green;
                    run_server = true;

                    if (!HRDserver)
                    {
                        Poll_thread = new Thread(new ThreadStart(PollStatusThread));
                        Poll_thread.Name = "Poll server thread";
                        Poll_thread.IsBackground = true;
                        Poll_thread.Priority = ThreadPriority.Normal;
                        Poll_thread.Start();
                    }

                    if (debug && !console.ConsoleClosing)
                        console.Invoke(new DebugCallbackFunction(console.DebugCallback),
                            "HRD Server started!");
                }

                return true;
            }
            catch (SocketException ex)
            {
                console.SetupForm.txtCATLocalIPAddress.ForeColor = Color.Red;
                console.SetupForm.EnableCATOverEthernetServer = false;
                MessageBox.Show("Cannot start CAT network server!\nCheck your Setting!\n\n"
                    + ex.ToString());

                return false;
            }
        }

        public bool Stop()
        {
            try
            {
                if (!HRDserver)
                {
                    /*if (voip != null)
                        voip.Dispose();

                    if (debug && !console.ConsoleClosing)
                        console.Invoke(new DebugCallbackFunction(console.DebugCallback),
                            "VoIP Server stoped!");*/
                }
                else
                {
                    run_server = false;
                    client1_connected = false;
                    client2_connected = false;

                    if (client1.Connected)
                    {
                        client1.Shutdown(SocketShutdown.Both);
                        client1.Close();
                    }
                    if (client2.Connected)
                    {
                        client2.Shutdown(SocketShutdown.Both);
                        client2.Disconnect(true);
                        client2.Close();
                    }

                    Listener.Close();
                    console.SetupForm.txtCATLocalIPAddress.ForeColor = Color.Red;
                    run_watchdog = false;
                    ConnectionWatchdog.Stop();
                    ConnectionWatchdog.Enabled = false;

                    if (debug && !console.ConsoleClosing)
                        console.Invoke(new DebugCallbackFunction(console.DebugCallback),
                            "HRD Server stoped!");
                }

                return true;
            }
            catch (System.Exception ex)
            {
                Debug.Print(ex.Message);
                return false;
            }
        }

        #endregion

        #region misc function

        public void OnConnectRequest(IAsyncResult ar)
        {
            try
            {
                Socket listener = (Socket)ar.AsyncState;

                if (client1_connected && client2_connected)
                {
                    Socket tmp_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    tmp_socket.EndAccept(ar);
                    tmp_socket.Close();
                    Listener.BeginAccept(new AsyncCallback(OnConnectRequest), Listener);

                    if (debug && !console.ConsoleClosing)
                        console.Invoke(new DebugCallbackFunction(console.DebugCallback),
                            "Client already connected!");
                }
                else if (client1_connected)
                    client2 = listener.EndAccept(ar);
                else
                    client1 = listener.EndAccept(ar);

                if (client1.Connected && !client1_connected)
                {
                    client1_connected = true;
                    ConnectionWatchdog.Enabled = true;
                    ConnectionWatchdog.Interval = WatchdogInterval;
                    ConnectionWatchdog.Start();

                    if (debug && !console.ConsoleClosing)
                        console.Invoke(new DebugCallbackFunction(console.DebugCallback),
                            "Client connected!");

                    client1.BeginReceive(receive_buffer1, 0, receive_buffer1.Length, SocketFlags.None, OnRecievedData, client1);
                    client1_hanle = client1.Handle;
                    Debug.Write("Client joined " + client1.RemoteEndPoint.ToString() + "\n");
                    listener.BeginAccept(new AsyncCallback(OnConnectRequest), listener);        // again
                }
                else if (client2.Connected && !client2_connected)
                {
                    client2_connected = true;
                    ConnectionWatchdog.Enabled = true;
                    ConnectionWatchdog.Interval = WatchdogInterval;
                    ConnectionWatchdog.Start();

                    if (debug && !console.ConsoleClosing)
                        console.Invoke(new DebugCallbackFunction(console.DebugCallback),
                            "Client connected!");

                    client2.BeginReceive(receive_buffer2, 0, receive_buffer2.Length, SocketFlags.None, OnRecievedData, client2);
                    client2_hanle = client2.Handle;
                    Debug.Write("Client joined " + client2.RemoteEndPoint.ToString() + "\n");
                    listener.BeginAccept(new AsyncCallback(OnConnectRequest), listener);        // again
                }
            }
            catch (Exception ex)
            {
                Debug.Write(ex.ToString());
            }
        }

        public void OnRecievedData(IAsyncResult result)
        {
            try
            {
                Socket sock = (Socket)result.AsyncState;
                int ret = 0;

                if(sock.Connected)
                    ret = sock.EndReceive(result);  // sock.Receive(receive_buffer, receive_buffer.Length, 0);


                if (ret > 0)
                {
                    if (HRDserver)
                    {
                        if (sock.Handle == client1_hanle)
                            HRDServerProcessData(ref sock, receive_buffer1, ret);
                        else
                            HRDServerProcessData(ref sock, receive_buffer2, ret);
                    }
                    else
                        ProcessData(receive_buffer1, ret);

                    if (sock.Handle == client1_hanle)
                        sock.BeginReceive(receive_buffer1, 0, receive_buffer1.Length, SocketFlags.None,
                            OnRecievedData, sock);
                    else
                        sock.BeginReceive(receive_buffer2, 0, receive_buffer2.Length, SocketFlags.None,
                            OnRecievedData, sock);
                }
                else
                {
                    sock.Shutdown(SocketShutdown.Both);     // loost connection
                    sock.Close(1000);

                    if (!client1.Connected && client1_connected)
                    {
                        client1_connected = false;

                        if (debug && !console.ConsoleClosing)
                            console.Invoke(new DebugCallbackFunction(console.DebugCallback),
                            "Client1 disconnected!");
                    }
                    if (!client2.Connected && client2_connected)
                    {
                        client2_connected = false;

                        if (debug && !console.ConsoleClosing)
                            console.Invoke(new DebugCallbackFunction(console.DebugCallback),
                            "Client2 disconnected!");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Write(ex.ToString());

                if (!client1.Connected && client1_connected)
                {
                    client1_connected = false;

                    if (debug && !console.ConsoleClosing)
                        console.Invoke(new DebugCallbackFunction(console.DebugCallback),
                        "Client1 disconnected!");
                }
                if (!client2.Connected && client2_connected)
                {
                    client2_connected = false;

                    if (debug && !console.ConsoleClosing)
                        console.Invoke(new DebugCallbackFunction(console.DebugCallback),
                        "Client disconnected!");
                }
            }
        }

        void HRDServerProcessData(ref Socket sock, byte[] data, int count)
        {
            try
            {
                string final = "";
                bool multi = false;
                ASCIIEncoding buffer = new ASCIIEncoding();
                int header_lenght = 18;
                string text = "";
                ASCIIEncoding ascii_buff = new ASCIIEncoding();
                byte[] ascii_buffer = new byte[4096];
                ascii_buffer = Encoding.Convert(Encoding.Unicode, Encoding.UTF8, data, 16, Math.Max(0, count - 16));
                string cmd = buffer.GetString(ascii_buffer, 0, ascii_buffer.Length);
                cmd = cmd.TrimEnd('\0');
                string[] c = cmd.Split('\t');

                if (c.Length > 1)
                    multi = true;

                if (debug && !console.ConsoleClosing && cmd.Length == 1)
                    console.Invoke(new DebugCallbackFunction(console.DebugCallback), cmd);

                foreach (string command in c)
                {
                    if (debug && !console.ConsoleClosing)
                        console.Invoke(new DebugCallbackFunction(console.DebugCallback), command);

                    text = "";

                    switch (command)
                    {
                        case "Get Frequency":
                        case "[0] Get Frequency":
                            text = (console.VFOAFreq * 1e6).ToString("f0");
                            text = text.PadLeft(11, '0');
                            break;

                        case "Get ID":
                        case "Get Radio":
                            text = "Genesis GSDR";
                            break;

                        case "Get Radios":
                            text = "0:Genesis GSDR";
                            break;

                        case "Get Version":
                            text = "v1.0.0";
                            break;

                        case "[0] Get Buttons":
                        case "Get Buttons":
                            text = "START,TX,SPLIT,SQL1,SQL2,AF,RF,ATT,MR,MS,MC,VFO A,TUN,MSG1,MSG2,MSG3,MSG4,MSG5,MSG6";
                            break;

                        case "[0] Get Dropdowns":
                        case "Get Dropdowns":
                            text = "Band,Mode,AGC,Display,DSP Filtr,Preamp";
                            break;

                        case "[0] Get Sliders":
                        case "Get Sliders":
                            text = "AF,RF,PWR,SQL1,SQL2,MIC,";
                            break;

                        case "Get Context":
                            text = "0";
                            break;

                        case "Get LogbookUpdates":
                            text = "0";
                            break;

                        case "[0] get radio":
                            text = "Genesis GSDR";
                            break;

                        case "[0] get user-active 5000":
                            text = "0";
                            break;

                        case "[0] set block":
                            text = "0";
                            break;

                        case "[0] set unblock":
                            text = "0";
                            break;

                        case "[0] get frequency":
                            ascii_buffer = new byte[2048];
                            text = (console.VFOAFreq * 1e6).ToString("f0");
                            text = text.PadLeft(11, '0') + ",";
                            break;

                        case "[0] get frequencies":
                            ascii_buffer = new byte[2048];
                            text = (console.VFOAFreq * 1e6).ToString("f0");
                            string vfoB = (console.VFOBFreq * 1e6).ToString("f0");
                            text += "-" + vfoB + ",";
                            break;

                        default:
                            {
                                if (command.StartsWith("set password"))
                                {
                                    text = "OK";
                                }
                                else if (command.StartsWith("[0] get dropdown-list"))
                                {
                                    if (command.Contains("{Mode}"))
                                    {
                                        text = "LSB,USB,DSB,CWL,CWU,FM,AM,SPEC,DIGU,DIGL,SAM,DRM";
                                    }
                                    else if (command.Contains("{Band}"))
                                    {
                                        text = "160m,80m,60m,40m,30m,20m,17m,15m,12m,10m,6m,2m,GEN,WWV";
                                    }
                                    else if (command.Contains("{AGC}"))
                                    {
                                        text = "Fixd,Long,Slow,Med,Fast,Custom";
                                    }
                                    else if (command.Contains("{Display}"))
                                    {
                                        text = "Panadapter,Panafall,Panafall_inv,Scope,Panasope,Phase,Phase2,Spectrum,Waterfall,Off";
                                    }
                                    else if (command.Contains("{Preamp}"))
                                    {
                                        text = "on,off";
                                    }
                                    else if (command.Contains("{DSP Fltr}") || command.Contains("{DSP Filtr}"))
                                    {
                                        text = "F1,F2,F3,F4,F5,F6,F7,F8,F9,F10,VAR1,VAR2";
                                    }
                                    else
                                    {
                                        text = "  ";
                                    }
                                }
                                else if (command.StartsWith("[0] set dropdown"))
                                {
                                    if (command.Contains("Mode"))
                                    {
                                        if (command.Contains("USB"))
                                        {
                                            console.CurrentDSPMode = DSPMode.USB;
                                        }
                                        else if (command.Contains("LSB"))
                                        {
                                            console.CurrentDSPMode = DSPMode.LSB;
                                        }
                                        else if (command.Contains("AM"))
                                        {
                                            console.CurrentDSPMode = DSPMode.AM;
                                        }
                                        else if (command.Contains("FM"))
                                        {
                                            console.CurrentDSPMode = DSPMode.FMN;
                                        }
                                        else if (command.Contains("DIGL"))
                                        {
                                            console.CurrentDSPMode = DSPMode.DIGL;
                                        }
                                        else if (command.Contains("DIGU"))
                                        {
                                            console.CurrentDSPMode = DSPMode.DIGU;
                                        }
                                        else if (command.Contains("CWL"))
                                        {
                                            console.CurrentDSPMode = DSPMode.CWL;
                                        }
                                        else if (command.Contains("CWU"))
                                        {
                                            console.CurrentDSPMode = DSPMode.CWU;
                                        }
                                        else if (command.Contains("SAM"))
                                        {
                                            console.CurrentDSPMode = DSPMode.SAM;
                                        }
                                        else if (command.Contains("DRM"))
                                        {
                                            console.CurrentDSPMode = DSPMode.DRM;
                                        }
                                        else if (command.Contains("SPEC"))
                                        {
                                            console.CurrentDSPMode = DSPMode.SPEC;
                                        }
                                        else if (command.Contains("DSB"))
                                        {
                                            console.CurrentDSPMode = DSPMode.DSB;
                                        }

                                        text = "1";
                                    }
                                    else if (command.Contains("Band"))
                                    {
                                        if (command.Contains("160m"))
                                        {
                                            console.CurrentBand = Band.B160M;
                                        }
                                        else if (command.Contains("80m"))
                                        {
                                            console.CurrentBand = Band.B80M;
                                        }
                                        else if (command.Contains("60m"))
                                        {
                                            console.CurrentBand = Band.B60M;
                                        }
                                        else if (command.Contains("40m"))
                                        {
                                            console.CurrentBand = Band.B40M;
                                        }
                                        else if (command.Contains("30m"))
                                        {
                                            console.CurrentBand = Band.B30M;
                                        }
                                        else if (command.Contains("20m"))
                                        {
                                            console.CurrentBand = Band.B20M;
                                        }
                                        else if (command.Contains("17m"))
                                        {
                                            console.CurrentBand = Band.B17M;
                                        }
                                        else if (command.Contains("15m"))
                                        {
                                            console.CurrentBand = Band.B15M;
                                        }
                                        else if (command.Contains("12m"))
                                        {
                                            console.CurrentBand = Band.B12M;
                                        }
                                        else if (command.Contains("10m"))
                                        {
                                            console.CurrentBand = Band.B10M;
                                        }
                                        else if (command.Contains("6m"))
                                        {
                                            console.CurrentBand = Band.B6M;
                                        }
                                        else if (command.Contains("2m"))
                                        {
                                            console.CurrentBand = Band.B2M;
                                        }

                                        text = "1";
                                    }
                                    else if (command.Contains("AGC"))
                                    {
                                        int[] parm2 = new int[1];
                                        AGCMode new_mode = AGCMode.MED;

                                        if (command.Contains("Fixd"))
                                        {
                                            new_mode = AGCMode.FIXD;
                                        }
                                        else if (command.Contains("Long"))
                                        {
                                            new_mode = AGCMode.LONG;
                                        }
                                        else if (command.Contains("Slow"))
                                        {
                                            new_mode = AGCMode.SLOW;
                                        }
                                        else if (command.Contains("Med"))
                                        {
                                            new_mode = AGCMode.MED;
                                        }
                                        else if (command.Contains("Fast"))
                                        {
                                            new_mode = AGCMode.FAST;
                                        }
                                        else if (command.Contains("Custom"))
                                        {
                                            new_mode = AGCMode.CUSTOM;
                                        }

                                        console.Invoke(new CATCrossThreadCallback(console.CATCallback),
                                            "AGC mode", (int)new_mode, parm2, "");

                                        text = "1";
                                    }
                                    else if (command.Contains("Display"))
                                    {
                                        int mode = 0;

                                        if (command.Contains("Panadapter"))
                                        {
                                            mode = (int)DisplayMode.PANADAPTER;
                                        }
                                        else if (command.Contains("Panafall"))
                                        {
                                            mode = (int)DisplayMode.PANAFALL;
                                        }
                                        else if (command.Contains("Panafall_inv"))
                                        {
                                            mode = (int)DisplayMode.PANAFALL_INV;
                                        }
                                        else if (command.Contains("Panascope"))
                                        {
                                            mode = (int)DisplayMode.PANASCOPE;
                                        }
                                        else if (command.Contains("Scope"))
                                        {
                                            mode = (int)DisplayMode.SCOPE;
                                        }
                                        else if (command.Contains("Waterfall"))
                                        {
                                            mode = (int)DisplayMode.WATERFALL;
                                        }
                                        else if (command.Contains("Phase"))
                                        {
                                            mode = (int)DisplayMode.PHASE;
                                        }
                                        else if (command.Contains("Phase2"))
                                        {
                                            mode = (int)DisplayMode.PHASE2;
                                        }
                                        else if (command.Contains("Off"))
                                        {
                                            mode = (int)DisplayMode.OFF;
                                        }

                                        int[] parm2 = new int[1];
                                        console.Invoke(new CATCrossThreadCallback(console.CATCallback), "Display Mode",
                                            (int)mode, parm2, "");

                                        text = "1";
                                    }
                                    else if (command.Contains("Preamp"))
                                    {
                                        text = "1";
                                    }
                                    else if (command.Contains("DSP~Fltr") || command.Contains("DSP~Filtr"))
                                    {
                                        if (command.Contains("F10"))
                                        {
                                            console.CurrentFilter = Filter.F10;
                                        }
                                        else if (command.Contains("F1"))
                                        {
                                            console.CurrentFilter = Filter.F1;
                                        }
                                        else if (command.Contains("F2"))
                                        {
                                            console.CurrentFilter = Filter.F2;
                                        }
                                        else if (command.Contains("F3"))
                                        {
                                            console.CurrentFilter = Filter.F3;
                                        }
                                        else if (command.Contains("F4"))
                                        {
                                            console.CurrentFilter = Filter.F4;
                                        }
                                        else if (command.Contains("F5"))
                                        {
                                            console.CurrentFilter = Filter.F5;
                                        }
                                        else if (command.Contains("F6"))
                                        {
                                            console.CurrentFilter = Filter.F6;
                                        }
                                        else if (command.Contains("F7"))
                                        {
                                            console.CurrentFilter = Filter.F7;
                                        }
                                        else if (command.Contains("F8"))
                                        {
                                            console.CurrentFilter = Filter.F8;
                                        }
                                        else if (command.Contains("F9"))
                                        {
                                            console.CurrentFilter = Filter.F9;
                                        }
                                        else if (command.Contains("VAR1"))
                                        {
                                            console.CurrentFilter = Filter.VAR1;
                                        }
                                        else if (command.Contains("VAR2"))
                                        {
                                            console.CurrentFilter = Filter.VAR2;
                                        }

                                        text = "1";
                                    }
                                    else
                                    {
                                        text = "1";
                                    }
                                }
                                else if (command.StartsWith("[0] get dropdown-text"))
                                {
                                    if (command.Contains("{Mode}"))
                                    {
                                            text = "Mode: " + console.CurrentDSPMode.ToString();
                                    }
                                    else if (command.Contains("{Band}"))
                                    {
                                            text = console.CurrentBand.ToString();
                                            text = text.Replace("B", "");
                                            text = text.Replace("M", "m");
                                            text = "Band: " + text;
                                    }
                                    else if (command.Contains("{AGC}"))
                                    {
                                            text = "AGC: " + console.current_agc_mode.ToString();
                                    }
                                    else if (command.Contains("{Display}"))
                                    {
                                            text = "Display: " + console.CurrentDisplayMode.ToString();
                                    }
                                    else if (command.Contains("{Preamp}"))
                                    {
                                            text = "Preamp: " + "off";
                                    }
                                    else if (command.Contains("{DSP~Fltr}") || command.Contains("{DSP~Filtr}"))
                                    {
                                            text = "Filter: " + Math.Abs(console.FilterHighValue - console.FilterLowValue).ToString() + "Hz";
                                    }
                                }
                                else if (command.StartsWith("[0] Get SMeter-Main"))
                                {
                                    ascii_buffer = new byte[2048];
                                    int sm = 0;

                                    {
                                        float num = 0f;

                                        if (console.PowerOn)
                                            num = DttSP.CalculateRXMeter(0, 0, DttSP.MeterType.AVG_SIGNAL_STRENGTH);

                                        num += console.MultimeterCalOffset + console.filter_size_cal_offset;

                                        num = Math.Max(-140, num);
                                        num = Math.Min(-10, num);
                                        sm = (int)num;
                                        text = sm.ToString() + "dB";
                                    }
                                }
                                else if (command.StartsWith("[0] set frequency-hz"))
                                {
                                    int[] parm2 = new int[1];
                                    string[] s = command.Split(' ');
                                    double vfoA = double.Parse(s[s.Length - 1]);
                                    vfoA /= 1e6;

                                    if (vfoA > 1.0)
                                    {
                                        if (vfoA > (console.MaxFreq) || vfoA < console.MinFreq)
                                        {
                                            if (console.BandByFreq(vfoA) != console.CurrentBand)
                                                console.SaveBand();

                                            console.cat_vfoa = true;
                                            console.Invoke(new CATCrossThreadCallback(console.CATCallback), "VFOA freq", 0, parm2, (vfoA * 1e6).ToString());
                                            console.cat_vfoa = false;

                                            if ((Audio.VACEnabled && Audio.VACDirectI_Q && Audio.VAC_RXshift_enabled) ||
                                                (Audio.PrimaryDirectI_Q && Audio.Primary_RXshift_enabled))
                                            {
                                                console.cat_losc = true;
                                                console.LOSCFreq = vfoA - Audio.RXShift / 1e6;
                                                console.cat_losc = false;
                                            }
                                            else
                                            {
                                                console.cat_losc = true;
                                                console.LOSCFreq = vfoA - 0.015;
                                                console.cat_losc = false;
                                            }

                                            if (console.VFOAFreq != vfoA)
                                                console.Invoke(new CATCrossThreadCallback(console.CATCallback), "VFOA freq", 0, parm2, (vfoA * 1e6).ToString());

                                            console.cat_vfoa = false;
                                        }
                                        else
                                        {
                                            console.cat_vfoa = true;
                                            console.Invoke(new CATCrossThreadCallback(console.CATCallback), "VFOA freq", 0, parm2, (vfoA * 1e6).ToString());

                                            if ((Audio.VACEnabled && Audio.VACDirectI_Q && Audio.VAC_RXshift_enabled) ||
                                                (Audio.PrimaryDirectI_Q && Audio.Primary_RXshift_enabled))
                                            {
                                                console.cat_losc = true;
                                                console.LOSCFreq = vfoA - Audio.RXShift / 1e6;
                                                console.cat_losc = false;
                                            }

                                            if (console.VFOAFreq != vfoA)
                                                console.Invoke(new CATCrossThreadCallback(console.CATCallback), "VFOA freq", 0, parm2, (vfoA * 1e6).ToString());

                                            console.cat_vfoa = false;
                                        }
                                    }

                                    text = "1";
                                }
                                else if (command.StartsWith("[0] set frequencies"))
                                {
                                    int[] parm2 = new int[1];
                                    string[] s = command.Split(' ');
                                    double vfoA = double.Parse(s[s.Length - 2]);
                                    double VfoB = double.Parse(s[s.Length - 1]);
                                    vfoA /= 1e6;
                                    VfoB /= 1e6;

                                    if (vfoA > 1.0)
                                    {
                                        if (vfoA > (console.MaxFreq) || vfoA < console.MinFreq)
                                        {
                                            if (console.BandByFreq(vfoA) != console.CurrentBand)
                                                console.SaveBand();

                                            console.cat_vfoa = true;
                                            console.Invoke(new CATCrossThreadCallback(console.CATCallback), "VFOA freq", 0, parm2, (vfoA * 1e6).ToString());
                                            console.cat_vfoa = false;

                                            if ((Audio.VACEnabled && Audio.VACDirectI_Q && Audio.VAC_RXshift_enabled) ||
                                                (Audio.PrimaryDirectI_Q && Audio.Primary_RXshift_enabled))
                                            {
                                                console.cat_losc = true;
                                                console.LOSCFreq = vfoA - Audio.RXShift / 1e6;
                                                console.cat_losc = false;
                                            }
                                            else
                                            {
                                                console.cat_losc = true;
                                                console.LOSCFreq = vfoA - 0.015;
                                                console.cat_losc = false;
                                            }

                                            if (console.VFOAFreq != vfoA)
                                            {
                                                console.cat_vfoa = true;
                                                console.Invoke(new CATCrossThreadCallback(console.CATCallback), "VFOA freq", 0, parm2, (vfoA*1e6).ToString());
                                                console.cat_vfoa = false;
                                            }
                                        }
                                        else
                                        {
                                            console.cat_vfoa = true;
                                            console.Invoke(new CATCrossThreadCallback(console.CATCallback), "VFOA freq", 0, parm2, (vfoA * 1e6).ToString());

                                            if ((Audio.VACEnabled && Audio.VACDirectI_Q && Audio.VAC_RXshift_enabled) ||
                                                (Audio.PrimaryDirectI_Q && Audio.Primary_RXshift_enabled))
                                            {
                                                console.cat_losc = true;
                                                console.LOSCFreq = vfoA - Audio.RXShift / 1e6;
                                                console.cat_losc = false;
                                            }

                                            if (console.VFOAFreq != vfoA)
                                                console.Invoke(new CATCrossThreadCallback(console.CATCallback), "VFOA freq", 0, parm2, (vfoA * 1e6).ToString());

                                            console.cat_vfoa = false;
                                        }
                                    }

                                    if (VfoB <= (console.MaxFreq) && VfoB >= console.MinFreq)
                                    {
                                        console.cat_vfob = true;
                                        console.Invoke(new CATCrossThreadCallback(console.CATCallback), "VFOB freq", 0, parm2, (VfoB * 1e6).ToString());
                                        console.cat_vfob = false;
                                    }

                                    text = "1";
                                }
                                else if (command.StartsWith("[0] get slider-range"))
                                {
                                    if (command.Contains("AF~gain") || command.Contains("GSDR AF"))
                                        text = "0,100,0";
                                    else if (command.Contains("RF~power") || command.Contains("GSDR RF"))
                                        text = "0,120,0";
                                    else if (command.Contains("Mic~gain") || command.Contains("GSDR MIC"))
                                        text = "0,100,0";
                                    else if (command.Contains("Power") || command.Contains("GSDR PWR"))
                                        text = "0,100,0";
                                    else if (command.Contains("Squelch") || command.Contains("GSDR SQL1") ||
                                        command.Contains("SQL2"))
                                        text = "0,160,0";
                                    else if (command.Contains("NB1~threshold") || command.Contains("GSDR NB1"))
                                        text = "0,200,0";
                                    else if (command.Contains("NB2~threshold") || command.Contains("GSDR NB2"))
                                        text = "0,100,0";
                                    else
                                        text = "0,100,0";
                                }
                                else if (command.StartsWith("[0] get slider-pos"))
                                {
                                    if (command.Contains("AF"))
                                    {
                                        int af = Math.Min(99, console.AF);
                                        af = Math.Min(100, af);
                                        text = af.ToString();
                                        text += "," + text;

                                    }
                                    else if (command.Contains("RF~gain") || command.Contains("GSDR RF"))
                                    {
                                        int rf = Math.Max(0, console.RF);
                                        rf = Math.Min(120, rf);
                                        text = rf.ToString();
                                        text +=  "," + text;
                                    }
                                    else if (command.Contains("RF~power") || command.Contains("GSDR PWR"))
                                    {
                                        int af = Math.Max(0, (int)console.PWR);
                                        af = Math.Min(100, af);
                                        text = af.ToString();
                                        text += "," + text;
                                    }
                                    else if (command.Contains("SQL1"))
                                    {
                                        int af = Math.Max(0, console.SquelchMainRX);
                                        af = Math.Min(160, af);
                                        text = af.ToString();
                                        text += "," + text;
                                    }
                                    else if (command.Contains("SQL2"))
                                    {
                                        int af = Math.Max(0, console.SquelchSubRX);
                                        af = Math.Min(160, af);
                                        text = af.ToString();
                                        text += "," + text;
                                    }
                                    else if (command.Contains("Mic") || command.Contains("GSDR MIC"))
                                    {
                                        int af = Math.Max(0, console.Mic);
                                        af = Math.Min(100, af);
                                        text = af.ToString();
                                        text += "," + text;
                                    }
                                    else
                                    {
                                        text = "0,0";
                                    }
                                }
                                else if (command.StartsWith("[0] Set slider-pos") || command.StartsWith("[0] set slider-pos"))
                                {
                                    if (command.Contains("AF"))
                                    {
                                        int[] parm2 = new int[1];
                                        string[] s = command.Split(' ');
                                        int val = Int32.Parse(s[s.Length - 1]);
                                        val = Math.Min(100, val);
                                        val = Math.Max(0, val);
                                        console.Invoke(new CATCrossThreadCallback(console.CATCallback), "AF", val, parm2, "");
                                    }
                                    else if (command.Contains("RF~gain") || command.Contains("GSDR RF"))
                                    {
                                        int[] parm2 = new int[1];
                                        string[] s = command.Split(' ');
                                        int val = Int32.Parse(s[s.Length - 1]);
                                        val = Math.Min(120, val);
                                        val = Math.Max(0, val);
                                        console.Invoke(new CATCrossThreadCallback(console.CATCallback), "RF", val, parm2, "");
                                    }
                                    else if (command.Contains("PWR") || command.Contains("RF~power"))
                                    {
                                        int[] parm2 = new int[1];
                                        string[] s = command.Split(' ');
                                        int val = Int32.Parse(s[s.Length - 1]);
                                        val = Math.Min(100, val);
                                        val = Math.Max(0, val);
                                        console.Invoke(new CATCrossThreadCallback(console.CATCallback), "PWR", val, parm2, "");
                                    }
                                    else if (command.Contains("SQL1"))
                                    {
                                        int[] parm2 = new int[1];
                                        string[] s = command.Split(' ');
                                        int val = Int32.Parse(s[s.Length - 1]);
                                        val = Math.Min(160, val);
                                        val = Math.Max(0, val);
                                        console.Invoke(new CATCrossThreadCallback(console.CATCallback), "SQL VFOA", val, parm2, "");
                                    }
                                    else if (command.Contains("SQL2"))
                                    {
                                        int[] parm2 = new int[1];
                                        string[] s = command.Split(' ');
                                        int val = Int32.Parse(s[s.Length - 1]);
                                        val = Math.Min(160, val);
                                        val = Math.Max(0, val);
                                        console.Invoke(new CATCrossThreadCallback(console.CATCallback), "SQL VFOB", val, parm2, "");
                                    }
                                    else if (command.Contains("MIC") || command.Contains("Mic~gain"))
                                    {
                                        int[] parm2 = new int[1];
                                        string[] s = command.Split(' ');
                                        int val = Int32.Parse(s[s.Length - 1]);
                                        val = Math.Min(100, val);
                                        val = Math.Max(0, val);
                                        console.Invoke(new CATCrossThreadCallback(console.CATCallback), "MIC", val, parm2, "");
                                    }

                                    text = "1";
                                }
                                else if (command.StartsWith("[0] get button-select"))
                                {
                                    text = "0";

                                    if (command.Contains("START") || command.Contains("ON"))
                                    {
                                        if (console.PowerOn)
                                            text = "1";
                                    }
                                    else if (command.Contains("TX"))
                                    {
                                        if (console.MOX)
                                            text = "1";
                                    }
                                    else if (command.Contains("TUN"))
                                    {
                                        if (console.TUN)
                                            text = "1";
                                    }
                                    else if (command.Contains("SPLIT"))
                                    {
                                        if (console.SplitAB_TX)
                                            text = "1";
                                    }
                                    else if (command.Contains("SQL1"))
                                    {
                                        if (console.CATSquelch == 1)
                                            text = "1";
                                    }
                                    else if (command.Contains("SQL2"))
                                    {
                                        if (console.CATSquelchSubRX == 1)
                                            text = "1";
                                    }
                                    else if (command.Contains("AF"))
                                    {
                                        if (console.CATAFPreampStatus == 1)
                                            text = "1";
                                    }
                                    else if (command.Contains("RF"))
                                    {
                                        if (console.CATRFPreampStatus == 1)
                                            text = "1";
                                    }
                                    else if (command.Contains("ATT"))
                                    {
                                        if (console.CATATTStatus == 1)
                                            text = "1";
                                    }
                                    else if (command.Contains("MSG1"))
                                    {
                                        text = parser.Get("ZZF1;");
                                    }
                                    else if (command.Contains("MSG2"))
                                    {
                                        text = parser.Get("ZZF2;");
                                    }
                                    else if (command.Contains("MSG3"))
                                    {
                                        text = parser.Get("ZZF3;");
                                    }
                                    else if (command.Contains("MSG4"))
                                    {
                                        text = parser.Get("ZZF4;");
                                    }
                                    else if (command.Contains("MSG5"))
                                    {
                                        text = parser.Get("ZZF5;");
                                    }
                                    else if (command.Contains("MSG6"))
                                    {
                                        text = parser.Get("ZZF6;");
                                    }

                                    if (text == "false")
                                        text = "0";
                                    else if (text == "true")
                                        text = "1";
                                }
                                else if (command.StartsWith("[0] set button-select"))
                                {
                                    if (command.Contains("MSG1"))
                                    {
                                        parser.Get("ZZF11;");
                                    }
                                    else if (command.Contains("MSG2"))
                                    {
                                        parser.Get("ZZF21;");
                                    }
                                    else if (command.Contains("MSG3"))
                                    {
                                        parser.Get("ZZF31;");
                                    }
                                    else if (command.Contains("MSG4"))
                                    {
                                        parser.Get("ZZF41;");
                                    }
                                    else if (command.Contains("MSG5"))
                                    {
                                        parser.Get("ZZF51;");
                                    }
                                    else if (command.Contains("MSG6"))
                                    {
                                        parser.Get("ZZF61;");
                                    }
                                    else if (command.Contains("START") || command.Contains("ON"))
                                    {
                                        if (command.Contains("START 1") || command.Contains("ON 1"))
                                            console.chkPower.Checked = true;
                                        else
                                            console.chkPower.Checked = false;
                                    }
                                    else if (command.Contains("TX"))
                                    {
                                        if (command.Contains("TX 1"))
                                            console.MOX = true;
                                        else
                                            console.MOX = false;
                                    }
                                    else if (command.Contains("TUN"))
                                    {
                                        if (command.Contains("TUN 1"))
                                            console.TUN = true;
                                        else
                                            console.TUN = false;
                                    }
                                    else if (command.Contains("SPLIT") || command.Contains("Split"))
                                    {
                                        if (command.Contains("SPLIT 1") || command.Contains("Split 1"))
                                            console.SplitAB_TX = true;
                                        else
                                            console.SplitAB_TX = false;
                                    }
                                    else if (command.Contains("SQL1"))
                                    {
                                        if (command.Contains("SQL1 1"))
                                            console.CATSquelch = 1;
                                        else
                                            console.CATSquelch = 0;
                                    }
                                    else if (command.Contains("SQL2"))
                                    {
                                        if (command.Contains("SQL2 1"))
                                            console.CATSquelchSubRX = 1;
                                        else
                                            console.CATSquelchSubRX = 0;
                                    }
                                    else if (command.Contains("AF"))
                                    {
                                        if (command.Contains("AF 1"))
                                            console.CATAFPreampStatus = 1;
                                        else
                                            console.CATAFPreampStatus = 0;
                                    }
                                    else if (command.Contains("RF"))
                                    {
                                        if (command.Contains("RF 1"))
                                            console.CATRFPreampStatus = 1;
                                        else
                                            console.CATRFPreampStatus = 0;
                                    }
                                    else if (command.Contains("ATT"))
                                    {
                                        if (command.Contains("ATT 1"))
                                            console.CATATTStatus = 1;
                                        else
                                            console.CATATTStatus = 0;
                                    }
                                    else if (command.Contains("MR"))
                                    {
                                        int[] parm2 = new int[1];
                                        console.Invoke(new CATCrossThreadCallback(console.CATCallback), "Memory recall", 0, parm2, "");
                                    }
                                    else if (command.Contains("MC"))
                                    {
                                        int[] parm2 = new int[1];
                                        console.Invoke(new CATCrossThreadCallback(console.CATCallback), "Memory clear", 0, parm2, "");
                                    }
                                    else if (command.Contains("MS"))
                                    {
                                        int[] parm2 = new int[1];
                                        console.Invoke(new CATCrossThreadCallback(console.CATCallback), "Memory store", 0, parm2, "");
                                    }
                                    else if (command.Contains("VFO~A"))
                                    {
                                        int[] parm2 = new int[1];
                                        console.Invoke(new CATCrossThreadCallback(console.CATCallback), "Restore VFOA", 0, parm2, "");
                                    }                                    

                                    text = "1";
                                }
                                else
                                {
                                    text = "1";
                                }

                            }
                            break;
                    }

                    if (multi)
                    {
                        final += text;

                        if (command != c[c.Length - 1])
                            final += "\t";
                    }
                    else
                        final = text;
                }

                for (int i = 16; i < 2048; i++)
                {
                    data[i] = 0;
                }

                ascii_buffer = new byte[4096];
                ascii_buff.GetBytes(final, 0, final.Length, ascii_buffer, 0);
                ascii_buffer = Encoding.Convert(Encoding.UTF8, Encoding.Unicode, ascii_buffer, 0, final.Length);

                for (int i = 0; i < ascii_buffer.Length; i++)
                {
                    data[i + 16] = ascii_buffer[i];
                }

                short byte_count = 0;
                byte_count = (short)(header_lenght + ascii_buffer.Length + 4);
                data[0] = (byte)(byte_count & 0x00ff);
                data[1] = (byte)(byte_count>>8 & 0x00ff);
                sock.Send(data, byte_count, SocketFlags.None);
            }
            catch (Exception ex)
            {
                Debug.Write(ex.ToString());

                if (debug && !console.ConsoleClosing)
                    console.Invoke(new DebugCallbackFunction(console.DebugCallback),
                        "HRDServerProcessData error!\n" + ex.ToString());
            }
        }

        public string ProcessData(byte[] data, int count)
        {
            ASCIIEncoding buffer = new ASCIIEncoding();
            string command_type;
            string version;
            string password;
            string CommBuffer = "";
            string answer = "";

            try
            {
                if (count < 16)
                    return "";

                string[] vals = buffer.GetString(data, 0, 16).Split(';');

                if (vals.Length < 3)
                    return "";

                command_type = vals[0];
                version = vals[1];
                password = vals[2];

                if (command_type == "CAT" && version == CATversion && CATpassword == password)
                {
                    string cmd = "";
                    answer = "CAT;" + CATversion + ";" + CATpassword + ";";
                    CommBuffer += Regex.Replace(buffer.GetString(data, 16, Math.Min(data.Length - 16, count - 16)),
                        @"[^\w\;.]", "");
                    Regex rex = new Regex(".*?;");

                    for (Match m = rex.Match(CommBuffer); m.Success; m = m.NextMatch())
                    {
                        cmd += m.Value;
                        answer += parser.Get(m.Value);
                        Debug.WriteLine(m.Value);
                        Debug.WriteLine(answer);
                        CommBuffer = CommBuffer.Replace(m.Value, "");
                    }

                    if (debug && !console.ConsoleClosing)
                    {
                        console.Invoke(new DebugCallbackFunction(console.DebugCallback),
                            "CAT command: " + cmd + "\n" +
                            "CAT answer: " + answer);
                    }
                }

                return answer;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                return "";
            }
        }

        private void PollStatusThread()
        {
            try
            {
                byte[] buffer = new byte[256];
                ASCIIEncoding ascii_buffer = new ASCIIEncoding();
                string header = "CAT01";
                string text;
                bool once = false;

                while (run_server)
                {                   
                    if (client1 != null && client1.Connected)
                    {
                        once = false;
                        text = header + CATpassword +
                            "ZZPS;ZZAG;ZZBI;ZZCL;ZZTX;ZZCP;ZZCS;ZZDA;ZZNR;" +
                            "ZZFI;ZZGT;ZZID;ZZCM;ZZMD;ZZME;ZZSP;" +
                            "ZZMT;ZZNB;ZZNL;ZZNM;ZZPA;ZZPL;ZZRI;" +
                            "ZZRM;ZZSF;ZZSM0;ZZSO;ZZSQ;ZZST;ZZTH;ZZTL;ZZVN;" +
                            "ZZSO;ZZXF;ZZRS;ZZST;ZZSV;ZZCB;ZZVG;ZZAR;" +
                            "ZZFO;ZZFA;ZZFB;ZZPC;ZZS1;";
                        ascii_buffer.GetBytes(text, 0, text.Length, buffer, 0);
                        client1.Send(buffer, SocketFlags.None);
                    }
                    else
                    {
                        if (!once)
                        {
                            client1.Shutdown(SocketShutdown.Both);
                            client1.Close();
                            once = true;
                        }
                    }

                    Thread.Sleep(10000);        // 5s refresh period
                }
            }
            catch (Exception ex)
            {
                client1.Close();
                Debug.Write("Error!\n" + ex.ToString());
            }
        }

        private void ServerWatchDogTimerTick(object sender, System.EventArgs e)
        {
            try
            {
                if (run_watchdog)
                {
                    if (client1 != null && !client1.Connected)
                    {
                        run_watchdog = false;
                        server_event.Set();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Write(ex.ToString());
            }
        }

        #endregion
    }

    #endregion

    #region CAT client class

    /// <summary>
    /// CAT Ethernet client class
    /// </summary>
    public class CAToverEthernetClient : IDisposable
    {
        #region Variable

        private Socket ClientSocket;
        private Console console;
        private string ServerIPAddress = "127.0.0.1";
        private int ServerPort = 5000;
        private string LocalIPAddress = "127.0.0.1";
        private delegate void CATCallback(byte[] data);
        private byte[] receive_buffer;
        private byte[] send_buffer;
        private string CATpassword = "12345678";
        private string CATversion = "01";
        private CATParser parser;
        private static AutoResetEvent send_event;
        private bool run_CAT_send_thread = false;
        private bool run_CAT_client = false;
        private Thread CAT_send_thread;
        private Thread CAT_thread;
        private Mutex send_mutex = null;
        private System.Windows.Forms.Timer timeout_timer = null;
        public bool IPv6_enabled = false;
        public int CollectionTime = 100;
        private int byte_to_send = 0;
        private int out_data_index = 0;
        private bool header_added = false;
        private delegate void DebugCallbackFunction(string name);
        public bool VoIP_enabled = true;
        public VoIP voip;
        public float[] display_data = new float[4096];

        #endregion

        #region properties

        private bool debug = false;
        public bool Debug_enable
        {
            get { return debug; }
            set 
            {
                debug = value;

                if (VoIP_enabled && voip != null)
                    voip.debug = value;
            }
        }

        #endregion

        #region constructor/destructor

        public CAToverEthernetClient(Console c)
        {
            try
            {
                console = c;
                parser = new CATParser(console);
                receive_buffer = new byte[2048];
                send_buffer = new byte[2048];
                send_event = new AutoResetEvent(false);
                //connect_event = new AutoResetEvent(false);
                send_mutex = new Mutex();
                timeout_timer = new System.Windows.Forms.Timer();
                timeout_timer.Tick += new System.EventHandler(SendEventTimerTick);
                timeout_timer.Enabled = true;
            }
            catch (Exception ex)
            {
                Debug.Write(ex.ToString());

                if (debug && !console.ConsoleClosing)
                    console.Invoke(new DebugCallbackFunction(console.DebugCallback),
                        "Ethernet client constructor error!\n" + ex.ToString());
            }
        }

        public virtual void Dispose()
        {
            if (VoIP_enabled)
            {
                if (voip != null)
                    voip.Dispose();
            }
            else
            {
                if (ClientSocket != null)
                    ClientSocket.Close(1000);

                timeout_timer.Stop();
            }
        }

        #endregion

        #region Start/Stop

        public bool Start(string ServerAddress, int serverPort,
            string LocalAddress, string password)
        {
            try
            {
                ServerIPAddress = ServerAddress;
                ServerPort = serverPort;
                LocalIPAddress = LocalAddress;
                CATpassword = password;
                run_CAT_client = true;
                SetupSocket();

                return true;
            }
            catch (Exception ex)
            {
                run_CAT_client = false;
                Debug.Write(ex.ToString());
                return false;
            }
        }

        public bool Stop()
        {
            try
            {
                if (VoIP_enabled)
                {
                    if (voip != null)
                        voip.Dispose();
                }
                else
                {
                    run_CAT_send_thread = false;
                    send_event.Set();
                    console.btnNetwork.BackColor = Color.Red;
                    console.SetupForm.txtCATServerIPAddress.ForeColor = Color.Red;
                    run_CAT_client = false;     // exit connection thread

                    if (ClientSocket != null && ClientSocket.Connected)
                    {
                        ClientSocket.Shutdown(SocketShutdown.Both);
                        ClientSocket.Close();
                    }
                    else if (ClientSocket != null)
                        ClientSocket.Close();
                }

                return true;
            }
            catch (System.Exception ex)
            {
                Debug.Print(ex.Message);

                if (debug && !console.ConsoleClosing)
                    console.Invoke(new DebugCallbackFunction(console.DebugCallback),
                        "Close CAT client error!\n" + ex.ToString());

                return false;
            }
        }

        #endregion

        #region misc function

        private bool SetupSocket()
        {
            try
            {
                if (VoIP_enabled)
                {
                    /*voip = new VoIP(console);
                    voip.debug = debug;
                    voip.OpMode = VoIP_mode.Client;
                    voip.Text = "Remote client";
                    voip.Show();
                    Win32.SetWindowPos(voip.Handle.ToInt32(),
                        -1, voip.Left, voip.Top, voip.Width, voip.Height, 0);
                    voip.Start(LocalIPAddress, ServerPort);

                    if (debug && !console.ConsoleClosing)
                        console.Invoke(new DebugCallbackFunction(console.DebugCallback),
                            "VoIP client started!");*/
                }
                else
                {
                    console.btnNetwork.BackColor = Color.Red;
                    console.SetupForm.txtCATServerIPAddress.ForeColor = Color.Red;

                    IPHostEntry ipHostInfo = Dns.GetHostEntry(ServerIPAddress);
                    IPAddress ipAddress = ipHostInfo.AddressList[0];
                    IPEndPoint ipepServer = new IPEndPoint(ipAddress, ServerPort);

                    switch (console.WinVer)
                    {
                        case WindowsVersion.Windows2000:
                        case WindowsVersion.WindowsXP:
                            {
                                ClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                                ClientSocket.Blocking = false;
                                ClientSocket.BeginConnect(ServerIPAddress, ServerPort, new AsyncCallback(ConnectCallback),
                                    ClientSocket);
                            }
                            break;
                        case WindowsVersion.WindowsVista:
                        case WindowsVersion.Windows7:
                        case WindowsVersion.Windows8:
                            {
                                if (IPv6_enabled && ipAddress.AddressFamily == AddressFamily.InterNetworkV6)
                                {
                                    ClientSocket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
                                    ClientSocket.Blocking = false;
                                    ClientSocket.BeginConnect(ipepServer, new AsyncCallback(ConnectCallback), ClientSocket);
                                }
                                else
                                {
                                    ClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                                    ClientSocket.Blocking = false;
                                    ClientSocket.BeginConnect(ServerIPAddress, ServerPort, new AsyncCallback(ConnectCallback),
                                        ClientSocket);
                                }
                            }
                            break;
                    }

                    if (debug && !console.ConsoleClosing)
                        console.Invoke(new DebugCallbackFunction(console.DebugCallback),
                            "VoIP client started!");
                }

                return true;
            }
            catch (Exception ex)
            {
                if (debug && !console.ConsoleClosing)
                    console.Invoke(new DebugCallbackFunction(console.DebugCallback),
                        "SetupSocket client error!\n" + ex.ToString());

                MessageBox.Show("Error!Check your CAT client network data!\n\n" + ex.ToString());
                return false;
            }
        }

        private void ConnectCallback(IAsyncResult result)
        {
            try
            {
                Socket sock = (Socket)result.AsyncState;

                if (sock.Connected)
                {
                    Debug.Write("Connected!\n");

                    if (debug && !console.ConsoleClosing)
                        console.Invoke(new DebugCallbackFunction(console.DebugCallback), "Connected!");

                    sock.BeginReceive(receive_buffer, 0, receive_buffer.Length, SocketFlags.None,
                        new AsyncCallback(ReceiveCallback), sock);

                    run_CAT_send_thread = true;
                    CAT_send_thread = new Thread(new ThreadStart(SendThread));
                    CAT_send_thread.Name = "CAT client send thread";
                    CAT_send_thread.IsBackground = true;
                    CAT_send_thread.Priority = ThreadPriority.Normal;
                    CAT_send_thread.Start();
                    console.btnNetwork.BackColor = Color.Green;
                    console.SetupForm.txtCATServerIPAddress.ForeColor = Color.Green;
                }
                else
                {
                    console.SetupForm.txtCATServerIPAddress.ForeColor = Color.Red;
                    console.btnNetwork.BackColor = Color.Red;

                    if (run_CAT_client)
                        SetupSocket();
                }
            }
            catch (Exception ex)
            {
                console.SetupForm.txtCATServerIPAddress.ForeColor = Color.Red;
                Debug.Write(ex.Message);

                if (run_CAT_client)
                    SetupSocket();
            }
        }

        private void ReceiveCallback(IAsyncResult result)
        {
            try
            {
                Socket sock = (Socket)result.AsyncState;
                int num_read = 0;
                string response = "";

                if (sock.Connected)
                {
                    num_read = sock.EndReceive(result);
                }

                if (num_read > 0)
                {
                    ProcessData(receive_buffer, num_read, out response);

                    for (int i = 0; i < receive_buffer.Length; i++)
                        receive_buffer[i] = 0;

                    sock.BeginReceive(receive_buffer, 0, receive_buffer.Length, SocketFlags.None,
                        new AsyncCallback(ReceiveCallback), sock);
                }
                else
                {
                    if (run_CAT_client)
                    {
                        send_event.Set();
                        Debug.Write("Disconnected!\n");

                        if (debug && !console.ConsoleClosing)
                            console.Invoke(new DebugCallbackFunction(console.DebugCallback), "Disconnected!");

                        if (ClientSocket != null)
                        {
                            sock.Shutdown(SocketShutdown.Both);
                            sock.Close();
                        }

                        if (run_CAT_client)
                            SetupSocket();
                    }
                    else
                    {

                    }
                }               
            }
            catch (SocketException socketException)
            {
                send_event.Set();

                if (socketException.ErrorCode == 10054)
                {
                    ClientSocket.Close(1000);

                    if (run_CAT_client)
                        SetupSocket();
                }
            }
            catch (ObjectDisposedException sockEx)
            {
                Debug.Write(sockEx.ToString() + "\n");
                send_event.Set();
                ClientSocket.Close(1000);

                if (run_CAT_client)
                    SetupSocket();
            }
        }

        public void ClientServerSync(string data)               // request for sending to server
        {
            ASCIIEncoding buffer = new ASCIIEncoding();
            string command_type = "CAT;";
            string version = CATversion + ";";
            string CommBuffer = "";

            try
            {
                CommBuffer += Regex.Replace(data, @"[^\w\;.]", "");
                Regex rex = new Regex(".*?;");

                string answer = "";
                byte[] out_string = new byte[1024];
                byte[] header = new byte[16];
                int out_string_index = 0;

                if (VoIP_enabled && voip != null)
                {
                    answer = command_type + version + CATpassword + ";";

                    for (Match m = rex.Match(CommBuffer); m.Success; m = m.NextMatch())
                    {
                        answer += parser.Get(m.Value);
                        Debug.WriteLine(m.Value);
                        Debug.WriteLine(answer);
                    }

                    voip.SendMessage(answer, "CAT");
                }
                else
                {
                    send_mutex.WaitOne();

                    if (!header_added)
                    {
                        buffer.GetBytes(command_type + version + CATpassword, 0, 16, header, 0);
                        string header_string = "";
                        header_string = buffer.GetString(header, 0, 16);
                        buffer.GetBytes(header_string, 0, 13, send_buffer, 0);
                        header_added = true;
                        byte_to_send = 16;
                        out_data_index = 16;
                    }

                    for (Match m = rex.Match(CommBuffer); m.Success; m = m.NextMatch())
                    {
                        answer = parser.Get(m.Value);
                        Debug.WriteLine(m.Value);
                        Debug.WriteLine(answer);
                        buffer.GetBytes(answer, 0, answer.Length, out_string, out_string_index);
                        CommBuffer = CommBuffer.Replace(m.Value, "");
                        out_string_index += answer.Length;
                    }

                    string tmp_string = "";
                    tmp_string = buffer.GetString(out_string, 0, answer.Length);
                    buffer.GetBytes(tmp_string, 0, answer.Length, send_buffer, out_data_index);
                    out_data_index += answer.Length;
                    byte_to_send = out_data_index;

                    timeout_timer.Interval = CollectionTime;
                    timeout_timer.Start();

                    send_mutex.ReleaseMutex();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());

                if (debug && !console.ConsoleClosing)
                    console.Invoke(new DebugCallbackFunction(console.DebugCallback),
                        "ClientServerSync client error!\n" + ex.ToString());
            }
        }

        public bool ProcessData(byte[] data, int count, out string response)                       // answer received from server
        {
            ASCIIEncoding buffer = new ASCIIEncoding();
            string command_type;
            string version;
            string password;
            string CommBuffer = "";
            string answer = "";
            response = answer;

            try
            {
                if (count < 16)
                    return false;

                string val = buffer.GetString(data, 0, 16);

                if (val.StartsWith("Display data"))
                {
                    int j = 0;
                    string[] vals = buffer.GetString(data, 0, count).Split(';');
                    
                    for(int i=0; i<vals.Length-1; i++)
                        display_data[i] = float.Parse(vals[i+1]);
                }
                else
                {
                    string[] vals = buffer.GetString(data, 0, 16).Split(';');

                    if (vals.Length < 3)
                        return false;

                    command_type = vals[0];
                    version = vals[1];
                    password = vals[2];

                    if (command_type == "CAT" && version == "01" && CATpassword == password)
                    {
                        CommBuffer += Regex.Replace(buffer.GetString(data, 16, data.Length - 16), @"[^\w\;.]", "");
                        Regex rex = new Regex(".*?;");
                        byte[] out_string = new byte[2048];
                        int out_index = 13;

                        if (VoIP_enabled && voip != null)
                        {
                            string ans = "";
                            string cmd = "";
                            //buffer.GetBytes(command_type + version + password, 0, 16, out_string, 0);

                            for (Match m = rex.Match(CommBuffer); m.Success; m = m.NextMatch())
                            {
                                cmd += m;
                                ans += parser.Get(m.Value);
                            }

                            Debug.WriteLine("CAT command: " + cmd);
                            Debug.WriteLine("CAT answer: " + answer);

                            if (debug && !console.ConsoleClosing)
                            {
                                console.Invoke(new DebugCallbackFunction(console.DebugCallback),
                                    "CAT command: " + cmd + "\n" +
                                    "CAT answer: " + ans);
                            }

                            answer = "CAT;01;" + CATpassword + ";" + ans;
                        }
                        else
                        {
                            buffer.GetBytes(command_type + version + password, 0, 16, out_string, 0);

                            for (Match m = rex.Match(CommBuffer); m.Success; m = m.NextMatch())
                            {
                                answer = parser.Get(m.Value);

                                if (debug && !console.ConsoleClosing)
                                {
                                    console.Invoke(new DebugCallbackFunction(console.DebugCallback),
                                        "CAT command: " + m.Value.ToString() + "\n" +
                                        "CAT answer: " + answer);
                                }

                                Debug.WriteLine(m.Value);
                                Debug.WriteLine(answer);
                                buffer.GetBytes(answer, 0, answer.Length, out_string, out_index);
                                CommBuffer = CommBuffer.Replace(m.Value, "");
                                out_index += answer.Length;

                                if (out_index > 1024)
                                {
                                    out_index = 16;
                                    send_buffer = out_string;
                                    byte_to_send = out_data_index;
                                    send_event.Set();
                                    buffer.GetBytes(command_type + version + password, 0, 13, out_string, 0);
                                }
                            }

                            send_buffer = out_string;
                            byte_to_send = out_index;
                            send_event.Set();
                        }
                    }
                }

                response = answer;
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                return false;
            }
        }

        private void SendThread()
        {
            try
            {
                while (run_CAT_send_thread)
                {
                    send_event.WaitOne();

                    if (ClientSocket.Connected && run_CAT_send_thread)
                    {
                        send_mutex.WaitOne();
                        int sendBytes = ClientSocket.Send(send_buffer, 0, byte_to_send, SocketFlags.None);
                        send_mutex.ReleaseMutex();

                        if (sendBytes != byte_to_send)
                        {
                            ClientSocket.Shutdown(SocketShutdown.Both);
                            ClientSocket.Close(1000);
                            console.btnNetwork.BackColor = Color.Red;
                            console.SetupForm.txtCATServerIPAddress.ForeColor = Color.Red;
                            run_CAT_send_thread = false;
                        }

                        for (int i = 0; i < send_buffer.Length; i++)
                            send_buffer[i] = 0;
                        byte_to_send = 13;
                    }
                    else
                    {
                        if (debug && !console.ConsoleClosing)
                            console.Invoke(new DebugCallbackFunction(console.DebugCallback),
                                "Client disconnected!");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Write("Error!\n" + ex.ToString());

                if (run_CAT_client)
                    SetupSocket();
            }
        }

        private void SendEventTimerTick(object sender, System.EventArgs e)
        {
            send_mutex.WaitOne();
            send_event.Set();
            out_data_index = 0;                        // reset for new packet
            header_added = false;
            timeout_timer.Stop();
            send_mutex.ReleaseMutex();
        }

        #endregion
    }


    /// <summary>
    /// //////////////////////////////////////////////////////////////////////////////////////
    /// State object for receiving data from remote device.
    /// </summary>
    public class StateObject
    {
        // Client socket.
        public Socket workSocket = null;
        // Size of receive buffer.
        public const int BufferSize = 128;
        // Receive buffer.
        public byte[] buffer = new byte[BufferSize];
        // Received data string.
        public StringBuilder sb = new StringBuilder();
    }

    #endregion
}