using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MidiSynth.ChainCommon;

namespace MidiSynth.ChannelManagers
{
    class NotePlayer : IDisposable
    {
        private List<CC_Channel> channels = new List<CC_Channel>();
        private CC_Info Info;
        private CC_ChannelAdder cAdder;

        public delegate void ChannelSetupDelegate(CC_Channel channel);
        private ChannelSetupDelegate channelSetupDelegate;

        public NotePlayer(CC_Info info, ChannelSetupDelegate channelSetupDelegate)
        {
            this.channelSetupDelegate = channelSetupDelegate;
            Info = info;
            cAdder = new CC_ChannelAdder(channels);
            // Initialize a few channels before-hand
            for (int i = 0; i < 8; i++)
                channels.Add(new CC_Channel(Info));
        }

        private CC_Channel getFreeChannel(Object newTag)
        {
            CC_Channel result = null;
            foreach (CC_Channel chan in channels)
            {
                if (chan.tag == null)
                {
                    result = chan;
                    break;
                }
            }
            if (result == null)
            {
                result = new CC_Channel(Info);
                channels.Add(result);
            }
            result.tag = newTag;
            return result;
        }

        private CC_Channel findChannel(Object tag)
        {
            foreach (CC_Channel chan in channels)
            {
                if (chan.tag != null && chan.tag.Equals(tag))
                    return chan;
            }
            return null;
        }

        public float GetOutput()
        {
            return cAdder.GetOutput();
        }

        public void TriggerNoteOn(float freq)
        {
            CC_Channel chan = findChannel(freq); // if same note is re-pressed
            if (chan == null) chan = getFreeChannel(freq);
            chan.defaultInput = freq;
            channelSetupDelegate(chan);
            chan.Activate();
        }

        public void TriggerNoteOff(float freq)
        {
            CC_Channel c = findChannel(freq);
            if (c != null)
                c.InputEnded = true;
        }

        public void Dispose()
        {

        }
    }
}
