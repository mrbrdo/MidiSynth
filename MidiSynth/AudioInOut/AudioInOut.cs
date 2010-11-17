using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sanford.Multimedia.Midi;
using System.Threading;
using System.IO;
using MidiSynth.ChainMembers;
using System.Windows;
using MidiSynth.InputSources;
using MidiSynth.ChainCommon;

namespace MidiSynth
{
    class AudioInOut : IDisposable
    {
        private CC_Info info;
        private IAudioInputSource inputSource;
        private CC_Channel processingChannel;
        private Wave.WaveOutDevice player;
        public int sampleLengthMs;
        private bool threadContinue = true;
        private Object threadContinueSync = new Object();
        private Thread loopThread = null;

        public AudioInOut(IAudioInputSource inputSource, CC_Channel processingChannel, CC_Info info)
        {
            this.info = info;
            this.inputSource = inputSource;
            this.processingChannel = processingChannel;

            sampleLengthMs = (int)(info.BufferSize / (double)info.SampleRate * 1000);

            player = new Wave.WaveOutDevice(info.SampleRate, 8, info.BufferSize);
            player.Open();
        }

        public void StartLoop()
        {
            loopThread = new Thread(new ThreadStart(MainLoop));
            loopThread.Name = "AIOThread_" + this.GetHashCode().ToString();
            loopThread.Start();
        }

        public void StopLoop()
        {
            if (loopThread != null)
            {
                lock (threadContinueSync)
                {
                    threadContinue = false;
                }
                loopThread.Join();
                loopThread = null;
            }
        }

        public void MainLoop()
        {
            float[] buffer = new float[info.BufferSize];

            Semaphore semaphore = new Semaphore(0, 2, player.SyncName);
            bool doContinue = true;
            while (doContinue)
            {
                if (semaphore.WaitOne(sampleLengthMs * 2)) // avoid full block
                {
                    for (int i = 0; i < info.BufferSize; i++)
                    {
                        buffer[i] = inputSource.GetOutput();
                        buffer[i] = processingChannel.Process(buffer[i]);
                    }

                    player.Write(buffer);
                }
                lock (threadContinueSync)
                {
                    doContinue = threadContinue;
                }
            }
        }

        public void Dispose()
        {
            StopLoop();
            player.Dispose();
            inputSource.Dispose();
        }
    }
}
