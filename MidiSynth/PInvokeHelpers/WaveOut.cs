using System;
using System.Runtime.InteropServices;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Windows;
using System.ComponentModel;
using System.Text;

	public class WaveOut
	{
        public class WaveOutDevice : IDisposable
        {
            protected IntPtr m_hwo = IntPtr.Zero;
            private int sampleRate, bufferSize;
            private bool opened = false;
            private uint deviceID;
            private string syncName = "MidiSynthWaveSync";
            public string SyncName
            {
                get { return syncName; }
            }

            public enum WaveSampleRate : uint
            {
                SR_8000 = 8000,
                SR_11025 = 11025,
                SR_22050 = 22050,
                SR_44100 = 44100
            }

            public WaveOutDevice(int sampleRate, int bufferSize)
            {
                this.sampleRate = sampleRate;
                this.bufferSize = bufferSize;
                this.syncName += "_" + this.GetHashCode().ToString();
            }

            public bool Open()
            {
                if (!opened)
                {
                    deviceID = OpenWaveDevice(sampleRate, bufferSize, syncName);
                    return true;
                }
                return false;
            }

            public void Write(float[] data)
            {
                byte[] ca = new byte[data.Length];

                for (int i = 0; i < data.Length; i++)
                {
                    byte b = (byte)Math.Round((data[i] + 1.0) * 255 / 2.0);
                    ca[i] = b;
                }
                WriteWaveBuffer(deviceID, ca, (uint) ca.Length);
            }

            public void Close()
            {
                CloseWaveDevice(deviceID);
            }

            public void Dispose()
            {
                Close();
            }
        }
        
        [DllImport("waveOutHelper.dll", CharSet=CharSet.Ansi, ExactSpelling=true, SetLastError=false)]
        private static extern uint OpenWaveDevice(int sampleRate, int bufferSize, string lpSyncName);
        [DllImport("waveOutHelper.dll", SetLastError=false)]
        private static extern uint CloseWaveDevice(uint deviceId);
        [DllImport("waveOutHelper.dll", CharSet=CharSet.Ansi, SetLastError=false)]
        private static extern uint WriteWaveBuffer(uint deviceId, byte[] input, uint szInput);
	}
