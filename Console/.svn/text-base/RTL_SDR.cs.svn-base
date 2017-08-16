//=================================================================
// RTL SDR external control
//=================================================================
//
//  USB communication with External RTL SDR DVB-T stick
//  Copyright (C)2013 YT7PWR Goran Radivojevic
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
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Diagnostics;
using System.Text;
using System.Drawing;
using System.Threading;
using Microsoft.Win32.SafeHandles;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;


namespace PowerSDR
{
    unsafe public class ExtIO_RTL
    {
        #region Dll Method Definitions

        [DllImport("ExtIO_RTL.dll", EntryPoint = "InitHW", CallingConvention = CallingConvention.StdCall)]
        public static extern int InitHW(byte* name, byte* model, int[] type);

        [DllImport("ExtIO_RTL.dll", EntryPoint = "OpenHW", CallingConvention = CallingConvention.StdCall)]
        public static extern int OpenHW();

        [DllImport("ExtIO_RTL.dll", EntryPoint = "StartHW", CallingConvention = CallingConvention.StdCall)]
        public static extern int StartHW();

        [DllImport("ExtIO_RTL.dll", EntryPoint = "GetStatus", CallingConvention = CallingConvention.StdCall)]
        public static extern int GetStatus();

        [DllImport("ExtIO_RTL.dll", EntryPoint = "StopHW", CallingConvention = CallingConvention.StdCall)]
        public static extern void StopHW();

        [DllImport("ExtIO_RTL.dll", EntryPoint = "CloseHW", CallingConvention = CallingConvention.StdCall)]
        public static extern void CloseHW();

        [DllImport("ExtIO_RTL.dll", EntryPoint = "SetHWLO", CallingConvention = CallingConvention.StdCall)]
        public static extern int SetHWLO(Int32 freq);

        [DllImport("ExtIO_RTL.dll", EntryPoint = "GetHWLO", CallingConvention = CallingConvention.StdCall)]
        public static extern int GetHWLO();

        [DllImport("ExtIO_RTL.dll", EntryPoint = "GetHWSR", CallingConvention = CallingConvention.StdCall)]
        public static extern int GetHWSR();

        [DllImport("ExtIO_RTL.dll", EntryPoint = "SetHWSR", CallingConvention = CallingConvention.StdCall)]
        public static extern int SetHWSR(int idx);

        [DllImport("ExtIO_RTL.dll", EntryPoint = "SetAudioCallback", CallingConvention = CallingConvention.StdCall)]
        public static extern void SetAudioCallback(AudioCallbackFunction function);

        [DllImport("ExtIO_RTL.dll", EntryPoint = "SetAttenuator", CallingConvention = CallingConvention.StdCall)]
        public static extern int SetAGC(int idx);

        [DllImport("ExtIO_RTL.dll", EntryPoint = "GetAttenuator", CallingConvention = CallingConvention.StdCall)]
        public static extern int GetAGC();

        [DllImport("ExtIO_RTL.dll", EntryPoint = "SetTunerAGCmode", CallingConvention = CallingConvention.StdCall)]
        public static extern int SetTunerAGC_Mode(int idx);

        [DllImport("ExtIO_RTL.dll", EntryPoint = "GetTunerAGCmode", CallingConvention = CallingConvention.StdCall)]
        public static extern int GetTunerAGC_Mode(int idx);

        [DllImport("ExtIO_RTL.dll", EntryPoint = "SetFreqOffset", CallingConvention = CallingConvention.StdCall)]
        public static extern int SetFRQoffset(int offset);

        [DllImport("ExtIO_RTL.dll", EntryPoint = "SetBufferSize", CallingConvention = CallingConvention.StdCall)]
        public static extern int SetBufferSize(int size);

        #endregion

        #region variables

        public Console console;
        public bool connected = false;
        public delegate void AudioCallbackFunction(float* in_l, float* in_r, float* out_l, float* out_r, int count);
        static AudioCallbackFunction audio_callback;
        public delegate void DebugCallbackFunction(string msg);
        int BufferSize = 0;
        public bool debug = false;

        #endregion

        #region properties

        private int buffer_length = 2048;
        public int BufferLength
        {
            get { return buffer_length; }
            set { buffer_length = value; }
        }

        #endregion

        #region constructor

        public ExtIO_RTL(Console c)
        {
            console = c;
        }

        #endregion

        #region ExtIO routines

        public void Start()
        {
            try
            {
                if (connected)
                {
                    int result = StartHW();

                    if (result > 0)
                        BufferSize = result;
                }
            }
            catch (Exception ex)
            {
                if (debug && !console.ConsoleClosing)
                    console.Invoke(new DebugCallbackFunction(console.DebugCallback), "StartHW error: \n" + ex.ToString());

                Debug.Write(ex.ToString());
            }
        }

