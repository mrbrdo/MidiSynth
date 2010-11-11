using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MidiSynth.ChainCommon;

namespace MidiSynth.ChainMembers
{
    // Purpose: close channel immediately after input has ended (e.g. MIDI key was released)
    // Do not use this when using an envelope with release for example
    class CM_ChannelTerminator : AudioChainMember
    {
        public CM_ChannelTerminator(CC_Channel c) : base(c) { }

        public override float Process(float input)
        {
            if (c.InputEnded)
            {
                c.Free();
                return 0;
            }
            else
            {
                return input;
            }
        }
    }
}
