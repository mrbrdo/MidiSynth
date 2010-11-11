using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MidiSynth.ChainCommon
{
    class AudioChainMember : IAudioChainMember
    {
        protected CC_Channel c;

        public AudioChainMember(CC_Channel channel)
        {
            Init(channel);
        }

        public void Init(CC_Channel channel)
        {
            this.c = channel;
        }

        public virtual float Process(float input)
        {
            return input;
        }
    }
}