        unsafe public bool InitUSB()
        {
            byte[] name = new byte[256];
            byte[] model = new byte[256];
            int[] type = new int[16];
            connected = false;
            string[] vals;
            ASCIIEncoding buffer = new ASCIIEncoding();

            try
            {
                if (File.Exists("ExtIO_RTL.dll"))
                {
                    fixed (byte* Name = &name[0])
                    fixed (byte* Model = &model[0])
                    {
                        if (InitHW(Name, Model, type) == 1)
                            connected = true;
                        else
                            connected = false;
                        vals = buffer.GetString(name).Split('\0') ;
                        string hw_name = vals[0];
                        vals = buffer.GetString(model).Split('\0');
                        string hw_model = vals[0];
                        console.SetupForm.txtRTL_SDR_Device.Text = hw_name + " " + hw_model;
                    }

                    if (connected)
                    {
                        if (OpenHW() == 1)
                        {
                            audio_callback = new AudioCallbackFunction(Audio.RTL_SDR_AudioCallback);
                            SetAudioCallback(audio_callback);

                            if (!console.SkinsEnabled)
                                console.btnUSB.BackColor = Color.GreenYellow;
                        }
                        else
                        {
                            connected = false;
                            CloseHW();

                            if (!console.SkinsEnabled)
                                console.btnUSB.BackColor = Color.Red;
                        }
                    }
                    else
                    {
                        if (!console.SkinsEnabled)
                            console.SetupForm.chkGeneralUSBPresent.Checked = false;

                        console.btnUSB.BackColor = Color.Red;
                    }

                    return connected;
                }
                else
                    return false;
            }
            catch (Exception ex)
            {
                if (debug && !console.ConsoleClosing)
                    console.Invoke(new DebugCallbackFunction(console.DebugCallback), "InitUSB error: \n" + ex.ToString());

                Debug.Write("Error while RTL_SDR init!\n",
                    "Error!\n" + ex.ToString());
                return false;
            }
        }

        public void CloseUSB()
        {
            try
            {
                if (connected)
                    CloseHW();

                connected = false;
            }
            catch(Exception ex)
            {
                if (debug && !console.ConsoleClosing)
                    console.Invoke(new DebugCallbackFunction(console.DebugCallback), "CloseUSB error: \n" + ex.ToString());

                Debug.Write("Error while closing USB connection!\n",
                    "Error!" + ex.ToString() + "\n");
            }
        }

        public void SetLOSC(Int32 freq)
        {
            try
            {
                int tmp;

                if (connected)
                    tmp = SetHWLO(freq);
            }
            catch (Exception ex)
            {
                if (debug && !console.ConsoleClosing)
                    console.Invoke(new DebugCallbackFunction(console.DebugCallback), "SetLOSC error: \n" + ex.ToString());

                Debug.Write("Error setting new frequency!\nValue is wrong!\n",
                    "Error!\n" + ex.ToString());
            }
        }

        public void SetAGCgain(int idx)
        {
            try
            {
                SetAGC(idx);
            }
            catch (Exception ex)
            {
                if (debug && !console.ConsoleClosing)
                    console.Invoke(new DebugCallbackFunction(console.DebugCallback), "SetAGCgain error: \n" + ex.ToString());

                Debug.Write("Error setting new AGC gain!\nValue is wrong!\n",
                    "Error!\n" + ex.ToString());
            }
        }

        public int GetAGCgain()
        {
            try
            {
                return GetAGC();
            }
            catch (Exception ex)
            {
                if (debug && !console.ConsoleClosing)
                    console.Invoke(new DebugCallbackFunction(console.DebugCallback), "GetAGCgain error: \n" + ex.ToString());

                Debug.Write("Error getting AGC gain!\nValue is wrong!\n",
                    "Error!\n" + ex.ToString());
                return -1;
            }
        }

        public void SetAGC_mode(int idx)
        {
            try
            {
                int result = SetTunerAGC_Mode(idx);
            }
            catch (Exception ex)
            {
                if (debug && !console.ConsoleClosing)
                    console.Invoke(new DebugCallbackFunction(console.DebugCallback), "SetAGCmode error: \n" + ex.ToString());

                Debug.Write("Error setting new AGC mode!\nValue is wrong!\n",
                    "Error!\n" + ex.ToString());
            }
        }

        public int GetAGC_mode()
        {
            try
            {
                int idx = GetAGC_mode();

                return idx;
            }
            catch (Exception ex)
            {
                if (debug && !console.ConsoleClosing)
                    console.Invoke(new DebugCallbackFunction(console.DebugCallback), "GetAGCmode error: \n" + ex.ToString());

                Debug.Write("Error getting AGC mode!\n",
                    "Error!\n" + ex.ToString());

                return -1;
            }
        }

        public void SetFrequencyOffset(int offset)
        {
            try
            {
                SetFRQoffset(offset);
            }
            catch (Exception ex)
            {
                if (debug && !console.ConsoleClosing)
                    console.Invoke(new DebugCallbackFunction(console.DebugCallback), "SetFrequencyOffset error: \n" + ex.ToString());

                Debug.Write("Error setting new Frequency offset!\nValue is wrong!\n",
                    "Error!\n" + ex.ToString());
            }
        }

        public void SetSampleRate(int idx)
        {
            try
            {
                if (connected)
                    SetHWSR(idx);
            }
            catch (Exception ex)
            {
                if (debug && !console.ConsoleClosing)
                    console.Invoke(new DebugCallbackFunction(console.DebugCallback), "SetSampleRate error: \n" + ex.ToString());

                Debug.Write(ex.ToString());
            }
        }

        public void Set_Buffer_Size(int new_buffer_size)
        {
            try
            {
                if (connected)
                {
                    buffer_length = new_buffer_size;
                    SetBufferSize(new_buffer_size);
                }
            }
            catch (Exception ex)
            {
                if (debug && !console.ConsoleClosing)
                    console.Invoke(new DebugCallbackFunction(console.DebugCallback), "SetBufferSize error: \n" + ex.ToString());

                Debug.Write(ex.ToString());
            }
        }

        public void Stop_HW()
        {
            try
            {
                if (connected)
                    StopHW();
            }
            catch(Exception ex)
            {
                if (debug && !console.ConsoleClosing)
                    console.Invoke(new DebugCallbackFunction(console.DebugCallback), "QRP2000: \n" + ex.ToString());

                Debug.Write("Error in StopHW!\n",
                    "Error!\n" + ex.ToString());
            }
        }

        #endregion
    }
}