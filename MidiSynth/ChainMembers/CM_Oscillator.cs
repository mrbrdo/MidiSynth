using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sanford.Multimedia.Midi;
using MidiSynth.ChainCommon;

namespace MidiSynth.ChainMembers
{
    class CM_Oscillator : AudioChainMember
    {
        private int oscStep = 0;

        public delegate float OscillatorFunctionDelegate(float frequency, int step, int sampleRate);
        public static OscillatorFunctionDelegate SineWave = (float freq, int step, int sr) =>
        {
            double oi = 2 * Math.PI * freq * step / (double)sr;
            return (float)(Math.Sin(oi));
        };

        private OscillatorFunctionDelegate funDelegate;

        public CM_Oscillator(CC_Channel channel, OscillatorFunctionDelegate oscillatorFunctionDelegate) : base(channel)
        {
            funDelegate = oscillatorFunctionDelegate;
        }

        public override float Process(float input)
        {
            float val = funDelegate(input, oscStep, c.Info.SampleRate);
            oscStep++;
            //if (oscStep < 0) oscStep = 0;
            return val;
        }
    }
}
