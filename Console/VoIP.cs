//=================================================================
// VoIP
//=================================================================
//
//  Copyright (C)2012 YT7PWR Goran Radivojevic
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
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Diagnostics;
using System.Windows.Forms;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.ComponentModel;

using LumiSoft.Net;
using LumiSoft.Net.SDP;
using LumiSoft.Net.SIP;
using LumiSoft.Net.SIP.Debug;
using LumiSoft.Net.SIP.Stack;
using LumiSoft.Net.SIP.Message;
using LumiSoft.Net.Media;
using LumiSoft.Net.Media.Codec;
using LumiSoft.Net.Media.Codec.Audio;
using LumiSoft.Net.RTP;
using LumiSoft.Net.RTP.Debug;
using LumiSoft.Net.STUN.Client;
using LumiSoft.Net.UPnP.NAT;

namespace PowerSDR
{
    #region enum

    public enum SIP_CallState
    {
        /// <summary>
        /// Outgoing calling is in progress.
        /// </summary>
        Calling,

        /// <summary>
        /// Call is active.
        /// </summary>
        Active,

        /// <summary>
        /// Call is terminating.
        /// </summary>
        Terminating,

        /// <summary>
        /// Call is terminated.
        /// </summary>
        Terminated,

        /// <summary>
        /// Call has disposed.
        /// </summary>
        Disposed
    }

    public enum VoIP_mode
    {
        Server = 1,
        Client = 2,
    }

    #endregion

    #region voip class

    public class VoIP : Form
    {
        #region variable

        Console console;
        private bool m_IsClosing = false;
        private bool m_IsDebug = false;
        private SIP_Stack m_pStack = null;
        private string m_StunServer = "";
        private UPnP_NAT_Client m_pUPnP = null;
        private int m_SipPort = 8888;
        private string m_LocalAddress = "192.168.1.1";
        private int m_RtpBasePort = 21240;
        private Dictionary<int, AudioCodec> m_pAudioCodecs = null;
        private AudioOutDevice m_pAudioOutDevice = null;
        private AudioInDevice m_pAudioInDevice = null;
        private SIP_Call m_pCall = null;
        private WavePlayer m_pPlayer = null;
        private Timer m_pTimerDuration = null;
        private StatusStrip m_pStatusBar;
        private LabelTS labelTS1;
        private LabelTS labelTS2;
        private ButtonTS m_pConnect;
        private ComboBoxTS m_pLocalIP;
        private ComboBoxTS m_pRemoteIP;
        private string m_NatHandlingType = "";
        bool connected = false;
        public bool debug = false;
        private ToolStripStatusLabel statusLabel_Text;
        private ToolStripStatusLabel statusLabel_Duration;
        private System.ComponentModel.IContainer components;
        private delegate void DebugCallbackFunction(string name);
        string audio_source = "";
        string mic_source = "";
        string nat_type = "";
        string LocalIP = "";
        string RemoteIP = "";

        #endregion
        private MenuStrip m_pToolbar;

        #region properties

        VoIP_mode op_mode = VoIP_mode.Client;
        public VoIP_mode OpMode
        {
            set { op_mode = value; }
        }

        #endregion

        #region constructor/destructor

        public VoIP(Console c)
        {
            console = c;
            this.AutoScaleMode = AutoScaleMode.Inherit;
            InitializeComponent();
            float dpi = this.CreateGraphics().DpiX;
            float ratio = dpi / 96.0f;
            string font_name = this.Font.Name;
            float size = (float)(8.25 / ratio);
            System.Drawing.Font new_font = new System.Drawing.Font(font_name, size);
            this.Font = new_font;
            this.PerformAutoScale();
            this.PerformLayout();
            GetOptions();
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                SaveOptions();
                m_IsClosing = true;

                if (m_pCall != null)
                {
                    m_pCall.Terminate("Hang up.");

                    // Wait call to start terminating.
                    System.Threading.Thread.Sleep(200);
                }

                if (m_pPlayer != null)
                {
                    m_pPlayer.Stop();
                }

                if (m_pTimerDuration != null)
                {
                    m_pTimerDuration.Dispose();
                    m_pTimerDuration = null;
                }

                if (m_pStack != null)
                {
                    m_pStack.Dispose();
                    m_pStack = null;
                }

                base.Dispose(disposing);
            }
            catch
            {

            }
        }

