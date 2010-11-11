using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MidiSynth.ChainCommon;

namespace MidiSynth.ChainMembers
{
    // Also acts as a channel terminator
    class CM_ADSR_Envelope : AudioChainMember
    {
        private float[] adr_in_seconds;
        private int[] adr;
        private int decaySampleStart, sustainSampleStart;
        private float sustainLevelFraction;
        private int iSample = 0;
        private int iSampleSinceInputEnded = 0;
        private float lastAmplitudeMultiplierBeforeInputEnded = 0;

        public delegate float ADSRFunctionDelegate(float fraction); // fraction = 0..1
        private ADSRFunctionDelegate adsrFunction;

        public static ADSRFunctionDelegate ADSRLinearFunction = (float fraction) =>
        {
            return fraction;
        };

        public CM_ADSR_Envelope(CC_Channel c, float[] adr_times, float sustainLevelFraction, ADSRFunctionDelegate adsrFunction) : base(c)
        {
            if (adr_times.Length != 3) throw new Exception("ADSR Envelope requires a float array of format {attack, decay, release} in seconds");
            this.adr_in_seconds = adr_times;
            this.adsrFunction = adsrFunction;
            this.sustainLevelFraction = sustainLevelFraction;
            this.adr = new int[3]; // in number of samples
            for (int i = 0; i < 3; i++)
            {
                this.adr[i] = (int)(adr_in_seconds[i] * c.Info.SampleRate);
            }
            this.decaySampleStart = adr[0];
            this.sustainSampleStart = adr[0] + adr[1];
        }

        public override float Process(float input)
        {
            float result_mult = 1;
            if (c.InputEnded)
            {
                // release
                if (iSampleSinceInputEnded < adr[2] && adr[2] != 0)
                {
                    //result = inputAtSustain - (inputAtSustain * adsrFunction((iSampleSinceInputEnded / (float)adr[2])));
                    result_mult = lastAmplitudeMultiplierBeforeInputEnded * (1.0f - adsrFunction((iSampleSinceInputEnded / (float)adr[2])));
                }
                else
                {
                    // terminate channel
                    c.Free();
                    result_mult = 0;
                }
                iSampleSinceInputEnded++;
            }
            else
            {
                // attack
                if (iSample < adr[0] && adr[0] != 0)
                {
                    result_mult = adsrFunction(iSample / (float)adr[0]);
                }
                // decay
                else if (iSample >= decaySampleStart && iSample < sustainSampleStart && adr[1] != 0)
                {
                    //float inputAtSustain = input * sustainLevelFraction;
                    //result = inputAtSustain + ((input - inputAtSustain) * );
                    result_mult = sustainLevelFraction +
                        (1.0f - adsrFunction((iSample - decaySampleStart) / (float)adr[1])) * (1.0f - sustainLevelFraction);
                }
                // sustain
                else
                {
                    result_mult = sustainLevelFraction;
                }
                iSample++;
                lastAmplitudeMultiplierBeforeInputEnded = result_mult;
            }
            return input * result_mult;
        }
    }
}
