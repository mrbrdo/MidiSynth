using System;
using System.Runtime.InteropServices;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Windows;
using System.ComponentModel;
using System.Text;

	public class Wave
	{
        private static int bytes2int(byte[] ba, int start, int length = 4)
        {
            int result = 0;

            for (int i = length - 1; i >= 0; i--)
            {
                result <<= 8;
                result += ba[start + i];
            }

            return result;
        }

        private static bool CompareStringToBytes(byte[] ba, int start, string compareTo)
        {
            for (int i = 0; i < compareTo.Length; i++)
            {
                if ((char)ba[start + i] != compareTo[i]) return false;
            }
            return true;
        }

        public static float[][] ReadWavePCM(string filename, out int sampleRate)
        {
            byte[] ba = File.ReadAllBytes(filename);
            if (!CompareStringToBytes(ba, 0, "RIFF"))
                throw new Exception("Not a valid RIFF file.");
            if (!CompareStringToBytes(ba, 8, "WAVE"))
                throw new Exception("Not a valid WAVE file.");
            if (!CompareStringToBytes(ba, 12, "fmt "))
                throw new Exception("Not a valid WAVE file (invalid Subchunk1ID).");
            if (!(bytes2int(ba, 16) == 16))
                throw new Exception("Not a valid PCM file (invalid Subchunk1Size).");
            if (!(bytes2int(ba, 20, 2) == 1))
                throw new Exception("Not a valid PCM file (invalid AudioFormat).");

            int numChannels = bytes2int(ba, 22, 2);
            sampleRate = bytes2int(ba, 24);
            int bytesPerSample = bytes2int(ba, 34, 2) / 8;
            float divider = (int) Math.Pow(2, bytesPerSample * 8 - 1);

            int dataStart = 36;

            // skip fact chunk if present
            if (CompareStringToBytes(ba, dataStart, "fact"))
                dataStart += 8 + bytes2int(ba, dataStart + 4);

            if (!CompareStringToBytes(ba, dataStart, "data"))
                throw new Exception("Not a valid PCM file (invalid Subchunk2ID).");

            int dataLen = bytes2int(ba, dataStart + 4);
            int nSamples = dataLen / (numChannels * bytesPerSample);

            float[][] data = new float[numChannels][];
            for (int i = 0; i < numChannels; i++)
                data[i] = new float[nSamples];

            int readPos = dataStart + 8;
            for (int i = 0; i < nSamples; i++)
            {
                for (int ch = 0; ch < numChannels; ch++)
                {
                    if (bytesPerSample == 1)
                      data[ch][i] = (float)(bytes2int(ba, readPos, bytesPerSample) / divider - 1.0f);
                    else
                      data[ch][i] = bytes2int(ba, readPos, bytesPerSample) / divider;
                    readPos += bytesPerSample;
                }
            }

            return data;
        }

        public class WaveOutDevice : IDisposable
        {
            protected IntPtr m_hwo = IntPtr.Zero;
            private int sampleRate, bufferSize, nChannels, bitsPerSample, bytesPerSample;
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

            public WaveOutDevice(int sampleRate, int bitsPerSample, int bufferSize)
            {
                this.sampleRate = sampleRate;
                this.bufferSize = bufferSize * bitsPerSample;
                this.nChannels = 1;
                this.bitsPerSample = bitsPerSample;
                this.bytesPerSample = (int) Math.Ceiling(bitsPerSample / 8.0);
                this.syncName += "_" + this.GetHashCode().ToString();
            }

            public bool Open()
            {
                if (!opened)
                {
                    deviceID = OpenWaveDevice(sampleRate, nChannels, bitsPerSample, bufferSize, syncName);
                    return true;
                }
                return false;
            }

            public void Write(float[] data)
            {
                byte[] ca = new byte[data.Length * bytesPerSample];
                float mult = (float) (Math.Pow(2, bitsPerSample) - 1.0f) / 2.0f;
                int minus = (int)Math.Pow(2, bitsPerSample - 1);
                if (bitsPerSample == 8) minus = 0;

                for (int i = 0, j = 0; i < data.Length; i++)
                {
                    int value = (int) Math.Round((data[i] + 1.0) * mult) - minus;
                    for (int k = 0; k < bytesPerSample; k++)
                    {
                        ca[j + k] = (byte)(value);
                        value >>= 8;
                    }
                    j += bytesPerSample;
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
        private static extern uint OpenWaveDevice(int sampleRate, int nChannels, int bitsPerSample, int bufferSize, string lpSyncName);
        [DllImport("waveOutHelper.dll", SetLastError=false)]
        private static extern uint CloseWaveDevice(uint deviceId);
        [DllImport("waveOutHelper.dll", CharSet=CharSet.Ansi, SetLastError=false)]
        private static extern uint WriteWaveBuffer(uint deviceId, byte[] input, uint szInput);
	}
