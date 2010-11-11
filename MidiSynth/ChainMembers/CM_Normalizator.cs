using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sanford.Multimedia.Midi;
using MidiSynth.ChainCommon;

namespace MidiSynth.ChainMembers
{
    class CM_Normalizator : AudioChainMember
    {
        private float maxVal = 1.0f;
        private float minVal = -1.0f;
        
        public CM_Normalizator(CC_Channel c) : base(c) { }

        public override float Process(float input)
        {
            if (input > maxVal) maxVal = input;
            if (input < minVal) minVal = input;
            float normVal = (-minVal > maxVal) ? -minVal : maxVal;
            return (input / normVal);
        }
    }
}
