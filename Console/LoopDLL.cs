//=================================================================
// loop.dll interface class
//=================================================================
//
//  Copyright (C)2010 YT7PWR Goran Radivojevic
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
using System.Collections;
using System.Text;
using System.Security;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Diagnostics;

namespace PowerSDR
{
    unsafe public class LoopDLL
    {
        #region Variable
        Console console;

        #endregion

        #region properties

        private int sample_rate = 48000;
        public int SampleRate
        {
            get { return sample_rate; }
            set { sample_rate = value; }
        }

        private int block_size = 2048;
        public int BlockSize
        {
            get { return block_size; }
            set { block_size = value; }
        }

        #endregion

        #region DLL export/import definition

        [DllImport("loop.dll", EntryPoint = "GetPTT")]
        public static extern byte GetPTT ();

        [DllImport("loop.dll", EntryPoint = "InitRXPlayback")]
        public static extern void InitRXPlayback(int sample_rate, int block_size);

        [DllImport("loop.dll", EntryPoint = "InitRXRecording")]
        public static extern void InitRXRecording(int sample_rate, int block_size);

        [DllImport("loop.dll", EntryPoint = "WriteRX")]
        public static extern bool WriteRX(double* buffer);

        [DllImport("loop.dll", EntryPoint = "WriteTX")]
        public static extern bool WriteTX(double* buffer);

        [DllImport("loop.dll", EntryPoint = "InitTXPlayback")]
        public static extern void InitTXPlayback(int sample_rate, int block_size);

        [DllImport("loop.dll", EntryPoint = "InitTXRecording")]
        public static extern void InitTXRecording(int sample_rate, int block_size);

        [DllImport("loop.dll", EntryPoint = "ReadTX")]
        public static extern bool ReadTX(double* e);

        [DllImport("loop.dll", EntryPoint = "ReadRX")]
        public static extern bool ReadRX(double* e);

        [DllImport("loop.dll", EntryPoint = "ReadRXs")]
        public static extern bool ReadRXs(short* e);

        [DllImport("loop.dll", EntryPoint = "ReadConfig")]
        public static extern int ReadConfig(int type);

        #endregion

        #region constructor

        public LoopDLL(Console c)
        {
            console = c;
        }

        ~LoopDLL()
        {

        }

        #endregion

        #region misc function

        public bool InitRXPlay(int SampleRate, int BlockSize)
        {
            InitRXPlayback(SampleRate, BlockSize);
            return true;
        }

        public bool InitRXRec(int SampleRate, int BlockSize)
        {
            InitRXRecording(SampleRate, BlockSize);
            return true;
        }

        public bool InitTXRec(int SampleRate, int BlockSize)
        {
            InitTXRecording(SampleRate, BlockSize);
            return true;
        }

        public bool InitTXPlay(int SampleRate, int BlockSize)
        {
            InitTXPlayback(SampleRate, BlockSize);
            return true;
        }

        public bool WriteRXBuffer(double *buffer)
        {
            if (WriteRX(buffer))
                return true;
            else
                return false;
        }

        public bool WriteTXBuffer(double* buffer)
        {
            if (WriteTX(buffer))
                return true;
            else
                return false;
        }

        public bool ReadTXBuffer(double *buffer)
        {
            if (ReadTX(buffer))
                return true;
            else
                return false;
        }

        public bool ReadRXBuffer(double* buffer)
        {
            if (ReadRX(buffer))
                return true;
            else
                return false;
        }

        public bool ReadRXBufferShort(short* buffer)
        {
            if (ReadRXs(buffer))
                return true;
            else
                return false;
        }

        public byte IsPTT()
        {
            return GetPTT();
        }

        public int ReadConfiguration(int type)
        {
            return ReadConfig(type);
        }

        #endregion

    }
}