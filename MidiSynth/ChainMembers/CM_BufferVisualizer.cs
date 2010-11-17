using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sanford.Multimedia.Midi;
using MidiSynth.ChainCommon;

namespace MidiSynth.ChainMembers
{
    class CM_BufferVisualizer : AudioChainMember
    {
        private DateTime lastUpdated = DateTime.Now;
        private float[] buffer;
        private float updateInterval = 0.2f;

        public CM_BufferVisualizer(CC_Channel c, int nSamplesToVisualize) : base(c) {
            buffer = new float[nSamplesToVisualize];
        }

        public override float Process(float input)
        {
            float[] newBuffer = new float[buffer.Length];
            Array.Copy(buffer, 0, newBuffer, 1, buffer.Length - 1);
            newBuffer[0] = input;
            buffer = newBuffer;

            if ((DateTime.Now - lastUpdated).TotalSeconds >= updateInterval)
            {
                MainWindow.drawingSurface.UpdateBuffer(buffer);
                lastUpdated = DateTime.Now;
            }

            return input;
        }
    }
}
