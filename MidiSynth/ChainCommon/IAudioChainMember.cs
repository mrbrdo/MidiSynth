using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sanford.Multimedia.Midi;
using MidiSynth.ChainMembers;
using MidiSynth.ChainCommon;

namespace MidiSynth
{
    interface IAudioChainMember
    {
        void Init(CC_Channel channel);
        float Process(float input);
    }
}
