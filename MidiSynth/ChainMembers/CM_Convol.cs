using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sanford.Multimedia.Midi;
using MidiSynth.ChainCommon;

namespace MidiSynth.ChainMembers
{
    class CM_Convol : AudioChainMember
    {
        private float[] buffer, h;

        public CM_Convol(CC_Channel c, float[] h)
            : base(c)
        {
            this.h = h;
            buffer = new float[h.Length];
        }

        public override float Process(float input)
        {
            float[] newBuffer = new float[buffer.Length];
            Array.Copy(buffer, 0, newBuffer, 1, buffer.Length - 1);
            newBuffer[0] = input;
            buffer = newBuffer;

            float y = 0;

            for (int i = 0; i < h.Length; i++)
            {
                y += h[i] * buffer[i];
            }

            return y;
        }
    }
}