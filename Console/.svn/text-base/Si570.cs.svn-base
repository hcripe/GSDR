//=================================================================
// Si570 external control
//=================================================================
//
//  USB communication with External Si570(QRP2000 from www.sdr-kits.net)
//  Copyright (C)2008-2011 YT7PWR Goran Radivojevic
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

#define Si570

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
    unsafe public class ExtIO_si570_usb
    {
        public Console console;
        public bool connected = false;
        int tmp;
        public delegate void CallbackFunction(int cnt, int status, float IQoffs, short[] IQdata);
        private static CallbackFunction callback;


        public ExtIO_si570_usb(Console c)
        {
            console = c;
        }

        #region Dll Method Definitions
        // ======================================================
        // DLL Method Definitions
        // ======================================================

        [DllImport("ExtIO_si570_usb.dll", EntryPoint = "InitHW")]
        public static extern bool InitHW(char[] name, char[] model, int[] type);

        [DllImport("ExtIO_si570_usb.dll", EntryPoint = "OpenHW")]
        public static extern bool OpenHW();

        [DllImport("ExtIO_si570_usb.dll", EntryPoint = "StartHW")]
        public static extern int StartHW(long LOfreq);

        [DllImport("ExtIO_si570_usb.dll", EntryPoint = "GetStatus")]
        public static extern int GetStatus();

        [DllImport("ExtIO_si570_usb.dll", EntryPoint = "StopHW")]
        public static extern void StopHW();

        [DllImport("ExtIO_si570_usb.dll", EntryPoint = "CloseHW")]
        public static extern void CloseHW();

        [DllImport("ExtIO_si570_usb.dll", EntryPoint = "SetHWLO")]
        public static extern int SetHWLO(long freq);

        [DllImport("ExtIO_si570_usb.dll", EntryPoint = "GetHWLO")]
        public static extern long GetHWLO();

        [DllImport("ExtIO_si570_usb.dll", EntryPoint = "GetHWSR")]
        public static extern long GetHWSR();

        [DllImport("ExtIO_si570_usb.dll", EntryPoint = "SetCallback")]
        public static extern void SetCallback(CallbackFunction function);

        [DllImport("ExtIO_si570_usb.dll", EntryPoint = "ShowGUI")]
        public static extern void ShowGUI();

        [DllImport("ExtIO_si570_usb.dll", EntryPoint = "HideGUI")]
        public static extern void HideGUI();

        [DllImport("ExtIO_si570_usb.dll", EntryPoint = "SetPTT")]
        public static extern void SetPTT(bool ptt);

        [DllImport("ExtIO_si570_usb.dll", EntryPoint = "GetMode")]
        public static extern char GetMode();

        [DllImport("ExtIO_si570_usb.dll", EntryPoint = "GetTune")]
        public static extern long GetTune();

        [DllImport("ExtIO_si570_usb.dll", EntryPoint = "GetFilters")]
        public static extern void GetFilters(ref int loCut, ref int hiCut, ref int pitch);

        [DllImport("ExtIO_si570_usb.dll", EntryPoint = "ModeChanged")]
        public static extern void ModeChanged(char new_mode);

        [DllImport("ExtIO_si570_usb.dll", EntryPoint = "IFLimitsChanged")]
        public static extern void ISLimitsChanged(long low, long high);

        [DllImport("ExtIO_si570_usb.dll", EntryPoint = "TuneChanged")]
        public static extern void TuneChanged(long new_freq);

        [DllImport("ExtIO_si570_usb.dll", EntryPoint = "RawDataReady")]
        public static extern void RawDataReady(long samplerate, int[] Ldata, int[] Rdata, int numsamples);

        #endregion

        #region ExtIO_Si570 routines
        // ======================================================
        // Misc Routines
        // ======================================================

        public void Start_SI570(long freq)
        {
            try
            {
                if (console.SI570 != null && console.SI570.connected)
                    StartHW(freq);
            }
            catch (Exception ex)
            {
                Debug.Write(ex.ToString());
            }
        }

        public void Show_SI570_GUI()
        {
            try
            {
                if (console.SI570 != null)
                {
                    ShowGUI();
                }
            }
            catch (Exception ex)
            {
                Debug.Write(ex.ToString());
            }
        }

        public void Hide_SI570_GUI()
        {
            try
            {
                if (console.SI570 != null && console.SI570.connected)
                    HideGUI();
            }
            catch (Exception ex)
            {
                Debug.Write(ex.ToString());
            }
        }

        public void SetTX(bool ptt)
        {
            try
            {
                if (console.SI570 != null && console.SI570.connected)
                    SetPTT(ptt);
            }
            catch (Exception ex)
            {
                Debug.Write(ex.ToString());
            }
        }

        unsafe public bool Init_USB()
        {
            char[] name = new char[64];
            char[] model = new char[64];
            int[] type = new int[1];
            connected = false;

            try
            {
                if (File.Exists("ExtIO_Si570_usb.dll"))
                {
                    connected = InitHW(name, model, type);

                    if (connected)
                    {
                        callback = new CallbackFunction(ExtSi570CallbackFunction);
                        SetCallback(callback);
                        if (!console.SkinsEnabled)
                            console.btnUSB.BackColor = Color.GreenYellow;
                        console.UsbSi570Enable = true;
                    }
                    else
                    {
                        if (!console.SkinsEnabled)
                            console.SetupForm.chkGeneralUSBPresent.Checked = false;
                        console.UsbSi570Enable = false;
                        console.btnUSB.BackColor = Color.Red;
                    }

                    return connected;
                }
                else
                    return false;
            }
            catch (Exception ex)
            {
                Debug.Write("Error while Si570 init!\n",
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
            catch
            {
                Debug.Write("Error while closing USB connection!\n",
                    "Error!");
            }
        }

        public void Get_Block()
        {

        }

        public void Tune_Changed(long newfreq)
        {
            try
            {
                TuneChanged(newfreq);
            }
            catch (Exception ex)
            {
                Debug.Write("Error setting new Tune frequency!\nValue is wrong!\n",
                    "Error!\n" + ex.ToString());
            }
        }

        public void Set_SI570_osc(long freq)
        {
            try
            {
                tmp = SetHWLO(freq);
            }
            catch (Exception ex)
            {
                Debug.Write("Error setting new frequency!\nValue is wrong!\n",
                    "Error!\n" + ex.ToString());
            }
        }

        public void Stop_HW()
        {
            try
            {
                StopHW();
            }
            catch(Exception ex)
            {
                Debug.Write("Error in StopHW!\n",
                    "Error!\n" + ex.ToString());
            }
        }

        private void ExtSi570CallbackFunction(int cnt, int status, float IQoffs, short[] IQdata)
        {
            long newfreq = 0;
            try
            {
                if(cnt<0)
                {
                    switch (status)
                    {
                        case(100):                              // sampling frequency changed
                            break;
                        case (101):                             // LO changed
                            newfreq = GetHWLO();
                            break;
                        case (102):                             // LO locked!
                            break;
                        case (103):                             // LO unlocked!
                            break;
                        case (104):                             // not implemented!
                            break;
                        case (105):                             // Tune frequency changed
                            newfreq = GetTune();
                            TuneChanged((long)(console.VFOAFreq * 1e6));
                            break;
                        case (106):                             // mode changed
                            char newmode = GetMode();

                            switch (newmode)
                            {
                                case 'U':
                                    console.CurrentDSPMode = DSPMode.USB;
                                    break;
                                case 'L':
                                    console.CurrentDSPMode = DSPMode.LSB;
                                    break;
                                case 'D':
                                    console.CurrentDSPMode = DSPMode.DRM;
                                    break;
                                case 'A':
                                    console.CurrentDSPMode = DSPMode.AM;
                                    break;
                                case 'S':
                                    console.CurrentDSPMode = DSPMode.SAM;
                                    break;
                                case 'C':
                                    console.CurrentDSPMode = DSPMode.CWU;
                                    break;
                            }

                            break;
                        case (107):                             // start command!
                            console.chkPower.Checked = true;
                            break;
                        case (108):                             // stop command!
                            console.chkPower.Checked = false;
                            break;
                        case (109):                             // passband limits changed!
                            int loCut = 0;
                            int hiCut = 0;
                            int pitch = 0;
                            GetFilters(ref loCut, ref hiCut, ref pitch);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Print("Error in ExtSi570Callback!\n" + ex.ToString());
            }

        }

        #endregion
    }
}