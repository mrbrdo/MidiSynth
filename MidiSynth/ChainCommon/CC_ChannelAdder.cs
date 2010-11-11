using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MidiSynth.ChainCommon
{
    class CC_ChannelAdder
    {
        private List<CC_Channel> channels;
        public CC_ChannelAdder(List<CC_Channel> channels)
        {
            this.channels = channels;
        }

        public float GetOutput()
        {
            float output = 0;
            foreach (CC_Channel c in channels)
            {
                if (c.Active)
                    output += c.Process();
            }
            return output;
        }
    }
}
