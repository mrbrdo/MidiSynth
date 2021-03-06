﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MidiSynth.ChainCommon;
using MidiSynth.InputSources;
using MidiSynth.ChainMembers;
using MidiSynth.ChannelManagers;

namespace MidiSynth
{
    class ConfSetup
    {
        private static CC_Info _info = null;
        public static CC_Info Info
        {
            get
            {
                if (_info == null)
                {
                    _info = new CC_Info();
                    _info.SampleRate = 8000;
                    _info.BufferSize = _info.SampleRate / 20;
                }
                return _info;
            }
        }

        public static void SetupClarinetWithEnvelope(out NotePlayer.ChannelSetupDelegate chanSetup, out CC_Channel outChannel)
        {
            chanSetup =
                (CC_Channel chan, Object caller) =>
                {
                    List<IAudioChainMember> is_chain = new List<IAudioChainMember>();
                    is_chain.Add(new CM_Oscillator(chan, (float freq, int step, int sr) =>
                    {
                        double oi = 2 * Math.PI * freq * step / (double)sr;
                        return (float)(Math.Cos(oi) + 0.375 * Math.Cos(3 * oi) +
                            0.581 * Math.Cos(5 * oi) + 0.382 * Math.Cos(7 * oi) +
                            0.141 * Math.Cos(9 * oi) + 0.028 * Math.Cos(11 * oi) + 0.009 * Math.Cos(13 * oi));
                    }));
                    is_chain.Add(new CM_ADSR_Envelope(chan, new float[] { 0.3f, 0, 0.3f }, 1, CM_ADSR_Envelope.ADSRLinearFunction));
                    chan.SetChain(is_chain);
                };

            outChannel = new CC_Channel(Info);
            List<IAudioChainMember> chain = new List<IAudioChainMember>();

            chain.Add(new CM_Normalizator(outChannel));
            chain.Add(new CM_BufferVisualizer(outChannel, 800));

            outChannel.SetChain(chain);
            outChannel.Activate();
        }

        public static void SetupStringsWithoutEnvelope(out NotePlayer.ChannelSetupDelegate chanSetup, out CC_Channel outChannel)
        {
            chanSetup =
                (CC_Channel chan, Object caller) =>
                {
                    List<IAudioChainMember> is_chain = new List<IAudioChainMember>();
                    is_chain.Add(new CM_KarplusStrong(chan));
                    is_chain.Add(new CM_ChannelTerminator(chan));

                    // midi only
                    if (caller.GetType().Name == "IS_MidiIn")
                    {
                        IS_MidiIn mi = (IS_MidiIn)caller;
                        mi.BindPropertyToController(0x7, is_chain[0], "K");
                    }
                    chan.SetChain(is_chain);
                };

            outChannel = new CC_Channel(Info);
            List<IAudioChainMember> chain = new List<IAudioChainMember>();

            chain.Add(new CM_Normalizator(outChannel));
            chain.Add(new CM_BufferVisualizer(outChannel, 800));

            outChannel.SetChain(chain);
            outChannel.Activate();
        }

        public static void SetupSineWaveWithoutEnvelope(out NotePlayer.ChannelSetupDelegate chanSetup, out CC_Channel outChannel)
        {
            chanSetup =
                (CC_Channel chan, Object caller) =>
                {
                    List<IAudioChainMember> is_chain = new List<IAudioChainMember>();
                    is_chain.Add(new CM_Oscillator(chan, (float freq, int step, int sr) =>
                    {
                        double oi = 2 * Math.PI * freq * step / (double)sr;
                        return (float)(Math.Sin(oi));
                    }));
                    is_chain.Add(new CM_ChannelTerminator(chan));
                    chan.SetChain(is_chain);
                };

            outChannel = new CC_Channel(Info);
            List<IAudioChainMember> chain = new List<IAudioChainMember>();

            chain.Add(new CM_Normalizator(outChannel));
            chain.Add(new CM_BufferVisualizer(outChannel, 800));

            outChannel.SetChain(chain);
            outChannel.Activate();
        }

        public static void SetupReadWaveTest()
        {
            int pcmRate;
            float[][] data = Wave.ReadWavePCM(@"C:\dps\scilab\delo\vaje4\FOLIG8K.wav", out pcmRate);

            CC_Info info = new CC_Info();
            info.SampleRate = pcmRate;
            info.BufferSize = data[0].Length;
            Wave.WaveOutDevice player = new Wave.WaveOutDevice(info.SampleRate, 16, info.BufferSize);
            player.Open();
            player.Write(data[0]);
        }

        public static void SetupConvolFolig(out NotePlayer.ChannelSetupDelegate chanSetup, out CC_Channel outChannel)
        {
            int pcmRate;
            float[][] data = Wave.ReadWavePCM(@"C:\dps\scilab\delo\vaje4\FOLIG8K.wav", out pcmRate);

            Info.SampleRate = pcmRate; // sync sample rate

            chanSetup =
                (CC_Channel chan, Object caller) =>
                {
                    List<IAudioChainMember> is_chain = new List<IAudioChainMember>();
                    is_chain.Add(new CM_Oscillator(chan, (float freq, int step, int sr) =>
                    {
                        double oi = 2 * Math.PI * freq * step / (double)sr;
                        return 50.0f * (float)(Math.Cos(oi) + 0.375 * Math.Cos(3 * oi) +
                            0.581 * Math.Cos(5 * oi) + 0.382 * Math.Cos(7 * oi) +
                            0.141 * Math.Cos(9 * oi) + 0.028 * Math.Cos(11 * oi) + 0.009 * Math.Cos(13 * oi));
                    }));
                    is_chain.Add(new CM_ChannelTerminator(chan));
                    chan.SetChain(is_chain);
                };

            outChannel = new CC_Channel(Info);
            List<IAudioChainMember> chain = new List<IAudioChainMember>();

            chain.Add(new CM_Convol(outChannel, data[0]));
            chain.Add(new CM_Normalizator(outChannel));
            chain.Add(new CM_BufferVisualizer(outChannel, 800));

            outChannel.SetChain(chain);
            outChannel.Activate();
        }

        public static void Setup(out IAudioInputSource inputSource, out CC_Channel outChannel)
        {
            NotePlayer.ChannelSetupDelegate chanSetup;

            //SetupStringsWithoutEnvelope(out chanSetup, out outChannel);
            SetupClarinetWithEnvelope(out chanSetup, out outChannel);
            //SetupSineWaveWithoutEnvelope(out chanSetup, out outChannel);
            //SetupConvolFolig(out chanSetup, out outChannel);

            //inputSource = new IS_MidiIn(Info, chanSetup);
            inputSource = new IS_ScalePlayer(Info, chanSetup, 1000);
        }
    }
}