        private void VoIP_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //this.Hide();
            e.Cancel = true;
        }

        #region Start/Stop

        public bool Start(string ServerAddress, int serverPort)
        {
            try
            {
                m_LocalAddress = ServerAddress;
                m_SipPort = serverPort;
                InitUI();
                InitStack();
                return true;
            }
            catch (Exception ex)
            {
                Debug.Write(ex.ToString());
                return false;
            }
        }

        public bool Stop()
        {
            try
            {
                console.btnNetwork.BackColor = Color.Red;
                console.SetupForm.txtCATServerIPAddress.ForeColor = Color.Red;
                return true;
            }
            catch (System.Exception ex)
            {
                Debug.Print(ex.Message);

                if (debug && !console.ConsoleClosing)
                    console.Invoke(new DebugCallbackFunction(console.DebugCallback),
                        "Close VoIP error!\n" + ex.ToString());

                return false;
            }
        }

        #endregion

        private void InitUI()
        {
            m_pRemoteIP.Text = RemoteIP;
            m_pLocalIP.Text = LocalIP;
            m_pConnect.Image = global::PowerSDR.Properties.Resources.call;
            System.ComponentModel.ComponentResourceManager resources = 
                new System.ComponentModel.ComponentResourceManager(typeof(VoIP));
            ToolStripDropDownButton button_Audio = new ToolStripDropDownButton();
            button_Audio.Name = "audio";
            button_Audio.Image = global::PowerSDR.Properties.Resources.speaker;

            foreach (AudioOutDevice device in AudioOut.Devices)
            {
                ToolStripMenuItem item = new ToolStripMenuItem(device.Name);
                //item.Checked = (button_Audio.DropDownItems.Count == 0);

                if (device.Name == audio_source)
                    item.Checked = true;

                item.Tag = device;
                button_Audio.DropDownItems.Add(item);
            }

            button_Audio.DropDown.ItemClicked += new ToolStripItemClickedEventHandler(m_pToolbar_Audio_ItemClicked);
            m_pToolbar.Items.Add(button_Audio);
            //--- Microphone button
            ToolStripDropDownButton button_Mic = new ToolStripDropDownButton();
            button_Mic.Name = "mic";
            button_Mic.Image = global::PowerSDR.Properties.Resources.mic;

            foreach (AudioInDevice device in AudioIn.Devices)
            {
                ToolStripMenuItem item = new ToolStripMenuItem(device.Name);

                if (device.Name == mic_source)
                    item.Checked = true;

                item.Tag = device;
                button_Mic.DropDownItems.Add(item);
            }

            button_Mic.DropDown.ItemClicked += new ToolStripItemClickedEventHandler(m_pToolbar_Mic_ItemClicked);
            m_pToolbar.Items.Add(button_Mic);

            // Separator
            m_pToolbar.Items.Add(new ToolStripSeparator());
            // NAT
            ToolStripDropDownButton button_NAT = new ToolStripDropDownButton();
            button_NAT.Name = "nat";
            button_NAT.Image = global::PowerSDR.Properties.Resources.router;
            button_NAT.DropDown.ItemClicked += new ToolStripItemClickedEventHandler(m_pToolbar_NAT_DropDown_ItemClicked);
            m_pToolbar.Items.Add(button_NAT);

            m_pTimerDuration = new Timer();
            m_pTimerDuration.Interval = 1000;
            m_pTimerDuration.Tick += new EventHandler(m_pTimerDuration_Tick);
            m_pTimerDuration.Enabled = true;
        }

        /// <summary>
        /// Initializes SIP stack.
        /// </summary>
        private void InitStack()
        {
            #region Init audio devices

            if (AudioOut.Devices.Length == 0)
            {
                MessageBox.Show("Calling not possible, there are no speakers in computer.", "Error:", MessageBoxButtons.OK, MessageBoxIcon.Error);

                return;
            }

            if (AudioIn.Devices.Length == 0)
            {
                MessageBox.Show("Calling not possible, there is no microphone in computer.", "Error:", MessageBoxButtons.OK, MessageBoxIcon.Error);

                return;
            }

            m_pAudioOutDevice = AudioOut.Devices[0];
            m_pAudioInDevice = AudioIn.Devices[0];

            m_pAudioCodecs = new Dictionary<int, AudioCodec>();
            m_pAudioCodecs.Add(0, new PCMU());
            m_pAudioCodecs.Add(8, new PCMA());

            m_pPlayer = new WavePlayer(AudioOut.Devices[0]);

            #endregion

            #region Get NAT handling methods

            m_pUPnP = new UPnP_NAT_Client();

            STUN_Result stunResult = new STUN_Result(STUN_NetType.UdpBlocked, null);
            try
            {
                stunResult = STUN_Client.Query(m_StunServer, 3478, new IPEndPoint(IPAddress.Any, 0));
            }
            catch
            {
            }

            if (stunResult.NetType == STUN_NetType.Symmetric || stunResult.NetType == STUN_NetType.UdpBlocked)
            {
                ToolStripMenuItem item_stun = new ToolStripMenuItem("STUN (" + stunResult.NetType + ")");
                item_stun.Name = "stun";
                item_stun.Enabled = false;
                ((ToolStripDropDownButton)m_pToolbar.Items["nat"]).DropDownItems.Add(item_stun);
            }
            else
            {
                ToolStripMenuItem item_stun = new ToolStripMenuItem("STUN (" + stunResult.NetType + ")");
                item_stun.Name = "stun";
                ((ToolStripDropDownButton)m_pToolbar.Items["nat"]).DropDownItems.Add(item_stun);
            }

            if (m_pUPnP.IsSupported)
            {
                ToolStripMenuItem item_upnp = new ToolStripMenuItem("UPnP");
                item_upnp.Name = "upnp";
                ((ToolStripDropDownButton)m_pToolbar.Items["nat"]).DropDownItems.Add(item_upnp);
            }
            else
            {
                ToolStripMenuItem item_upnp = new ToolStripMenuItem("UPnP Not Supported");
                item_upnp.Name = "upnp";
                item_upnp.Enabled = false;
                ((ToolStripDropDownButton)m_pToolbar.Items["nat"]).DropDownItems.Add(item_upnp);
            }

            //if(!((ToolStripDropDownButton)m_pToolbar.Items["nat"]).DropDownItems["stun"].Enabled && !((ToolStripDropDownButton)m_pToolbar.Items["nat"]).DropDownItems["upnp"].Enabled){
                //MessageBox.Show("Calling may not possible, your firewall or router blocks STUN and doesn't support UPnP.\r\n\r\nSTUN Net Type: " + stunResult.NetType + "\r\n\r\nUPnP Supported: " + m_pUPnP.IsSupported,"Error:",MessageBoxButtons.OK,MessageBoxIcon.Error);
            //}

            ToolStripMenuItem item_no_nat = new ToolStripMenuItem("No NAT handling");
            item_no_nat.Name = "no_nat";
            ((ToolStripDropDownButton)m_pToolbar.Items["nat"]).DropDownItems.Add(item_no_nat);

            // Select first enabled item.
            foreach (ToolStripItem it in ((ToolStripDropDownButton)m_pToolbar.Items["nat"]).DropDownItems)
            {
                if (it.Enabled)
                {
                    ((ToolStripMenuItem)it).Checked = true;
                    m_NatHandlingType = it.Name;

                    break;
                }
            }

            #endregion

            m_pStack = new SIP_Stack();
            m_pStack.UserAgent = "GSDR";
            m_pStack.BindInfo = new IPBindInfo[] { new IPBindInfo("", BindInfoProtocol.UDP, IPAddress.Any, m_SipPort) };
            //m_pStack.Allow
            m_pStack.Error += new EventHandler<ExceptionEventArgs>(m_pStack_Error);
            m_pStack.RequestReceived += new EventHandler<SIP_RequestReceivedEventArgs>(m_pStack_RequestReceived);
            m_pStack.Start();

            if (m_IsDebug)
            {
                wfrm_SIP_Debug debug = new wfrm_SIP_Debug(m_pStack);
                debug.Show();
            }
        }

        #endregion

        #region misc function

        private void m_pConnect_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                if (m_pCall != null)
                {
                    m_pCall.Terminate("Hang up.");
                    connected = false;
                    m_pConnect.Image = global::PowerSDR.Properties.Resources.call;
                }
                else
                {
                    #region Validate From:/To:

                    SIP_t_NameAddress to = null;
                    try
                    {
                        to = new SIP_t_NameAddress(m_pRemoteIP.Text);

                        if (!to.IsSipOrSipsUri)
                        {
                            throw new ArgumentException("To: is not SIP URI.");
                        }
                    }
                    catch
                    {
                        MessageBox.Show("To: is not SIP URI.", "Error:", MessageBoxButtons.OK, MessageBoxIcon.Error);

                        return;
                    }
                    SIP_t_NameAddress from = null;
                    try
                    {
                        from = new SIP_t_NameAddress(m_pLocalIP.Text);

                        if (!to.IsSipOrSipsUri)
                        {
                            throw new ArgumentException("From: is not SIP URI.");
                        }
                    }
                    catch
                    {
                        MessageBox.Show("From: is not SIP URI.", "Error:", MessageBoxButtons.OK, MessageBoxIcon.Error);

                        return;
                    }

                    #endregion

                    Call(from, to);
                }
            }
            catch (Exception x)
            {
                MessageBox.Show("Error: " + x.Message, "Error:", MessageBoxButtons.OK, MessageBoxIcon.Error);
                m_pCall = null;
            }
            this.Cursor = Cursors.Default;
        }

        private void Call(SIP_t_NameAddress from, SIP_t_NameAddress to)
        {
            if (from == null)
            {
                throw new ArgumentNullException("from");
            }
            if (to == null)
            {
                throw new ArgumentNullException("to");
            }

            #region Setup RTP session

            RTP_MultimediaSession rtpMultimediaSession = new RTP_MultimediaSession(RTP_Utils.GenerateCNAME());
            RTP_Session rtpSession = CreateRtpSession(rtpMultimediaSession);
            // Port search failed.
            if (rtpSession == null)
            {
                throw new Exception("Calling not possible, RTP session failed to allocate IP end points.");
            }

            if (m_IsDebug)
            {
                wfrm_RTP_Debug rtpDebug = new wfrm_RTP_Debug(rtpMultimediaSession);
                rtpDebug.Show();
            }

            #endregion

            #region Create SDP offer

            SDP_Message sdpOffer = new SDP_Message();
            sdpOffer.Version = "0";
            sdpOffer.Origin = new SDP_Origin("-", sdpOffer.GetHashCode(), 1, "IN", "IP4", System.Net.Dns.GetHostAddresses("")[0].ToString());
            sdpOffer.SessionName = "SIP Call";
            sdpOffer.Times.Add(new SDP_Time(0, 0));

            #region Add 1 audio stream

            SDP_MediaDescription mediaStream = new SDP_MediaDescription(SDP_MediaTypes.audio, 0, 1, "RTP/AVP", null);

            rtpSession.NewReceiveStream += delegate(object s, RTP_ReceiveStreamEventArgs e)
            {
                AudioOut_RTP audioOut = new AudioOut_RTP(m_pAudioOutDevice, e.Stream, m_pAudioCodecs);
                audioOut.Start();
                mediaStream.Tags["rtp_audio_out"] = audioOut;
            };

            if (!HandleNAT(mediaStream, rtpSession))
            {
                throw new Exception("Calling not possible, because of NAT or firewall restrictions.");
            }

            foreach (KeyValuePair<int, AudioCodec> entry in m_pAudioCodecs)
            {
                mediaStream.Attributes.Add(new SDP_Attribute("rtpmap", entry.Key + " " + entry.Value.Name + "/" + entry.Value.CompressedAudioFormat.SamplesPerSecond));
                mediaStream.MediaFormats.Add(entry.Key.ToString());
            }
            mediaStream.Attributes.Add(new SDP_Attribute("ptime", "20"));
            mediaStream.Attributes.Add(new SDP_Attribute("sendrecv", ""));
            mediaStream.Tags["rtp_session"] = rtpSession;
            mediaStream.Tags["audio_codecs"] = m_pAudioCodecs;
            sdpOffer.MediaDescriptions.Add(mediaStream);

            #endregion

            #endregion

            // Create INVITE request.
            SIP_Request invite = m_pStack.CreateRequest(SIP_Methods.INVITE, to, from);
            invite.ContentType = "application/sdp";
            invite.Data = sdpOffer.ToByte();

            SIP_RequestSender sender = m_pStack.CreateRequestSender(invite);

            // Create call.
            m_pCall = new SIP_Call(m_pStack, sender, rtpMultimediaSession);
            m_pCall.LocalSDP = sdpOffer;
            m_pCall.StateChanged += new EventHandler(m_pCall_StateChanged);

            bool finalResponseSeen = false;
            List<SIP_Dialog_Invite> earlyDialogs = new List<SIP_Dialog_Invite>();
            sender.ResponseReceived += delegate(object s, SIP_ResponseReceivedEventArgs e)
            {
                // Skip 2xx retransmited response.
                if (finalResponseSeen)
                {
                    return;
                }
                if (e.Response.StatusCode >= 200)
                {
                    finalResponseSeen = true;
                }

                try
                {
                    #region Provisional

                    if (e.Response.StatusCodeType == SIP_StatusCodeType.Provisional)
                    {
                        /* RFC 3261 13.2.2.1.
                            Zero, one or multiple provisional responses may arrive before one or
                            more final responses are received.  Provisional responses for an
                            INVITE request can create "early dialogs".  If a provisional response
                            has a tag in the To field, and if the dialog ID of the response does
                            not match an existing dialog, one is constructed using the procedures
                            defined in Section 12.1.2.
                        */
                        if (e.Response.StatusCode > 100 && e.Response.To.Tag != null)
                        {
                            earlyDialogs.Add((SIP_Dialog_Invite)e.GetOrCreateDialog);
                        }

                        // 180_Ringing.
                        if (e.Response.StatusCode == 180)
                        {
                            //m_pPlayer.Play(ResManager.GetStream("ringing.wav"), 10);

                            // We need BeginInvoke here, otherwise we block client transaction.
                            m_pStatusBar.BeginInvoke(new MethodInvoker(delegate()
                            {
                                m_pStatusBar.Items[0].Text = "Ringing";
                            }));
                        }
                    }

                    #endregion

                    #region Success

                    else if (e.Response.StatusCodeType == SIP_StatusCodeType.Success)
                    {
                        SIP_Dialog dialog = e.GetOrCreateDialog;

                        /* Exit all all other dialogs created by this call (due to forking).
                           That is not defined in RFC but, since UAC can send BYE to early and confirmed dialogs, 
                           all this is 100% valid.
                        */
                        foreach (SIP_Dialog_Invite d in earlyDialogs.ToArray())
                        {
                            if (!d.Equals(dialog))
                            {
                                d.Terminate("Another forking leg accepted.", true);
                            }
                        }

                        m_pCall.InitCalling(dialog, sdpOffer);

                        // Remote-party provided SDP.
                        if (e.Response.ContentType != null && e.Response.ContentType.ToLower().IndexOf("application/sdp") > -1)
                        {
                            try
                            {
                                // SDP offer. We sent offerless INVITE, we need to send SDP answer in ACK request.'
                                if (e.ClientTransaction.Request.ContentType == null || e.ClientTransaction.Request.ContentType.ToLower().IndexOf("application/sdp") == -1)
                                {
                                    // Currently we never do it, so it never happens. This is place holder, if we ever support it.
                                }
                                // SDP answer to our offer.
                                else
                                {
                                    // This method takes care of ACK sending and 2xx response retransmission ACK sending.
                                    HandleAck(m_pCall.Dialog, e.ClientTransaction);

                                    ProcessMediaAnswer(m_pCall, m_pCall.LocalSDP, SDP_Message.Parse(Encoding.UTF8.GetString(e.Response.Data)));
                                }
                            }
                            catch
                            {
                                m_pCall.Terminate("SDP answer parsing/processing failed.");
                            }
                        }
                        else
                        {
                            // If we provided SDP offer, there must be SDP answer.
                            if (e.ClientTransaction.Request.ContentType != null && e.ClientTransaction.Request.ContentType.ToLower().IndexOf("application/sdp") > -1)
                            {
                                m_pCall.Terminate("Invalid 2xx response, required SDP answer is missing.");
                            }
                        }

                        // Stop ringing.
                        m_pPlayer.Stop();
                    }

                    #endregion

                    #region Failure

                    else
                    {
                        /* RFC 3261 13.2.2.3.
                            All early dialogs are considered terminated upon reception of the non-2xx final response.
                        */
                        foreach (SIP_Dialog_Invite dialog in earlyDialogs.ToArray())
                        {
                            dialog.Terminate("All early dialogs are considered terminated upon reception of the non-2xx final response. (RFC 3261 13.2.2.3)", false);
                        }

                        // We need BeginInvoke here, otherwise we block client transaction while message box open.
                        if (m_pCall.State != SIP_CallState.Terminating)
                        {
                            this.BeginInvoke(new MethodInvoker(delegate()
                            {
                                m_pConnect.Image = global::PowerSDR.Properties.Resources.call;
                                connected = false;
                                MessageBox.Show("Calling failed: " + e.Response.StatusCode_ReasonPhrase, "Error:", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }));
                        }

                        // We need BeginInvoke here, otherwise we block client transaction.
                        m_pStatusBar.BeginInvoke(new MethodInvoker(delegate()
                        {
                            m_pStatusBar.Items[0].Text = "";
                        }));
                        // Stop calling or ringing.
                        m_pPlayer.Stop();

                        // Terminate call.
                        m_pCall.Terminate("Remote party rejected a call.", false);
                    }

                    #endregion
                }
                catch (Exception x)
                {
                    // We need BeginInvoke here, otherwise we block client transaction while message box open.
                    this.BeginInvoke(new MethodInvoker(delegate()
                    {
                        MessageBox.Show("Error: " + x.Message, "Error:", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }));
                }
            };

            m_pStatusBar.Items[0].Text = "Calling";
            m_pStatusBar.Items[1].Text = "00:00:00";
            //m_pPlayer.Play(ResManager.GetStream("calling.wav"), 10);

            // Start calling.
            sender.Start();
        }

        private void m_pCall_StateChanged(object sender, EventArgs e)
        {
            #region Active

            if (m_pCall.State == SIP_CallState.Active)
            {
                // We need invoke here, we are running on thread pool thread.
                this.BeginInvoke(new MethodInvoker(delegate()
                {
                    m_pConnect.Image = global::PowerSDR.Properties.Resources.call_hangup;
                    m_pStatusBar.Items[0].Text = "Call established";
                }));
            }

            #endregion

            #region Terminated

            else if (m_pCall.State == SIP_CallState.Terminated)
            {
                SDP_Message localSDP = m_pCall.LocalSDP;

                foreach (SDP_MediaDescription media in localSDP.MediaDescriptions)
                {
                    if (media.Tags.ContainsKey("rtp_audio_in"))
                    {
                        ((AudioIn_RTP)media.Tags["rtp_audio_in"]).Dispose();
                    }
                    if (media.Tags.ContainsKey("rtp_audio_out"))
                    {
                        ((AudioOut_RTP)media.Tags["rtp_audio_out"]).Dispose();
                    }

                    if (media.Tags.ContainsKey("upnp_rtp_map"))
                    {
                        try
                        {
                            m_pUPnP.DeletePortMapping((UPnP_NAT_Map)media.Tags["upnp_rtp_map"]);
                        }
                        catch
                        {
                        }
                    }
                    if (media.Tags.ContainsKey("upnp_rtcp_map"))
                    {
                        try
                        {
                            m_pUPnP.DeletePortMapping((UPnP_NAT_Map)media.Tags["upnp_rtcp_map"]);
                        }
                        catch
                        {
                        }
                    }
                }

                if (m_pCall.RtpMultimediaSession != null)
                {
                    m_pCall.RtpMultimediaSession.Dispose();
                }

                if (m_pCall.Dialog != null && m_pCall.Dialog.IsTerminatedByRemoteParty)
                {
                    //m_pPlayer.Play(ResManager.GetStream("hangup.wav"), 1);
                }
            }

            #endregion

            #region Disposed

            else if (m_pCall.State == SIP_CallState.Disposed)
            {
                if (!m_IsClosing)
                {
                    // We need invoke here, we are running on thread pool thread.
                    this.BeginInvoke(new MethodInvoker(delegate()
                    {
                        m_pConnect.Image = global::PowerSDR.Properties.Resources.call;
                        connected = false;
                        m_pStatusBar.Items[0].Text = "Call ended.";
                    }));
                }

                m_pCall = null;
            }

            #endregion
        }

        private RTP_Session CreateRtpSession(RTP_MultimediaSession rtpMultimediaSession)
        {
            if (rtpMultimediaSession == null)
            {
                throw new ArgumentNullException("rtpMultimediaSession");
            }

            //--- Search RTP IP -------------------------------------------------------//
            IPAddress rtpIP = null;
            foreach (IPAddress ip in Dns.GetHostAddresses(""))
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    rtpIP = ip;
                    break;
                }
            }
            if (rtpIP == null)
            {
                throw new Exception("None of the network connection is available.");
            }
            //------------------------------------------------------------------------//

            // Search free ports for RTP session.
            for (int i = 0; i < 100; i += 2)
            {
                try
                {
                    return rtpMultimediaSession.CreateSession(new RTP_Address(rtpIP, m_RtpBasePort, m_RtpBasePort + 1), new RTP_Clock(1, 8000));
                }
                catch
                {
                    m_RtpBasePort += 2;
                }
            }

            return null;
        }


        private void Handle2xx(SIP_Dialog dialog, SIP_ServerTransaction transaction)
        {
            if (dialog == null)
            {
                throw new ArgumentNullException("dialog");
            }
            if (transaction == null)
            {
                throw new ArgumentException("transaction");
            }

            /* RFC 6026 8.1.
                Once the response has been constructed, it is passed to the INVITE
                server transaction.  In order to ensure reliable end-to-end
                transport of the response, it is necessary to periodically pass
                the response directly to the transport until the ACK arrives.  The
                2xx response is passed to the transport with an interval that
                starts at T1 seconds and doubles for each retransmission until it
                reaches T2 seconds (T1 and T2 are defined in Section 17).
                Response retransmissions cease when an ACK request for the
                response is received.  This is independent of whatever transport
                protocols are used to send the response.
             
                If the server retransmits the 2xx response for 64*T1 seconds without
                receiving an ACK, the dialog is confirmed, but the session SHOULD be
                terminated.  This is accomplished with a BYE, as described in Section
                15.
              
                 T1 - 500
                 T2 - 4000
            */

            TimerEx timer = null;

            EventHandler<SIP_RequestReceivedEventArgs> callback = delegate(object s1, SIP_RequestReceivedEventArgs e)
            {
                try
                {
                    if (e.Request.RequestLine.Method == SIP_Methods.ACK)
                    {
                        // ACK for INVITE 2xx response received, stop retransmitting INVITE 2xx response.
                        if (transaction.Request.CSeq.SequenceNumber == e.Request.CSeq.SequenceNumber)
                        {
                            if (timer != null)
                            {
                                timer.Dispose();
                            }
                        }
                    }
                }
                catch
                {
                    // We don't care about errors here.
                }
            };
            dialog.RequestReceived += callback;

            // Create timer and sart retransmitting INVITE 2xx response.
            timer = new TimerEx(500);
            timer.AutoReset = false;
            timer.Elapsed += delegate(object s, System.Timers.ElapsedEventArgs e)
            {
                try
                {
                    lock (transaction.SyncRoot)
                    {
                        if (transaction.State == SIP_TransactionState.Accpeted)
                        {
                            transaction.SendResponse(transaction.FinalResponse);
                        }
                        else
                        {
                            timer.Dispose();

                            return;
                        }
                    }

                    timer.Interval = Math.Min(timer.Interval * 2, 4000);
                    timer.Enabled = true;
                }
                catch
                {
                    // We don't care about errors here.
                }
            };
            timer.Disposed += delegate(object s1, EventArgs e1)
            {
                try
                {
                    dialog.RequestReceived -= callback;
                }
                catch
                {
                    // We don't care about errors here.
                }
            };
            timer.Enabled = true;
        }

        private bool HandleNAT(SDP_MediaDescription mediaStream, RTP_Session rtpSession)
        {
            if (mediaStream == null)
            {
                throw new ArgumentNullException("mediaStream");
            }
            if (rtpSession == null)
            {
                throw new ArgumentNullException("rtpSession");
            }

            IPEndPoint rtpPublicEP = null;
            IPEndPoint rtcpPublicEP = null;

            // We have public IP.
            if (!Net_Utils.IsPrivateIP(rtpSession.LocalEP.IP))
            {
                rtpPublicEP = rtpSession.LocalEP.RtpEP;
                rtcpPublicEP = rtpSession.LocalEP.RtcpEP;
            }
            // No NAT handling.
            else if (m_NatHandlingType == "no_nat")
            {
                rtpPublicEP = rtpSession.LocalEP.RtpEP;
                rtcpPublicEP = rtpSession.LocalEP.RtcpEP;
            }
            // Use STUN.
            else if (m_NatHandlingType == "stun")
            {
                rtpSession.StunPublicEndPoints(m_StunServer, 3478, out rtpPublicEP, out rtcpPublicEP);
            }
            // Use UPnP.
            else if (m_NatHandlingType == "upnp")
            {
                // Try to open UPnP ports.
                if (m_pUPnP.IsSupported)
                {
                    int rtpPublicPort = rtpSession.LocalEP.RtpEP.Port;
                    int rtcpPublicPort = rtpSession.LocalEP.RtcpEP.Port;

                    try
                    {
                        UPnP_NAT_Map[] maps = m_pUPnP.GetPortMappings();
                        while (true)
                        {
                            bool conficts = false;
                            // Check that some other application doesn't use that port.
                            foreach (UPnP_NAT_Map map in maps)
                            {
                                // Existing map entry conflicts.
                                if (Convert.ToInt32(map.ExternalPort) == rtpPublicPort || Convert.ToInt32(map.ExternalPort) == rtcpPublicPort)
                                {
                                    rtpPublicPort += 2;
                                    rtcpPublicPort += 2;
                                    conficts = true;

                                    break;
                                }
                            }
                            if (!conficts)
                            {
                                break;
                            }
                        }

                        m_pUPnP.AddPortMapping(true, "LS RTP", "UDP", null, rtpPublicPort, rtpSession.LocalEP.RtpEP, 0);
                        m_pUPnP.AddPortMapping(true, "LS RTCP", "UDP", null, rtcpPublicPort, rtpSession.LocalEP.RtcpEP, 0);

                        IPAddress publicIP = m_pUPnP.GetExternalIPAddress();

                        rtpPublicEP = new IPEndPoint(publicIP, rtpPublicPort);
                        rtcpPublicEP = new IPEndPoint(publicIP, rtcpPublicPort);

                        mediaStream.Tags.Add("upnp_rtp_map", new UPnP_NAT_Map(true, "UDP", "", rtpPublicPort.ToString(), rtpSession.LocalEP.IP.ToString(), rtpSession.LocalEP.RtpEP.Port, "LS RTP", 0));
                        mediaStream.Tags.Add("upnp_rtcp_map", new UPnP_NAT_Map(true, "UDP", "", rtcpPublicPort.ToString(), rtpSession.LocalEP.IP.ToString(), rtpSession.LocalEP.RtcpEP.Port, "LS RTCP", 0));
                    }
                    catch
                    {
                    }
                }
            }

            if (rtpPublicEP != null && rtcpPublicEP != null)
            {
                mediaStream.Port = rtpPublicEP.Port;
                if ((rtpPublicEP.Port + 1) != rtcpPublicEP.Port)
                {
                    // Remove old rport attribute, if any.
                    for (int i = 0; i < mediaStream.Attributes.Count; i++)
                    {
                        if (string.Equals(mediaStream.Attributes[i].Name, "rport", StringComparison.InvariantCultureIgnoreCase))
                        {
                            mediaStream.Attributes.RemoveAt(i);
                            i--;
                        }
                    }
                    mediaStream.Attributes.Add(new SDP_Attribute("rport", rtcpPublicEP.Port.ToString()));
                }
                mediaStream.Connection = new SDP_Connection("IN", "IP4", rtpPublicEP.Address.ToString());

                return true;
            }

            return false;
        }

        private void HandleAck(SIP_Dialog dialog, SIP_ClientTransaction transaction)
        {
            if (dialog == null)
            {
                throw new ArgumentNullException("dialog");
            }
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }

            /* RFC 3261 6.
                The ACK for a 2xx response to an INVITE request is a separate transaction.
              
               RFC 3261 13.2.2.4.
                The UAC core MUST generate an ACK request for each 2xx received from
                the transaction layer.  The header fields of the ACK are constructed
                in the same way as for any request sent within a dialog (see Section
                12) with the exception of the CSeq and the header fields related to
                authentication.  The sequence number of the CSeq header field MUST be
                the same as the INVITE being acknowledged, but the CSeq method MUST
                be ACK.  The ACK MUST contain the same credentials as the INVITE.  If
                the 2xx contains an offer (based on the rules above), the ACK MUST
                carry an answer in its body.
            */

            SIP_t_ViaParm via = new SIP_t_ViaParm();
            via.ProtocolName = "SIP";
            via.ProtocolVersion = "2.0";
            via.ProtocolTransport = transaction.Flow.Transport;
            via.SentBy = new HostEndPoint(transaction.Flow.LocalEP);
            via.Branch = SIP_t_ViaParm.CreateBranch();
            via.RPort = 0;

            SIP_Request ackRequest = dialog.CreateRequest(SIP_Methods.ACK);
            ackRequest.Via.AddToTop(via.ToStringValue());
            ackRequest.CSeq = new SIP_t_CSeq(transaction.Request.CSeq.SequenceNumber, SIP_Methods.ACK);
            // Authorization
            foreach (SIP_HeaderField h in transaction.Request.Authorization.HeaderFields)
            {
                ackRequest.Authorization.Add(h.Value);
            }
            // Proxy-Authorization 
            foreach (SIP_HeaderField h in transaction.Request.ProxyAuthorization.HeaderFields)
            {
                ackRequest.Authorization.Add(h.Value);
            }

            // Send ACK.
            SendAck(dialog, ackRequest);

            // Start receive 2xx retransmissions.
            transaction.ResponseReceived += delegate(object sender, SIP_ResponseReceivedEventArgs e)
            {
                if (dialog.State == SIP_DialogState.Disposed || dialog.State == SIP_DialogState.Terminated)
                {
                    return;
                }

                // Don't send ACK for forked 2xx, our sent BYE(to all early dialogs) or their early timer will kill these dialogs.
                // Send ACK only to our accepted dialog 2xx response retransmission.
                if (e.Response.From.Tag == ackRequest.From.Tag && e.Response.To.Tag == ackRequest.To.Tag)
                {
                    SendAck(dialog, ackRequest);
                }
            };
        }

        private void SendAck(SIP_Dialog dialog, SIP_Request ack)
        {
            if (dialog == null)
            {
                throw new ArgumentNullException("dialog");
            }
            if (ack == null)
            {
                throw new ArgumentNullException("ack");
            }

            try
            {
                // Try existing flow.
                dialog.Flow.Send(ack);

                // Log
                if (dialog.Stack.Logger != null)
                {
                    byte[] ackBytes = ack.ToByteData();

                    dialog.Stack.Logger.AddWrite(
                        dialog.ID,
                        null,
                        ackBytes.Length,
                        "Request [DialogID='" + dialog.ID + "';" + "method='" + ack.RequestLine.Method + "'; cseq='" + ack.CSeq.SequenceNumber + "'; " +
                        "transport='" + dialog.Flow.Transport + "'; size='" + ackBytes.Length + "'] sent '" + dialog.Flow.LocalEP + "' -> '" + dialog.Flow.RemoteEP + "'.",
                        dialog.Flow.LocalEP,
                        dialog.Flow.RemoteEP,
                        ackBytes
                    );
                }
            }
            catch
            {
                /* RFC 3261 13.2.2.4.
                    Once the ACK has been constructed, the procedures of [4] are used to
                    determine the destination address, port and transport.  However, the
                    request is passed to the transport layer directly for transmission,
                    rather than a client transaction.
                */
                try
                {
                    dialog.Stack.TransportLayer.SendRequest(ack);
                }
                catch (Exception x)
                {
                    // Log
                    if (dialog.Stack.Logger != null)
                    {
                        dialog.Stack.Logger.AddText("Dialog [id='" + dialog.ID + "'] ACK send for 2xx response failed: " + x.Message + ".");
                    }
                }
            }
        }

        public void SendMessage(string msg, string type)
        {
            try
            {
                ASCIIEncoding buff = new ASCIIEncoding();
                byte[] data = buff.GetBytes(msg);
                // Create MESSAGE request.
                SIP_Request message = m_pCall.Dialog.CreateRequest(SIP_Methods.MESSAGE);
                message.ContentType = type;
                message.Data = data;

                bool finalResponseSeen = false;
                SIP_RequestSender sender = m_pCall.Dialog.CreateRequestSender(message);

                sender.ResponseReceived += delegate(object s, SIP_ResponseReceivedEventArgs e)
                {
                    // Skip 2xx retransmited response.
                    if (finalResponseSeen)
                    {
                        return;
                    }
                    if (e.Response.StatusCode >= 200)
                    {
                        finalResponseSeen = true;
                    }

                    try
                    {
                        #region Provisional

                        if (e.Response.StatusCodeType == SIP_StatusCodeType.Provisional)
                        {
                            // We don't care provisional responses here.
                        }

                        #endregion

                        #region Success

                        else if (e.Response.StatusCodeType == SIP_StatusCodeType.Success)
                        {
                            // Remote-party provided SDP answer.
                            if (e.Response.ContentType != null && e.Response.ContentType.ToLower().IndexOf(type) > -1)
                            {
                                try
                                {
                                    // This method takes care of ACK sending and 2xx response retransmission ACK sending.
                                    HandleAck(m_pCall.Dialog, e.ClientTransaction);

                                    //ProcessMediaAnswer(m_pCall, MsgOffer, SDP_Message.Parse(Encoding.UTF8.GetString(e.Response.Data)));
                                }
                                catch
                                {
                                    m_pCall.Terminate("CAT answer failed.");
                                }
                            }
                        }

                        #endregion

                        #region Failure

                        else
                        {
                            // We need BeginInvoke here, otherwise we block client transaction while message box open.
                            this.BeginInvoke(new MethodInvoker(delegate()
                            {
                                MessageBox.Show("Re-INVITE Error: " + e.Response.StatusCode_ReasonPhrase, "Error:", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }));

                            // 481 Call Transaction Does Not Exist.
                            if (e.Response.StatusCode == 481)
                            {
                                m_pCall.Terminate("Remote-party call does not exist any more.", false);
                            }
                        }

                        #endregion
                    }
                    catch (Exception x)
                    {
                        // We need BeginInvoke here, otherwise we block client transaction while message box open.
                        this.BeginInvoke(new MethodInvoker(delegate()
                        {
                            MessageBox.Show("Error: " + x.Message, "Error:", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }));
                    }
                };

                sender.Start();
            }
            catch (Exception ex)
            {
                Debug.Write(ex.ToString());
            }
        }


        private void m_pStack_Error(object sender, ExceptionEventArgs e)
        {
            if (!m_IsClosing)
            {
                Debug.Write("Error: " + e.Exception.Message + "\n");
                m_pStatusBar.Items[0].Text = "Error!";

                if (debug && !console.ConsoleClosing)
                    console.Invoke(new DebugCallbackFunction(console.DebugCallback),
                        "SIP stack error:\n" + e.Exception.Message);
            }
        }

        private void m_pTimerDuration_Tick(object sender, EventArgs e)
        {
            try
            {
                if (m_pCall != null && m_pCall.State == SIP_CallState.Active)
                {
                    TimeSpan duration = (DateTime.Now - m_pCall.StartTime);
                    m_pStatusBar.Items[1].Text = duration.Hours.ToString("00") + ":" +
                        duration.Minutes.ToString("00") + ":" + duration.Seconds.ToString("00");
                }
            }
            catch
            {
                // We don't care about errors here.
            }
        }

        private void m_pStack_RequestReceived(object sender, SIP_RequestReceivedEventArgs e)
        {
            try
            {
                #region CANCEL

                if (e.Request.RequestLine.Method == SIP_Methods.CANCEL)
                {
                    SIP_ServerTransaction trToCancel = m_pStack.TransactionLayer.MatchCancelToTransaction(e.Request);

                    if (trToCancel != null)
                    {
                        trToCancel.Cancel();
                        e.ServerTransaction.SendResponse(m_pStack.CreateResponse(SIP_ResponseCodes.x200_Ok, e.Request));
                    }
                    else
                    {
                        e.ServerTransaction.SendResponse(m_pStack.CreateResponse(SIP_ResponseCodes.x481_Call_Transaction_Does_Not_Exist, e.Request));
                    }
                }

                #endregion

                #region BYE

                else if (e.Request.RequestLine.Method == SIP_Methods.BYE)
                {
                    // Currently we match BYE to dialog and it processes it,
                    // so BYE what reaches here doesnt match to any dialog.

                    e.ServerTransaction.SendResponse(m_pStack.CreateResponse(SIP_ResponseCodes.x481_Call_Transaction_Does_Not_Exist, e.Request));
                }

                #endregion

                #region INVITE

                else if (e.Request.RequestLine.Method == SIP_Methods.INVITE)
                {

                    #region Incoming call

                    if (e.Dialog == null)
                    {
                        #region Validate incoming call

                        // We don't accept more than 1 call at time.
                        if (connected || m_pCall != null)
                        {
                            e.ServerTransaction.SendResponse(m_pStack.CreateResponse(SIP_ResponseCodes.x600_Busy_Everywhere, e.Request));
                            return;
                        }

                        // We don't accept SDP offerless calls.
                        if (e.Request.ContentType == null || e.Request.ContentType.ToLower().IndexOf("application/sdp") == -1)
                        {
                            e.ServerTransaction.SendResponse(m_pStack.CreateResponse(SIP_ResponseCodes.x606_Not_Acceptable + 
                                ": We don't accpet SDP offerless calls.", e.Request));
                            return;
                        }

                        SDP_Message sdpOffer = SDP_Message.Parse(Encoding.UTF8.GetString(e.Request.Data));

                        // Check if we can accept any media stream.
                        bool canAccept = false;
                        foreach (SDP_MediaDescription media in sdpOffer.MediaDescriptions)
                        {
                            if (CanSupportMedia(media))
                            {
                                canAccept = true;

                                break;
                            }
                        }

                        if (!canAccept)
                        {
                            e.ServerTransaction.SendResponse(m_pStack.CreateResponse(SIP_ResponseCodes.x606_Not_Acceptable, e.Request));

                            return;
                        }

                        #endregion

                        // Send ringing to remote-party.
                        SIP_Response responseRinging = m_pStack.CreateResponse(SIP_ResponseCodes.x180_Ringing, e.Request, e.Flow);
                        responseRinging.To.Tag = SIP_Utils.CreateTag();
                        e.ServerTransaction.SendResponse(responseRinging);

                        SIP_Dialog_Invite dialog = (SIP_Dialog_Invite)m_pStack.TransactionLayer.GetOrCreateDialog(e.ServerTransaction, responseRinging);

                        // We need invoke here, otherwise we block SIP stack RequestReceived event while incoming call UI showed.
                        this.BeginInvoke(new MethodInvoker(delegate()
                        {
                            try
                            {
                                //m_pPlayer.Play(ResManager.GetStream("ringing.wav"), 20);

                                // Call accepted.
                                RTP_MultimediaSession rtpMultimediaSession = new RTP_MultimediaSession(RTP_Utils.GenerateCNAME());

                                // Build local SDP template
                                SDP_Message sdpLocal = new SDP_Message();
                                sdpLocal.Version = "0";
                                sdpLocal.Origin = new SDP_Origin("-", sdpLocal.GetHashCode(), 1, "IN", "IP4", 
                                    System.Net.Dns.GetHostAddresses("")[0].ToString());
                                sdpLocal.SessionName = "SIP Call";
                                sdpLocal.Times.Add(new SDP_Time(0, 0));

                                ProcessMediaOffer(dialog, e.ServerTransaction, rtpMultimediaSession, sdpOffer, sdpLocal);

                                // Create call.
                                m_pCall = new SIP_Call(m_pStack, dialog, rtpMultimediaSession, sdpLocal);
                                m_pCall.StateChanged += new EventHandler(m_pCall_StateChanged);
                                m_pCall_StateChanged(m_pCall, new EventArgs());

                                if (m_IsDebug)
                                {
                                    wfrm_RTP_Debug rtpDebug = new wfrm_RTP_Debug(m_pCall.RtpMultimediaSession);
                                    rtpDebug.Show();
                                }

                                connected = true;

                            }
                            catch (Exception x1)
                            {
                                MessageBox.Show("Error: " + x1.Message, "Error:", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                connected = false;
                                m_pConnect.Image = global::PowerSDR.Properties.Resources.call;
                            }
                        }));
                    }

                    #endregion

                    #region Re-INVITE

                    else
                    {
                        try
                        {
                            // Remote-party provided SDP offer.
                            if (e.Request.ContentType != null && e.Request.ContentType.ToLower().IndexOf("application/sdp") > -1)
                            {
                                ProcessMediaOffer(m_pCall.Dialog, e.ServerTransaction, m_pCall.RtpMultimediaSession, 
                                    SDP_Message.Parse(Encoding.UTF8.GetString(e.Request.Data)), m_pCall.LocalSDP);

                                // Remote-party is holding a call.
                                if (IsRemotePartyHolding(SDP_Message.Parse(Encoding.UTF8.GetString(e.Request.Data))))
                                {
                                    // We need invoke here, we are running on thread pool thread.
                                    this.BeginInvoke(new MethodInvoker(delegate()
                                    {
                                        m_pStatusBar.Items[0].Text = "Remote party holding a call";
                                    }));

                                    //m_pPlayer.Play(ResManager.GetStream("onhold.wav"), 20);
                                }
                                // Call is active.
                                else
                                {
                                    // We need invoke here, we are running on thread pool thread.
                                    this.BeginInvoke(new MethodInvoker(delegate()
                                    {
                                        m_pStatusBar.Items[0].Text = "Call established";
                                    }));

                                    m_pPlayer.Stop();
                                }
                            }
                            // Error: Re-INVITE can't be SDP offerless.
                            else
                            {
                                e.ServerTransaction.SendResponse(m_pStack.CreateResponse(SIP_ResponseCodes.x500_Server_Internal_Error + 
                                    ": Re-INVITE must contain SDP offer.", e.Request));
                            }
                        }
                        catch (Exception x1)
                        {
                            e.ServerTransaction.SendResponse(m_pStack.CreateResponse(SIP_ResponseCodes.x500_Server_Internal_Error + 
                                ": " + x1.Message, e.Request));
                        }
                    }

                    #endregion
                }

                #endregion

                #region ACK

                else if (e.Request.RequestLine.Method == SIP_Methods.ACK)
                {
                    // Abandoned ACK, just skip it.
                }

                #endregion

                #region MESSAGE

                else if (e.Request.RequestLine.Method == SIP_Methods.MESSAGE)
                {
                    e.ServerTransaction.SendResponse(m_pStack.CreateResponse(SIP_ResponseCodes.x200_Ok, e.Request));
                    byte[] msg = e.Request.Data;
                    ASCIIEncoding buffer = new ASCIIEncoding();
                    string data = buffer.GetString(msg);
                    string answer = "";

                    if (debug && !console.ConsoleClosing)
                        console.Invoke(new DebugCallbackFunction(console.DebugCallback), data);

                    if (op_mode == VoIP_mode.Server)
                        answer = console.CAT_server_socket.ProcessData(msg, msg.Length);
                    else
                    {
                        if (console.CAT_client_socket.ProcessData(msg, msg.Length, out answer))
                            SendMessage(answer, "CAT");
                    }
                }

                #endregion

                #region Other

                else
                {
                    // ACK is response less method.
                    if (e.Request.RequestLine.Method != SIP_Methods.ACK)
                    {
                        e.ServerTransaction.SendResponse(m_pStack.CreateResponse(SIP_ResponseCodes.x501_Not_Implemented, e.Request));
                    }
                }

                #endregion
            }
            catch
            {
                e.ServerTransaction.SendResponse(m_pStack.CreateResponse(SIP_ResponseCodes.x500_Server_Internal_Error, e.Request));
            }
        }

        private bool IsRemotePartyHolding(SDP_Message sdp)
        {
            if (sdp == null)
            {
                throw new ArgumentNullException("sdp");
            }

            // Check if first audio stream is SendRecv, otherwise remote-party holding audio.
            foreach (SDP_MediaDescription media in sdp.MediaDescriptions)
            {
                if (media.Port != 0 && media.MediaType == "audio")
                {
                    if (GetRtpStreamMode(sdp, media) != RTP_StreamMode.SendReceive)
                    {
                        return true;
                    }

                    break;
                }
            }

            return false;
        }

        private void m_pToolbar_Audio_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            try
            {
                foreach (ToolStripMenuItem item in ((ToolStripDropDownMenu)sender).Items)
                {
                    if (item.Equals(e.ClickedItem))
                    {
                        item.Checked = true;
                        audio_source = item.Text;
                    }
                    else
                    {
                        item.Checked = false;
                    }
                }

                m_pAudioOutDevice = (AudioOutDevice)e.ClickedItem.Tag;

                // Update active call audio-out device.
                if (m_pCall != null && m_pCall.LocalSDP != null)
                {
                    foreach (SDP_MediaDescription media in m_pCall.LocalSDP.MediaDescriptions)
                    {
                        if (media.Tags.ContainsKey("rtp_audio_out"))
                        {
                            ((AudioOut_RTP)media.Tags["rtp_audio_out"]).AudioOutDevice = m_pAudioOutDevice;
                        }
                    }
                }
            }
            catch (Exception x)
            {
                MessageBox.Show("Error: " + x.Message, "Error:", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void m_pToolbar_Mic_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            try
            {
                foreach (ToolStripMenuItem item in ((ToolStripDropDownMenu)sender).Items)
                {
                    if (item.Equals(e.ClickedItem))
                    {
                        item.Checked = true;
                        mic_source = item.Text;
                    }
                    else
                    {
                        item.Checked = false;
                    }
                }

                m_pAudioInDevice = (AudioInDevice)e.ClickedItem.Tag;

                // Update active call audio-in device.
                if (m_pCall != null && m_pCall.LocalSDP != null)
                {
                    foreach (SDP_MediaDescription media in m_pCall.LocalSDP.MediaDescriptions)
                    {
                        if (media.Tags.ContainsKey("rtp_audio_in"))
                        {
                            ((AudioIn_RTP)media.Tags["rtp_audio_in"]).AudioInDevice = m_pAudioInDevice;
                        }
                    }
                }
            }
            catch (Exception x)
            {
                MessageBox.Show("Error: " + x.Message, "Error:", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void m_pToolbar_NAT_DropDown_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if (!e.ClickedItem.Enabled)
            {
                return;
            }

            foreach (ToolStripMenuItem item in ((ToolStripDropDownMenu)sender).Items)
            {
                if (item.Equals(e.ClickedItem))
                {
                    item.Checked = true;
                    m_NatHandlingType = item.Name;
                    nat_type = item.Text;
                }
                else
                {
                    item.Checked = false;
                }
            }
        }
        private void ProcessMediaAnswer(SIP_Call call, SDP_Message offer, SDP_Message answer)
        {
            if (call == null)
            {
                throw new ArgumentNullException("call");
            }
            if (offer == null)
            {
                throw new ArgumentNullException("offer");
            }
            if (answer == null)
            {
                throw new ArgumentNullException("answer");
            }

            try
            {
                #region SDP basic validation

                // Version field must exist.
                if (offer.Version == null)
                {
                    call.Terminate("Invalid SDP answer: Required 'v'(Protocol Version) field is missing.");

                    return;
                }

                // Origin field must exist.
                if (offer.Origin == null)
                {
                    call.Terminate("Invalid SDP answer: Required 'o'(Origin) field is missing.");

                    return;
                }

                // Session Name field.

                // Check That global 'c' connection attribute exists or otherwise each enabled media stream must contain one.
                if (offer.Connection == null)
                {
                    for (int i = 0; i < offer.MediaDescriptions.Count; i++)
                    {
                        if (offer.MediaDescriptions[i].Connection == null)
                        {
                            call.Terminate("Invalid SDP answer: Global or per media stream no: " + i + " 'c'(Connection) attribute is missing.");

                            return;
                        }
                    }
                }


                // Check media streams count.
                if (offer.MediaDescriptions.Count != answer.MediaDescriptions.Count)
                {
                    call.Terminate("Invalid SDP answer, media descriptions count in answer must be equal to count in media offer (RFC 3264 6.).");

                    return;
                }

                #endregion

                // Process media streams info.
                for (int i = 0; i < offer.MediaDescriptions.Count; i++)
                {
                    SDP_MediaDescription offerMedia = offer.MediaDescriptions[i];
                    SDP_MediaDescription answerMedia = answer.MediaDescriptions[i];

                    // Remote-party disabled this stream.
                    if (answerMedia.Port == 0)
                    {

                        #region Cleanup active RTP stream and it's resources, if it exists

                        // Dispose existing RTP session.
                        if (offerMedia.Tags.ContainsKey("rtp_session"))
                        {
                            ((RTP_Session)offerMedia.Tags["rtp_session"]).Dispose();
                            offerMedia.Tags.Remove("rtp_session");
                        }

                        // Release UPnPports if any.
                        if (offerMedia.Tags.ContainsKey("upnp_rtp_map"))
                        {
                            try
                            {
                                m_pUPnP.DeletePortMapping((UPnP_NAT_Map)offerMedia.Tags["upnp_rtp_map"]);
                            }
                            catch
                            {
                            }
                            offerMedia.Tags.Remove("upnp_rtp_map");
                        }
                        if (offerMedia.Tags.ContainsKey("upnp_rtcp_map"))
                        {
                            try
                            {
                                m_pUPnP.DeletePortMapping((UPnP_NAT_Map)offerMedia.Tags["upnp_rtcp_map"]);
                            }
                            catch
                            {
                            }
                            offerMedia.Tags.Remove("upnp_rtcp_map");
                        }

                        #endregion
                    }
                    // Remote-party accepted stream.
                    else
                    {
                        Dictionary<int, AudioCodec> audioCodecs = (Dictionary<int, AudioCodec>)offerMedia.Tags["audio_codecs"];

                        #region Validate stream-mode disabled,inactive,sendonly,recvonly

                        /* RFC 3264 6.1.
                            If a stream is offered as sendonly, the corresponding stream MUST be
                            marked as recvonly or inactive in the answer.  If a media stream is
                            listed as recvonly in the offer, the answer MUST be marked as
                            sendonly or inactive in the answer.  If an offered media stream is
                            listed as sendrecv (or if there is no direction attribute at the
                            media or session level, in which case the stream is sendrecv by
                            default), the corresponding stream in the answer MAY be marked as
                            sendonly, recvonly, sendrecv, or inactive.  If an offered media
                            stream is listed as inactive, it MUST be marked as inactive in the
                            answer.
                        */

                        // If we disabled this stream in offer and answer enables it (no allowed), terminate call.
                        if (offerMedia.Port == 0)
                        {
                            call.Terminate("Invalid SDP answer, you may not enable sdp-offer disabled stream no: " + i + " (RFC 3264 6.).");

                            return;
                        }

                        RTP_StreamMode offerStreamMode = GetRtpStreamMode(offer, offerMedia);
                        RTP_StreamMode answerStreamMode = GetRtpStreamMode(answer, answerMedia);
                        if (offerStreamMode == RTP_StreamMode.Send && answerStreamMode != RTP_StreamMode.Receive)
                        {
                            call.Terminate("Invalid SDP answer, sdp stream no: " + i + " stream-mode must be 'recvonly' (RFC 3264 6.).");

                            return;
                        }
                        if (offerStreamMode == RTP_StreamMode.Receive && answerStreamMode != RTP_StreamMode.Send)
                        {
                            call.Terminate("Invalid SDP answer, sdp stream no: " + i + " stream-mode must be 'sendonly' (RFC 3264 6.).");

                            return;
                        }
                        if (offerStreamMode == RTP_StreamMode.Inactive && answerStreamMode != RTP_StreamMode.Inactive)
                        {
                            call.Terminate("Invalid SDP answer, sdp stream no: " + i + " stream-mode must be 'inactive' (RFC 3264 6.).");

                            return;
                        }

                        #endregion

                        #region Create/modify RTP session

                        RTP_Session rtpSession = (RTP_Session)offerMedia.Tags["rtp_session"];
                        rtpSession.Payload = Convert.ToInt32(answerMedia.MediaFormats[0]);
                        rtpSession.StreamMode = (answerStreamMode == RTP_StreamMode.Inactive ? RTP_StreamMode.Inactive : offerStreamMode);
                        rtpSession.RemoveTargets();
                        if (GetSdpHost(answer, answerMedia) != "0.0.0.0")
                        {
                            rtpSession.AddTarget(GetRtpTarget(answer, answerMedia));
                        }
                        rtpSession.Start();

                        #endregion

                        #region Create/modify audio-in source

                        if (!offerMedia.Tags.ContainsKey("rtp_audio_in"))
                        {
                            AudioIn_RTP rtpAudioIn = new AudioIn_RTP(m_pAudioInDevice, 20, audioCodecs, rtpSession.CreateSendStream());
                            rtpAudioIn.Start();
                            offerMedia.Tags.Add("rtp_audio_in", rtpAudioIn);
                        }

                        #endregion
                    }
                }

                call.LocalSDP = offer;
                call.RemoteSDP = answer;
            }
            catch (Exception x)
            {
                call.Terminate("Error processing SDP answer: " + x.Message);
            }
        }

        private void ProcessMediaOffer(SIP_Dialog dialog, SIP_ServerTransaction transaction, RTP_MultimediaSession rtpMultimediaSession, SDP_Message offer, SDP_Message localSDP)
        {
            if (dialog == null)
            {
                throw new ArgumentNullException("dialog");
            }
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }
            if (rtpMultimediaSession == null)
            {
                throw new ArgumentNullException("rtpMultimediaSession");
            }
            if (offer == null)
            {
                throw new ArgumentNullException("offer");
            }
            if (localSDP == null)
            {
                throw new ArgumentNullException("localSDP");
            }

            try
            {
                #region SDP basic validation

                // Version field must exist.
                if (offer.Version == null)
                {
                    transaction.SendResponse(m_pStack.CreateResponse(SIP_ResponseCodes.x500_Server_Internal_Error + ": Invalid SDP answer: Required 'v'(Protocol Version) field is missing.", transaction.Request));

                    return;
                }

                // Origin field must exist.
                if (offer.Origin == null)
                {
                    transaction.SendResponse(m_pStack.CreateResponse(SIP_ResponseCodes.x500_Server_Internal_Error + ": Invalid SDP answer: Required 'o'(Origin) field is missing.", transaction.Request));

                    return;
                }

                // Session Name field.

                // Check That global 'c' connection attribute exists or otherwise each enabled media stream must contain one.
                if (offer.Connection == null)
                {
                    for (int i = 0; i < offer.MediaDescriptions.Count; i++)
                    {
                        if (offer.MediaDescriptions[i].Connection == null)
                        {
                            transaction.SendResponse(m_pStack.CreateResponse(SIP_ResponseCodes.x500_Server_Internal_Error + ": Invalid SDP answer: Global or per media stream no: " + i + " 'c'(Connection) attribute is missing.", transaction.Request));

                            return;
                        }
                    }
                }

                #endregion

                // Re-INVITE media streams count must be >= current SDP streams.
                if (localSDP.MediaDescriptions.Count > offer.MediaDescriptions.Count)
                {
                    transaction.SendResponse(m_pStack.CreateResponse(SIP_ResponseCodes.x500_Server_Internal_Error + ": re-INVITE SDP offer media stream count must be >= current session stream count.", transaction.Request));

                    return;
                }

                bool audioAccepted = false;
                // Process media streams info.
                for (int i = 0; i < offer.MediaDescriptions.Count; i++)
                {
                    SDP_MediaDescription offerMedia = offer.MediaDescriptions[i];
                    SDP_MediaDescription answerMedia = (localSDP.MediaDescriptions.Count > i ? localSDP.MediaDescriptions[i] : null);

                    // Disabled stream.
                    if (offerMedia.Port == 0)
                    {
                        // Remote-party offered new disabled stream.
                        if (answerMedia == null)
                        {
                            // Just copy offer media stream data to answer and set port to zero.
                            localSDP.MediaDescriptions.Add(offerMedia);
                            localSDP.MediaDescriptions[i].Port = 0;
                        }
                        // Existing disabled stream or remote party disabled it.
                        else
                        {
                            answerMedia.Port = 0;

                            #region Cleanup active RTP stream and it's resources, if it exists

                            // Dispose existing RTP session.
                            if (answerMedia.Tags.ContainsKey("rtp_session"))
                            {
                                ((RTP_Session)offerMedia.Tags["rtp_session"]).Dispose();
                                answerMedia.Tags.Remove("rtp_session");
                            }

                            // Release UPnPports if any.
                            if (answerMedia.Tags.ContainsKey("upnp_rtp_map"))
                            {
                                try
                                {
                                    m_pUPnP.DeletePortMapping((UPnP_NAT_Map)answerMedia.Tags["upnp_rtp_map"]);
                                }
                                catch
                                {
                                }
                                answerMedia.Tags.Remove("upnp_rtp_map");
                            }
                            if (answerMedia.Tags.ContainsKey("upnp_rtcp_map"))
                            {
                                try
                                {
                                    m_pUPnP.DeletePortMapping((UPnP_NAT_Map)answerMedia.Tags["upnp_rtcp_map"]);
                                }
                                catch
                                {
                                }
                                answerMedia.Tags.Remove("upnp_rtcp_map");
                            }

                            #endregion
                        }
                    }
                    // Remote-party wants to communicate with this stream.
                    else
                    {
                        // See if we can support this stream.
                        if (!audioAccepted && CanSupportMedia(offerMedia))
                        {
                            // New stream.
                            if (answerMedia == null)
                            {
                                answerMedia = new SDP_MediaDescription(SDP_MediaTypes.audio, 0, 2, "RTP/AVP", null);
                                localSDP.MediaDescriptions.Add(answerMedia);
                            }

                            #region Build audio codec map with codecs which we support

                            Dictionary<int, AudioCodec> audioCodecs = GetOurSupportedAudioCodecs(offerMedia);
                            answerMedia.MediaFormats.Clear();
                            answerMedia.Attributes.Clear();
                            foreach (KeyValuePair<int, AudioCodec> entry in audioCodecs)
                            {
                                answerMedia.Attributes.Add(new SDP_Attribute("rtpmap", entry.Key + " " + entry.Value.Name + "/" + entry.Value.CompressedAudioFormat.SamplesPerSecond));
                                answerMedia.MediaFormats.Add(entry.Key.ToString());
                            }
                            answerMedia.Attributes.Add(new SDP_Attribute("ptime", "20"));
                            answerMedia.Tags["audio_codecs"] = audioCodecs;

                            #endregion

                            #region Create/modify RTP session

                            // RTP session doesn't exist, create it.
                            if (!answerMedia.Tags.ContainsKey("rtp_session"))
                            {
                                RTP_Session rtpSess = CreateRtpSession(rtpMultimediaSession);
                                // RTP session creation failed,disable this stream.
                                if (rtpSess == null)
                                {
                                    answerMedia.Port = 0;

                                    break;
                                }
                                answerMedia.Tags.Add("rtp_session", rtpSess);

                                rtpSess.NewReceiveStream += delegate(object s, RTP_ReceiveStreamEventArgs e)
                                {
                                    if (answerMedia.Tags.ContainsKey("rtp_audio_out"))
                                    {
                                        ((AudioOut_RTP)answerMedia.Tags["rtp_audio_out"]).Dispose();
                                    }

                                    AudioOut_RTP audioOut = new AudioOut_RTP(m_pAudioOutDevice, e.Stream, audioCodecs);
                                    audioOut.Start();
                                    answerMedia.Tags["rtp_audio_out"] = audioOut;
                                };

                                // NAT
                                if (!HandleNAT(answerMedia, rtpSess))
                                {
                                    // NAT handling failed,disable this stream.
                                    answerMedia.Port = 0;

                                    break;
                                }
                            }

                            RTP_StreamMode offerStreamMode = GetRtpStreamMode(offer, offerMedia);
                            if (offerStreamMode == RTP_StreamMode.Inactive)
                            {
                                answerMedia.SetStreamMode("inactive");
                            }
                            else if (offerStreamMode == RTP_StreamMode.Receive)
                            {
                                answerMedia.SetStreamMode("sendonly");
                            }
                            else if (offerStreamMode == RTP_StreamMode.Send)
                            {
                                answerMedia.SetStreamMode("recvonly");
                            }
                            else if (offerStreamMode == RTP_StreamMode.SendReceive)
                            {
                                answerMedia.SetStreamMode("sendrecv");
                            }

                            RTP_Session rtpSession = (RTP_Session)answerMedia.Tags["rtp_session"];
                            rtpSession.Payload = Convert.ToInt32(answerMedia.MediaFormats[0]);
                            rtpSession.StreamMode = GetRtpStreamMode(localSDP, answerMedia);
                            rtpSession.RemoveTargets();
                            if (GetSdpHost(offer, offerMedia) != "0.0.0.0")
                            {
                                rtpSession.AddTarget(GetRtpTarget(offer, offerMedia));
                            }
                            rtpSession.Start();

                            #endregion

                            #region Create/modify audio-in source

                            if (!answerMedia.Tags.ContainsKey("rtp_audio_in"))
                            {
                                AudioIn_RTP rtpAudioIn = new AudioIn_RTP(m_pAudioInDevice, 20, audioCodecs, rtpSession.CreateSendStream());
                                rtpAudioIn.Start();
                                answerMedia.Tags.Add("rtp_audio_in", rtpAudioIn);
                            }
                            else
                            {
                                ((AudioIn_RTP)answerMedia.Tags["rtp_audio_in"]).AudioCodecs = audioCodecs;
                            }

                            #endregion

                            audioAccepted = true;
                        }
                        // We don't accept this stream, so disable it.
                        else
                        {
                            // Just copy offer media stream data to answer and set port to zero.

                            // Delete exisiting media stream.
                            if (answerMedia != null)
                            {
                                localSDP.MediaDescriptions.RemoveAt(i);
                            }
                            localSDP.MediaDescriptions.Add(offerMedia);
                            localSDP.MediaDescriptions[i].Port = 0;
                        }
                    }
                }

                #region Create and send 2xx response

                SIP_Response response = m_pStack.CreateResponse(SIP_ResponseCodes.x200_Ok, transaction.Request, transaction.Flow);
                //response.Contact = SIP stack will allocate it as needed;
                response.ContentType = "application/sdp";
                response.Data = localSDP.ToByte();

                transaction.SendResponse(response);

                // Start retransmitting 2xx response, while ACK receives.
                Handle2xx(dialog, transaction);

                // REMOVE ME: 27.11.2010
                // Start retransmitting 2xx response, while ACK receives.
                //m_pInvite2xxMgr.Add(dialog,transaction);

                #endregion
            }
            catch (Exception x)
            {
                transaction.SendResponse(m_pStack.CreateResponse(SIP_ResponseCodes.x500_Server_Internal_Error + ": " + x.Message, transaction.Request));
            }
        }

        private bool CanSupportMedia(SDP_MediaDescription media)
        {
            if (media == null)
            {
                throw new ArgumentNullException("media");
            }

            if (!string.Equals(media.MediaType, SDP_MediaTypes.audio, StringComparison.InvariantCultureIgnoreCase))
            {
                return false;
            }
            if (!string.Equals(media.Protocol, "RTP/AVP", StringComparison.InvariantCultureIgnoreCase))
            {
                return false;
            }

            if (GetOurSupportedAudioCodecs(media).Count > 0)
            {
                return true;
            }

            return false;
        }

        private Dictionary<int, AudioCodec> GetOurSupportedAudioCodecs(SDP_MediaDescription media)
        {
            if (media == null)
            {
                throw new ArgumentNullException("media");
            }

            Dictionary<int, AudioCodec> codecs = new Dictionary<int, AudioCodec>();

            // Check for IANA registered payload. Custom range is 96-127 and always must have rtpmap attribute.
            foreach (string format in media.MediaFormats)
            {
                int payload = Convert.ToInt32(format);
                if (payload < 96 && m_pAudioCodecs.ContainsKey(payload))
                {
                    if (!codecs.ContainsKey(payload))
                    {
                        codecs.Add(payload, m_pAudioCodecs[payload]);
                    }
                }
            }

            // Check rtpmap payloads.
            foreach (SDP_Attribute a in media.Attributes)
            {
                if (string.Equals(a.Name, "rtpmap", StringComparison.InvariantCultureIgnoreCase))
                {
                    // Example: 0 PCMU/8000
                    string[] parts = a.Value.Split(' ');
                    int payload = Convert.ToInt32(parts[0]);
                    string codecName = parts[1].Split('/')[0];

                    foreach (AudioCodec codec in m_pAudioCodecs.Values)
                    {
                        if (string.Equals(codec.Name, codecName, StringComparison.InvariantCultureIgnoreCase))
                        {
                            if (!codecs.ContainsKey(payload))
                            {
                                codecs.Add(payload, codec);
                            }
                        }
                    }
                }
            }

            return codecs;
        }

        private RTP_StreamMode GetRtpStreamMode(SDP_Message sdp, SDP_MediaDescription media)
        {
            if (sdp == null)
            {
                throw new ArgumentNullException("sdp");
            }
            if (media == null)
            {
                throw new ArgumentNullException("media");
            }

            // Try to get per media stream mode.
            foreach (SDP_Attribute a in media.Attributes)
            {
                if (string.Equals(a.Name, "sendrecv", StringComparison.InvariantCultureIgnoreCase))
                {
                    return RTP_StreamMode.SendReceive;
                }
                else if (string.Equals(a.Name, "sendonly", StringComparison.InvariantCultureIgnoreCase))
                {
                    return RTP_StreamMode.Send;
                }
                else if (string.Equals(a.Name, "recvonly", StringComparison.InvariantCultureIgnoreCase))
                {
                    return RTP_StreamMode.Receive;
                }
                else if (string.Equals(a.Name, "inactive", StringComparison.InvariantCultureIgnoreCase))
                {
                    return RTP_StreamMode.Inactive;
                }
            }

            // No per media stream mode, try to get per session stream mode.
            foreach (SDP_Attribute a in sdp.Attributes)
            {
                if (string.Equals(a.Name, "sendrecv", StringComparison.InvariantCultureIgnoreCase))
                {
                    return RTP_StreamMode.SendReceive;
                }
                else if (string.Equals(a.Name, "sendonly", StringComparison.InvariantCultureIgnoreCase))
                {
                    return RTP_StreamMode.Send;
                }
                else if (string.Equals(a.Name, "recvonly", StringComparison.InvariantCultureIgnoreCase))
                {
                    return RTP_StreamMode.Receive;
                }
                else if (string.Equals(a.Name, "inactive", StringComparison.InvariantCultureIgnoreCase))
                {
                    return RTP_StreamMode.Inactive;
                }
            }

            return RTP_StreamMode.SendReceive;
        }

        private string GetSdpHost(SDP_Message sdp, SDP_MediaDescription mediaStream)
        {
            if (sdp == null)
            {
                throw new ArgumentNullException("sdp");
            }
            if (mediaStream == null)
            {
                throw new ArgumentNullException("mediaStream");
            }

            // We must have SDP global or per media connection info.
            string host = mediaStream.Connection != null ? mediaStream.Connection.Address : null;
            if (host == null)
            {
                host = sdp.Connection.Address != null ? sdp.Connection.Address : null;

                if (host == null)
                {
                    throw new ArgumentException("Invalid SDP message, global or per media 'c'(Connection) attribute is missing.");
                }
            }

            return host;
        }

        private RTP_Address GetRtpTarget(SDP_Message sdp, SDP_MediaDescription mediaStream)
        {
            if (sdp == null)
            {
                throw new ArgumentNullException("sdp");
            }
            if (mediaStream == null)
            {
                throw new ArgumentNullException("mediaStream");
            }

            // We must have SDP global or per media connection info.
            string host = mediaStream.Connection != null ? mediaStream.Connection.Address : null;
            if (host == null)
            {
                host = sdp.Connection.Address != null ? sdp.Connection.Address : null;

                if (host == null)
                {
                    throw new ArgumentException("Invalid SDP message, global or per media 'c'(Connection) attribute is missing.");
                }
            }

            int remoteRtcpPort = mediaStream.Port + 1;
            // Use specified RTCP port, if specified.
            foreach (SDP_Attribute attribute in mediaStream.Attributes)
            {
                if (string.Equals(attribute.Name, "rtcp", StringComparison.InvariantCultureIgnoreCase))
                {
                    remoteRtcpPort = Convert.ToInt32(attribute.Value);

                    break;
                }
            }

            return new RTP_Address(System.Net.Dns.GetHostAddresses(host)[0], mediaStream.Port, remoteRtcpPort);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(VoIP));
            this.m_pStatusBar = new System.Windows.Forms.StatusStrip();
            this.statusLabel_Text = new System.Windows.Forms.ToolStripStatusLabel();
            this.statusLabel_Duration = new System.Windows.Forms.ToolStripStatusLabel();
            this.labelTS1 = new System.Windows.Forms.LabelTS();
            this.labelTS2 = new System.Windows.Forms.LabelTS();
            this.m_pLocalIP = new System.Windows.Forms.ComboBoxTS();
            this.m_pRemoteIP = new System.Windows.Forms.ComboBoxTS();
            this.m_pConnect = new System.Windows.Forms.ButtonTS();
            this.m_pTimerDuration = new System.Windows.Forms.Timer(this.components);
            this.m_pToolbar = new System.Windows.Forms.MenuStrip();
            this.m_pStatusBar.SuspendLayout();
            this.SuspendLayout();
            // 
            // m_pStatusBar
            // 
            this.m_pStatusBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusLabel_Text,
            this.statusLabel_Duration});
            this.m_pStatusBar.Location = new System.Drawing.Point(0, 112);
            this.m_pStatusBar.Name = "m_pStatusBar";
            this.m_pStatusBar.Size = new System.Drawing.Size(305, 22);
            this.m_pStatusBar.TabIndex = 3;
            this.m_pStatusBar.Text = "statusStrip1";
            // 
            // statusLabel_Text
            // 
            this.statusLabel_Text.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top)
                        | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right)
                        | System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom)));
            this.statusLabel_Text.Name = "statusLabel_Text";
            this.statusLabel_Text.Size = new System.Drawing.Size(230, 17);
            this.statusLabel_Text.Spring = true;
            this.statusLabel_Text.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // statusLabel_Duration
            // 
            this.statusLabel_Duration.AutoSize = false;
            this.statusLabel_Duration.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top)
                        | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right)
                        | System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom)));
            this.statusLabel_Duration.Name = "statusLabel_Duration";
            this.statusLabel_Duration.Size = new System.Drawing.Size(60, 17);
            // 
            // labelTS1
            // 
            this.labelTS1.AutoSize = true;
            this.labelTS1.Image = null;
            this.labelTS1.Location = new System.Drawing.Point(6, 42);
            this.labelTS1.Name = "labelTS1";
            this.labelTS1.Size = new System.Drawing.Size(46, 13);
            this.labelTS1.TabIndex = 4;
            this.labelTS1.Text = "Local IP";
            // 
            // labelTS2
            // 
            this.labelTS2.AutoSize = true;
            this.labelTS2.Image = null;
            this.labelTS2.Location = new System.Drawing.Point(6, 79);
            this.labelTS2.Name = "labelTS2";
            this.labelTS2.Size = new System.Drawing.Size(57, 13);
            this.labelTS2.TabIndex = 5;
            this.labelTS2.Text = "Remote IP";
            // 
            // m_pLocalIP
            // 
            this.m_pLocalIP.FormattingEnabled = true;
            this.m_pLocalIP.Items.AddRange(new object[] {
            "sip:xxx@192.168.1.3"});
            this.m_pLocalIP.Location = new System.Drawing.Point(63, 39);
            this.m_pLocalIP.Name = "m_pLocalIP";
            this.m_pLocalIP.Size = new System.Drawing.Size(151, 21);
            this.m_pLocalIP.TabIndex = 8;
            // 
            // m_pRemoteIP
            // 
            this.m_pRemoteIP.FormattingEnabled = true;
            this.m_pRemoteIP.Items.AddRange(new object[] {
            "sip:xxx@192.168.1.3:9999"});
            this.m_pRemoteIP.Location = new System.Drawing.Point(63, 75);
            this.m_pRemoteIP.Name = "m_pRemoteIP";
            this.m_pRemoteIP.Size = new System.Drawing.Size(151, 21);
            this.m_pRemoteIP.TabIndex = 9;
            // 
            // m_pConnect
            // 
            this.m_pConnect.Image = null;
            this.m_pConnect.Location = new System.Drawing.Point(223, 47);
            this.m_pConnect.Name = "m_pConnect";
            this.m_pConnect.Size = new System.Drawing.Size(75, 41);
            this.m_pConnect.TabIndex = 6;
            this.m_pConnect.UseVisualStyleBackColor = true;
            this.m_pConnect.Click += new System.EventHandler(this.m_pConnect_Click);
            // 
            // m_pTimerDuration
            // 
            this.m_pTimerDuration.Enabled = true;
            this.m_pTimerDuration.Interval = 1000;
            this.m_pTimerDuration.Tick += new System.EventHandler(this.m_pTimerDuration_Tick);
            // 
            // m_pToolbar
            // 
            this.m_pToolbar.Location = new System.Drawing.Point(0, 0);
            this.m_pToolbar.Name = "m_pToolbar";
            this.m_pToolbar.Size = new System.Drawing.Size(305, 24);
            this.m_pToolbar.TabIndex = 10;
            this.m_pToolbar.Text = "menuStrip1";
            // 
            // VoIP
            // 
            this.ClientSize = new System.Drawing.Size(305, 134);
            this.Controls.Add(this.m_pRemoteIP);
            this.Controls.Add(this.m_pLocalIP);
            this.Controls.Add(this.m_pConnect);
            this.Controls.Add(this.labelTS2);
            this.Controls.Add(this.labelTS1);
            this.Controls.Add(this.m_pStatusBar);
            this.Controls.Add(this.m_pToolbar);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.m_pToolbar;
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(321, 172);
            this.MinimumSize = new System.Drawing.Size(321, 172);
            this.Name = "VoIP";
            this.Text = "Remote connection";
            this.Closing += new System.ComponentModel.CancelEventHandler(this.VoIP_Closing);
            this.m_pStatusBar.ResumeLayout(false);
            this.m_pStatusBar.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        #region Save/Restore settings

        public void SaveOptions()
        {
            try
            {
                ArrayList a = new ArrayList();

                a.Add("Local IP/" + m_pLocalIP.Text);
                a.Add("Remote IP/" + m_pRemoteIP.Text);
                a.Add("audio_source/" + audio_source);
                a.Add("mic_source/" + mic_source);
                a.Add("nat_type/" + nat_type);
                a.Add("voip_top/" + this.Top.ToString());		    // save form positions
                a.Add("voip_left/" + this.Left.ToString());
                a.Add("voip_width/" + this.Width.ToString());
                a.Add("voip_height/" + this.Height.ToString());
                DB.SaveVars("VoIPOptions", ref a);		            // save the values to the DB
                DB.Update();

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error in VoIP SaveOptions function!\n" + ex.ToString());
            }
        }

        public void GetOptions()
        {
            try
            {
                ArrayList a = DB.GetVars("VoIPOptions");
                a.Sort();

                foreach (string s in a)
                {
                    string[] vals = s.Split('/');
                    string name = vals[0];
                    string val = vals[1];

                    if (s.StartsWith("voip_top"))
                    {
                        int top = Int32.Parse(vals[1]);
                        this.Top = top;
                    }
                    else if (s.StartsWith("voip_left"))
                    {
                        int left = Int32.Parse(vals[1]);
                        this.Left = left;
                    }
                    else if (s.StartsWith("voip_width"))
                    {
                        int width = Int32.Parse(vals[1]);
                        this.Width = width;
                    }
                    else if (s.StartsWith("voip_height"))
                    {
                        int height = Int32.Parse(vals[1]);
                        this.Height = height;
                    }
                    else if (s.StartsWith("audio_source"))
                    {
                        audio_source = val;
                    }
                    else if (s.StartsWith("mic_source"))
                    {
                        mic_source = val;
                    }
                    else if (s.StartsWith("nat_type"))
                    {
                        nat_type = val;
                    }
                    else if (s.StartsWith("Local IP"))
                    {
                        LocalIP = val;
                    }
                    else if (s.StartsWith("Remote IP"))
                    {
                        RemoteIP = val;
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

    #region VoIPincomming class

    public class VoipIncomingCall : Form
    {
        private Label m_pFrom = null;
        private Button m_pAccpet = null;
        private Button m_pReject = null;

        private SIP_ServerTransaction m_pTransaction = null;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="invite">SIP INVITE server transaction.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>invite</b> is null reference.</exception>
        public VoipIncomingCall(SIP_ServerTransaction invite)
        {
            if (invite == null)
            {
                throw new ArgumentNullException("invite");
            }

            InitUI();

            m_pTransaction = invite;
            m_pTransaction.Canceled += new EventHandler(m_pTransaction_Canceled);

            m_pFrom.Text = invite.Request.To.Address.ToStringValue();
        }

        #region method InitUI

        /// <summary>
        /// Creates and initializes UI.
        /// </summary>
        private void InitUI()
        {
            this.ClientSize = new Size(250, 100);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MinimizeBox = false;
            this.MaximizeBox = false;
            this.Text = "Incoming Call:";

            m_pFrom = new Label();
            m_pFrom.Size = new Size(250, 20);
            m_pFrom.Location = new Point(0, 15);
            m_pFrom.TextAlign = ContentAlignment.MiddleCenter;
            m_pFrom.ForeColor = Color.Gray;
            m_pFrom.Font = new Font(m_pFrom.Font.FontFamily, 8, FontStyle.Bold);

            m_pAccpet = new Button();
            m_pAccpet.Size = new Size(45, 45);
            m_pAccpet.Location = new Point(10, 40);
            m_pAccpet.Click += new EventHandler(m_pAccpet_Click);

            m_pReject = new Button();
            m_pReject.Size = new Size(45, 45);
            m_pReject.Location = new Point(195, 40);
            m_pReject.Click += new EventHandler(m_pReject_Click);

            this.Controls.Add(m_pFrom);
            this.Controls.Add(m_pAccpet);
            this.Controls.Add(m_pReject);
        }

        #endregion


        #region Event handlign

        #region method m_pAccpet_Click

        /// <summary>
        /// Is called when accpet button has clicked.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event data.</param>
        private void m_pAccpet_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Yes;
        }

        #endregion

        #region method m_pReject_Click

        /// <summary>
        /// Is called when reject button has clicked.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event data.</param>
        private void m_pReject_Click(object sender, EventArgs e)
        {
            m_pTransaction.SendResponse(m_pTransaction.Stack.CreateResponse(SIP_ResponseCodes.x600_Busy_Everywhere, m_pTransaction.Request));

            this.DialogResult = DialogResult.No;
        }

        #endregion


        #region method m_pTransaction_Canceled

        /// <summary>
        /// Is called when transcation has canceled.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event data.</param>
        private void m_pTransaction_Canceled(object sender, EventArgs e)
        {
            // We need invoke here, we are running on thread pool thread.
            this.BeginInvoke(new MethodInvoker(delegate()
            {
                this.DialogResult = DialogResult.No;
            }));
        }

        #endregion

        #endregion
    }

    #endregion

    #region SIP call class

    public class SIP_Call
    {
        #region enum

        /// <summary>
        /// This enum specifies SIP UA call states.
        /// </summary>

        #endregion

        private object m_pLock = new object();
        private SIP_CallState m_CallState = SIP_CallState.Calling;
        private SIP_Stack m_pStack = null;
        private SIP_RequestSender m_pInitialInviteSender = null;
        private RTP_MultimediaSession m_pRtpMultimediaSession = null;
        private DateTime m_StartTime;
        private SIP_Dialog_Invite m_pDialog = null;
        private SIP_Flow m_pFlow = null;
        private TimerEx m_pKeepAliveTimer = null;
        private SDP_Message m_pLocalSDP = null;
        private SDP_Message m_pRemoteSDP = null;
        private Dictionary<string, object> m_pTags = null;

        /// <summary>
        /// Calling constructor.
        /// </summary>
        /// <param name="stack">Reference to SIP stack.</param>
        /// <param name="sender">Initial INVITE sender.</param>
        /// <param name="session">Call RTP multimedia session.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>stack</b>,<b>sender</b> or <b>session</b> is null reference.</exception>
        internal SIP_Call(SIP_Stack stack, SIP_RequestSender sender, RTP_MultimediaSession session)
        {
            if (stack == null)
            {
                throw new ArgumentNullException("stack");
            }
            if (sender == null)
            {
                throw new ArgumentNullException("sender");
            }
            if (session == null)
            {
                throw new ArgumentNullException("session");
            }

            m_pStack = stack;
            m_pInitialInviteSender = sender;
            m_pRtpMultimediaSession = session;

            m_pTags = new Dictionary<string, object>();

            m_pInitialInviteSender.Completed += new EventHandler(delegate(object s, EventArgs e)
            {
                m_pInitialInviteSender = null;

                if (this.State == SIP_CallState.Terminating)
                {
                    SetState(SIP_CallState.Terminated);
                }
            });

            m_CallState = SIP_CallState.Calling;
        }

        /// <summary>
        /// Incoming call constructor.
        /// </summary>
        /// <param name="stack">Reference to SIP stack.</param>
        /// <param name="dialog">Reference SIP dialog.</param>
        /// <param name="session">Call RTP multimedia session.</param>
        /// <param name="localSDP">Local SDP.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>stack</b>,<b>dialog</b>,<b>session</b> or <b>localSDP</b> is null reference.</exception>
        internal SIP_Call(SIP_Stack stack, SIP_Dialog dialog, RTP_MultimediaSession session, SDP_Message localSDP)
        {
            if (stack == null)
            {
                throw new ArgumentNullException("stack");
            }
            if (dialog == null)
            {
                throw new ArgumentNullException("dialog");
            }
            if (session == null)
            {
                throw new ArgumentNullException("session");
            }
            if (localSDP == null)
            {
                throw new ArgumentNullException("localSDP");
            }

            m_pStack = stack;
            m_pDialog = (SIP_Dialog_Invite)dialog;
            m_pRtpMultimediaSession = session;
            m_pLocalSDP = localSDP;

            m_StartTime = DateTime.Now;
            m_pFlow = dialog.Flow;
            dialog.StateChanged += new EventHandler(m_pDialog_StateChanged);

            SetState(SIP_CallState.Active);

            // Start ping timer.
            m_pKeepAliveTimer = new TimerEx(40000);
            m_pKeepAliveTimer.Elapsed += new System.Timers.ElapsedEventHandler(m_pKeepAliveTimer_Elapsed);
            m_pKeepAliveTimer.Enabled = true;
        }

        #region method Dispose

        /// <summary>
        /// Cleans up any resource being used.
        /// </summary>
        public void Dispose()
        {
            lock (m_pLock)
            {
                if (this.State == SIP_CallState.Disposed)
                {
                    return;
                }
                SetState(SIP_CallState.Disposed);

                // TODO: Clean up
                m_pStack = null;
                m_pLocalSDP = null;
                if (m_pDialog != null)
                {
                    m_pDialog.Dispose();
                    m_pDialog = null;
                }
                m_pFlow = null;
                if (m_pKeepAliveTimer != null)
                {
                    m_pKeepAliveTimer.Dispose();
                    m_pKeepAliveTimer = null;
                }

                this.StateChanged = null;
            }
        }

        #endregion

        #region method InitCalling

        /// <summary>
        /// Initializes call from Calling state to active..
        /// </summary>
        /// <param name="dialog">SIP dialog.</param>
        /// <param name="localSDP">Local SDP.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>dialog</b> or <b>localSDP</b> is null reference.</exception>
        internal void InitCalling(SIP_Dialog dialog, SDP_Message localSDP)
        {
            if (dialog == null)
            {
                throw new ArgumentNullException("dialog");
            }
            if (localSDP == null)
            {
                throw new ArgumentNullException("localSDP");
            }

            m_pDialog = (SIP_Dialog_Invite)dialog;
            m_pFlow = dialog.Flow;
            m_pLocalSDP = localSDP;

            m_StartTime = DateTime.Now;
            dialog.StateChanged += new EventHandler(m_pDialog_StateChanged);

            SetState(SIP_CallState.Active);

            // Start ping timer.
            m_pKeepAliveTimer = new TimerEx(40000);
            m_pKeepAliveTimer.Elapsed += new System.Timers.ElapsedEventHandler(m_pKeepAliveTimer_Elapsed);
            m_pKeepAliveTimer.Enabled = true;
        }

        #endregion


        #region Events handling

        #region method m_pDialog_StateChanged

        /// <summary>
        /// Is called when SIP dialog state has changed.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event data.</param>
        private void m_pDialog_StateChanged(object sender, EventArgs e)
        {
            if (this.State == SIP_CallState.Disposed || this.State == SIP_CallState.Terminated)
            {
                return;
            }

            if (m_pDialog.State == SIP_DialogState.Terminated)
            {
                SetState(SIP_CallState.Terminated);
            }
        }

        #endregion

        #region method m_pKeepAliveTimer_Elapsed

        /// <summary>
        /// Is called when ping timer triggers.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event data.</param>
        private void m_pKeepAliveTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                // Send ping request if any flow using object has not sent ping within 30 seconds.
                if (m_pFlow.LastPing.AddSeconds(30) < DateTime.Now)
                {
                    m_pFlow.SendPing();
                }
            }
            catch
            {
            }
        }

        #endregion

        #endregion


        #region method Terminate

        /// <summary>
        /// Terminates call.
        /// </summary>
        /// <param name="reason">Call termination reason. This text is sent to remote-party.</param>
        /// <param name="sendBye">If true BYE request with <b>reason</b> text is sent remote-party.</param>
        public void Terminate(string reason, bool sendBye)
        {
            Terminate(reason);
        }

        /// <summary>
        /// Terminates a call.
        /// </summary>
        /// <param name="reason">Call termination reason. This text is sent to remote-party.</param>
        public void Terminate(string reason)
        {
            lock (m_pLock)
            {
                if (this.State == SIP_CallState.Disposed)
                {
                    throw new ObjectDisposedException(this.GetType().Name);
                }

                if (this.State == SIP_CallState.Terminating || this.State == SIP_CallState.Terminated)
                {
                    return;
                }
                else if (this.State == SIP_CallState.Active)
                {
                    SetState(SIP_CallState.Terminating);

                    m_pDialog.Terminate(reason, true);
                }
                else if (this.State == SIP_CallState.Calling && m_pInitialInviteSender != null)
                {
                    /* RFC 3261 15.
                        If we are caller and call is not active yet, we must do following actions:
                            *) Send CANCEL, set call Terminating flag.
                            *) If we get non 2xx final response, we are done. (Normally cancel causes '408 Request terminated')
                            *) If we get 2xx response (2xx sent by remote party before our CANCEL reached), we must send BYE to active dialog.
                    */

                    SetState(SIP_CallState.Terminating);

                    m_pInitialInviteSender.Cancel();
                }
            }
        }

        #endregion


        #region method SetState

        /// <summary>
        /// Set call state.
        /// </summary>
        /// <param name="state">New call state.</param>
        private void SetState(SIP_CallState state)
        {
            // Disposed call may not change state.
            if (this.State == SIP_CallState.Disposed)
            {
                return;
            }

            m_CallState = state;

            OnStateChanged(state);

            if (state == SIP_CallState.Terminated)
            {
                Dispose();
            }
        }

        #endregion


        #region Properties implementation

        /// <summary>
        /// Gets current call state.
        /// </summary>
        public SIP_CallState State
        {
            get { return m_CallState; }
        }

        /// <summary>
        /// Gets call RTP multimedia session.
        /// </summary>
        public RTP_MultimediaSession RtpMultimediaSession
        {
            get { return m_pRtpMultimediaSession; }
        }

        /// <summary>
        /// Gets call start time.
        /// </summary>
        public DateTime StartTime
        {
            get { return m_StartTime; }
        }

        /// <summary>
        /// Gets call dialog. Returns null if dialog not created yet.
        /// </summary>
        public SIP_Dialog_Invite Dialog
        {
            get { return m_pDialog; }
        }

        /// <summary>
        /// Gets or sets current local SDP.
        /// </summary>
        public SDP_Message LocalSDP
        {
            get { return m_pLocalSDP; }

            set { m_pLocalSDP = value; }
        }

        /// <summary>
        /// Gets or sets current remote SDP.
        /// </summary>
        public SDP_Message RemoteSDP
        {
            get { return m_pRemoteSDP; }

            set { m_pRemoteSDP = value; }
        }

        /// <summary>
        /// Gets user data items collection.
        /// </summary>
        public Dictionary<string, object> Tags
        {
            get { return m_pTags; }
        }

        #endregion

        /// <summary>
        /// Is raised when call state has changed.
        /// </summary>
        public event EventHandler StateChanged = null;


        /// <summary>
        /// Raises <b>StateChanged</b> event.
        /// </summary>
        /// <param name="state">New call state.</param>
        private void OnStateChanged(SIP_CallState state)
        {
            if (this.StateChanged != null)
            {
                this.StateChanged(this, new EventArgs());
            }
        }
    }

    #endregion
}
